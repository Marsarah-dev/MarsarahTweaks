using HarmonyLib;
using MarsarahTweaks.Managers;
using MarsarahTweaks.Patches;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class SerpentSpawnChanges
	{
		private static readonly LogManager log = new LogManager("Serpent Spawns", LogManager.LogLevel.Info);

		private static readonly float serpentChanceNightBonemass = 8f;
		private static readonly float serpentIntervalNightBonemass = 800f;
		private static readonly float serpentChanceRainModer = 8f;
		private static readonly float serpentChanceNightModer = 12f;
		private static readonly float serpentIntervalRainModer = 800f;
		private static readonly float serpentIntervalNightModer = 700f;

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

		// Weak keys retain each SpawnData's original values without keeping unloaded zones alive.
		private static ConditionalWeakTable<SpawnSystem.SpawnData, OriginalSpawnValues> originalSpawnValues = new ConditionalWeakTable<SpawnSystem.SpawnData, OriginalSpawnValues>();

		private static ProgressionStage? lastCheckedStage;
		private static bool? lastCheckedEnabled;
		private static ProgressionStage? lastLoggedStage;
		private static bool? lastLoggedEnabled;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		private static class WorldReset_Patch
		{
			private static void Prefix()
			{
				originalSpawnValues = new ConditionalWeakTable<SpawnSystem.SpawnData, OriginalSpawnValues>();
				lastCheckedStage = null;
				lastCheckedEnabled = null;
				lastLoggedStage = null;
				lastLoggedEnabled = null;
			}
		}

		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		private static class SpawnSystemAwake_Patch
		{
			private static void Postfix(SpawnSystem __instance)
			{
				// Each spawned system needs its own SpawnData updated, including new zones.
				UpdateSerpentSpawns(__instance);
			}
		}

		// GlobalKeyChecker also patches ZoneSystem.Update; use last priority to see updated keys.
		[HarmonyPatch(typeof(ZoneSystem), "Update")]
		[HarmonyPriority(Priority.Last)]
		private static class BossProgressionWatcher_Patch
		{
			private static void Postfix()
			{
				ProgressionStage stage = GetProgressionStage();
				bool enabled = ConfigManager.MoreSerpentSpawnsEnabled.Value;

				// ZoneSystem.Update runs frequently, but scanning SpawnSystems only occurs on a transition.
				if (lastCheckedStage == stage && lastCheckedEnabled == enabled) return;

				lastCheckedStage = stage;
				lastCheckedEnabled = enabled;
				UpdateAllSerpentSpawns();
			}
		}

		public static void UpdateAllSerpentSpawns()
		{
			foreach (SpawnSystem spawnSystem in Object.FindObjectsByType<SpawnSystem>(FindObjectsSortMode.None))
			{
				UpdateSerpentSpawns(spawnSystem);
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
