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

namespace MarsarahTweaks.Patches
{
	internal class ProgressionHalt
	{
		private static bool eikthyrDefeated = false;
		private static bool elderDefeated = false;
		private static bool bonemassDefeated = false;
		private static bool moderDefeated = false;
		private static bool yagluthDefeated = false;
		private static bool queenDefeated = false;


		// Check boss status 
		[HarmonyPatch(typeof(ZoneSystem), "Update")]
		class ClearMistlandsUpdate_Patch
		{
			static void Postfix(ZoneSystem __instance)
			{
				if (!ConfigManager.clearMistlandsEnabled.Value) return;

				BossStateChecker.UpdateDefeatedStates(__instance);

				eikthyrDefeated = BossStateChecker.IsBossDefeated("defeated_eikthyr");
				elderDefeated = BossStateChecker.IsBossDefeated("defeated_gdking");
				bonemassDefeated = BossStateChecker.IsBossDefeated("defeated_bonemass");
				moderDefeated = BossStateChecker.IsBossDefeated("defeated_dragon");
				yagluthDefeated = BossStateChecker.IsBossDefeated("defeated_goblinking");
				queenDefeated = BossStateChecker.IsBossDefeated("defeated_queen");
			}
		}

		[HarmonyPatch(typeof(Piece), "DropResources")]
		class ProgressionHaltPiece_Patch
		{
			// Create a dictionary to map bosses to resources that need to be prevented
			private static readonly Dictionary<string, List<string>> pieceResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "eikthyr", new List<string> { "piece_chair", "piece_chair02", "piece_table", "wood_pole_log", "wood_wall_log" } }, // wood_pole_log_4 and wood_wall_log_4x0.5 should be taken care of
				{ "elder", new List<string> { "iron", "dungeon_sunkencrypt_irongate" } },
				{ "yagluth", new List<string> { "blackmarble", "piece_dvergr", "dvergrprops", "dvergrtown", "dverger_guardstone" } },
				{ "queen", new List<string> { "Piece_grausten", "Ashlands", "piece_blackwood_bench" } }
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

						bool bossDefeated = GetBossDefeatedState(bossName);

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

		/*[HarmonyPatch(typeof(DropTable), "GetDropListItems")]
		public static class DropTable_GetDropListItems_Patch
		{
			// This is the Postfix that will execute after GetDropListItems
			static void Postfix(DropTable __instance, ref List<ItemDrop.ItemData> __result)
			{
				// Log or process the return value (the drop list) here
				MarsarahTweaks.MLog($"[Patch] GetDropListItems {__instance.ToString()} returned {__result.Count} items.");

				// Optionally, modify the return value if needed
				// Example: Add an item to the drop list (be careful with modifying the return value)
				// __result.Add(new ItemDrop.ItemData());
			}
		}*/


		[HarmonyPatch(typeof(Container), "Interact")]
		public static class Container_Interact_Patch
		{
			private static readonly Dictionary<string, List<string>> chestResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "eikthyr", new List<string> { "TreasureChest_forestcrypt", "TreasureChest_trollcave", "TreasureChest_blackforest" } },
				{ "elder", new List<string> { "TreasureChest_swamp", "TreasureChest_sunkencrypt" } },
				{ "bonemass", new List<string> { "TreasureChest_mountaincave" } },
				{ "yagluth", new List<string> { "TreasureChest_dvergrtower", "TreasureChest_dvergrtown" } },
				{ "queen", new List<string> { "TreasureChest_charredfortress", "TreasureChest_ashland_stone" } }
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
					bool bossDefeated = GetBossDefeatedState(bossName);

