using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class CreatureUnleveler
	{
		private static Dictionary <string, Dictionary <string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>> creatureSpawnChanges = new Dictionary<string, Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>>()
		{
			{ "eikthyrDefeated", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					// Meadows
					{ "deer", (null, 15f, null) },
					{ "Boar", (null, 15f, 400f) },
					{ "Neck lakes", (null, 15f, 400f) },
					{ "Neck IN RAIN", (null, 15f, 400f) },
					{ "Greyling", (2, 15f, null) }
				}
			},
			{ "elderDefeated", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
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
			{ "bonemassDefeated", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
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
			{ "moderDefeated", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
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
			{ "yagluthDefeated", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
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
			{ "queenDefeated", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
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
			{ "faderDefeated", new Dictionary<string, (int? maxLevel, float? levelUpChance, float? levelUpMinCenterDistance)>
				{
					{ "Dverger", (null, 30f, null) },
					{ "Charred Melee [Other biomes when Fader is defeated]", (2, 10f, null) },
					{ "Charred Archer [Other biomes when Fader is defeated]", (2, 10f, null) }
				}
			}
		};

		[HarmonyPatch(typeof(SpawnSystem), "Awake")]
		class RemoveMinimumDistanceForStars
		{
			static void Postfix(SpawnSystem __instance)
			{
				if (!ZNet.instance || !ZNet.instance.IsServer())
				{
					MarsarahTweaks.MLog($"SpawnSystem Awake - I am a client - no changes made to {ConfigManager.Configs.CreatureUnleveler.Name}");
					return; // Ensure it only runs on the server
				}

				if (__instance != null && ConfigManager.creatureUnlevelerEnabled.Value)
				{
					// Apply the changes based on the defeated bosses.
					foreach (SpawnSystemList spawnList in __instance.m_spawnLists)
					{
						foreach (SpawnSystem.SpawnData spawner in spawnList.m_spawners)
						{
							// Iterate through the bosses and check if they are defeated.
							foreach (var bossEntry in creatureSpawnChanges)
							{
								string bossName = bossEntry.Key;
								bool bossDefeated = false;

								// Check if the boss is defeated by looking up the key in the global keys or a specific flag.
								switch (bossName)
								{
									case "eikthyrDefeated":
										bossDefeated = GlobalKeyChecker.eikthyrDefeated;
										break;
									case "elderDefeated":
										bossDefeated = GlobalKeyChecker.elderDefeated;
										break;
									case "bonemassDefeated":
										bossDefeated = GlobalKeyChecker.bonemassDefeated;
										break;
									case "moderDefeated":
										bossDefeated = GlobalKeyChecker.moderDefeated;
										break;
									case "yagluthDefeated":
										bossDefeated = GlobalKeyChecker.yagluthDefeated;
										break;
									case "queenDefeated":
										bossDefeated = GlobalKeyChecker.queenDefeated;
										break;
									case "faderDefeated":
										bossDefeated = GlobalKeyChecker.faderDefeated;
										break;
								}

								// If the boss is defeated, apply the changes.
								if (bossDefeated && creatureSpawnChanges.TryGetValue(bossName, out var creatureChanges))
								{
									if (creatureChanges.TryGetValue(spawner.m_name, out var changes))
									{
										// Apply the changes to the spawn data
										if (changes.levelUpChance.HasValue)
											spawner.m_overrideLevelupChance = changes.levelUpChance.Value;
										if (changes.levelUpMinCenterDistance.HasValue)
											spawner.m_levelUpMinCenterDistance = changes.levelUpMinCenterDistance.Value;
										if (changes.maxLevel.HasValue)
											spawner.m_maxLevel = changes.maxLevel.Value;
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
