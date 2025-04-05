using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

		[HarmonyPatch(typeof(SpawnSystem), "Awake")] // UpdateSpawning
		class DynamicBossChangeWatcher_Patch
		{
			private static float checkTimer = 0f;
			private static bool hasAppliedSpawnChangesOnce = false;

			private static void Postfix(SpawnSystem __instance)
			{
				// Run on both server and client - no checks made

				// Run once when the game/server starts
				if (!hasAppliedSpawnChangesOnce)
				{
					//MarsarahTweaks.MLog("Initial creature spawn changes applied.");
					ApplyCreatureLevelChanges(__instance);
					hasAppliedSpawnChangesOnce = true;
				}

				// Run again if any boss state changes
				checkTimer += Time.deltaTime;
				if (checkTimer < 2f) return; // Only check every 2 seconds
				checkTimer = 0f;

				if (BossStateChanged())
				{
					//MarsarahTweaks.MLog("Boss state changed, reapplying spawn changes");
					ApplyCreatureLevelChanges(__instance);
				}
			}
		}

		public static void ApplyCreatureLevelChanges(SpawnSystem __instance)
		{
			foreach (SpawnSystemList spawnList in __instance.m_spawnLists)
			{
				foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
				{
					foreach (var bossEntry in creatureSpawnChanges)
					{
						if (!GlobalKeyChecker.IsBossDefeated(bossEntry.Key)) continue;

						if (bossEntry.Value.TryGetValue(spawner.m_name, out var changes))
						{
							if (ConfigManager.CreatureUnlevelerEnabled.Value)
							{
								// Backup if not already backed up
								if (!creatureSpawnBackups.ContainsKey(spawner.m_name))
								{
									//MarsarahTweaks.MLog($"Backing up {spawner.m_name} (Boss: {bossEntry.Key})");
									creatureSpawnBackups[spawner.m_name] = (spawner.m_maxLevel,	spawner.m_overrideLevelupChance, spawner.m_levelUpMinCenterDistance);
								}

								// Apply changes
								if (changes.levelUpChance.HasValue)
								{
									//MarsarahTweaks.MLog($"Applying levelUpChance={changes.levelUpChance.Value} to {spawner.m_name} (Boss: {bossEntry.Key})");
									spawner.m_overrideLevelupChance = changes.levelUpChance.Value;
								}
								if (changes.levelUpMinCenterDistance.HasValue)
								{
									//MarsarahTweaks.MLog($"Applying levelUpMinCenterDistance={changes.levelUpMinCenterDistance.Value} to {spawner.m_name} (Boss: {bossEntry.Key})");
									spawner.m_levelUpMinCenterDistance = changes.levelUpMinCenterDistance.Value;
								}
								if (changes.maxLevel.HasValue)
								{
									//MarsarahTweaks.MLog($"Applying maxLevel={changes.maxLevel.Value} to {spawner.m_name} (Boss: {bossEntry.Key})");
									spawner.m_maxLevel = changes.maxLevel.Value;
								}
							}
							else
							{
								// Restore from backup if exists
								if (creatureSpawnBackups.TryGetValue(spawner.m_name, out var backup))
								{
									//MarsarahTweaks.MLog($"Restoring spawn values for {spawner.m_name} from backup");

									spawner.m_maxLevel = backup.maxLevel ?? spawner.m_maxLevel;
									spawner.m_overrideLevelupChance = backup.levelUpChance ?? spawner.m_overrideLevelupChance;
									spawner.m_levelUpMinCenterDistance = backup.levelUpMinCenterDistance ?? spawner.m_levelUpMinCenterDistance;

									creatureSpawnBackups.Remove(spawner.m_name);
								}
							}
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
