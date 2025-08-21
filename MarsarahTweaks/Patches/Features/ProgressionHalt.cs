using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using static InventoryGrid;

namespace MarsarahTweaks.Patches.Features
{
	internal class ProgressionHalt
	{
		/*[HarmonyPatch(typeof(TreeBase), "RPC_Damage")]
		class ProgressionHaltTreeDrop_Patch
		{
			static void Prefix(TreeBase __instance, ref DropTable ___m_dropWhenDestroyed)
			{
				if (ConfigManager.AutomaticProgressionHaltEnabled.Value)
				{
					//MarsarahTweaks.LogInfo($"[TreBase] Name: {__instance.name}");
					//List<GameObject> dropList = ___m_dropWhenDestroyed.GetDropList();
					//for (int i = 0; i < dropList.Count; i++)
					//{
					//	MarsarahTweaks.LogInfo($"[TreeBase] Drops: {dropList[i].name}");
					//}

					if (__instance.name.StartsWith("AshlandsTree"))
					{
						string suffix = __instance.name.Substring("AshlandsTree".Length);

						if (suffix.StartsWith("1") ||
							suffix.StartsWith("3") ||
							suffix.StartsWith("4") ||
							suffix.StartsWith("5") ||
							suffix.StartsWith("6") ||
							suffix.StartsWith("6_big"))
						{
							MarsarahTweaks.LogInfo($"[TreBase] Name: {__instance.name}");
							MarsarahTweaks.LogInfo($"[TreBase] Drop Chance: {___m_dropWhenDestroyed.m_dropChance}");
							___m_dropWhenDestroyed.m_dropChance = 0;
						}
					}
				}
			}
		}*/

		[HarmonyPatch(typeof(Piece), "DropResources")]
		class ProgressionHaltPiece_Patch
		{
			// Create a dictionary to map bosses to resources that need to be prevented
			private static readonly Dictionary<string, List<string>> pieceResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ 
					"Eikthyr", new List<string> 
					{ 
						"piece_chair", 
						"piece_chair02", 
						"piece_table", 
						"wood_pole_log",
						"wood_wall_log", // wood_pole_log_4 and wood_wall_log_4x0.5 should be taken care of
						//"TreasureChest_blackforest",
						//"TreasureChest_forestcrypt",
						//"TreasureChest_trollcave"
					}
				}, 
				{ 
					"The Elder", new List<string> 
					{ 
						"iron", 
						"dungeon_sunkencrypt_irongate",
						//"TreasureChest_swamp",
						//"TreasureChest_sunkencrypt"
					} 
				},
				{ 
					"Bonemass", new List<string> 
					{ 
						//"TreasureChest_mountains", 
						//"TreasureChest_mountaincave" 
					} 
				},
				{ 
					"Moder", new List<string> 
					{ 
						//"TreasureChest_heath", 
						//"TreasureChest_plains_stone" 
					} 
				},
				{ 
					"Yagluth", new List<string> 
					{ 
						"blackmarble", 
						"piece_dvergr", 
						"dvergrprops", 
						"dvergrtown", 
						"dverger_guardstone",
						//"TreasureChest_dvergrtower",
						//"TreasureChest_dvergrtown"
					} 
				},
				{ 
					"The Queen", new List<string> 
					{ 
						"Piece_grausten", 
						"Ashlands", 
						"piece_blackwood_bench",
						//"TreasureChest_charredfortress",
						//"TreasureChest_ashland_stone"
					} 
				}
			};

