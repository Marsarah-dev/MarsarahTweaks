using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class NighttimeSpawnChanges
	{
		private static readonly LogManager log = new LogManager("Night Spawns", LogManager.LogLevel.Info);

		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		class NighttimeSpawn_Patch
		{
			private static void Postfix(SpawnSystem __instance)
			{
				// Run on both server and client - no checks made

				if (__instance == null) return;

				UpdateNighttimeSpawns(__instance);
				//LogNighttimeSpawns(__instance);
			}
		}

		// Dictionaries
		private static readonly Dictionary<string, Heightmap.Biome> originalBiomes = new Dictionary<string, Heightmap.Biome>();
		private static readonly Dictionary<string, (Heightmap.Biome biome, string globalKey)> nighttimeBiomeChanges = new Dictionary<string, (Heightmap.Biome biome, string globalKey)>()
		{
			{ "Goblin", (Heightmap.Biome.Mountain, "defeated_goblinking") },
			{ "Seeker defeated queen other biomes", (Heightmap.Biome.Plains, "defeated_queen") },
			{ "Charred Melee [Other biomes when Fader is defeated]", (Heightmap.Biome.Plains, "defeated_fader") },
			{ "Charred Archer [Other biomes when Fader is defeated]", (Heightmap.Biome.Plains, "defeated_fader") }
			//{ "Charred Melee [Other biomes when Fader is defeated]", (Heightmap.Biome.Plains | Heightmap.Biome.Mistlands, "defeated_fader") },
			//{ "Charred Archer [Other biomes when Fader is defeated]", (Heightmap.Biome.Plains | Heightmap.Biome.Mistlands, "defeated_fader") }
		};

		public static void UpdateNighttimeSpawns(SpawnSystem spawnSystem)
		{
			foreach (SpawnSystemList spawnList in spawnSystem.m_spawnLists)
			{
				foreach (var spawner in spawnList.m_spawners)
				{
					if (spawner.m_spawnAtNight == false)
						continue;

					if (nighttimeBiomeChanges.TryGetValue(spawner.m_name, out var newBiomeData))
					{
						if (ConfigManager.StopNighttimeInvasionEnabled.Value)
						{
							if ((spawner.m_requiredGlobalKey ?? "") == newBiomeData.globalKey)
							{
								// Backup
								if (!originalBiomes.ContainsKey(spawner.m_name))
								{
									log.Info($"Backing up: {spawner.m_name} - biomes: {spawner.m_biome}");
									originalBiomes[spawner.m_name] = (spawner.m_biome);
								}

								spawner.m_biome = newBiomeData.biome;
								log.Info($"Adjusted spawner {spawner.m_name} (key: {newBiomeData.globalKey}) to biomes {spawner.m_biome}");
							}
						}
						else if ((spawner.m_requiredGlobalKey ?? "") == newBiomeData.globalKey)
						{
							// Restore
							if (originalBiomes.TryGetValue(spawner.m_name, out var originalBiome))
							{
								spawner.m_biome = originalBiome;
								log.Info($"Restored {spawner.m_name} - biomes: {spawner.m_biome}");

								originalBiomes.Remove(spawner.m_name);
							}
						}
					}
				}
			}
		}

		private static void LogNighttimeSpawns(SpawnSystem spawnSystem)
		{
			// For logging
			foreach (SpawnSystemList spawnList in spawnSystem.m_spawnLists)
			{
				// Print the biomes this list covers
				string biomes = spawnList.m_biomeFolded != null && spawnList.m_biomeFolded.Count > 0
					? string.Join(", ", spawnList.m_biomeFolded)
					: "(none)";

				log.Info($"=== Spawn List (biomeFolded): {biomes} ===");

				log.Info($"| {"Enemy",-25} | {"Biome",-30} | {"Day?",-5} | {"Night?",-7} | {"Chance",-6} | {"Key",-20} |");
				log.Info(new string('-', 110));

				foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
				{
					string biome = spawner.m_biome.ToString();
					string key = string.IsNullOrEmpty(spawner.m_requiredGlobalKey) ? "-" : spawner.m_requiredGlobalKey;

					log.Info(
						$"| {spawner.m_name,-25} | {biome,-30} | {spawner.m_spawnAtDay,-5} | {spawner.m_spawnAtNight,-7} | {spawner.m_spawnChance,-6:F0}% | {key,-20} |"
					);
				}

				log.Info(""); // blank line for spacing
			}
		}
	}
}
