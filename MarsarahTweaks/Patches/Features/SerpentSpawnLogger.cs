using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Reflection;

namespace MarsarahTweaks.Patches.Features
{
	// Temporary diagnostic. Remove this class after verifying the live spawn data.
	internal class SerpentSpawnLogger
	{
		private static readonly LogManager log = new LogManager("Serpent Spawns", LogManager.LogLevel.Info);
		private static readonly FieldInfo spawnDistanceMinField = AccessTools.Field(typeof(SpawnSystem), "m_spawnDistanceMin");
		private static readonly FieldInfo spawnDistanceMaxField = AccessTools.Field(typeof(SpawnSystem), "m_spawnDistanceMax");
		private static bool logged;

		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		private static class SpawnSystemAwake_Patch
		{
			private static void Postfix(SpawnSystem __instance)
			{
				if (logged || __instance == null || __instance.m_spawnLists == null) return;

				int serpentCount = 0;
				int bonemawCount = 0;

				log.Info("=== Serpent and Bonemaw spawn definitions (live values) ===");
				string minDistance = spawnDistanceMinField?.GetValue(__instance)?.ToString() ?? "unavailable";
				string maxDistance = spawnDistanceMaxField?.GetValue(__instance)?.ToString() ?? "unavailable";
				log.Info($"SpawnSystem player-distance range: {minDistance} to {maxDistance} m");

				foreach (SpawnSystemList spawnList in __instance.m_spawnLists)
				{
					if (spawnList == null || spawnList.m_spawners == null) continue;

					foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
					{
						if (spawner == null) continue;

						string prefabName = spawner.m_prefab != null ? spawner.m_prefab.name : "(null)";
						if (spawner.m_name == "Serpent" || prefabName == "Serpent")
						{
							LogSpawner(spawner, prefabName, "Serpent", ++serpentCount);
						}
						else if (spawner.m_name == "Bonemaw Serpent" || spawner.m_name == "BonemawSerpent" || prefabName == "BonemawSerpent")
						{
							LogSpawner(spawner, prefabName, "Bonemaw", ++bonemawCount);
						}
					}
				}

				log.Info($"=== End spawn definitions: {serpentCount} Serpent entries, {bonemawCount} Bonemaw entries ===");
				if (serpentCount > 0 || bonemawCount > 0) logged = true;
			}
		}

		private static void LogSpawner(SpawnSystem.SpawnData spawner, string prefabName, string creatureName, int entry)
		{
			string environments = spawner.m_requiredEnvironments != null
				? string.Join(", ", spawner.m_requiredEnvironments)
				: "(any)";

			if (string.IsNullOrEmpty(environments)) environments = "(any)";

			log.Info($"{creatureName} entry {entry}: {spawner.m_name} | Prefab: {prefabName} | Enabled: {spawner.m_enabled} | DevDisabled: {spawner.m_devDisabled}");
			log.Info($"  Biome: {spawner.m_biome} | Biome area: {spawner.m_biomeArea} | Altitude: {spawner.m_minAltitude} to {spawner.m_maxAltitude}");
			log.Info($"  Day: {spawner.m_spawnAtDay} | Night: {spawner.m_spawnAtNight} | Weather: {environments}");
			log.Info($"  Chance: {spawner.m_spawnChance}% | Interval: {spawner.m_spawnInterval}s | Max spawned: {spawner.m_maxSpawned}");
			log.Info($"  Group size: {spawner.m_groupSizeMin} to {spawner.m_groupSizeMax} | Same-creature separation: {spawner.m_spawnDistance}m");
			log.Info($"  Spawn radius: {spawner.m_spawnRadiusMin} to {spawner.m_spawnRadiusMax}m | Ocean depth: {spawner.m_minOceanDepth} to {spawner.m_maxOceanDepth}");
			log.Info($"  Required key: {spawner.m_requiredGlobalKey} | Required event: {spawner.m_requiredPersistentEvent}");
		}
	}
}