			static bool Prefix(Piece __instance)
			{
				if (ConfigManager.AutomaticProgressionHaltEnabled.Value)
				{
					if (__instance.IsPlacedByPlayer())
					{
						return true;
					}

					foreach (var bossEntry in pieceResourceRestrictions)
					{
						string bossName = bossEntry.Key;
						List<string> restrictedPieces = bossEntry.Value;

						bool bossDefeated = GlobalKeyChecker.IsBossDefeated(bossName);

						if (!bossDefeated)
						{
							foreach (var piece in restrictedPieces)
							{
								if (__instance.name.StartsWith(piece))
								{
									//MarsarahTweaks.LogInfo($"Progression Halt: Prevented Piece piece drop for {__instance.name} because {bossName} has not been defeated.");
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
		static class ContainerInteract_Patch
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
				if (hold) return true; // Allow normal behavior for hold interactions ??

				if (!ConfigManager.AutomaticProgressionHaltEnabled.Value) return true; // Skip if disabled

				//string chestName = __instance.name.Replace("(Clone)", "").Trim();
				string chestName = __instance.name;
				foreach (var restriction in chestResourceRestrictions)
				{
					string bossName = restriction.Key;
					List<string> restrictedChests = restriction.Value;
					bool bossDefeated = GlobalKeyChecker.IsBossDefeated(bossName);

					//if (!bossDefeated && restrictedChests.Contains(chestName))
					if (!bossDefeated && restrictedChests.Any(rc => chestName.StartsWith(rc)))
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

		[HarmonyPatch(typeof(Container), "DropAllItems", new Type[] { })] // Patch the version without parameters (there are two overloads)
		static class ContainerDropItems_Patch
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

			static bool Prefix(Container __instance)
			{
				if (!ConfigManager.AutomaticProgressionHaltEnabled.Value) return true; // Skip if disabled

				//string chestName = __instance.name.Replace("(Clone)", "").Trim();
				string chestName = __instance.name;
				foreach (var restriction in chestResourceRestrictions)
				{
					string bossName = restriction.Key;
					List<string> restrictedChests = restriction.Value;
					bool bossDefeated = GlobalKeyChecker.IsBossDefeated(bossName);

					//if (!bossDefeated && restrictedChests.Contains(chestName))
					if (!bossDefeated && restrictedChests.Any(rc => chestName.StartsWith(rc)))
					{
						// Prevent content drop
						//MarsarahTweaks.LogInfo($"Progression Halt: Prevented Container content drop for {__instance.name} because {bossName} has not been defeated.");
						return false; // Skip DropAllItems
					}
				}
				return true; // Allow normal behavior for unrestricted chests
			}
		}


		[HarmonyPatch(typeof(Pickable), "Interact")]
		static class PickableInteract_Patch
		{
			private static readonly Dictionary<string, List<string>> pickableResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "Eikthyr", new List<string> 
					{ 
						"Pickable_Carrot", 
						"Pickable_SeedCarrot",
						"Pickable_Thistle",
						//"Pickable_ForestCryptRemains01", // MC these are the bones that also spawn in the meadows too
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
						"Pickable_VoltureEgg",
						"Pickable_Pot_Shard"
					} 
				}
			};

			static bool Prefix(Pickable __instance, Humanoid character, ref bool __result)
			{
				if (!ConfigManager.AutomaticProgressionHaltEnabled.Value) return true;

				// Access private fields via reflection
				FieldInfo nviewField = typeof(Pickable).GetField("m_nview", BindingFlags.NonPublic | BindingFlags.Instance);
				FieldInfo enabledField = typeof(Pickable).GetField("m_enabled", BindingFlags.NonPublic | BindingFlags.Instance);

				if (nviewField == null || enabledField == null)
				{
					MarsarahTweaks.LogError("[ERROR] Could not access private fields in Pickable!");
					return true;
				}

				ZNetView nview = (ZNetView)nviewField.GetValue(__instance);
				int enabled = (int)enabledField.GetValue(__instance);

				if (!nview.IsValid() || enabled == 0)
				{
					return true;
				}

				string pickableName = __instance.name;
				//string rawName = __instance.name;
				//string pickableName = Regex.Split(rawName, @"[\s\(]")[0];
				//MarsarahTweaks.LogInfo($"[PickupCheck] Raw Name: {rawName}, Cleaned Name: {pickableName}");

				/*ZDO zdo = nview.GetZDO();
				if (zdo != null)
				{
					//MarsarahTweaks.LogInfo($"[PickupCheck] Prefab: {__instance.gameObject.name}, Trimmed: {pickableName}");
					MarsarahTweaks.LogInfo($"[PickupCheck] Raw Name: {rawName}, Cleaned Name: {pickableName}");
					MarsarahTweaks.LogInfo($"[PickupCheck] Owner: {zdo.GetOwner()}, ZDO ID: {zdo.m_uid}");
					//MarsarahTweaks.LogInfo($"[PickupCheck] Owner: {zdo.GetOwner()}, ZDO ID: {zdo.m_uid}, Pos: {__instance.transform.position}");

					long ownerId = zdo.GetOwner();
					long localPlayerId = ZNet.instance.LocalPlayerCharacterID.UserID;

					//MarsarahTweaks.LogInfo($"[PickupCheck] My ID: {localPlayerId}");

					// Try to find the player's name from the player list
					var playerInfo = ZNet.instance.GetPlayerList().Where(p => p.m_characterID.UserID == ownerId).Cast<ZNet.PlayerInfo?>().FirstOrDefault();

					string ownerName = playerInfo.HasValue ? playerInfo.Value.m_name : "Unknown";

					// LogInfo ownership info
					if (ownerId == localPlayerId)
					{
						MarsarahTweaks.LogInfo($"[PickupCheck] This object belongs to me ({localPlayerId} - {ownerName})");
					}
					else if (ZNet.instance.GetServerPeer() != null && ownerId == ZNet.instance.GetServerPeer().m_uid)
					{
						MarsarahTweaks.LogInfo($"[PickupCheck] This object belongs to the server ({ownerName})");
					}
					else
					{
						MarsarahTweaks.LogInfo($"[PickupCheck] This object belongs to {ownerName} (ID: {ownerId})");
					}

					// LogInfo Venture's VV_LastReset info if present
					if (zdo.GetInt("VV_LastReset", -1) != -1)
					{
						int resetDay = zdo.GetInt("VV_LastReset", -1);
						MarsarahTweaks.LogInfo($"[PickupCheck] Venture reset detected - VV_LastReset = {resetDay}");
					}
					else
					{
						MarsarahTweaks.LogInfo($"[PickupCheck] No VV_LastReset value found (not reset by Venture?)");
					}
				}
				else
				{
					MarsarahTweaks.LogInfo("[PickupCheck] No ZDO found for pickable.");
				}*/

				foreach (var restriction in pickableResourceRestrictions)
				{

					string bossName = restriction.Key;
					List<string> restrictedPickables = restriction.Value;

					bool bossDefeated = GlobalKeyChecker.IsBossDefeated(bossName);

					//if (!bossDefeated && restrictedPickables.Contains(pickableName))
					if (!bossDefeated && restrictedPickables.Any(rp => pickableName.StartsWith(rp)))
					{
						character.Message(MessageHud.MessageType.Center, $"{bossName} has a strong hold on this object");
						//MarsarahTweaks.LogInfo($"Halted Pickable {pickableName} for boss {bossName}");
						__result = false;
						return false;
					}
					/*else
					{
						MarsarahTweaks.LogInfo($"Did not halt {pickableName} for boss {bossName}");
					}*/
				}

				return true; // Allow original method
			}
		}

		[HarmonyPatch(typeof(PickableItem), "Interact")]
		static class PickableItemInteract_Patch
		{
			private static readonly Dictionary<string, List<string>> pickableItemResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "Eikthyr", new List<string> {	"Pickable_ForestCryptRandom" } },
				{ "The Elder", new List<string> { "Pickable_SunkenCryptRandom" } },
				{ "Bonemass", new List<string> { "Pickable_MountainCaveRandom"	} }
			};

			static bool Prefix(Pickable __instance, Humanoid character, ref bool __result)
			{
				if (!ConfigManager.AutomaticProgressionHaltEnabled.Value) return true;

				// Access private fields via reflection
				FieldInfo nviewField = typeof(PickableItem).GetField("m_nview", BindingFlags.NonPublic | BindingFlags.Instance);

				if (nviewField == null)
				{
					MarsarahTweaks.LogError("[ERROR] Could not access private fields in PickableItem!");
					return true;
				}

				ZNetView nview = (ZNetView)nviewField.GetValue(__instance);

				if (!nview.IsValid())
				{
					return true;
				}

				string pickableItemName = __instance.name;
				//string pickableItemName = __instance.name.Replace("(Clone)", "").Trim();
				//string pickableItemName = Regex.Split(__instance.name, @"[\s\(]")[0];

				foreach (var restriction in pickableItemResourceRestrictions)
				{

					string bossName = restriction.Key;
					List<string> restrictedPickableItems = restriction.Value;

					bool bossDefeated = GlobalKeyChecker.IsBossDefeated(bossName);

					//if (!bossDefeated && restrictedPickableItems.Contains(pickableItemName))
					if (!bossDefeated && restrictedPickableItems.Any(rpi => pickableItemName.StartsWith(rpi)))
					{
						character.Message(MessageHud.MessageType.Center, $"{bossName} has a strong hold on this object");
						//MarsarahTweaks.LogInfo($"Halted PickableItem {pickableItemName} for boss {bossName}");
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
			private static readonly Dictionary<string, Dictionary<string, float>> mobDropChanceBackup = new Dictionary<string, Dictionary<string, float>>();
			private static readonly Dictionary<string, float> mineDropBackup = new Dictionary<string, float>();
			private static readonly Dictionary<string, float> mine5DropBackup = new Dictionary<string, float>();
			private static readonly Dictionary<string, float> destroyedDropBackup = new Dictionary<string, float>();
			private static readonly Dictionary<string, GameObject> destructibleDropBackup = new Dictionary<string, GameObject>();
			private static readonly Dictionary<string, DropTable> treeLogDropBackup = new Dictionary<string, DropTable>();
			private static readonly Dictionary<string, float> treeBaseDropBackup = new Dictionary<string, float>();

			// Prefab dictionary
			private static readonly Dictionary<string, List<string>> bossPrefabHolds = new Dictionary<string, List<string>>()
			{
				{ 
					"Eikthyr", new List<string> 
					{
						//"Greydwarf",
						//"Greydwarf_Elite",
						//"Greydwarf_Shaman",
						"Troll",
						//"Skeleton",
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
						"barrell",
						"shipwreck_karve_bottomboards",
						"shipwreck_karve_bow",
						//"shipwreck_karve_chest", // MC: this is not a DropOnDestroyed like the rest
						"shipwreck_karve_dragonhead",
						"shipwreck_karve_stern",
						"shipwreck_karve_sternpost"
					}
				},
				{
					"The Elder", new List<string>
					{
						"Leech",
						"Draugr",
						"Draugr_Ranged",
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
						//"Serpent", // Ocean
						//"Leviathan", // Ocean
						"Pickable_MountainCaveCrystal",
						"Pickable_MountainCaveObsidian",
						//"Pickable_MeatPile",
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
						//"Pickable_DvergerThing",
						//"Pickable_DvergrLantern",
						//"Pickable_DvergrMineTreasure",
						//"Pickable_DvergrStein",
						//"Pickable_Mushroom_JotunPuffs",
						//"Pickable_Mushroom_Magecap",
						//"Pickable_RoyalJelly",
						//"Pickable_BlackCoreStand",
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
						//"VineAsh",
						//"Pickable_Ashstone",
						//"Pickable_Charredskull",
						//"Pickable_Fiddlehead",
						//"Pickable_Meteorite",
						//"Pickable_MoltenCoreStand",
						//"Pickable_SmokePuff",
						//"Pickable_VoltureEgg",
						"Pickable_SulfurRock",
						"FlametalRockstand",
						"FlametalRockstand_frac",
						"LeviathanLava",
						"dvergrprops_crate_ashlands",
						"AshlandsTree1",
						"AshlandsTree3",
						"AshlandsTree4",
						"AshlandsTree5",
						"AshlandsTree6",
						"AshlandsTree6_big",
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
						"GraveStone_Broken_World",
						"GraveStone_CharredTwitcherNest",
						"GraveStone_CharredFaderLocation",
						"GraveStone_Elite_Broken_CharredTwitcherNest",
						"GraveStone_Elite_CharredTwitcherNest",
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
						"cliff_ashlandsflowrock_frac",
						// These are not halted but dun't know if they are the ones that drop things
						//"Ashlands_Arch1",
						//"Ashlands_Arch2",
						//"Ashlands_Pillar4_tip",
						//"Ashlands_Pillar4_tip2",
						//"Ashlands_Pillar4_tip3",
						//"Ashlands_Ramp",
						//"Ashlands_Ruins_Wall_Broken3_4x6",
						//"Ashlands_Ruins_Wall_Broken4_4x6",
						//"Ashlands_Ruins_Wall_Broken5_4x6",
						//"Ashlands_Ruins_Wall_Top_wHole",
						//"Ashlands_Ruins_Wall_Window_4x6_broken2",
						//"Ashlands_Ruins_Wall_Window_4x6_broken3",
						//"Ashlands_Ruins_Wall_Window_4x6_broken4",
						//"Ashlands_Ruins_Wall_Window_4x6_broken5",
						//"Ashlands_Ruins_Wall_Window_4x6_broken6",
						//"Ashlands_StairsBroad",
						//"Ashlands_Wall_2x2",
						//"Ashlands_Wall_2x2_cornerL",
						//"Ashlands_Wall_2x2_cornerL_top",
						//"Ashlands_Wall_2x2_cornerR",
						//"Ashlands_Wall_2x2_cornerR_top",
						//"Ashlands_Wall_2x2_edge",
						//"Ashlands_Wall_2x2_edge_top",
						//"Ashlands_Wall_2x2_edge2",
						//"Ashlands_Wall_2x2_edge2_top",
						//"Ashlands_Wall_2x2_top",
						//"Ashlands_WallBlock_1x2x2",
						//"Ashlands_WallBlock_base",
					}
				}
			};
			private static readonly List<string> oceanPrefabs = new List<string>()
			{
				"Serpent",
				"Leviathan"
			};

			// Tracking last defeated states to determine when changes occur
			private static bool lastEikthyrDefeated = GlobalKeyChecker.IsBossDefeated("Eikthyr");
			private static bool lastElderDefeated = GlobalKeyChecker.IsBossDefeated("The Elder");
			private static bool lastBonemassDefeated = GlobalKeyChecker.IsBossDefeated("Bonemass");
			private static bool lastModerDefeated = GlobalKeyChecker.IsBossDefeated("Moder");
			private static bool lastYagluthDefeated = GlobalKeyChecker.IsBossDefeated("Yagluth");
			private static bool lastQueenDefeated = GlobalKeyChecker.IsBossDefeated("The Queen");

			private static bool lastProgressionHaltState = ConfigManager.AutomaticProgressionHaltEnabled.Value;
			private static bool lastOceanElderProgressionHaltState = ConfigManager.HaltOceanBehindElderEnabled.Value;
			private static bool lastTrophyDropsState = ConfigManager.BetterTrophyDropsEnabled.Value;

			static void Postfix(ref ZNetScene __instance)
			{
				if (__instance == null) return;

				bool bossStateChanged = false;

				if (GlobalKeyChecker.EikthyrDefeated != lastEikthyrDefeated)
				{
					lastEikthyrDefeated = GlobalKeyChecker.EikthyrDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.ElderDefeated != lastElderDefeated)
				{
					lastElderDefeated = GlobalKeyChecker.ElderDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.BonemassDefeated != lastBonemassDefeated)
				{
					lastBonemassDefeated = GlobalKeyChecker.BonemassDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.ModerDefeated != lastModerDefeated)
				{
					lastModerDefeated = GlobalKeyChecker.ModerDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.YagluthDefeated != lastYagluthDefeated)
				{
					lastYagluthDefeated = GlobalKeyChecker.YagluthDefeated;
					bossStateChanged = true;
				}
				if (GlobalKeyChecker.QueenDefeated != lastQueenDefeated)
				{
					lastQueenDefeated = GlobalKeyChecker.QueenDefeated;
					bossStateChanged = true;
				}

				// Check if Progression Halt was toggled
				bool progressionHaltNowEnabled = ConfigManager.AutomaticProgressionHaltEnabled.Value;
				bool oceanElderProgressionHaltNowEnabled = ConfigManager.HaltOceanBehindElderEnabled.Value;
				bool trophyDropsNowEnabled = ConfigManager.BetterTrophyDropsEnabled.Value;

				if (progressionHaltNowEnabled != lastProgressionHaltState)
				{
					lastProgressionHaltState = progressionHaltNowEnabled;

					if (progressionHaltNowEnabled)
					{
						// Handle case Trophy Drops ON and Progression Halt toggled from OFF to ON
						// Restore Trophy Drops and then apply Progression Halt
						if (trophyDropsNowEnabled)
						{
							//MarsarahTweaks.LogInfo($"Restoring Trophy Drops Special");
							TrophyDropsChanges.RestoreTrophyDrops(__instance);
						}

						// If Progression Halt was turned ON mid-game, run it without checking boss states or drops set
						//MarsarahTweaks.LogInfo($"Setting up Progression Halt due to re-enabling");
						HandleProgressionHalt(__instance);
						dropsSet = true;
					}
					else
					{
						// If Progression Halt was turned OFF mid-game, restore original drops
						//MarsarahTweaks.LogInfo($"Restoring Progression Halt to default entirely");
						RestoreProgressionHalt(__instance);

						// Run Trophy Drops here
						//MarsarahTweaks.LogInfo($"Setting up Trophy Drops due to Progression Halt being off");
						TrophyDropsChanges.UpdateTrophyDrops(__instance);
					}
				}

				if (oceanElderProgressionHaltNowEnabled != lastOceanElderProgressionHaltState)
				{
					lastOceanElderProgressionHaltState = oceanElderProgressionHaltNowEnabled;

					if (progressionHaltNowEnabled)
					{
						// Re-run the progression halt handler to apply updated ocean boss logic
						//MarsarahTweaks.LogInfo("Ocean Progression Halt setting toggled, reapplying drops.");
						HandleProgressionHalt(__instance);
						dropsSet = true;
					}
				}

				// Progression Halt logic (only if enabled)
				if (progressionHaltNowEnabled)
				{
					if (!dropsSet || bossStateChanged)
					{
						//MarsarahTweaks.LogInfo($"Setting up Progression Halt standard way");
						HandleProgressionHalt(__instance);
						dropsSet = true;
					}
				}

				// Check if Trophy Drops was toggled
				if (trophyDropsNowEnabled != lastTrophyDropsState)
				{
					lastTrophyDropsState = trophyDropsNowEnabled;

					// If Trophy Drops was toggled mid-game, run it without checking Progression Halt state (since it checks inside) or trophyDropsSet
					// This needs to be ran regardless if it's on or off
					//MarsarahTweaks.LogInfo($"Setting up Trophy Drops due to toggling");
					TrophyDropsChanges.UpdateTrophyDrops(__instance);
				}

				// Trophy Drops logic (ONLY run once on game start OR when Progression Halt is enabled and bosses change)
				if (!trophyDropsSet || (progressionHaltNowEnabled && bossStateChanged))
				{
					//MarsarahTweaks.LogInfo($"Setting up Trophy Drops standard way");
					TrophyDropsChanges.UpdateTrophyDrops(__instance);
					trophyDropsSet = true;
				}
			}

			// =======================================================================
			// Progression Halt patch handler
			private static void HandleProgressionHalt (ZNetScene instance)
			{
				if (!GlobalKeyChecker.EikthyrDefeated)
				{
					HaltDropsForBoss(instance, "Eikthyr");
				}
				else
				{
					RestoreDropsForBoss(instance, "Eikthyr");
				}

				if (!GlobalKeyChecker.ElderDefeated)
				{
					HaltDropsForBoss(instance, "The Elder");
				}
				else
				{
					RestoreDropsForBoss(instance, "The Elder");
				}

				if (!GlobalKeyChecker.BonemassDefeated)
				{
					HaltDropsForBoss(instance, "Bonemass");
					HaltDrops("MountainKit");
					HaltDrops("mountainkit");
				}
				else
				{
					RestoreDropsForBoss(instance, "Bonemass");
					RestoreDrops("MountainKit");
					RestoreDrops("mountainkit");
				}

				if (!GlobalKeyChecker.ModerDefeated)
				{
					HaltDropsForBoss(instance, "Moder");
				}
				else
				{
					RestoreDropsForBoss(instance, "Moder");
				}

				if (!GlobalKeyChecker.YagluthDefeated)
				{
					HaltDropsForBoss(instance, "Yagluth");
					HaltDrops("dvergrprops");
					HaltDrops("dvergrtown");
				}
				else
				{
					RestoreDropsForBoss(instance, "Yagluth");
					RestoreDrops("dvergrprops");
					RestoreDrops("dvergrtown");
				}

				if (!GlobalKeyChecker.QueenDefeated)
				{
					HaltDropsForBoss(instance, "The Queen");
				}
				else
				{
					RestoreDropsForBoss(instance, "The Queen");
				}

				if (!GlobalKeyChecker.ElderDefeated && ConfigManager.HaltOceanBehindElderEnabled.Value)
				{
					HaltOceanPrefabs(instance);
				}
				else if (!GlobalKeyChecker.BonemassDefeated && !ConfigManager.HaltOceanBehindElderEnabled.Value)
				{
					HaltOceanPrefabs(instance);
				}
				else
				{
					RestoreOceanPrefabs(instance);
				}

			}

			// Restore all Progression Halt data
			private static void  RestoreProgressionHalt(ZNetScene instance)
			{
				RestoreDropsForBoss(instance, "Eikthyr");
				RestoreDropsForBoss(instance, "The Elder");
				RestoreDropsForBoss(instance, "Bonemass");
				RestoreDrops("MountainKit");
				RestoreDrops("mountainkit");
				RestoreDropsForBoss(instance, "Moder");
				RestoreDropsForBoss(instance, "Yagluth");
				RestoreDrops("dvergrprops");
				RestoreDrops("dvergrtown");
				RestoreDropsForBoss(instance, "The Queen");
				RestoreOceanPrefabs(instance);
			}

			// Main Halt Drops for Boss
			private static void HaltDropsForBoss (ZNetScene instance, string bossName)
			{
				if (bossPrefabHolds.TryGetValue(bossName, out List<string> prefabStrings))
				{
					foreach (string prefabString in prefabStrings)
					{
						HaltDrops(instance.GetPrefab(prefabString));
					}
				}
			}

			// Halt Ocean prefabs
			private static void HaltOceanPrefabs(ZNetScene instance)
			{
				foreach (string prefab in oceanPrefabs)
				{
					HaltDrops(instance.GetPrefab(prefab));
				}
			}

			// Overloaded HaltDrops method to handle string-based StartsWith
			private static void HaltDrops(string prefabPrefix)
			{
				foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
				{
					if (prefab.name.StartsWith(prefabPrefix))
					{
						HaltDrops(prefab); // Call the regular HaltDrops with the GameObject
					}
				}
			}

			// HaltDrops main handler
			private static void HaltDrops(GameObject prefab)
			{
				if (prefab == null)
				{
					//MarsarahTweaks.LogInfo("Could not get prefab to halt drops");
					return;
				}

				// Dictionary mapping component types to their respective handlers
				Dictionary<Type, Func<GameObject, bool>> dropHandlers = new Dictionary<Type, Func<GameObject, bool>>()
				{
					{ typeof(CharacterDrop), HaltDropsMob },
					{ typeof(MineRock), HaltDropsMine },
					{ typeof(MineRock5), HaltDropsMine5 },
					{ typeof(DropOnDestroyed), HaltDropsOnDestroyed },
					{ typeof(Destructible), HaltDropsDestructible },
					{ typeof(TreeLog), HaltDropsTreeLog },
					{ typeof(TreeBase), HaltDropsTreeBase }
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
							//MarsarahTweaks.LogInfo($"Halted drop for {prefab.name} as component {handler.Key}");
							modified = true; // Mark that at least one modification was made
						}*/
					}
				}

				// LogInfo components if no drop handler was triggered
				/*if (!modified)
				{
					Component[] prefabComponents = prefab.GetComponents<Component>();
					foreach (Component comp in prefabComponents)
					{
						MarsarahTweaks.LogInfo(prefab.name + " - " + comp.ToString());
					}
				}*/
			}

			// Halt Drops X
			private static bool HaltDropsMob(GameObject prefab)
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
							//MarsarahTweaks.LogInfo($"Backing up {prefabName}");
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

			private static bool HaltDropsMine(GameObject prefab)
			{
				MineRock prefabComponent = prefab.GetComponent<MineRock>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!mineDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.LogInfo($"Backing up {prefabName}");
						mineDropBackup[prefabName] = prefabComponent.m_dropItems.m_dropChance;
					}
					prefabComponent.m_dropItems.m_dropChance = 0;
					return true;
				}
				return false;
			}

			private static bool HaltDropsMine5(GameObject prefab)
			{
				MineRock5 prefabComponent = prefab.GetComponent<MineRock5>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!mine5DropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.LogInfo($"Backing up {prefabName}");
						mine5DropBackup[prefabName] = prefabComponent.m_dropItems.m_dropChance;
					}
					prefabComponent.m_dropItems.m_dropChance = 0;
					return true;
				}
				return false;
			}

