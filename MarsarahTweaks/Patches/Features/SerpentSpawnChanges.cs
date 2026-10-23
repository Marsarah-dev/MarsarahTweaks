using HarmonyLib;
using MarsarahTweaks.Managers;
using MarsarahTweaks.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class SerpentSpawnChanges
	{
		private static readonly LogManager log = new LogManager("Serpent Spawns", LogManager.LogLevel.Info);

		private const string SmallSerpentPrefabName = "MSmallSerpent";
		private const string SmallSerpentSpawnerName = "MSmallSerpent [Ocean]";
		private const string BonemassGlobalKey = "defeated_bonemass";

		private static readonly float serpentChanceNightBonemass = 10f; // 8f;
		private static readonly float serpentIntervalNightBonemass = 800f;
		private static readonly float serpentChanceRainModer = 10f; // 8f;
		private static readonly float serpentChanceNightModer = 15f; // 12f;
		private static readonly float serpentIntervalRainModer = 800f;
		private static readonly float serpentIntervalNightModer = 700f;

		private static readonly float smallSerpentScale = 0.55f;
		private static readonly float smallSerpentHealth = 200f;
		private static readonly float smallSerpentDamageMultiplier = 0.5f;
		private static readonly float smallSerpentChanceBonemass = 5f; // 2f;
		private static readonly float smallSerpentIntervalBonemass = 900f;
		private static readonly float smallSerpentChanceModer = 10f; // 3f;
		private static readonly float smallSerpentIntervalModer = 700f;

		private static readonly MethodInfo memberwiseClone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo namedPrefabsField = AccessTools.Field(typeof(ZNetScene), "m_namedPrefabs");

		private enum ProgressionStage
		{
			Vanilla,
			Bonemass,
			Moder
		}

		private sealed class OriginalSpawnValues
		{
			public readonly float Chance;
			public readonly float Interval;

			public OriginalSpawnValues(float chance, float interval)
			{
				Chance = chance;
				Interval = interval;
			}
		}

		// Do not retain SpawnData from unloaded zones.
		private static ConditionalWeakTable<SpawnSystem.SpawnData, OriginalSpawnValues> originalSpawnValues = new ConditionalWeakTable<SpawnSystem.SpawnData, OriginalSpawnValues>();
		private static ProgressionStage? lastCheckedStage;
		private static bool? lastCheckedEnabled;
		private static bool? lastCheckedSmallEnabled;
		private static ProgressionStage? lastLoggedStage;
		private static bool? lastLoggedEnabled;
		private static ProgressionStage? lastLoggedSmallStage;
		private static bool? lastLoggedSmallEnabled;

		private static GameObject smallSerpentPrefab;
		private static GameObject prefabContainer;
		private static bool registrationErrorLogged;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		private static class WorldResetAndRegistration_Patch
		{
			private static void Prefix()
			{
				originalSpawnValues = new ConditionalWeakTable<SpawnSystem.SpawnData, OriginalSpawnValues>();
				lastCheckedStage = null;
				lastCheckedEnabled = null;
				lastCheckedSmallEnabled = null;
				lastLoggedStage = null;
				lastLoggedEnabled = null;
				lastLoggedSmallStage = null;
				lastLoggedSmallEnabled = null;
				smallSerpentPrefab = null;
				prefabContainer = null;
				registrationErrorLogged = false;
			}

			private static void Postfix(ZNetScene __instance)
			{
				RegisterSmallSerpent(__instance);
			}
		}

		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		private static class SpawnSystemAwake_Patch
		{
			private static void Postfix(SpawnSystem __instance)
			{
				UpdateSerpentSpawns(__instance);
				UpdateSmallSerpentSpawns(__instance);
			}
		}

		// Alter only the small creature's owned weapon data after its default items are given.
		// This leaves the original Serpent's attack items and their damage unchanged.
		[HarmonyPatch(typeof(Humanoid), "GiveDefaultItems")]
		private static class SmallSerpentWeapons_Patch
		{
			private static void Postfix(Humanoid __instance)
			{
				if (__instance == null || (__instance.gameObject.name != SmallSerpentPrefabName && __instance.gameObject.name != SmallSerpentPrefabName + "(Clone)")) return;
				if (__instance.GetInventory() == null) return;

				foreach (ItemDrop.ItemData item in __instance.GetInventory().GetAllItems())
				{
					if (item == null || item.m_shared == null) continue;

					// SharedData is a reference type. Clone it so changes cannot affect vanilla weapons.
					item.m_shared = CloneObject(item.m_shared);
					item.m_shared.m_damages.Modify(smallSerpentDamageMultiplier);
				}
			}
		}

		// GlobalKeyChecker updates boss states from ZoneSystem.Update. Run afterward.
		[HarmonyPatch(typeof(ZoneSystem), "Update")]
		[HarmonyPriority(Priority.Last)]
		private static class BossProgressionWatcher_Patch
		{
			private static void Postfix()
			{
				ProgressionStage stage = GetProgressionStage();
				bool enabled = ConfigManager.MoreSerpentSpawnsEnabled.Value;
				bool smallEnabled = ConfigManager.SmallSerpentSpawnsEnabled.Value;

				if (lastCheckedStage == stage && lastCheckedEnabled == enabled && lastCheckedSmallEnabled == smallEnabled) return;

				lastCheckedStage = stage;
				lastCheckedEnabled = enabled;
				lastCheckedSmallEnabled = smallEnabled;
				UpdateAllSerpentSpawns();
			}
		}

		public static void UpdateAllSerpentSpawns()
		{
			foreach (SpawnSystem spawnSystem in UnityEngine.Object.FindObjectsByType<SpawnSystem>(FindObjectsSortMode.None))
			{
				UpdateSerpentSpawns(spawnSystem);
				UpdateSmallSerpentSpawns(spawnSystem);
			}
		}

		public static void UpdateSerpentSpawns(SpawnSystem spawnSystem)
		{
			if (spawnSystem == null || spawnSystem.m_spawnLists == null) return;

			ProgressionStage stage = GetProgressionStage();
			bool enabled = ConfigManager.MoreSerpentSpawnsEnabled.Value;
			bool reportStage = lastLoggedStage != stage || lastLoggedEnabled != enabled;
			int matchedEntries = 0;

			foreach (SpawnSystemList spawnList in spawnSystem.m_spawnLists)
			{
				if (spawnList == null || spawnList.m_spawners == null) continue;

				foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
				{
					if (spawner == null || spawner.m_prefab == null || spawner.m_prefab.name != "Serpent") continue;
					if (spawner.m_biome != Heightmap.Biome.Ocean) continue;

					bool isNightSpawner = IsNightSpawner(spawner);
					bool isStormSpawner = IsStormSpawner(spawner);
					if (!isNightSpawner && !isStormSpawner) continue;

					matchedEntries++;

					OriginalSpawnValues original = originalSpawnValues.GetValue(spawner, s => new OriginalSpawnValues(s.m_spawnChance, s.m_spawnInterval));
					float chance = original.Chance;
					float interval = original.Interval;

					if (enabled)
					{
						if (stage == ProgressionStage.Bonemass && isNightSpawner)
						{
							chance = serpentChanceNightBonemass;
							interval = serpentIntervalNightBonemass;
						}
						else if (stage == ProgressionStage.Moder)
						{
							chance = isNightSpawner ? serpentChanceNightModer : serpentChanceRainModer;
							interval = isNightSpawner ? serpentIntervalNightModer : serpentIntervalRainModer;
						}
					}

					if (spawner.m_spawnChance == chance && spawner.m_spawnInterval == interval) continue;

					if (reportStage)
					{
						log.Info($"{(isNightSpawner ? "Night" : "Rain/Storm")} Serpent: {spawner.m_spawnChance}% / {spawner.m_spawnInterval}s -> {chance}% / {interval}s");
					}

					spawner.m_spawnChance = chance;
					spawner.m_spawnInterval = interval;
				}
			}

			if (matchedEntries == 0) return;

			if (reportStage)
			{
				if (matchedEntries != 2)
				{
					log.Warn($"Expected two vanilla Serpent spawn entries; found {matchedEntries}. Check the current game's spawn definitions.");
				}

				log.Info($"More Serpent Spawns: {(enabled ? "enabled" : "disabled")} | Stage: {stage} | Entries: {matchedEntries}");
				lastLoggedStage = stage;
				lastLoggedEnabled = enabled;
			}
		}

		private static bool RegisterSmallSerpent(ZNetScene scene)
		{
			if (smallSerpentPrefab != null) return true;
			if (scene == null || scene.m_prefabs == null) return false;

			Dictionary<int, GameObject> namedPrefabs = namedPrefabsField?.GetValue(scene) as Dictionary<int, GameObject>;

			if (namedPrefabs == null)
			{
				if (!registrationErrorLogged)
				{
					log.Error("Cannot register MSmallSerpent: unable to access ZNetScene.m_namedPrefabs.");
					registrationErrorLogged = true;
				}
				return false;
			}

			GameObject original = scene.GetPrefab("Serpent");
			if (original == null)
			{
				if (!registrationErrorLogged)
				{
					log.Warn("Cannot create MSmallSerpent: vanilla Serpent prefab not available yet.");
					registrationErrorLogged = true;
				}
				return false;
			}

			int hash = SmallSerpentPrefabName.GetStableHashCode();
			if (namedPrefabs.ContainsKey(hash))
			{
				if (!registrationErrorLogged)
				{
					log.Error($"Cannot register {SmallSerpentPrefabName}: prefab hash already registered by another mod.");
					registrationErrorLogged = true;
				}
				return false;
			}

			// An inactive parent keeps the cloned prefab from running creature logic before spawning.
			prefabContainer = new GameObject("MarsarahTweaks_SerpentPrefabs");
			prefabContainer.SetActive(false);
			GameObject clone = UnityEngine.Object.Instantiate(original, prefabContainer.transform);
			clone.name = SmallSerpentPrefabName;
			clone.SetActive(true);
			clone.transform.localScale = original.transform.localScale * smallSerpentScale;

			Character character = clone.GetComponent<Character>();
			Humanoid humanoid = clone.GetComponent<Humanoid>();
			CharacterDrop drops = clone.GetComponent<CharacterDrop>();
			CharacterDrop originalDrops = original.GetComponent<CharacterDrop>();
			CharacterDrop.Drop originalMeat = originalDrops?.m_drops?.Find(d => d != null && d.m_prefab != null && d.m_prefab.name == "SerpentMeat");

			if (character == null || humanoid == null || humanoid.m_defaultItems == null || humanoid.m_defaultItems.Length == 0 || drops == null || originalMeat == null)
			{
				if (!registrationErrorLogged)
				{
					log.Error("Cannot register MSmallSerpent: required Humanoid default attacks, CharacterDrop, or SerpentMeat data is missing.");
					registrationErrorLogged = true;
				}

				UnityEngine.Object.Destroy(prefabContainer);
				prefabContainer = null;
				return false;
			}

			character.m_health = smallSerpentHealth;
			character.m_name = "Sea Wyrm";

			// 1.0's drop maximum is exclusive: 2 to 4 gives 2 or 3 meat.
			CharacterDrop.Drop meat = CloneObject(originalMeat);
			meat.m_amountMin = 2;
			meat.m_amountMax = 4;
			meat.m_chance = 100f;
			meat.m_dontScale = true;
			drops.m_drops = new List<CharacterDrop.Drop> { meat };

			// Every peer must register this prefab even when Small Serpent Spawns is disabled.
			scene.m_prefabs.Add(clone);
			namedPrefabs.Add(hash, clone);
			smallSerpentPrefab = clone;
			registrationErrorLogged = false;
			log.Info($"Registered {SmallSerpentPrefabName}: scale {smallSerpentScale:P0}, health {smallSerpentHealth}, attack damage x{smallSerpentDamageMultiplier}, 2-3 SerpentMeat only.");
			return true;
		}

		private static void UpdateSmallSerpentSpawns(SpawnSystem spawnSystem)
		{
			if (spawnSystem == null || spawnSystem.m_spawnLists == null) return;
			if (!RegisterSmallSerpent(ZNetScene.instance)) return;

			ProgressionStage stage = GetProgressionStage();
			bool configEnabled = ConfigManager.SmallSerpentSpawnsEnabled.Value;
			bool shouldSpawn = configEnabled && GlobalKeyChecker.BonemassDefeated;
			float chance = stage == ProgressionStage.Moder ? smallSerpentChanceModer : smallSerpentChanceBonemass;
			float interval = stage == ProgressionStage.Moder ? smallSerpentIntervalModer : smallSerpentIntervalBonemass;
			int matchedEntries = 0;

			foreach (SpawnSystemList spawnList in spawnSystem.m_spawnLists)
			{
				if (spawnList == null || spawnList.m_spawners == null) continue;

				SpawnSystem.SpawnData smallSpawner = null;
				SpawnSystem.SpawnData nightSpawner = null;

				foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
				{
					if (spawner == null) continue;
					if (spawner.m_name == SmallSerpentSpawnerName)
					{
						smallSpawner = spawner;
						break;
					}
					if (spawner.m_prefab != null && spawner.m_prefab.name == "Serpent" && spawner.m_biome == Heightmap.Biome.Ocean && IsNightSpawner(spawner))
					{
						nightSpawner = spawner;
					}
				}

				if (smallSpawner == null && nightSpawner != null)
				{
					// Preserve all the game's placement requirements and spawn mechanics.
					smallSpawner = CloneObject(nightSpawner);
					smallSpawner.m_name = SmallSerpentSpawnerName;
					smallSpawner.m_prefab = smallSerpentPrefab;
					smallSpawner.m_biome = Heightmap.Biome.Ocean | Heightmap.Biome.Meadows | Heightmap.Biome.BlackForest | Heightmap.Biome.Swamp | Heightmap.Biome.Plains | Heightmap.Biome.Mistlands;
					smallSpawner.m_biomeArea = Heightmap.BiomeArea.Everything;
					smallSpawner.m_minAltitude = -1000f;
					smallSpawner.m_maxAltitude = -20f;
					smallSpawner.m_spawnAtDay = true;
					smallSpawner.m_spawnAtNight = true;
					smallSpawner.m_requiredEnvironments = new List<string>();
					smallSpawner.m_requiredGlobalKey = BonemassGlobalKey;
					smallSpawner.m_maxSpawned = 1;
					smallSpawner.m_groupSizeMin = 1;
					smallSpawner.m_groupSizeMax = 1;
					spawnList.m_spawners.Add(smallSpawner);
				}

				if (smallSpawner == null) continue;

				matchedEntries++;
				smallSpawner.m_enabled = shouldSpawn;
				smallSpawner.m_spawnChance = shouldSpawn ? chance : 0f;
				smallSpawner.m_spawnInterval = interval;
			}

			if (matchedEntries == 0) return;

			bool report = lastLoggedSmallStage != stage || lastLoggedSmallEnabled != configEnabled;
			if (report)
			{
				log.Info($"Small Serpent Spawns: {(shouldSpawn ? "enabled" : "disabled")} | Stage: {stage} | Chance: {(shouldSpawn ? chance : 0f)}% | Interval: {interval}s | Entries: {matchedEntries}");
				lastLoggedSmallStage = stage;
				lastLoggedSmallEnabled = configEnabled;
			}
		}

		private static T CloneObject<T>(T source) where T : class
		{
			return (T)memberwiseClone.Invoke(source, null);
		}

		private static ProgressionStage GetProgressionStage()
		{
			if (GlobalKeyChecker.ModerDefeated) return ProgressionStage.Moder;
			if (GlobalKeyChecker.BonemassDefeated) return ProgressionStage.Bonemass;
			return ProgressionStage.Vanilla;
		}

		private static bool IsNightSpawner(SpawnSystem.SpawnData spawner)
		{
			return !spawner.m_spawnAtDay && spawner.m_spawnAtNight &&
				(spawner.m_requiredEnvironments == null || spawner.m_requiredEnvironments.Count == 0);
		}

		private static bool IsStormSpawner(SpawnSystem.SpawnData spawner)
		{
			return spawner.m_spawnAtDay && spawner.m_spawnAtNight &&
				spawner.m_requiredEnvironments != null &&
				spawner.m_requiredEnvironments.Contains("Rain") &&
				spawner.m_requiredEnvironments.Contains("ThunderStorm");
		}
	}
}