					if (!bossDefeated && restrictedChests.Contains(chestName))
					{
						// Prevent interaction and show message
						character.Message(MessageHud.MessageType.Center, "The chest is magically sealed.");
						__result = false;
						return false;
					}
				}
				return true; // Allow normal behavior for unrestricted chests
			}
		}


		[HarmonyPatch(typeof(Pickable), "Interact")]
		public static class Pickable_Interact_Patch
		{
			private static readonly Dictionary<string, List<string>> pickableResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "eikthyr", new List<string> 
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
				{ "elder", new List<string> 
					{
						"Pickable_Turnip",
						"Pickable_SeedTurnip"
					} 
				},
				{ "bonemass", new List<string> 
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
				{  "moder", new List<string>
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
				{ "yagluth", new List<string> 
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
				{ "queen", new List<string> 
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

					bool bossDefeated = GetBossDefeatedState(bossName);

					if (!bossDefeated && restrictedPickables.Contains(pickableName))
					{
						character.Message(MessageHud.MessageType.Center, "This object refuses to be picked.");
						__result = false;
						return false;
					}
				}

				return true; // Allow original method
			}
		}

		private static bool GetBossDefeatedState(string bossName)
		{
			return bossName switch
			{
				"eikthyr" => eikthyrDefeated,
				"elder" => elderDefeated,
				"bonemass" => bonemassDefeated,
				"moder" => moderDefeated,
				"yagluth" => yagluthDefeated,
				"queen" => queenDefeated,
				_ => false
			};
		}


		/*[HarmonyPatch(typeof(Container), "Awake")] // Awake, Load
		public static class Container_Awake_Patch
		{
			private static Dictionary<string, List<ItemDrop.ItemData>> originalChestItems = new Dictionary<string, List<ItemDrop.ItemData>>();
			// Updated dictionary structure
			private static readonly Dictionary<string, List<string>> chestResourceRestrictions = new Dictionary<string, List<string>>()
			{
				{ "eikthyr", new List<string> { "TreasureChest_forestcrypt", "TreasureChest_trollcave", "TreasureChest_blackforest" } },
				{ "elder", new List<string> { "TreasureChest_swamp", "TreasureChest_sunkencrypt" } },
				{ "bonemass", new List<string> { "TreasureChest_mountaincave" } },
				{ "yagluth", new List<string> { "TreasureChest_dvergrtower", "TreasureChest_dvergrtown" } },
				{ "queen", new List<string> { "TreasureChest_charredfortress", "TreasureChest_ashland_stone" } }
			};

			static void Postfix(Container __instance, ref Inventory ___m_inventory, ref DropTable ___m_defaultItems)
			{
				if (!ConfigManager.automaticProgressionHaltEnabled.Value) return;

				// Remove "(Clone)" if present
				string chestName = __instance.name.Replace("(Clone)", "").Trim();

				// Iterate over chestResourceRestrictions to find matching key
				foreach (var restriction in chestResourceRestrictions)
				{
					string bossName = restriction.Key;
					List<string> restrictedChests = restriction.Value;

					bool bossDefeated = false;

					// Check if the boss is defeated by looking up the global key for the boss
					switch (bossName)
					{
						case "eikthyr":
							bossDefeated = eikthyrDefeated;
							break;
						case "elder":
							bossDefeated = elderDefeated;
							break;
						case "bonemass":
							bossDefeated = bonemassDefeated;
							break;
						case "moder":
							bossDefeated = moderDefeated;
							break;
						case "yagluth":
							bossDefeated = yagluthDefeated;
							break;
						case "queen":
							bossDefeated = queenDefeated;
							break;
					}

					if (!bossDefeated)
					{
						// Check if the current chest's name is in the restricted list for the corresponding progression key
						if (restrictedChests.Contains(chestName))
						{
							MarsarahTweaks.MLog($"[Patch] Processing chest: {chestName}");

							// DefaultItems Magnagement
							if (___m_defaultItems == null)
							{
								MarsarahTweaks.MLog($"[ERROR] DefaultItems is NULL in Awake() for {chestName}");
								return;
							}

							if (!originalChestItems.ContainsKey(chestName))
							{
								originalChestItems[chestName] = new List<ItemDrop.ItemData>();

								foreach (var dropData in ___m_defaultItems.m_drops) // Directly access m_drops
								{
									if (dropData.m_item == null) continue;

									ItemDrop itemDropComponent = dropData.m_item.GetComponent<ItemDrop>();
									if (itemDropComponent == null) continue;

									ItemDrop.ItemData itemData = itemDropComponent.m_itemData.Clone(); // Clone to avoid modifications
									itemData.m_dropPrefab = dropData.m_item; // Keep prefab reference
									originalChestItems[chestName].Add(itemData);
								}

								MarsarahTweaks.MLog($"[Backup] Stored {originalChestItems[chestName].Count} possible items for {chestName}");
							}


							List<ItemDrop.ItemData> dropListItems =  ___m_defaultItems.GetDropListItems();
							MarsarahTweaks.MLog($"[DefaultDrops] Chest {chestName} attempt to get dropListItems: {dropListItems.Count}");

							foreach (var item in dropListItems)
							{
								MarsarahTweaks.MLog($"- {item.m_shared.m_name}");
							}

							// Inventory Management
							if (___m_inventory == null)
							{
								MarsarahTweaks.MLog($"[ERROR] Inventory is NULL in Awake() for {chestName}");
								return;
							}

							// Access private field m_inventory via reflection
							FieldInfo inventoryField = typeof(Inventory).GetField("m_inventory", BindingFlags.NonPublic | BindingFlags.Instance);
							if (inventoryField == null)
							{
								MarsarahTweaks.MLog($"[ERROR] Could not access m_inventory field in Inventory class.");
								return;
							}

							List<ItemDrop.ItemData> inventoryItems = (List<ItemDrop.ItemData>)inventoryField.GetValue(___m_inventory);

							// Log the contents of the inventory
							MarsarahTweaks.MLog($"[Inventory] Chest {chestName} contains {inventoryItems.Count} items.");
							foreach (var item in inventoryItems)
							{
								MarsarahTweaks.MLog($"- {item.m_shared.m_name} (Stack: {item.m_stack})");
							}

							// Proceed with clearing loot
							//MarsarahTweaks.MLog($"✅ Cleared loot for {__instance.name} in Awake()");
							//___m_inventory.RemoveAll();

							return;
						}
					}
				}

				foreach (var originalItemEntry in originalChestItems)
				{
					string originalChestName = originalItemEntry.Key;
					List<ItemDrop.ItemData> originalLoot = originalItemEntry.Value;

					MarsarahTweaks.MLog($"Original chest type {originalChestName} contains the following: ");
					foreach (var item in originalLoot)
					{
						MarsarahTweaks.MLog($"- {item.m_shared.m_name}");
					}
				}

				//MarsarahTweaks.MLog($"Skipping chest {chestName} (No restrictions applied)");
			}
		}*/

		[HarmonyPatch(typeof(ZNetScene), "Update")] // Awake
		class ProgressionHalt_Patch
		{
			private static bool dropsSet = false;
			static void Postfix(ref ZNetScene __instance)
			{
				if (ConfigManager.automaticProgressionHaltEnabled.Value && __instance != null && !dropsSet)
				{
					/*foreach(GameObject prefab in __instance.m_prefabs)
					{
						//MarsarahMod.MModLog(prefab.name);
					}*/

					if (!eikthyrDefeated)
					{
						//MarsarahMod.MModLog("Halting drops for Eikthyr");
						haltDrops(__instance.GetPrefab("Greydwarf")); // done
						haltDrops(__instance.GetPrefab("Greydwarf_Elite")); // done
						haltDrops(__instance.GetPrefab("Greydwarf_Shaman")); // done
						haltDrops(__instance.GetPrefab("Troll")); // done
						haltDrops(__instance.GetPrefab("Skeleton")); // done
						haltDrops(__instance.GetPrefab("Skeleton_Poison")); // done
						haltDrops(__instance.GetPrefab("Pickable_Carrot"));
						haltDrops(__instance.GetPrefab("Pickable_SeedCarrot")); // done
						haltDrops(__instance.GetPrefab("Pickable_Thistle")); // done
						haltDrops(__instance.GetPrefab("Pickable_ForestCryptRandom")); // done
						haltDrops(__instance.GetPrefab("Pickable_ForestCryptRemains01")); // done
						haltDrops(__instance.GetPrefab("Pickable_ForestCryptRemains02")); // done
						haltDrops(__instance.GetPrefab("Pickable_ForestCryptRemains03")); // done
						haltDrops(__instance.GetPrefab("Pickable_ForestCryptRemains04")); // done
						haltDrops(__instance.GetPrefab("BlueberryBush")); // done
						haltDrops(__instance.GetPrefab("Pickable_Mushroom_yellow")); // done
						haltDrops(__instance.GetPrefab("Pickable_SurtlingCoreStand")); // done
						haltDrops(__instance.GetPrefab("Pickable_Tin"));
						haltDrops(__instance.GetPrefab("MineRock_Tin")); // done
						haltDrops(__instance.GetPrefab("MineRock_Copper"));
						haltDrops(__instance.GetPrefab("rock4_copper_frac")); // done
						haltDrops(__instance.GetPrefab("Birch_log_half")); // done
						haltDrops(__instance.GetPrefab("BirchStub")); // done
						haltDrops(__instance.GetPrefab("Oak_log_half")); // done
						haltDrops(__instance.GetPrefab("Pinetree_01_Stub")); // done
						haltDrops(__instance.GetPrefab("PineTree_log_half")); // done
						haltDrops(__instance.GetPrefab("BonePileSpawner")); // done
						haltDrops(__instance.GetPrefab("Spawner_GreydwarfNest")); // done
						haltDrops(__instance.GetPrefab("barrell")); // done
					}
					if (!elderDefeated)
					{
						//MarsarahMod.MModLog("Halting drops for Elder");
						haltDrops(__instance.GetPrefab("Leech")); // done
						haltDrops(__instance.GetPrefab("Draugr")); // done
						haltDrops(__instance.GetPrefab("Draugr_Elite")); // done
						haltDrops(__instance.GetPrefab("Surtling")); // done
						haltDrops(__instance.GetPrefab("Blob")); // done
						haltDrops(__instance.GetPrefab("BlobElite")); // done
						haltDrops(__instance.GetPrefab("Wraith")); // done
						haltDrops(__instance.GetPrefab("Abomination")); // done
						haltDrops(__instance.GetPrefab("sapling_turnip"));
						haltDrops(__instance.GetPrefab("sapling_seedturnip"));
						haltDrops(__instance.GetPrefab("Pickable_Turnip"));
						haltDrops(__instance.GetPrefab("Pickable_SeedTurnip")); // done
						haltDrops(__instance.GetPrefab("Pickable_SunkenCryptRandom")); // done
						haltDrops(__instance.GetPrefab("MineRock_Iron"));
						haltDrops(__instance.GetPrefab("dungeon_sunkencrypt_irongate_rusty"));
						haltDrops(__instance.GetPrefab("mudpile_frac"));
						haltDrops(__instance.GetPrefab("mudpile2_frac")); // done
						haltDrops(__instance.GetPrefab("mudpile_beacon")); // done
						haltDrops(__instance.GetPrefab("SwampTree1_log")); // done
						//haltDrops(__instance.GetPrefab("SwampTree2_log")); // Dunno what this is
						haltDrops(__instance.GetPrefab("Spawner_DraugrPile")); // done
						haltDrops(__instance.GetPrefab("GuckSack")); // done
						haltDrops(__instance.GetPrefab("GuckSack_small")); // done
					}
					if (!bonemassDefeated)
					{
						//MarsarahMod.MModLog("Halting drops for Bonemass");
						haltDrops(__instance.GetPrefab("Wolf")); // done
						haltDrops(__instance.GetPrefab("Ulv")); // done
						haltDrops(__instance.GetPrefab("Fenring")); // done
						haltDrops(__instance.GetPrefab("Fenring_Cultist")); // done
						haltDrops(__instance.GetPrefab("Hatchling")); // done
						haltDrops(__instance.GetPrefab("StoneGolem")); // done
						haltDrops(__instance.GetPrefab("Serpent")); // done
						haltDrops(__instance.GetPrefab("Leviathan")); // Abyssal Barnacle - done
						haltDrops(__instance.GetPrefab("Pickable_DragonEgg")); // done
						haltDrops(__instance.GetPrefab("Pickable_MountainCaveRandom")); // done
						haltDrops(__instance.GetPrefab("Pickable_MountainCaveCrystal")); // done
						haltDrops(__instance.GetPrefab("Pickable_MountainCaveObsidian"));
						haltDrops(__instance.GetPrefab("Pickable_MountainRemains01_buried"));
						haltDrops(__instance.GetPrefab("Pickable_Hairstrands01"));
						haltDrops(__instance.GetPrefab("Pickable_Hairstrands02"));
						haltDrops(__instance.GetPrefab("Pickable_MeatPile")); // done
						haltDrops(__instance.GetPrefab("Pickable_Obsidian"));
						haltDrops(__instance.GetPrefab("Pickable_Onion"));
						haltDrops(__instance.GetPrefab("Pickable_SeedOnion"));
						haltDrops(__instance.GetPrefab("sapling_onion"));
						haltDrops(__instance.GetPrefab("sapling_seedonion"));
						haltDrops(__instance.GetPrefab("MineRock_Obsidian")); // done
						haltDrops(__instance.GetPrefab("silvervein_frac")); // done
						haltDrops(__instance.GetPrefab("rock3_silver_frac"));
						haltDrops(__instance.GetPrefab("fenrirhide_hanging")); // done
						haltDrops(__instance.GetPrefab("fenrirhide_hanging_door"));
						haltDrops(__instance.GetPrefab("hanging_hairstrands")); // done
						haltDrops(__instance.GetPrefab("cloth_hanging_door")); // done
						haltDrops(__instance.GetPrefab("cloth_hanging_door_double")); // done
						haltDrops(__instance.GetPrefab("cloth_hanging_long")); // done
						haltDrops("MountainKit"); // done
						haltDrops("mountainkit"); // done
					}
					if (!moderDefeated)
					{
						//MarsarahMod.MModLog("Halting drops for Moder");
						haltDrops(__instance.GetPrefab("Goblin")); // done
						haltDrops(__instance.GetPrefab("GoblinArcher")); // done
						haltDrops(__instance.GetPrefab("GoblinBrute")); // done
						haltDrops(__instance.GetPrefab("GoblinShaman")); // done
						haltDrops(__instance.GetPrefab("Deathsquito")); // done
						haltDrops(__instance.GetPrefab("Lox")); // done
						haltDrops(__instance.GetPrefab("BlobTar")); // done
						haltDrops(__instance.GetPrefab("CloudberryBush")); // done
						haltDrops(__instance.GetPrefab("Pickable_Barley"));
						haltDrops(__instance.GetPrefab("Pickable_Barley_Wild")); // done
						haltDrops(__instance.GetPrefab("sapling_flax"));
						haltDrops(__instance.GetPrefab("Pickable_Flax"));
						haltDrops(__instance.GetPrefab("Pickable_Flax_Wild")); // done
						haltDrops(__instance.GetPrefab("Pickable_Tar")); // done
						haltDrops(__instance.GetPrefab("Pickable_TarBig"));
						haltDrops(__instance.GetPrefab("goblin_totempole")); // done
					}
					if (!yagluthDefeated)
					{
						//MarsarahMod.MModLog("Halting drops for Yagluth");
						haltDrops(__instance.GetPrefab("Dverger")); //  done
						haltDrops(__instance.GetPrefab("DvergerMage")); // done
						haltDrops(__instance.GetPrefab("Tick")); // done
						haltDrops(__instance.GetPrefab("Seeker")); // done
						haltDrops(__instance.GetPrefab("SeekerBrute")); // done
						haltDrops(__instance.GetPrefab("SeekerBrood")); // done
						haltDrops(__instance.GetPrefab("SeekerQueen"));
						haltDrops(__instance.GetPrefab("Gjall")); // done
						haltDrops(__instance.GetPrefab("Hare")); // done
						haltDrops(__instance.GetPrefab("Pickable_DvergerThing"));
						haltDrops(__instance.GetPrefab("Pickable_DvergrLantern")); // done
						haltDrops(__instance.GetPrefab("Pickable_DvergrMineTreasure")); // done - this is just coin
						haltDrops(__instance.GetPrefab("Pickable_DvergrStein"));
						haltDrops(__instance.GetPrefab("Pickable_Mushroom_JotunPuffs")); // done
						haltDrops(__instance.GetPrefab("Pickable_Mushroom_Magecap")); // done
						haltDrops(__instance.GetPrefab("Pickable_RoyalJelly")); // done
						haltDrops(__instance.GetPrefab("Pickable_BlackCoreStand")); // done
						haltDrops(__instance.GetPrefab("sapling_jotunpuffs"));
						haltDrops(__instance.GetPrefab("sapling_magecap"));
						haltDrops(__instance.GetPrefab("giant_arm"));
						haltDrops(__instance.GetPrefab("giant_brain_frac")); // done
						haltDrops(__instance.GetPrefab("giant_helmet1_destruction")); // done
						haltDrops(__instance.GetPrefab("giant_helmet2_destruction"));
						haltDrops(__instance.GetPrefab("giant_ribs_frac")); // done
						haltDrops(__instance.GetPrefab("giant_skull_frac")); // done
						haltDrops(__instance.GetPrefab("giant_sword1_destruction")); // done
						haltDrops(__instance.GetPrefab("giant_sword2_destruction")); // done
						haltDrops(__instance.GetPrefab("yggashoot_log_half")); // done
						haltDrops(__instance.GetPrefab("YggaShoot_small1")); // done
						haltDrops(__instance.GetPrefab("blackmarble_post01")); // done
						haltDrops(__instance.GetPrefab("trader_wagon_destructable")); // <--------------
						haltDrops(__instance.GetPrefab("blackmarble_altar_crystal")); // <--------------
						haltDrops("dvergrprops");
						haltDrops("dvergrtown");
					}
					/*else if (MarsarahMod.seekerSoldierTrophyEnabled.Value)
					{
						setSeekerTrophyDrops(__instance.GetPrefab("SeekerBrute"));
					}*/
					if (!queenDefeated)
					{
						//MarsarahMod.MModLog("Halting drops for Queen");
						haltDrops(__instance.GetPrefab("DvergerAshlands")); // done
						haltDrops(__instance.GetPrefab("Charred_Archer")); // done
						haltDrops(__instance.GetPrefab("Charred_Archer_Fader"));
						haltDrops(__instance.GetPrefab("Charred_Melee")); // done
						haltDrops(__instance.GetPrefab("Charred_Melee_Fader"));
						haltDrops(__instance.GetPrefab("Charred_Twitcher")); // done
						haltDrops(__instance.GetPrefab("Charred_Mage")); // done
						haltDrops(__instance.GetPrefab("Morgen"));
						haltDrops(__instance.GetPrefab("Morgen_NonSleeping"));
						haltDrops(__instance.GetPrefab("Volture")); // done
						haltDrops(__instance.GetPrefab("Asksvin")); // done
						haltDrops(__instance.GetPrefab("FallenValkyrie")); // done
						haltDrops(__instance.GetPrefab("BlobLava")); // done
						haltDrops(__instance.GetPrefab("BonemawSerpent")); // done
						haltDrops(__instance.GetPrefab("lavarock_ashlands1")); // MC
						haltDrops(__instance.GetPrefab("VineAsh")); // done
						haltDrops(__instance.GetPrefab("Pickable_Ashstone"));
						haltDrops(__instance.GetPrefab("Pickable_Charredskull"));
						haltDrops(__instance.GetPrefab("Pickable_Fiddlehead")); // done
						haltDrops(__instance.GetPrefab("Pickable_Meteorite"));
						haltDrops(__instance.GetPrefab("Pickable_MoltenCoreStand")); // This works in Putrid hole, but not in Charred Fortress
						haltDrops(__instance.GetPrefab("Pickable_SmokePuff")); // done
						haltDrops(__instance.GetPrefab("Pickable_SulfurRock"));
						haltDrops(__instance.GetPrefab("Pickable_VoltureEgg"));
						haltDrops(__instance.GetPrefab("Pickable_SulfurRock"));
						haltDrops(__instance.GetPrefab("FlametalRockstand"));
						haltDrops(__instance.GetPrefab("FlametalRockstand_frac"));
						haltDrops(__instance.GetPrefab("LeviathanLava")); // done
						haltDrops(__instance.GetPrefab("dvergrprops_crate_ashlands")); // done
						haltDrops(__instance.GetPrefab("AshlandsTreeLogHalf1")); // done
						haltDrops(__instance.GetPrefab("AshlandsTreeLogHalf2"));
						haltDrops(__instance.GetPrefab("AshlandsTreeStump1"));
						haltDrops(__instance.GetPrefab("AshlandsTreeStump2")); // done
						haltDrops(__instance.GetPrefab("AshlandsTreeStump3"));
						haltDrops(__instance.GetPrefab("AshlandsBranch1")); // done
						haltDrops(__instance.GetPrefab("AshlandsBranch2"));
						haltDrops(__instance.GetPrefab("AshlandsBranch3"));
						haltDrops(__instance.GetPrefab("AshlandsBush1"));
						haltDrops(__instance.GetPrefab("AshlandsBush2"));
						haltDrops(__instance.GetPrefab("Ashlands_rock1"));
						haltDrops(__instance.GetPrefab("Ashlands_rock2"));
						haltDrops(__instance.GetPrefab("MineRock_Meteorite"));
						haltDrops(__instance.GetPrefab("UnstableLavaRock")); // done
						haltDrops(__instance.GetPrefab("Spawner_CharredCross")); // Effigy of Malice - done
						haltDrops(__instance.GetPrefab("Spawner_CharredStone")); // Monument of Torment - done
						haltDrops(__instance.GetPrefab("Spawner_CharredStone_Elite"));
						haltDrops(__instance.GetPrefab("GraveStone_Broken_CharredTwitcherNest")); // done
						haltDrops(__instance.GetPrefab("GraveStone_CharredTwitcherNest")); // done
						haltDrops(__instance.GetPrefab("ashland_pot1_green")); // done
						haltDrops(__instance.GetPrefab("ashland_pot1_red")); // done
						haltDrops(__instance.GetPrefab("ashland_pot2_green")); // done
						haltDrops(__instance.GetPrefab("ashland_pot2_red"));
						haltDrops(__instance.GetPrefab("ashland_pot3_green")); // done
						haltDrops(__instance.GetPrefab("ashland_pot3_red")); // done
						haltDrops(__instance.GetPrefab("CharredBanner1")); // done
						haltDrops(__instance.GetPrefab("CharredBanner2")); // done
						haltDrops(__instance.GetPrefab("CharredBanner3")); // done
						haltDrops(__instance.GetPrefab("Charred_altar_bellfragment")); // done
						haltDrops(__instance.GetPrefab("piece_Charred_Balista")); // done

						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Floor"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_Pillar"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_Pillar_base"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_Pillar_base_frac"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_Pillar_frac"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_PillarTop"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_PillarTop_frac"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_PillarTopStone"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_PillarTopStone_frac"));
						haltDrops(__instance.GetPrefab("Ashlands_Fortress_Wall_Spikes"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_1point5x1point5"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_1point5x1point5_broken"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_3x3"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_3x3_broken1"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_3x3_broken2"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_3x3_broken3"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_6x6"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_6x6_broken1"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Floor_6x6_broken2"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Ramp"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Ramp_Upsidedown"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_TopStone"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_twist_ArchBig"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_twist_PillarBase"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_twist_PillarBaseSmall"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Wall_4x6"));
						haltDrops(__instance.GetPrefab("Ashlands_Ruins_Wall_Windows_Broken_4x6"));

						haltDrops(__instance.GetPrefab("Ashland_Stair"));
						haltDrops(__instance.GetPrefab("Ashland_Steepstair"));
						haltDrops(__instance.GetPrefab("Ashlands_Altar"));
						haltDrops(__instance.GetPrefab("Ashlands_Arch2_Broken1"));
						haltDrops(__instance.GetPrefab("Ashlands_Arch2_Broken2"));
						haltDrops(__instance.GetPrefab("Ashlands_ArchRoof"));
						haltDrops(__instance.GetPrefab("Ashlands_ArchRoofDamaged"));
						haltDrops(__instance.GetPrefab("Ashlands_ArchRoofDamaged_half1"));
						haltDrops(__instance.GetPrefab("Ashlands_ArchRoofDamaged_half2"));
						haltDrops(__instance.GetPrefab("Ashlands_ArchRoofLong_Damaged"));
						haltDrops(__instance.GetPrefab("Ashlands_Boss_Pillar"));
						haltDrops(__instance.GetPrefab("Ashlands_Boss_Pillar_Twist_broken1"));
						haltDrops(__instance.GetPrefab("Ashlands_Boss_Pillar_Twist_broken2"));
						haltDrops(__instance.GetPrefab("Ashlands_Boss_Pillar_Twist_broken3"));
						haltDrops(__instance.GetPrefab("Ashlands_Floor"));
						haltDrops(__instance.GetPrefab("Ashlands_floor_large"));
						haltDrops(__instance.GetPrefab("Ashlands_floor_large_fractured"));
						haltDrops(__instance.GetPrefab("Ashlands_Pillar4"));
						haltDrops(__instance.GetPrefab("Ashlands_Pillar4_tip_broken1"));
						haltDrops(__instance.GetPrefab("Ashlands_Pillar4_tip_broken2"));
						haltDrops(__instance.GetPrefab("Ashlands_Pillar4_tip2_broken1"));
						haltDrops(__instance.GetPrefab("Ashlands_Pillar4_tip2_broken2"));
						haltDrops(__instance.GetPrefab("Ashlands_Pillar4_tip3_broken1"));
						haltDrops(__instance.GetPrefab("Ashlands_Pillar4_tip3_broken2"));
						haltDrops(__instance.GetPrefab("Ashlands_Pillar4_tip3_broken3"));
						haltDrops(__instance.GetPrefab("Ashlands_PillarBase3_double"));
						haltDrops(__instance.GetPrefab("Ashlands_WallBlock"));

						haltDrops(__instance.GetPrefab("rock4_ashlands_frac"));
						haltDrops(__instance.GetPrefab("cliff_ashlands_Arch_frac"));
						haltDrops(__instance.GetPrefab("cliff_ashlands1_frac"));
						haltDrops(__instance.GetPrefab("cliff_ashlands2_frac"));
						haltDrops(__instance.GetPrefab("cliff_ashlands4_frac"));
						haltDrops(__instance.GetPrefab("cliff_ashlands6_frac"));
						haltDrops(__instance.GetPrefab("cliff_ashlands7_HalfArch_frac"));
						haltDrops(__instance.GetPrefab("cliff_ashlandsflowrock_frac"));
					}

					dropsSet = true;
				}

				// Better Seeker Soldier Trophy Rate =======================================================
				/*if (MarsarahMod.seekerSoldierTrophyEnabled.Value && !MarsarahMod.automaticProgressionHaltEnabled.Value && __instance != null)
				{
					setSeekerTrophyDrops(__instance.GetPrefab("SeekerBrute"));
				}*/
			}
		}

		// Overloaded haltDrops method to handle string-based StartsWith
		static void haltDrops(string prefabPrefix)
		{
			foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
			{
				if (prefab.name.StartsWith(prefabPrefix))
				{
					haltDrops(prefab); // Call the regular haltDrops with the GameObject
				}
			}
		}

		static void haltDrops(GameObject prefab)
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
				//{ typeof(Pickable), haltDropsPickable },
				//{ typeof(PickableItem), haltDropsPickableItem },
				{ typeof(DropOnDestroyed), haltDropsOnDestroyed },
				{ typeof(Destructible), haltDropsDestructible },
				{ typeof(TreeLog), haltDropsTreeLog },
				//{ typeof(Container), haltDropsContainer }
			};

			bool modified = false; // Track if any drop was halted

			// Iterate through all handlers and apply every matching one
			foreach (var handler in dropHandlers)
			{
				if (prefab.GetComponent(handler.Key) != null)
				{
					handler.Value(prefab);
					if (handler.Value(prefab))
					{
						//MarsarahTweaks.MLog($"Halted drop for {prefab.name} as component {handler.Key}");
						modified = true; // Mark that at least one modification was made
					}
				}
			}

			// Log components if no drop handler was triggered
			if (!modified)
			{
				Component[] prefabComponents = prefab.GetComponents<Component>();
				foreach (Component comp in prefabComponents)
				{
					MarsarahTweaks.MLog(prefab.name + " - " + comp.ToString());
				}
			}
		}


		static bool haltDropsMob(GameObject prefab)
		{
			CharacterDrop characterDrop = prefab.GetComponent<CharacterDrop>();
			if (characterDrop != null)
			{
				foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
				{
					drop.m_chance = 0;
				}
				return true;
			}
			return false;
		}

		static bool haltDropsMine(GameObject prefab)
		{
			MineRock prefabComponent = prefab.GetComponent<MineRock>();
			if (prefabComponent != null)
			{
				prefabComponent.m_dropItems.m_dropChance = 0;
				return true;
			}
			return false;
		}

		static bool haltDropsMine5(GameObject prefab)
		{
			MineRock5 prefabComponent = prefab.GetComponent<MineRock5>();
			if (prefabComponent != null)
			{
				prefabComponent.m_dropItems.m_dropChance = 0;
				return true;
			}
			return false;
		}

		static bool haltDropsOnDestroyed(GameObject prefab)
		{
			DropOnDestroyed prefabComponent = prefab.GetComponent<DropOnDestroyed>();
			if (prefabComponent != null)
			{
				prefabComponent.m_dropWhenDestroyed.m_dropChance = 0;
				return true;
			}
			return false;
		}

		static bool haltDropsDestructible(GameObject prefab)
		{
			Destructible prefabComponent = prefab.GetComponent<Destructible>();
			if (prefabComponent != null)
			{
				prefabComponent.m_spawnWhenDestroyed = null;
				return true;
			}
			return false;
		}

		/*static bool haltDropsPickable(GameObject prefab)
		{
			Pickable prefabComponent = prefab.GetComponent<Pickable>();
			if (prefabComponent != null)
			{
				prefabComponent.m_itemPrefab = ZNetScene.instance.GetPrefab("Pukeberries");
				prefabComponent.m_amount = 1;
				prefabComponent.m_bonusYieldAmount = 0;
				prefabComponent.m_extraDrops.m_dropMin = 0;
				prefabComponent.m_extraDrops.m_dropMax = 0;
				return true;
			}
			return false;
		}

		static bool haltDropsPickableItem(GameObject prefab)
		{
			PickableItem prefabComponent = prefab.GetComponent<PickableItem>();
			if (prefabComponent != null)
			{
				prefabComponent.m_itemPrefab = ZNetScene.instance.GetPrefab("Pukeberries")?.GetComponent<ItemDrop>();
				return true;
			}
			return false;
		}*/

		static bool haltDropsTreeLog(GameObject prefab)
		{
			TreeLog prefabComponent = prefab.GetComponent<TreeLog>();
			if (prefabComponent != null)
			{
				prefabComponent.m_dropWhenDestroyed = null;
				return true;
			}
			return false;
		}

		/*static bool haltDropsPiece(GameObject prefab)
		{
			Piece prefabComponent = prefab.GetComponent<Piece>();
			if (prefabComponent != null)
			{
				if (prefabComponent.IsPlacedByPlayer())
				{
					return false;
				}

				prefabComponent.
				return true;
			}
			return false;
		}*/

		/*static bool haltDropsContainer(GameObject prefab)
		{
			Container container = prefab.GetComponent<Container>();
			if (container == null)
			{
				MarsarahTweaks.MLog($"[ERROR] No Container found on {prefab.name}");
				return false;
			}

			MarsarahTweaks.MLog($"Found Container on {prefab.name}, attempting to access m_inventory...");

			// Use reflection to access the private field m_inventory
			FieldInfo inventoryField = typeof(Container).GetField("m_inventory", BindingFlags.NonPublic | BindingFlags.Instance);
			if (inventoryField == null)
			{
				MarsarahTweaks.MLog($"[ERROR] Could not find m_inventory field via reflection for {prefab.name}");
				return false;
			}

			Inventory inventory = (Inventory)inventoryField.GetValue(container);
			if (inventory == null)
			{
				MarsarahTweaks.MLog($"[ERROR] Inventory is NULL for {prefab.name}");
				return false;
			}

			MarsarahTweaks.MLog($"Successfully accessed inventory for {prefab.name}. Removing items...");

			inventory.RemoveAll();

			MarsarahTweaks.MLog($"✅ Cleared loot for {prefab.name}");
			return true;
		}*/
	}
}