			private static bool HaltDropsOnDestroyed(GameObject prefab)
			{
				DropOnDestroyed prefabComponent = prefab.GetComponent<DropOnDestroyed>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!destroyedDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.LogInfo($"Backing up {prefabName}");
						destroyedDropBackup[prefabName] = prefabComponent.m_dropWhenDestroyed.m_dropChance;
					}
					prefabComponent.m_dropWhenDestroyed.m_dropChance = 0;
					return true;
				}
				return false;
			}

			private static bool HaltDropsDestructible(GameObject prefab)
			{
				Destructible prefabComponent = prefab.GetComponent<Destructible>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!destructibleDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.LogInfo($"Backing up {prefabName}");
						destructibleDropBackup[prefabName] = prefabComponent.m_spawnWhenDestroyed;
					}
					prefabComponent.m_spawnWhenDestroyed = null;
					return true;
				}
				return false;
			}

			private static bool HaltDropsTreeLog(GameObject prefab)
			{
				TreeLog prefabComponent = prefab.GetComponent<TreeLog>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (!treeLogDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.LogInfo($"Backing up {prefabName}");
						treeLogDropBackup[prefabName] = prefabComponent.m_dropWhenDestroyed;
					}
					prefabComponent.m_dropWhenDestroyed = null;
					return true;
				}
				return false;
			}

			private static bool HaltDropsTreeBase(GameObject prefab)
			{
				TreeBase prefabComponent = prefab.GetComponent<TreeBase>();
				if (prefabComponent != null && prefabComponent.m_dropWhenDestroyed != null)
				{
					string prefabName = prefab.name;
					if (!treeBaseDropBackup.ContainsKey(prefabName))
					{
						//MarsarahTweaks.LogInfo($"Backing up {prefabName}");
						treeBaseDropBackup[prefabName] = prefabComponent.m_dropWhenDestroyed.m_dropChance;
					}

					prefabComponent.m_dropWhenDestroyed.m_dropChance = 0f;
					return true;
				}
				return false;
			}

			// =======================================================================
			// Main Restore Drops for Boss
			private static void RestoreDropsForBoss(ZNetScene instance, string bossName)
			{
				if (bossPrefabHolds.TryGetValue(bossName, out List<string> prefabStrings))
				{
					foreach (string prefabString in prefabStrings)
					{
						RestoreDrops(instance.GetPrefab(prefabString));
					}
				}
			}

			// Restore Ocean Prefabs
			private static void RestoreOceanPrefabs(ZNetScene instance)
			{
				foreach (string prefab in oceanPrefabs)
				{
					RestoreDrops(instance.GetPrefab(prefab));
				}
			}

			// Overloaded RestoreDrops method to handle string-based StartsWith
			private static void RestoreDrops(string prefabPrefix)
			{
				foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
				{
					if (prefab.name.StartsWith(prefabPrefix))
					{
						RestoreDrops(prefab); // Call the regular RestoreDrops with the GameObject
					}
				}
			}

			// RestoreDrops main handler
			private static void RestoreDrops(GameObject prefab)
			{
				if (prefab == null)
				{
					//MarsarahTweaks.LogInfo("Could not get prefab to restore drops");
					return;
				}

				// Dictionary mapping component types to their respective restore handlers
				Dictionary<Type, Func<GameObject, bool>> dropHandlers = new Dictionary<Type, Func<GameObject, bool>>()
				{
					{ typeof(CharacterDrop), RestoreDropsMob },
					{ typeof(MineRock), RestoreDropsMine },
					{ typeof(MineRock5), RestoreDropsMine5 },
					{ typeof(DropOnDestroyed), RestoreDropsOnDestroyed },
					{ typeof(Destructible), RestoreDropsDestructible },
					{ typeof(TreeLog), RestoreDropsTreeLog },
					{ typeof(TreeBase), RestoreDropsTreeBase }
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
			private static bool RestoreDropsMob(GameObject prefab)
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
								//MarsarahTweaks.LogInfo($"Restoring backup for {prefabName}");
								drop.m_chance = originalChance; // Restore original chance
							}
						}
						mobDropChanceBackup.Remove(prefabName);

						return true;
					}
				}
				return false;
			}

			private static bool RestoreDropsMine(GameObject prefab)
			{
				MineRock prefabComponent = prefab.GetComponent<MineRock>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (mineDropBackup.TryGetValue(prefabName, out var originalChance))
					{
						//MarsarahTweaks.LogInfo($"Restoring backup for {prefabName}");
						prefabComponent.m_dropItems.m_dropChance = originalChance;

						mineDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool RestoreDropsMine5(GameObject prefab)
			{
				MineRock5 prefabComponent = prefab.GetComponent<MineRock5>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (mine5DropBackup.TryGetValue(prefabName, out var originalChance))
					{
						//MarsarahTweaks.LogInfo($"Restoring backup for {prefabName}");
						prefabComponent.m_dropItems.m_dropChance = originalChance;

						mine5DropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool RestoreDropsOnDestroyed(GameObject prefab)
			{
				DropOnDestroyed prefabComponent = prefab.GetComponent<DropOnDestroyed>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (destroyedDropBackup.TryGetValue(prefabName, out var originalChance))
					{
						//MarsarahTweaks.LogInfo($"Restoring backup for {prefabName}");
						prefabComponent.m_dropWhenDestroyed.m_dropChance = originalChance;

						destroyedDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool RestoreDropsDestructible(GameObject prefab)
			{
				Destructible prefabComponent = prefab.GetComponent<Destructible>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (destructibleDropBackup.TryGetValue(prefabName, out var originalSpawn))
					{
						//MarsarahTweaks.LogInfo($"Restoring backup for {prefabName}");
						prefabComponent.m_spawnWhenDestroyed = originalSpawn;

						destructibleDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool RestoreDropsTreeLog(GameObject prefab)
			{
				TreeLog prefabComponent = prefab.GetComponent<TreeLog>();
				if (prefabComponent != null)
				{
					string prefabName = prefab.name;
					if (treeLogDropBackup.TryGetValue(prefabName, out var originalDrop))
					{
						//MarsarahTweaks.LogInfo($"Restoring backup for {prefabName}");
						prefabComponent.m_dropWhenDestroyed = originalDrop;

						treeLogDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}

			private static bool RestoreDropsTreeBase(GameObject prefab)
			{
				TreeBase prefabComponent = prefab.GetComponent<TreeBase>();
				if (prefabComponent != null && prefabComponent.m_dropWhenDestroyed != null)
				{
					string prefabName = prefab.name;
					if (treeBaseDropBackup.TryGetValue(prefabName, out float originalChance))
					{
						//MarsarahTweaks.LogInfo($"Restoring backup for {prefabName}");
						prefabComponent.m_dropWhenDestroyed.m_dropChance = originalChance;

						treeBaseDropBackup.Remove(prefabName);
						return true;
					}
				}
				return false;
			}
		}
	}
}
