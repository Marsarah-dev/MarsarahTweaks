using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class ProgressionHalt
	{
		[HarmonyPatch(typeof(Piece), "DropResources")]
		class ProgressionHaltPiece_Patch
		{
			// Create a dictionary to map bosses to resources that need to be prevented
			private static readonly Dictionary<string, List<string>> pieceResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "Eikthyr", new List<string> { "piece_chair", "piece_chair02", "piece_table", "wood_pole_log", "wood_wall_log" } }, // wood_pole_log_4 and wood_wall_log_4x0.5 should be taken care of
				{ "The Elder", new List<string> { "iron", "dungeon_sunkencrypt_irongate" } },
				{ "Yagluth", new List<string> { "blackmarble", "piece_dvergr", "dvergrprops", "dvergrtown", "dverger_guardstone" } },
				{ "The Queen", new List<string> { "Piece_grausten", "Ashlands", "piece_blackwood_bench" } }
			};

			static bool Prefix(Piece __instance)
			{
				if (ConfigManager.automaticProgressionHaltEnabled.Value)
				{
					if (__instance.IsPlacedByPlayer())
					{
						return true;
					}

					//return PreventDrop(__instance.name, pieceResourceRestrictions, "Piece");
					foreach (var bossEntry in pieceResourceRestrictions)
					{
						string bossName = bossEntry.Key;
						List<string> restrictedResources = bossEntry.Value;

						bool bossDefeated = GlobalKeyChecker.isBossDefeated(bossName);

						if (!bossDefeated)
						{
							foreach (var resource in restrictedResources)
							{
								if (__instance.name.StartsWith(resource))
								{
									//MarsarahTweaks.MLog($"Progression Halt: Prevented Piece resource drop {__instance.name} because {bossName} has not been defeated.");
									return false; // Prevent dropping
								}
							}
						}
					}

					return true; // Allow dropping if none of the conditions match
				}

				return true; // Default return value
			}
		}

		[HarmonyPatch(typeof(Container), "Interact")]
		public static class ContainerInteract_Patch
		{
			private static readonly Dictionary<string, List<string>> chestResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "Eikthyr", new List<string> { "TreasureChest_blackforest", "TreasureChest_forestcrypt", "TreasureChest_trollcave" } },
				{ "The Elder", new List<string> { "TreasureChest_swamp", "TreasureChest_sunkencrypt" } },
				{ "Bonemass", new List<string> { "TreasureChest_mountains", "TreasureChest_mountaincave" } },
				{ "Moder", new List<string> { "TreasureChest_heath", "TreasureChest_plains_stone" } },
				{ "Yagluth", new List<string> { "TreasureChest_dvergrtower", "TreasureChest_dvergrtown" } },
				{ "The Queen", new List<string> { "TreasureChest_charredfortress", "TreasureChest_ashland_stone" } }
			};

			static bool Prefix(Container __instance, Humanoid character, bool hold, bool alt, ref bool __result)
			{
				if (hold) return true; // Allow normal behavior for hold interactions

				if (!ConfigManager.automaticProgressionHaltEnabled.Value) return true; // Skip if disabled

				string chestName = __instance.name.Replace("(Clone)", "").Trim();
				foreach (var restriction in chestResourceRestrictions)
				{
					string bossName = restriction.Key;
					List<string> restrictedChests = restriction.Value;
					bool bossDefeated = GlobalKeyChecker.isBossDefeated(bossName);

					if (!bossDefeated && restrictedChests.Contains(chestName))
					{
						// Prevent interaction and show message
						character.Message(MessageHud.MessageType.Center, $"This chest is magically sealed by {bossName}.");
						__result = false;
						return false;
					}
				}
				return true; // Allow normal behavior for unrestricted chests
			}
		}


		[HarmonyPatch(typeof(Pickable), "Interact")]
		public static class PickableInteract_Patch
		{
			private static readonly Dictionary<string, List<string>> pickableResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "Eikthyr", new List<string> 
					{ 
						"Pickable_Carrot", 
						"Pickable_SeedCarrot",
						"Pickable_Thistle",
						"Pickable_ForestCryptRemains01", 
						"Pickable_ForestCryptRemains02", 
						"Pickable_ForestCryptRemains03", 
						"Pickable_ForestCryptRemains04",
						"BlueberryBush",
						"Pickable_Mushroom_yellow",
						"Pickable_SurtlingCoreStand",
						"Pickable_Tin"
					} 
				},
				{ "The Elder", new List<string> 
					{
						"Pickable_Turnip",
						"Pickable_SeedTurnip"
					} 
				},
				{ "Bonemass", new List<string> 
					{
						"Pickable_DragonEgg",
						"Pickable_MountainCaveCrystal",
						"Pickable_MountainCaveObsidian",
						"Pickable_MountainRemains01_buried",
						"Pickable_Hairstrands01",
						"Pickable_Hairstrands02",
						"Pickable_MeatPile",
						"Pickable_Obsidian",
						"Pickable_Onion",
						"Pickable_SeedOnion",
						"hanging_hairstrands"
					} 
				},
				{  "Moder", new List<string>
					{
						"CloudberryBush",
						"Pickable_Barley",
						"Pickable_Barley_Wild",
						"Pickable_Flax",
						"Pickable_Flax_Wild",
						"Pickable_Tar",
						"Pickable_TarBig",
						"goblin_totempole"
					}
				},
				{ "Yagluth", new List<string> 
					{
						"Pickable_DvergerThing",
						"Pickable_DvergrLantern",
						"Pickable_DvergrMineTreasure",
						"Pickable_DvergrStein",
						"Pickable_Mushroom_JotunPuffs",
						"Pickable_Mushroom_Magecap",
						"Pickable_RoyalJelly",
						"Pickable_BlackCoreStand",
					} 
				},
				{ "The Queen", new List<string> 
					{
						"VineAsh",
						"Pickable_Ashstone",
						"Pickable_Charredskull",
						"Pickable_Fiddlehead",
						"Pickable_SmokePuff",
						"Pickable_Meteorite",
						"Pickable_MoltenCoreStand",
						"Pickable_SulfurRock",
						"Pickable_VoltureEgg"
					} 
				}
			};

			static bool Prefix(Pickable __instance, Humanoid character, ref bool __result)
			{
				if (!ConfigManager.automaticProgressionHaltEnabled.Value) return true;

				// Access private fields via reflection
				FieldInfo nviewField = typeof(Pickable).GetField("m_nview", BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo enabledField = typeof(Pickable).GetField("m_enabled", BindingFlags.NonPublic | BindingFlags.Instance);

				if (nviewField == null || enabledField == null)
				{
					MarsarahTweaks.MLog("[ERROR] Could not access private fields in Pickable!");
					return true;
				}

				ZNetView nview = (ZNetView)nviewField.GetValue(__instance);
				int enabled = (int)enabledField.GetValue(__instance);

				if (!nview.IsValid() || enabled == 0)
				{
					return true;
				}

				//string pickableName = __instance.name;
				string pickableName = __instance.name.Replace("(Clone)", "").Trim();

				foreach (var restriction in pickableResourceRestrictions)
				{

					string bossName = restriction.Key;
					List<string> restrictedPickables = restriction.Value;

					bool bossDefeated = GlobalKeyChecker.isBossDefeated(bossName);

					if (!bossDefeated && restrictedPickables.Contains(pickableName))
					{
						character.Message(MessageHud.MessageType.Center, $"{bossName} has a strong hold on this object");
						__result = false;
						return false;
					}
				}

				return true; // Allow original method
			}
		}

		[HarmonyPatch(typeof(PickableItem), "Interact")]
		public static class PickableItemInteract_Patch
		{
			private static readonly Dictionary<string, List<string>> pickableItemResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "Eikthyr", new List<string> {	"Pickable_ForestCryptRandom" } },
				{ "The Elder", new List<string> { "Pickable_SunkenCryptRandom" } },
				{ "Bonemass", new List<string> { "Pickable_MountainCaveRandom"	} }
			};

			static bool Prefix(Pickable __instance, Humanoid character, ref bool __result)
			{
				if (!ConfigManager.automaticProgressionHaltEnabled.Value) return true;

				// Access private fields via reflection
				FieldInfo nviewField = typeof(PickableItem).GetField("m_nview", BindingFlags.NonPublic | BindingFlags.Instance);

				if (nviewField == null)
				{
					MarsarahTweaks.MLog("[ERROR] Could not access private fields in PickableItem!");
					return true;
				}

				ZNetView nview = (ZNetView)nviewField.GetValue(__instance);

				if (!nview.IsValid())
				{
					return true;
				}

				//string pickableItemName = __instance.name;
				string pickableItemName = __instance.name.Replace("(Clone)", "").Trim();

				foreach (var restriction in pickableItemResourceRestrictions)
				{

					string bossName = restriction.Key;
					List<string> restrictedPickableItems = restriction.Value;

					bool bossDefeated = GlobalKeyChecker.isBossDefeated(bossName);

					if (!bossDefeated && restrictedPickableItems.Contains(pickableItemName))
					{
						character.Message(MessageHud.MessageType.Center, $"{bossName} has a strong hold on this object");
						__result = false;
						return false;
					}
				}

				return true; // Allow original method
			}
		}

		[HarmonyPatch(typeof(ZNetScene), "Update")]
		class ProgressionHalt_Patch
		{
			private static bool dropsSet = false;
			private static bool trophyDropsSet = false;

			// Backup dictionaries
			private static Dictionary<string, Dictionary<string, float>> mobDropChanceBackup = new Dictionary<string, Dictionary<string, float>>();
			private static Dictionary<string, float> mineDropBackup = new Dictionary<string, float>();
			private static Dictionary<string, float> mine5DropBackup = new Dictionary<string, float>();
			private static Dictionary<string, float> destroyedDropBackup = new Dictionary<string, float>();
			private static Dictionary<string, GameObject> destructibleDropBackup = new Dictionary<string, GameObject>();
			private static Dictionary<string, DropTable> treeLogDropBackup = new Dictionary<string, DropTable>();

			// Prefab dictionary
			private static readonly Dictionary<string, List<string>> bossPrefabHolds = new Dictionary<string, List<string>>()
			{
				{ 
					"Eikthyr", new List<string> 
					{
						"Greydwarf",
						"Greydwarf_Elite",
						"Greydwarf_Shaman",
						"Troll",
						"Skeleton",
						"Skeleton_Poison",
						"Pickable_Carrot",
						"Pickable_SeedCarrot",
						"BlueberryBush",
						"MineRock_Tin",
						"MineRock_Copper",
						"rock4_copper_frac",
						"Birch_log_half",
						"BirchStub",
						"Oak_log_half",
						"Pinetree_01_Stub",
						"PineTree_log_half",
						"BonePileSpawner",
						"Spawner_GreydwarfNest",
						"barrell"
					}
				},
				{
					"The Elder", new List<string>
					{
						"Leech",
						"Draugr",
						"Draugr_Elite",
						"Surtling",
						"Blob",
						"BlobElite",
						"Wraith",
						"Abomination",
						"sapling_turnip",
						"sapling_seedturnip",
						"Pickable_Turnip",
						"Pickable_SeedTurnip",
						"MineRock_Iron",
						"dungeon_sunkencrypt_irongate_rusty",
						"mudpile_frac",
						"mudpile2_frac",
						"mudpile_beacon",
						"SwampTree1_log",
						"Spawner_DraugrPile",
						"GuckSack",
						"GuckSack_small"
					}
				},
				{
					"Bonemass", new List<string>
					{
						"Wolf",
						"Ulv",
						"Fenring",
						"Fenring_Cultist",
						"Hatchling",
						"StoneGolem",
						"Serpent",
						"Leviathan",
						"Pickable_MountainCaveCrystal",
						"Pickable_MountainCaveObsidian",
						"Pickable_MeatPile",
						"Pickable_Onion",
						"Pickable_SeedOnion",
						"sapling_onion",
						"sapling_seedonion",
						"MineRock_Obsidian",
						"silvervein_frac",
						"rock3_silver_frac",
						"fenrirhide_hanging",
						"fenrirhide_hanging_door",
						"hanging_hairstrands",
						"cloth_hanging_door",
						"cloth_hanging_door_double",
						"cloth_hanging_long"
					}
				},
				{
					"Moder", new List<string>
					{
						"Goblin",
						"GoblinArcher",
						"GoblinBrute",
						"GoblinShaman",
						"Deathsquito",
						"Lox",
						"BlobTar",
						"CloudberryBush",
						"Pickable_Barley",
						"Pickable_Barley_Wild",
						"sapling_flax",
						"Pickable_Flax",
						"Pickable_Flax_Wild",
						"goblin_totempole"
					}
				},
				{
					"Yagluth", new List<string>
					{
						"Dverger",
						"DvergerMage",
						"Tick",
						"Seeker",
						"SeekerBrute",
						"SeekerBrood",
						"SeekerQueen",
						"Gjall",
						"Hare",
						"Pickable_DvergerThing",
						"Pickable_DvergrLantern",
						"Pickable_DvergrMineTreasure",
						"Pickable_DvergrStein",
						"Pickable_Mushroom_JotunPuffs",
						"Pickable_Mushroom_Magecap",
						"Pickable_RoyalJelly",
						"Pickable_BlackCoreStand",
						"sapling_jotunpuffs",
						"sapling_magecap",
						"giant_arm",
						"giant_brain_frac",
						"giant_helmet1_destruction",
						"giant_helmet2_destruction",
						"giant_ribs_frac",
						"giant_skull_frac",
						"giant_sword1_destruction",
						"giant_sword2_destruction",
						"yggashoot_log_half",
						"YggaShoot_small1",
						"blackmarble_post01",
						"trader_wagon_destructable",
						"blackmarble_altar_crystal"
					}
				},
				{
					"The Queen", new List<string>
					{
						"DvergerAshlands",
						"Charred_Archer",
						"Charred_Archer_Fader",
						"Charred_Melee",
						"Charred_Melee_Fader",
						"Charred_Twitcher",
						"Charred_Mage",
						"Morgen",
						"Morgen_NonSleeping",
						"Volture",
						"Asksvin",
						"FallenValkyrie",
						"BlobLava",
						"BonemawSerpent",
						"lavarock_ashlands1",
						"VineAsh",
						"Pickable_Ashstone",
						"Pickable_Charredskull",
						"Pickable_Fiddlehead",
						"Pickable_Meteorite",
						"Pickable_MoltenCoreStand",
						"Pickable_SmokePuff",
						"Pickable_VoltureEgg",
						"Pickable_SulfurRock",
						"FlametalRockstand",
						"FlametalRockstand_frac",
						"LeviathanLava",
						"dvergrprops_crate_ashlands",
						"AshlandsTreeLogHalf1",
						"AshlandsTreeLogHalf2",
						"AshlandsTreeStump1",
						"AshlandsTreeStump2",
						"AshlandsTreeStump3",
						"AshlandsBranch1",
						"AshlandsBranch2",
						"AshlandsBranch3",
						"AshlandsBush1",
						"AshlandsBush2",
						"Ashlands_rock1",
						"Ashlands_rock2",
						"MineRock_Meteorite",
						"UnstableLavaRock",
						"Spawner_CharredCross",
						"Spawner_CharredStone",
						"Spawner_CharredStone_Elite",
						"GraveStone_Broken_CharredTwitcherNest",
						"GraveStone_CharredTwitcherNest",
						"ashland_pot1_green",
						"ashland_pot1_red",
						"ashland_pot2_green",
						"ashland_pot2_red",
						"ashland_pot3_green",
						"ashland_pot3_red",
						"CharredBanner1",
						"CharredBanner2",
						"CharredBanner3",
						"Charred_altar_bellfragment",
						"piece_Charred_Balista",
						"Ashlands_Fortress_Floor",
						"Ashlands_Fortress_Wall_Pillar",
						"Ashlands_Fortress_Wall_Pillar_base",
						"Ashlands_Fortress_Wall_Pillar_base_frac",
						"Ashlands_Fortress_Wall_Pillar_frac",
						"Ashlands_Fortress_Wall_PillarTop",
						"Ashlands_Fortress_Wall_PillarTop_frac",
						"Ashlands_Fortress_Wall_PillarTopStone",
						"Ashlands_Fortress_Wall_PillarTopStone_frac",
						"Ashlands_Fortress_Wall_Spikes",
						"Ashlands_Ruins_Floor_1point5x1point5",
						"Ashlands_Ruins_Floor_1point5x1point5_broken",
						"Ashlands_Ruins_Floor_3x3",
						"Ashlands_Ruins_Floor_3x3_broken1",
						"Ashlands_Ruins_Floor_3x3_broken2",
						"Ashlands_Ruins_Floor_3x3_broken3",
						"Ashlands_Ruins_Floor_6x6",
						"Ashlands_Ruins_Floor_6x6_broken1",
						"Ashlands_Ruins_Floor_6x6_broken2",
						"Ashlands_Ruins_Ramp",
						"Ashlands_Ruins_Ramp_Upsidedown",
						"Ashlands_Ruins_TopStone",
						"Ashlands_Ruins_twist_ArchBig",
						"Ashlands_Ruins_twist_PillarBase",
						"Ashlands_Ruins_twist_PillarBaseSmall",
						"Ashlands_Ruins_Wall_4x6",
						"Ashlands_Ruins_Wall_Windows_Broken_4x6",
						"Ashland_Stair",
						"Ashland_Steepstair",
						"Ashlands_Altar",
						"Ashlands_Arch2_Broken1",
						"Ashlands_Arch2_Broken2",
						"Ashlands_ArchRoof",
						"Ashlands_ArchRoofDamaged",
						"Ashlands_ArchRoofDamaged_half1",
						"Ashlands_ArchRoofDamaged_half2",
						"Ashlands_ArchRoofLong_Damaged",
						"Ashlands_Boss_Pillar",
						"Ashlands_Boss_Pillar_Twist_broken1",
						"Ashlands_Boss_Pillar_Twist_broken2",
						"Ashlands_Boss_Pillar_Twist_broken3",
						"Ashlands_Floor",
						"Ashlands_floor_large",
						"Ashlands_floor_large_fractured",
						"Ashlands_Pillar4",
						"Ashlands_Pillar4_tip_broken1",
						"Ashlands_Pillar4_tip_broken2",
						"Ashlands_Pillar4_tip2_broken1",
						"Ashlands_Pillar4_tip2_broken2",
						"Ashlands_Pillar4_tip3_broken1",
						"Ashlands_Pillar4_tip3_broken2",
						"Ashlands_Pillar4_tip3_broken3",
						"Ashlands_PillarBase3_double",
						"Ashlands_WallBlock",
						"rock4_ashlands_frac",
						"cliff_ashlands_Arch_frac",
						"cliff_ashlands1_frac",
						"cliff_ashlands2_frac",
						"cliff_ashlands4_frac",
						"cliff_ashlands6_frac",
						"cliff_ashlands7_HalfArch_frac",
						"cliff_ashlandsflowrock_frac"
					}
				}
			};

			// Tracking last defeated states to determine when changes occur
			private static bool lastEikthyrDefeated = GlobalKeyChecker.isBossDefeated("Eikthyr");
			private static bool lastElderDefeated = GlobalKeyChecker.isBossDefeated("The Elder");
			private static bool lastBonemassDefeated = GlobalKeyChecker.isBossDefeated("Bonemass");
			private static bool lastModerDefeated = GlobalKeyChecker.isBossDefeated("Moder");
			private static bool lastYagluthDefeated = GlobalKeyChecker.isBossDefeated("Yagluth");
			private static bool lastQueenDefeated = GlobalKeyChecker.isBossDefeated("The Queen");

			private static bool lastProgressionHaltState = ConfigManager.automaticProgressionHaltEnabled.Value;
			private static bool lastTrophyDropsState = ConfigManager.betterTrophyDropsEnabled.Value;

			static void Postfix(ref ZNetScene __instance)
			{
				if (__instance == null) return;

				bool bossStateChanged = false;

				if (GlobalKeyChecker.eikthyrDefeated != lastEikthyrDefeated)
				{
					lastEikthyrDefeated = GlobalKeyChecker.eikthyrDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.elderDefeated != lastElderDefeated)
				{
					lastElderDefeated = GlobalKeyChecker.elderDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.bonemassDefeated != lastBonemassDefeated)
				{
					lastBonemassDefeated = GlobalKeyChecker.bonemassDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.moderDefeated != lastModerDefeated)
				{
					lastModerDefeated = GlobalKeyChecker.moderDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.yagluthDefeated != lastYagluthDefeated)
				{
					lastYagluthDefeated = GlobalKeyChecker.yagluthDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.queenDefeated != lastQueenDefeated)
				{
					lastQueenDefeated = GlobalKeyChecker.queenDefeated;
					bossStateChanged = true;
				}

				// Check if Progression Halt was toggled
				bool progressionHaltNowEnabled = ConfigManager.automaticProgressionHaltEnabled.Value;
				bool trophyDropsNowEnabled = ConfigManager.betterTrophyDropsEnabled.Value;
				if (progressionHaltNowEnabled != lastProgressionHaltState)
				{
					lastProgressionHaltState = progressionHaltNowEnabled;

					if (progressionHaltNowEnabled)
					{
						// Handle case Trophy Drops ON and Progression Halt toggled from OFF to ON
						// Restore Trophy Drops and then apply Progression Halt
						if (trophyDropsNowEnabled)
						{
							//MarsarahTweaks.MLog($"Restoring Trophy Drops Special");
							TrophyDropsChanges.restoreTrophyDrops(__instance);
						}

						// If Progression Halt was turned ON mid-game, run it without checking boss states or drops set
						//MarsarahTweaks.MLog($"Setting up Progression Halt due to re-enabling");
						handleProgressionHalt(__instance);
						dropsSet = true;
					}
					else
					{
						// If Progression Halt was turned OFF mid-game, restore original drops
						//MarsarahTweaks.MLog($"Restoring Progression Halt to default entirely");
						restoreProgressionHalt(__instance);

						// Run Trophy Drops here
						//MarsarahTweaks.MLog($"Setting up Trophy Drops due to Progression Halt being off");
						TrophyDropsChanges.updateTrophyDrops(__instance);
					}
				}

				// Progression Halt logic (only if enabled)
				if (progressionHaltNowEnabled)
				{
					if (!dropsSet || bossStateChanged)
					{
						//MarsarahTweaks.MLog($"Setting up Progression Halt standard way");
						handleProgressionHalt(__instance);
						dropsSet = true;
					}
				}

				// Check if Trophy Drops was toggled
				if (trophyDropsNowEnabled != lastTrophyDropsState)
				{
					lastTrophyDropsState = trophyDropsNowEnabled;

					// If Trophy Drops was toggled mid-game, run it without checking Progression Halt state (since it checks inside) or trophyDropsSet
					// This needs to be ran regardless if it's on or off
					//MarsarahTweaks.MLog($"Setting up Trophy Drops due to toggling");
					TrophyDropsChanges.updateTrophyDrops(__instance);
				}

				// Trophy Drops logic (ONLY run once on game start OR when Progression Halt is enabled and bosses change)
				if (!trophyDropsSet || (progressionHaltNowEnabled && bossStateChanged))
				{
					//MarsarahTweaks.MLog($"Setting up Trophy Drops standard way");
					TrophyDropsChanges.updateTrophyDrops(__instance);
					trophyDropsSet = true;
				}
			}

			// =======================================================================
			// Progression Halt patch handler
			private static void handleProgressionHalt (ZNetScene instance)
			{
				if (!GlobalKeyChecker.eikthyrDefeated)
				{
					haltDropsForBoss(instance, "Eikthyr");
				}
				else
				{
					restoreDropsForBoss(instance, "Eikthyr");
				}

				if (!GlobalKeyChecker.elderDefeated)
				{
					haltDropsForBoss(instance, "The Elder");
				}
				else
				{
					restoreDropsForBoss(instance, "The Elder");
				}

				if (!GlobalKeyChecker.bonemassDefeated)
				{
					haltDropsForBoss(instance, "Bonemass");
					haltDrops("MountainKit");
					haltDrops("mountainkit");
				}
				else
				{
					restoreDropsForBoss(instance, "Bonemass");
					restoreDrops("MountainKit");
					restoreDrops("mountainkit");
				}

				if (!GlobalKeyChecker.moderDefeated)
				{
					haltDropsForBoss(instance, "Moder");
				}
				else
				{
					restoreDropsForBoss(instance, "Moder");
				}

				if (!GlobalKeyChecker.yagluthDefeated)
				{
					haltDropsForBoss(instance, "Yagluth");
					haltDrops("dvergrprops");
					haltDrops("dvergrtown");
				}
				else
				{
					restoreDropsForBoss(instance, "Yagluth");
					restoreDrops("dvergrprops");
					restoreDrops("dvergrtown");
				}

				if (!GlobalKeyChecker.queenDefeated)
				{
					haltDropsForBoss(instance, "The Queen");
				}
				else
				{
					restoreDropsForBoss(instance, "The Queen");
				}
			}

			// Restore all Progression Halt data
			private static void  restoreProgressionHalt(ZNetScene instance)
			{
				restoreDropsForBoss(instance, "Eikthyr");
				restoreDropsForBoss(instance, "The Elder");
				restoreDropsForBoss(instance, "Bonemass");
				restoreDrops("MountainKit");
				restoreDrops("mountainkit");
				restoreDropsForBoss(instance, "Moder");
				restoreDropsForBoss(instance, "Yagluth");
				restoreDrops("dvergrprops");
				restoreDrops("dvergrtown");
				restoreDropsForBoss(instance, "The Queen");
			}

			// Main Halt Drops for Boss
			private static void haltDropsForBoss (ZNetScene instance, string bossName)
			{
				if (bossPrefabHolds.TryGetValue(bossName, out List<string> prefabStrings))
				{
					foreach (string prefabString in prefabStrings)
					{
						haltDrops(instance.GetPrefab(prefabString));
					}
				}
			}

			// Overloaded haltDrops method to handle string-based StartsWith
			private static void haltDrops(string prefabPrefix)
			{
				foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
				{
					if (prefab.name.StartsWith(prefabPrefix))
					{
						haltDrops(prefab); // Call the regular haltDrops with the GameObject
					}
				}
			}

			// HaltDrops main handler
			private static void haltDrops(GameObject prefab)
			{
				if (prefab == null)
				{
					MarsarahTweaks.MLog("Could not get prefab to halt drops");
					return;
				}

				// Dictionary mapping component types to their respective handlers
				Dictionary<Type, Func<GameObject, bool>> dropHandlers = new Dictionary<Type, Func<GameObject, bool>>()
				{
					{ typeof(CharacterDrop), haltDropsMob },
					{ typeof(MineRock), haltDropsMine },
					{ typeof(MineRock5), haltDropsMine5 },
					{ typeof(DropOnDestroyed), haltDropsOnDestroyed },
					{ typeof(Destructible), haltDropsDestructible },
					{ typeof(TreeLog), haltDropsTreeLog }
				};

				//bool modified = false; // Track if any drop was halted

				// Iterate through all handlers and apply every matching one
				foreach (var handler in dropHandlers)
				{
					if (prefab.GetComponent(handler.Key) != null)
					{
						handler.Value(prefab);
						/*if (handler.Value(prefab))
						{
							//MarsarahTweaks.MLog($"Halted drop for {prefab.name} as component {handler.Key}");
							modified = true; // Mark that at least one modification was made
						}*/
					}
				}

				// Log components if no drop handler was triggered
				/*if (!modified)
				{
					Component[] prefabComponents = prefab.GetComponents<Component>();
					foreach (Component comp in prefabComponents)
					{
						MarsarahTweaks.MLog(prefab.name + " - " + comp.ToString());
					}
				}*/
			}

			// Halt Drops X
			private static bool haltDropsMob(GameObject prefab)
			{
				CharacterDrop characterDrop = prefab.GetComponent<CharacterDrop>();
				if (characterDrop != null)
				{
					string prefabName = prefab.name;

					if (!mobDropChanceBackup.ContainsKey(prefabName))
					{
						// Store only drop chances (ensuring deep copy)
						Dictionary<string, float> dropChances = new Dictionary<string, float>();
						foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
						{
							//MarsarahTweaks.MLog($"Backing up {prefabName}");
							dropChances[drop.m_prefab?.name ?? "UNKNOWN_PREFAB"] = drop.m_chance;
						}
						mobDropChanceBackup[prefabName] = dropChances;
					}

					// Halt drops by setting all chances to 0
					foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
					{
						drop.m_chance = 0f;
					}

					return true;
				}
				return false;
			}

			private static bool haltDropsMine(GameObject prefab)
			{
				MineRock prefabComponent = prefab.GetComponent<MineRock>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!mineDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.MLog($"Backing up {prefabName}");
						mineDropBackup[prefabName] = prefabComponent.m_dropItems.m_dropChance;
					}
					prefabComponent.m_dropItems.m_dropChance = 0;
					return true;
				}
				return false;
			}

			private static bool haltDropsMine5(GameObject prefab)
			{
				MineRock5 prefabComponent = prefab.GetComponent<MineRock5>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!mine5DropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.MLog($"Backing up {prefabName}");
						mine5DropBackup[prefabName] = prefabComponent.m_dropItems.m_dropChance;
					}
					prefabComponent.m_dropItems.m_dropChance = 0;
					return true;
				}
				return false;
			}

			private static bool haltDropsOnDestroyed(GameObject prefab)
			{
				DropOnDestroyed prefabComponent = prefab.GetComponent<DropOnDestroyed>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!destroyedDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.MLog($"Backing up {prefabName}");
						destroyedDropBackup[prefabName] = prefabComponent.m_dropWhenDestroyed.m_dropChance;
					}
					prefabComponent.m_dropWhenDestroyed.m_dropChance = 0;
					return true;
				}
				return false;
			}

			private static bool haltDropsDestructible(GameObject prefab)
			{
				Destructible prefabComponent = prefab.GetComponent<Destructible>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!destructibleDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.MLog($"Backing up {prefabName}");
						destructibleDropBackup[prefabName] = prefabComponent.m_spawnWhenDestroyed;
					}
					prefabComponent.m_spawnWhenDestroyed = null;
					return true;
				}
				return false;
			}

			private static bool haltDropsTreeLog(GameObject prefab)
			{
				TreeLog prefabComponent = prefab.GetComponent<TreeLog>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!treeLogDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.MLog($"Backing up {prefabName}");
						treeLogDropBackup[prefabName] = prefabComponent.m_dropWhenDestroyed;
					}
					prefabComponent.m_dropWhenDestroyed = null;
					return true;
				}
				return false;
			}

			// =======================================================================
			// Main Restore Drops for Boss
			private static void restoreDropsForBoss(ZNetScene instance, string bossName)
			{
				if (bossPrefabHolds.TryGetValue(bossName, out List<string> prefabStrings))
				{
					foreach (string prefabString in prefabStrings)
					{
						restoreDrops(instance.GetPrefab(prefabString));
					}
				}
			}

			// Overloaded restoreDrops method to handle string-based StartsWith
			private static void restoreDrops(string prefabPrefix)
			{
				foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
				{
					if (prefab.name.StartsWith(prefabPrefix))
					{
						restoreDrops(prefab); // Call the regular restoreDrops with the GameObject
					}
				}
			}

			// RestoreDrops main handler
			private static void restoreDrops(GameObject prefab)
			{
				if (prefab == null)
				{
					MarsarahTweaks.MLog("Could not get prefab to restore drops");
					return;
				}

				// Dictionary mapping component types to their respective restore handlers
				Dictionary<Type, Func<GameObject, bool>> dropHandlers = new Dictionary<Type, Func<GameObject, bool>>()
				{
					{ typeof(CharacterDrop), restoreDropsMob },
					{ typeof(MineRock), restoreDropsMine },
					{ typeof(MineRock5), restoreDropsMine5 },
					{ typeof(DropOnDestroyed), restoreDropsOnDestroyed },
					{ typeof(Destructible), restoreDropsDestructible },
					{ typeof(TreeLog), restoreDropsTreeLog }
				};

				// Iterate through all handlers and apply every matching one
				foreach (var handler in dropHandlers)
				{
					if (prefab.GetComponent(handler.Key) != null)
					{
						handler.Value(prefab);
					}
				}
			}

			// Restore Drops X
			private static bool restoreDropsMob(GameObject prefab)
			{
				CharacterDrop characterDrop = prefab.GetComponent<CharacterDrop>();
				if (characterDrop != null)
				{
					string prefabName = prefab.name;
					if (mobDropChanceBackup.TryGetValue(prefabName, out var originalChances))
					{
						foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
						{
							string dropPrefabName = drop.m_prefab?.name ?? "UNKNOWN_PREFAB";
							if (originalChances.TryGetValue(dropPrefabName, out float originalChance))
							{
								//MarsarahTweaks.MLog($"Restoring backup for {prefabName}");
								drop.m_chance = originalChance; // Restore original chance
							}
						}
						mobDropChanceBackup.Remove(prefabName);

						return true;
					}
				}
				return false;
			}

			private static bool restoreDropsMine(GameObject prefab)
			{
				MineRock prefabComponent = prefab.GetComponent<MineRock>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (mineDropBackup.TryGetValue(prefabName, out var originalChance))
					{
						//MarsarahTweaks.MLog($"Restoring backup for {prefabName}");
						prefabComponent.m_dropItems.m_dropChance = originalChance;

						mineDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool restoreDropsMine5(GameObject prefab)
			{
				MineRock5 prefabComponent = prefab.GetComponent<MineRock5>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (mine5DropBackup.TryGetValue(prefabName, out var originalChance))
					{
						//MarsarahTweaks.MLog($"Restoring backup for {prefabName}");
						prefabComponent.m_dropItems.m_dropChance = originalChance;

						mine5DropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool restoreDropsOnDestroyed(GameObject prefab)
			{
				DropOnDestroyed prefabComponent = prefab.GetComponent<DropOnDestroyed>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (destroyedDropBackup.TryGetValue(prefabName, out var originalChance))
					{
						//MarsarahTweaks.MLog($"Restoring backup for {prefabName}");
						prefabComponent.m_dropWhenDestroyed.m_dropChance = originalChance;

						destroyedDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool restoreDropsDestructible(GameObject prefab)
			{
				Destructible prefabComponent = prefab.GetComponent<Destructible>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (destructibleDropBackup.TryGetValue(prefabName, out var originalSpawn))
					{
						//MarsarahTweaks.MLog($"Restoring backup for {prefabName}");
						prefabComponent.m_spawnWhenDestroyed = originalSpawn;

						destructibleDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool restoreDropsTreeLog(GameObject prefab)
			{
				TreeLog prefabComponent = prefab.GetComponent<TreeLog>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (treeLogDropBackup.TryGetValue(prefabName, out var originalDrop))
					{
						//MarsarahTweaks.MLog($"Restoring backup for {prefabName}");
						prefabComponent.m_dropWhenDestroyed = originalDrop;

						treeLogDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}
		}
	}
}
