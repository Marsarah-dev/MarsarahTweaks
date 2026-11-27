using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Features
{
	internal class CreatureUnleveler
	{
		private static readonly LogManager log = new LogManager("Creature Unleveler", LogManager.LogLevel.Warning);

		private static readonly Dictionary<SpawnSystem.SpawnData, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)> creatureSpawnBackups = new Dictionary<SpawnSystem.SpawnData, (int?, float?, float?)>();
		private static readonly Dictionary<string, Dictionary <string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>> creatureSpawnChanges = new Dictionary<string, Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>>()
		{
			{ "Eikthyr", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Meadows
					{ "deer", (null, 15f, null) },
					{ "Boar", (null, 15f, 400f) },
					{ "Neck lakes", (null, 15f, 400f) },
					{ "Neck IN RAIN", (null, 15f, 400f) },
					{ "Greyling", (2, 15f, null) }
				}
			},
			{ "The Elder", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Meadows
					{ "deer", (null, 20f, null) },
					{ "Boar", (null, 20f, 0f) },
					{ "Neck lakes", (null, 20f, 0f) },
					{ "Neck IN RAIN", (null, 20f, 0f) },
					{ "Greyling", (3, 20f, null) },

					// Black Forest
					{ "greydwarf DAY", (null, 20f, null) },
					{ "greydwarf Night", (null, 20f, null) },					
					{ "greydwarf ELITE", (null, 20f, 0f) },
					{ "Troll", (null, 20f, 0f) },
					{ "Bjorn DAY", (null, 20f, null) },
					{ "Bjorn NIGHT", (null, 20f, null) },
					{ "greydwarf After boss", (3, 20f, null) },
					{ "Greydwarf Elite", (3, 20f, null) },
					{ "Greydwarf Shaman", (3, 20f, null) },
					{ "Greydwarf", (3, 20f, null) }
				}
			},
			{ "Bonemass", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Meadows
					{ "Boar", (null, 30f, null) },
					{ "Neck lakes", (null, 30f, null) },
					{ "Neck IN RAIN", (null, 30f, null) },
					{ "Greyling", (null, 30f, null) },

					// Black Forest
					{ "greydwarf ELITE", (null, 30f, null) },
					{ "greydwarf DAY", (null, 30f, null) },
					{ "greydwarf Night", (null, 30f, null) },
					{ "greydwarf After boss", (null, 30f, null) },
					{ "Greydwarf Elite", (null, 30f, null) },
					{ "Greydwarf Shaman", (null, 30f, null) },
					{ "Greydwarf", (null, 30f, null) },
					{ "Bjorn DAY", (null, 30f, null) },
					{ "Bjorn NIGHT", (null, 30f, null) },

					// Swamp
					{ "Marsh draugr", (null, 20f, null) },
					{ "Leech", (null, 20f, null) },
					{ "Skeleton", (3, 20f, null) },
					{ "Draugr", (3, 20f, null) },
					{ "Draugr Elite", (3, 20f, null) },
					{ "Marsh surtling", (3, 20f, null) },
					//{ "Surtling", (3, 20f, null) },
					{ "Writhan", (null, 20f, null) }
				}
			},
			{ "Moder", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Black Forest
					{ "Troll", (null, 30f, null) },

					// Swamp
					{ "Skeleton", (null, 30f, null) },
					{ "Marsh draugr", (null, 30f, null) },
					{ "Leech", (null, 30f, null) },
					{ "Draugr", (null, 30f, null) },
					{ "Draugr Elite", (null, 30f, null) },
					{ "Marsh surtling", (null, 30f, null) },
					//{ "Surtling", (null, 30f, null) },
					{ "Writhan", (null, 30f, null) },

					// Mountain
					{ "Fenring", (2, 10f, null) },
					{ "Wolf", (3, 20f, null) }
				}
			},
			{ "Yagluth", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Swamp
					{ "Blob", (2, 20f, null) },
					{ "BlobElite", (2, 20f, null) },
					{ "Wraith", (2, 20f, null) },
					{ "Abomination", (2, 20f, null) },

					// Mountain
					{ "StoneGolem", (2, 20f, null) },
					{ "Hatchling", (2, 20f, null) },
					{ "Fenring", (3, 20f, null) },
					{ "Wolf", (null, 30f, null) },

					// Plains
					{ "GoblinBrute", (null, 20f, null) },
					{ "Goblin", (3, 20f, null) }
					
				}
			},
			{ "The Queen", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Plains
					{ "Lox", (2, 20f, null) },
					{ "Unbjorn", (2, 20f, null) },
					{ "Deathsquito", (2, 20f, null) },
					{ "Goblin", (null, 30f, null) },
					{ "GoblinBrute", (null, 30f, null) },

					// Mistlands
					{ "Dverger", (null, 20f, null) },
					{ "Seeker Brute", (null, 20f, null) },
					{ "Tick defeated queen other biomes", (null, 20f, null) },
					{ "Seeker defeated queen other biomes", (2, 10f, null) },
					{ "SeekerBrood  defeated queen other biomes", (2, 10f, null) },
					{ "Seeker", (3, 20f, null) }					
				}
			},
			{ "Fader", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Mistlands
					{ "Dverger", (null, 30f, null) },
					{ "Seeker", (3, 30f, null) },
					{ "Seeker Brute", (null, 25f, null) },
					{ "Gjall", (null, 15f, null) },
					{ "Hare", (null, 20f, null) },
					{ "Tick defeated queen other biomes", (null, 30f, null) },
					{ "Seeker defeated queen other biomes", (2, 20f, null) },
					{ "SeekerBrood  defeated queen other biomes", (2, 20f, null) },

					// Ashlands
					{ "Charred Melee", (null, 15f, null) },
					{ "Charred Archer", (null, 15f, null) },
					{ "Charred Twitcher [DAY]", (null, 15f, null) },
					{ "Charred Twitcher [NIGHT]", (null, 15f, null) },
					{ "Asksvin [DAY]", (null, 15f, null) },
					{ "Asksvin [NIGHT]", (null, 15f, null) },
					{ "Volture", (2, 10f, null) },

					// Other biomes
					{ "Charred Melee [Other biomes when Fader is defeated]", (2, 10f, null) },
					{ "Charred Archer [Other biomes when Fader is defeated]", (2, 10f, null) }
				}
			}
		};

		// Tracking last defeated states to determine when changes occur
		private static bool lastEikthyrDefeated = GlobalKeyChecker.IsBossDefeated("Eikthyr");
		private static bool lastElderDefeated = GlobalKeyChecker.IsBossDefeated("The Elder");
		private static bool lastBonemassDefeated = GlobalKeyChecker.IsBossDefeated("Bonemass");
		private static bool lastModerDefeated = GlobalKeyChecker.IsBossDefeated("Moder");
		private static bool lastYagluthDefeated = GlobalKeyChecker.IsBossDefeated("Yagluth");
		private static bool lastQueenDefeated = GlobalKeyChecker.IsBossDefeated("The Queen");
		private static bool lastFaderDefeated = GlobalKeyChecker.IsBossDefeated("Fader");

		// Track whether changes have been applied
		private static bool hasAppliedSpawnChangesOnce = false;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class WorldChangeReset_Patch
		{
			static void Prefix()
			{
				ResetCreatureUnlevelerState();
			}
		}

		public static void ResetCreatureUnlevelerState()
		{
			hasAppliedSpawnChangesOnce = false;
			creatureSpawnBackups.Clear();

			// Reset boss state tracking
			lastEikthyrDefeated = GlobalKeyChecker.IsBossDefeated("Eikthyr");
			lastElderDefeated = GlobalKeyChecker.IsBossDefeated("The Elder");
			lastBonemassDefeated = GlobalKeyChecker.IsBossDefeated("Bonemass");
			lastModerDefeated = GlobalKeyChecker.IsBossDefeated("Moder");
			lastYagluthDefeated = GlobalKeyChecker.IsBossDefeated("Yagluth");
			lastQueenDefeated = GlobalKeyChecker.IsBossDefeated("The Queen");
			lastFaderDefeated = GlobalKeyChecker.IsBossDefeated("Fader");

			log.Info("New world load reset done.");
		}


		[HarmonyPatch(typeof(SpawnSystem), "Awake")] // UpdateSpawning
		class DynamicBossChangeWatcher_Patch
		{
			private static float checkTimer = 0f;

			private static void Postfix(SpawnSystem __instance)
			{
				// Run on both server and client - no checks made

				// Run once when the game/server starts
				if (!hasAppliedSpawnChangesOnce)
				{
					log.Info("Initial creature spawn changes applied.");
					ApplyCreatureLevelChanges(__instance);
					//LogAltBiomeLevelChances();
					hasAppliedSpawnChangesOnce = true;
				}

				// Run again if any boss state changes
				checkTimer += Time.deltaTime;
				if (checkTimer < 2f) return; // Only check every 2 seconds
				checkTimer = 0f;

				if (BossStateChanged())
				{
					log.Info("Boss state changed, reapplying spawn changes");
					ApplyCreatureLevelChanges(__instance);
				}
			}
		}

		private static readonly List<string> BossProgressionOrder = new List<string>()
		{
			"Eikthyr", "The Elder", "Bonemass", "Moder", "Yagluth", "The Queen", "Fader"
		};

		public static void ApplyCreatureLevelChanges(SpawnSystem __instance)
		{
			if (__instance == null) return;

			bool enabled = ConfigManager.CreatureUnlevelerEnabled.Value;

			// Get all defeated bosses in progression order
			List<string> defeatedBosses = BossProgressionOrder
				.Where(boss => enabled && GlobalKeyChecker.IsBossDefeated(boss))
				.ToList();

			foreach (SpawnSystemList spawnList in __instance.m_spawnLists)
			{
				if (spawnList == null) continue;

				foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
				{
					if (spawner == null) continue;

					string spawnerName = spawner.m_name;

					// Get the original values, or use the current values if no backup exists
					bool hasBackup = creatureSpawnBackups.TryGetValue(spawner, out var original);

					if (!hasBackup)
					{
						original = (
							spawner.m_maxLevel,
							spawner.m_overrideLevelupChance,
							spawner.m_levelUpMinCenterDistance
						);
					}

					// Start from the original values
					int maxLevel = original.maxLevel ?? spawner.m_maxLevel;
					float levelUpChance = original.levelUpChance ?? spawner.m_overrideLevelupChance;
					float levelUpMinCenterDistance = original.levelUpMinCenterDistance ?? spawner.m_levelUpMinCenterDistance;

					string highestDefeatedBoss = null;

					// Apply cumulative changes in boss progression order
					foreach (string boss in defeatedBosses)
					{
						if (!creatureSpawnChanges.TryGetValue(boss, out var bossChanges)) continue;
						if (!bossChanges.TryGetValue(spawnerName, out var changes)) continue;

						maxLevel = changes.maxLevel ?? maxLevel;
						levelUpChance = changes.levelUpChance ?? levelUpChance;
						levelUpMinCenterDistance = changes.levelUpMinCenterDistance ?? levelUpMinCenterDistance;

						highestDefeatedBoss = boss;
					}

					// No applicable changes and nothing to restore
					if (highestDefeatedBoss == null && !hasBackup) continue;

					// Backup original values before applying changes
					if (highestDefeatedBoss != null && !hasBackup)
					{
						creatureSpawnBackups[spawner] = original;
					}

					// Apply the final calculated values
					spawner.m_maxLevel = maxLevel;
					spawner.m_overrideLevelupChance = levelUpChance;
					spawner.m_levelUpMinCenterDistance = levelUpMinCenterDistance;

					if (highestDefeatedBoss != null)
					{
						log.Info($"Spawner: {spawnerName} | Biome: {spawner.m_biome} | Boss: {highestDefeatedBoss} | Max Level: {original.maxLevel} -> {spawner.m_maxLevel} | Level Chance: {original.levelUpChance}% -> {spawner.m_overrideLevelupChance}% | Min Distance: {original.levelUpMinCenterDistance} -> {spawner.m_levelUpMinCenterDistance}");
					}
					else
					{
						// Restore original values when disabled or no relevant boss is defeated
						creatureSpawnBackups.Remove(spawner);

						log.Info($"Restored spawner: {spawnerName} | Biome: {spawner.m_biome} | Max Level: {spawner.m_maxLevel} | Level Chance: {spawner.m_overrideLevelupChance}% | Min Distance: {spawner.m_levelUpMinCenterDistance}");
					}
				}
			}
		}


		private static bool BossStateChanged()
		{
			bool changed = false;

			if (GlobalKeyChecker.EikthyrDefeated != lastEikthyrDefeated)
			{
				lastEikthyrDefeated = GlobalKeyChecker.EikthyrDefeated;
				changed = true;
			}
			if (GlobalKeyChecker.ElderDefeated != lastElderDefeated)
			{
				lastElderDefeated = GlobalKeyChecker.ElderDefeated;
				changed = true;
			}
			if (GlobalKeyChecker.BonemassDefeated != lastBonemassDefeated)
			{
				lastBonemassDefeated = GlobalKeyChecker.BonemassDefeated;
				changed = true;
			}
			if (GlobalKeyChecker.ModerDefeated != lastModerDefeated)
			{
				lastModerDefeated = GlobalKeyChecker.ModerDefeated;
				changed = true;
			}
			if (GlobalKeyChecker.YagluthDefeated != lastYagluthDefeated)
			{
				lastYagluthDefeated = GlobalKeyChecker.YagluthDefeated;
				changed = true;
			}
			if (GlobalKeyChecker.QueenDefeated != lastQueenDefeated)
			{
				lastQueenDefeated = GlobalKeyChecker.QueenDefeated;
				changed = true;
			}
			if (GlobalKeyChecker.FaderDefeated != lastFaderDefeated)
			{
				lastFaderDefeated = GlobalKeyChecker.FaderDefeated;
				changed = true;
			}

			return changed;
		}

		/*private static void LogAltBiomeLevelChances()
		{
			log.Info("=== Alternate Biome Level-Up Chances ===");

			foreach (AltBiome altBiome in AltBiomeList.m_altBiomes)
			{
				log.Info(
					$"Alt Biome: {altBiome.m_name} | " +
					$"Biome: {altBiome.m_biome} | " +
					$"Level Chance Multiplier: {altBiome.m_levelUpChanceMultiplier}x | " +
					$"10% Base Result: {10f * altBiome.m_levelUpChanceMultiplier}%"
				);
			}

			log.Info("=== End Alternate Biome Level-Up Chances ===");
		}*/
	}
}
