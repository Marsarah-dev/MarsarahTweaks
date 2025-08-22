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
		private static readonly Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)> creatureSpawnBackups	= new Dictionary<string, (int?, float?, float?)>();

		private static readonly Dictionary <string, Dictionary <string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>> creatureSpawnChanges = new Dictionary<string, Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>>()
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
					{ "Boar", (null, 20f, 0f) },
					{ "Neck lakes", (null, 20f, 0f) },
					{ "Neck IN RAIN", (null, 20f, 0f) },
					{ "Greyling", (3, 20f, null) },

					// Black Forest
					{ "greydwarf DAY", (null, 20f, null) },
					{ "greydwarf Night", (null, 20f, null) },					
					{ "greydwarf ELITE", (null, 20f, 0f) },
					{ "Troll", (null, 20f, 0f) },					
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

					// Swamp
					{ "Marsh draugr", (null, 20f, null) },
					{ "Leech", (null, 20f, null) },
					{ "Skeleton", (3, 20f, null) },
					{ "Draugr", (3, 20f, null) },
					{ "Draugr Elite", (3, 20f, null) },
					{ "Marsh surtling", (3, 20f, null) },
					{ "Surtling", (3, 20f, null) }
				}
			},
			{ "Moder", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Black Forest
					{ "Troll", (null, 30f, null) },

					// Swamp
					{ "Skeleton", (null, 30f, null) },
					{ "Marsh draugr", (null, 30f, null) },
					{ "Draugr", (null, 30f, null) },
					{ "Draugr Elite", (null, 30f, null) },
					{ "Marsh surtling", (null, 30f, null) },
					{ "Surtling", (null, 30f, null) },

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
					{ "Dverger", (null, 30f, null) },
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
			//creatureSpawnBackups.Clear();

			// Reset boss state tracking
			lastEikthyrDefeated = GlobalKeyChecker.IsBossDefeated("Eikthyr");
			lastElderDefeated = GlobalKeyChecker.IsBossDefeated("The Elder");
			lastBonemassDefeated = GlobalKeyChecker.IsBossDefeated("Bonemass");
			lastModerDefeated = GlobalKeyChecker.IsBossDefeated("Moder");
			lastYagluthDefeated = GlobalKeyChecker.IsBossDefeated("Yagluth");
			lastQueenDefeated = GlobalKeyChecker.IsBossDefeated("The Queen");
			lastFaderDefeated = GlobalKeyChecker.IsBossDefeated("Fader");

			//MarsarahTweaks.LogInfo("[Creatue Unleveler] New world load reset done.");
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
					//MarsarahTweaks.LogInfo("Initial creature spawn changes applied.");
					ApplyCreatureLevelChanges(__instance);
					hasAppliedSpawnChangesOnce = true;
				}

				// Run again if any boss state changes
				checkTimer += Time.deltaTime;
				if (checkTimer < 2f) return; // Only check every 2 seconds
				checkTimer = 0f;

				if (BossStateChanged())
				{
					//MarsarahTweaks.LogInfo("Boss state changed, reapplying spawn changes");
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
			foreach (SpawnSystemList spawnList in __instance.m_spawnLists)
			{
				foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
				{
					string spawnerName = spawner.m_name;

					// Find all bosses that define changes for this creature
					List<string> affectingBosses = creatureSpawnChanges
						.Where(kvp => kvp.Value.ContainsKey(spawnerName))
						.Select(kvp => kvp.Key)
						.ToList();

					if (affectingBosses.Count == 0) continue;

					// Sort by progression order
					affectingBosses.Sort((a, b) => BossProgressionOrder.IndexOf(a).CompareTo(BossProgressionOrder.IndexOf(b)));

					// Find the highest defeated boss
					string highestDefeatedBoss = affectingBosses.LastOrDefault(GlobalKeyChecker.IsBossDefeated);

					if (ConfigManager.CreatureUnlevelerEnabled.Value)
					{
						if (highestDefeatedBoss != null && highestDefeatedBoss != "")
						{
							//MarsarahTweaks.LogInfo($"Applying changes to: {spawnerName} - defeated boss: {highestDefeatedBoss}");
							var changes = creatureSpawnChanges[highestDefeatedBoss][spawnerName];

							if (!creatureSpawnBackups.ContainsKey(spawnerName))
							{
								creatureSpawnBackups[spawnerName] = (
									spawner.m_maxLevel,
									spawner.m_overrideLevelupChance,
									spawner.m_levelUpMinCenterDistance
								);
							}

							if (changes.levelUpChance.HasValue)
								spawner.m_overrideLevelupChance = changes.levelUpChance.Value;

							if (changes.levelUpMinCenterDistance.HasValue)
								spawner.m_levelUpMinCenterDistance = changes.levelUpMinCenterDistance.Value;

							if (changes.maxLevel.HasValue)
								spawner.m_maxLevel = changes.maxLevel.Value;
						}
						else
						{
							if (creatureSpawnBackups.TryGetValue(spawnerName, out var backup))
							{
								//MarsarahTweaks.LogInfo($"(Enabled) Restoring changes for: {spawnerName} - defeated boss: {highestDefeatedBoss}");
								spawner.m_maxLevel = backup.maxLevel ?? spawner.m_maxLevel;
								spawner.m_overrideLevelupChance = backup.levelUpChance ?? spawner.m_overrideLevelupChance;
								spawner.m_levelUpMinCenterDistance = backup.levelUpMinCenterDistance ?? spawner.m_levelUpMinCenterDistance;

								creatureSpawnBackups.Remove(spawnerName);
							}
						}
					}
					else
					{
						if (creatureSpawnBackups.TryGetValue(spawnerName, out var backup))
						{
							//MarsarahTweaks.LogInfo($"(Disabled) Restoring changes for: {spawnerName} - defeated boss: {highestDefeatedBoss}");
							spawner.m_maxLevel = backup.maxLevel ?? spawner.m_maxLevel;
							spawner.m_overrideLevelupChance = backup.levelUpChance ?? spawner.m_overrideLevelupChance;
							spawner.m_levelUpMinCenterDistance = backup.levelUpMinCenterDistance ?? spawner.m_levelUpMinCenterDistance;

							creatureSpawnBackups.Remove(spawnerName);
						}
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
	}
}
