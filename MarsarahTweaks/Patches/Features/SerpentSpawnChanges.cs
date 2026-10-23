using HarmonyLib;
using MarsarahTweaks.Managers;
using MarsarahTweaks.Patches;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.Features
{
	internal class SerpentSpawnChanges
	{
		private static readonly LogManager log = new LogManager("Serpent Spawns", LogManager.LogLevel.Info);

		private static readonly float serpentChanceNightBonemass = 50f; //8f;
		private static readonly float serpentIntervalNightBonemass = 800f;
		private static readonly float serpentChanceRainModer = 50f; //8f;
		private static readonly float serpentChanceNightModer = 75f; //12f;
		private static readonly float serpentIntervalRainModer = 800f;
		private static readonly float serpentIntervalNightModer = 700f;

		private enum ProgressionStage
		{
			Vanilla,
			Bonemass,
			Moder
		}

		// Use SpawnData references as keys: both vanilla entries are named "Serpent".
		private static readonly Dictionary<SpawnSystem.SpawnData, (float chance, float interval)> originalSpawnValues = new Dictionary<SpawnSystem.SpawnData, (float chance, float interval)>();
		private static SpawnSystem activeSpawnSystem;
		private static ProgressionStage? lastAppliedStage;
		private static bool? lastAppliedEnabled;

		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		private static class SpawnSystemAwake_Patch
		{
			private static void Postfix(SpawnSystem __instance)
			{
				UpdateSerpentSpawns(__instance);
			}
		}

		// GlobalKeyChecker also runs after ZoneSystem.Update. Run after it so boss states are current.
		[HarmonyPatch(typeof(ZoneSystem), "Update")]
		[HarmonyPriority(Priority.Last)]
		private static class BossProgressionWatcher_Patch
		{
			private static void Postfix()
			{
				if (activeSpawnSystem == null) return;

				ProgressionStage stage = GetProgressionStage();
				bool enabled = ConfigManager.MoreSerpentSpawnsEnabled.Value;

				if (lastAppliedStage == stage && lastAppliedEnabled == enabled) return;

				UpdateSerpentSpawns(activeSpawnSystem);
			}
		}

		public static void UpdateSerpentSpawns(SpawnSystem spawnSystem)
		{
			if (spawnSystem == null || spawnSystem.m_spawnLists == null) return;

			if (activeSpawnSystem != spawnSystem)
			{
				// World/SpawnSystem changed: discard backups for the previous instance.
				activeSpawnSystem = spawnSystem;
				originalSpawnValues.Clear();
				lastAppliedStage = null;
				lastAppliedEnabled = null;
			}

			ProgressionStage stage = GetProgressionStage();
			bool enabled = ConfigManager.MoreSerpentSpawnsEnabled.Value;
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

					if (!originalSpawnValues.TryGetValue(spawner, out var original))
					{
						original = (spawner.m_spawnChance, spawner.m_spawnInterval);
						originalSpawnValues.Add(spawner, original);
					}

					float chance = original.chance;
					float interval = original.interval;

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

					log.Info($"{(isNightSpawner ? "Night" : "Rain/Storm")} Serpent: {spawner.m_spawnChance}% / {spawner.m_spawnInterval}s -> {chance}% / {interval}s");
					spawner.m_spawnChance = chance;
					spawner.m_spawnInterval = interval;
				}
			}

			// If the spawn list is not ready yet, retry on the next ZoneSystem update.
			if (matchedEntries == 0) return;

			if (matchedEntries != 2)
			{
				log.Warn($"Expected two vanilla Serpent spawn entries; found {matchedEntries}. Check the current game's spawn definitions.");
			}

			log.Info($"More Serpent Spawns: {(enabled ? "enabled" : "disabled")} | Stage: {stage} | Entries: {matchedEntries}");
			lastAppliedStage = stage;
			lastAppliedEnabled = enabled;
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
