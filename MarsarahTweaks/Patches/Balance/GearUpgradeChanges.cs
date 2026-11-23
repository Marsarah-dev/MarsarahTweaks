using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class GearUpgradeChanges
	{
		private static readonly LogManager log = new LogManager("Gear Upgrade", LogManager.LogLevel.Info);

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class GearUpgradeUnlock_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateGearRecipeUnlock(__instance, false);
				UpdateGearRecipeStations(__instance, false);
			}
		}

		[HarmonyPatch(typeof(Recipe), "GetRequiredStationLevel")]
		class GearUpgradeUnlockStationLevel_Patch
		{
			static bool Prefix(int quality, ref int ___m_minStationLevel, ref int __result, ref bool __runOriginal)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated())
				{
					// Run the original function on dedicated servers
					__runOriginal = true;
					return true;
				}

				if (ConfigManager.GearUpgradeUnlockEnabled.Value)
				{
					__runOriginal = false;
					__result = Mathf.Max(0, ___m_minStationLevel) + (quality - 1);
					return false;
				}

				__runOriginal = true;
				return true;
			}
		}


		// Dictionaries
		private static readonly Dictionary<string, int> originalRecipeStationLevels = new Dictionary<string, int>();

		private static readonly Dictionary<string, int> newRecipeStationLevels = new Dictionary<string, int>()
		{
			// Heavy Armor

			// Black Forest
			{ "Recipe_HelmetBronze", 0 }, { "Recipe_ArmorBronzeChest", 0 }, { "Recipe_ArmorBronzeLegs", 0 },

			// Mistlands
			{ "Recipe_HelmetCarapace", 0 }, { "Recipe_ArmorCarapaceChest", 0 }, { "Recipe_ArmorCarapaceLegs", 0 },

			// Ashlands
			{ "Recipe_HelmetFlametal", 2 }, { "Recipe_ArmorFlametalChest", 2 }, { "Recipe_ArmorFlametalLegs", 2 },

			// Deep North
			{ "Recipe_HelmetGold", 3 }, { "Recipe_HelmetGoldUncooked", 3 },
			{ "Recipe_ArmorGoldChest", 3 }, { "Recipe_ArmorGoldChestUncooked", 3 },
			{ "Recipe_ArmorGoldLegs", 3 }, { "Recipe_ArmorGoldLegsUncooked", 3 },


			// Medium Armor

			// Ashlands
			{ "Recipe_HelmetMedium_Ashlands", 2 }, { "Recipe_ArmorMediumChest_Ashlands", 2 }, { "Recipe_ArmorMediumLegs_Ashlands", 2 },

			// Deep North
			{ "Recipe_HelmetGoldMedium", 3 }, { "Recipe_HelmetGoldMediumUncooked", 3 },
			{ "Recipe_ArmorGoldMediumChest", 3 }, { "Recipe_ArmorGoldMediumChestUncooked", 3 },
			{ "Recipe_ArmorGoldMediumLegs", 3 }, { "Recipe_ArmorGoldMediumLegsUncooked", 3 },


			// Light Armor

			// Meadows
			{ "Recipe_HelmetLeather", 1 }, { "Recipe_ArmorLeatherChest", 1 }, { "Recipe_ArmorLeatherLegs", 1 },

			// Black Forest
			{ "Recipe_HelmetTrollLeather", 1 }, { "Recipe_ArmorTrollLeatherChest", 1 }, { "Recipe_ArmorTrollLeatherLegs", 1 },
			{ "Recipe_HelmetBerserker", 1 }, { "Recipe_ArmorBerserkerChest", 1 }, { "Recipe_ArmorBerserkerLegs", 1 },


			// Mage Armor

			// Mistlands
			{ "Recipe_HelmetMage", 0 }, { "Recipe_ArmorMageChest", 0 }, { "Recipe_ArmorMageLegs", 0 },

			// Ashlands
			{ "Recipe_HelmetMage_Ashlands", 1 }, { "Recipe_ArmorMageChest_Ashlands", 1 }, { "Recipe_ArmorMageLegs_Ashlands", 1 },

			// Deep North
			{ "Recipe_HelmetGoldMage", 2 }, { "Recipe_HelmetGoldMageUncooked", 2 },
			{ "Recipe_ArmorGoldMageChest", 2 }, { "Recipe_ArmorGoldMageChestUncooked", 2 },
			{ "Recipe_ArmorGoldMageLegs", 2 }, { "Recipe_ArmorGoldMageLegsUncooked", 2 },


			// Capes

			// Meadows
			{ "Recipe_CapeDeerHide", 1 },

			// Black Forest
			{ "Recipe_CapeTrollHide", 1 },

			// Mistlands
			{ "Recipe_CapeFeather", 0 },

			// Ashlands
			{ "Recipe_CapeAsksvin", 1 }, { "Recipe_CapeAsh", 2 },

			// Deep North
			{ "Recipe_CapeDeepNorth", 3 }, { "Recipe_CapeDeepNorthMage", 2 },


			// Shields

			// Deep North
			{ "Recipe_ShieldBucklerGold", 3 }, { "Recipe_ShieldBucklerGoldUncooked", 3 },
			{ "Recipe_ShieldRoundGold", 3 }, { "Recipe_ShieldRoundGoldUncooked", 3 },
			{ "Recipe_ShieldTowerGold", 3 }, { "Recipe_ShieldTowerGoldUncooked", 3 },


			// Melee Weapons

			// Meadows
			{ "Recipe_SledgeStagbreaker", 1 },

			// Black Forest
			{ "Recipe_AtgeirBronze", 0 }, { "Recipe_AxeBronze", 0 }, { "Recipe_KnifeCopper", 0 },
			{ "Recipe_MaceBronze", 0 }, { "Recipe_PickaxeBronze", 0 }, { "Recipe_SpearBronze", 0 },
			{ "Recipe_SwordBronze", 0 }, { "Recipe_FistBjornClaw", 1 },

			// Mistlands
			{ "Recipe_AxeJotunBane", 0 }, { "Recipe_SpearCarapace", 0 }, { "Recipe_SwordMistwalker", 0 },
			{ "Recipe_AtgeirHimminAfl", 0 }, { "Recipe_KnifeSkollAndHati", 0 }, { "Recipe_SledgeDemolisher", 0 },
			{ "Recipe_SwordKrom", 0 },

			// Ashlands
			{ "Recipe_MaceEldner", 2 }, { "Recipe_SpearSplitner", 2 }, { "Recipe_SwordNiedhogg", 2 },
			{ "Recipe_AxeBerzerkr", 2 }, { "Recipe_SwordSlayer", 2 }, { "Recipe_SwordFire", 3 }, // Dyrnwyn

			// Ashlands - Elemental Variants
			{ "Recipe_MaceEldner_Blood", 3 }, { "Recipe_MaceEldner_Lightning", 3 }, { "Recipe_MaceEldner_Nature", 3 },
			{ "Recipe_SpearSplitner_Blood", 3 }, { "Recipe_SpearSplitner_Lightning", 3 }, { "Recipe_SpearSplitner_Nature", 3 },
			{ "Recipe_SwordNiedhogg_Blood", 3 }, { "Recipe_SwordNiedhogg_Lightning", 3 }, { "Recipe_SwordNiedhogg_Nature", 3 },
			{ "Recipe_AxeBerzerkr_Blood", 3 }, { "Recipe_AxeBerzerkr_Lightning", 3 }, { "Recipe_AxeBerzerkr_Nature", 3 },
			{ "Recipe_SwordSlayer_Blood", 3 }, { "Recipe_SwordSlayer_Lightning", 3 }, { "Recipe_SwordSlayer_Nature", 3 },

			// Deep North
			{ "Recipe_SwordGold", 3 }, { "Recipe_SwordGoldUncooked", 3 },
			{ "Recipe_THSwordGold", 3 }, { "Recipe_THSwordGoldUncooked", 3 },
			{ "Recipe_AxeGold", 3 }, { "Recipe_AxeGoldUncooked", 3 },
			{ "Recipe_BattleaxeGold", 3 }, { "Recipe_BattleaxeGoldUncooked", 3 },
			{ "Recipe_MaceGold", 3 }, { "Recipe_MaceGoldUncooked", 3 },
			{ "Recipe_SledgeGold", 3 }, { "Recipe_SledgeGoldUncooked", 3 },
			{ "Recipe_AtgeirGold", 3 }, { "Recipe_AtgeirGoldUncooked", 3 },
			{ "Recipe_SpearGold", 3 }, { "Recipe_SpearGoldUncooked", 3 },
			{ "Recipe_KnifeGold", 3 }, { "Recipe_KnifeGoldUncooked", 3 },
			{ "Recipe_FistweaponGold", 3 }, { "Recipe_FistweaponGoldUncooked", 3 },

			// Deep North - Elemental Variants
			{ "Recipe_SwordGold_FrostFire", 3 }, { "Recipe_SwordGold_BloodLightning", 3 },
			{ "Recipe_THSwordGold_FrostFire", 3 }, { "Recipe_THSwordGold_BloodLightning", 3 },
			{ "Recipe_AxeGold_FrostFire", 3 }, { "Recipe_AxeGold_BloodLightning", 3 },
			{ "Recipe_BattleaxeGold_FrostFire", 3 }, { "Recipe_BattleaxeGold_BloodLightning", 3 },
			{ "Recipe_MaceGold_FrostFire", 3 }, { "Recipe_MaceGold_BloodLightning", 3 },
			{ "Recipe_SledgeGold_FrostFire", 3 }, { "Recipe_SledgeGold_BloodLightning", 3 },
			{ "Recipe_AtgeirGold_FrostFire", 3 }, { "Recipe_AtgeirGold_BloodLightning", 3 },
			{ "Recipe_SpearGold_FrostFire", 3 }, { "Recipe_SpearGold_BloodLightning", 3 },
			{ "Recipe_KnifeGold_FrostFire", 3 }, { "Recipe_KnifeGold_BloodLightning", 3 },
			{ "Recipe_FistGold_FrostFire", 3 }, { "Recipe_FistGold_BloodLightning", 3 },


			// Ranged Weapons

			// Mistlands
			{ "Recipe_BowSpineSnap", 0 }, { "Recipe_CrossbowArbalest", 0 },

			// Ashlands
			{ "Recipe_BowAshlands", 2 }, { "Recipe_CrossbowRipper", 2 },

			// Ashlands - Elemental Variants
			{ "Recipe_BowAshlands_Blood", 3 }, { "Recipe_BowAshlands_Lightning", 3 }, { "Recipe_BowAshlands_Nature", 3 },
			{ "Recipe_CrossbowRipper_Blood", 3 }, { "Recipe_CrossbowRipper_Lightning", 3 }, { "Recipe_CrossbowRipper_Nature", 3 },

			// Deep North
			{ "Recipe_BowGold", 3 }, { "Recipe_BowGoldUncooked", 3 },
			{ "Recipe_CrossbowGold", 3 }, { "Recipe_CrossbowGoldUncooked", 3 },

			// Deep North - Elemental Variants
			{ "Recipe_BowGold_FrostFire", 3 }, { "Recipe_BowGold_BloodLightning", 3 },
			{ "Recipe_CrossbowGold_FrostFire", 3 }, { "Recipe_CrossbowGold_BloodLightning", 3 },


			// Staves

			// Mistlands
			{ "Recipe_StaffFireball", 0 }, { "Recipe_StaffIceShards", 0 },
			{ "Recipe_StaffShield", 0 }, { "Recipe_StaffSkeleton", 0 },

			// Ashlands
			{ "Recipe_StaffClusterbomb", 1 }, { "Recipe_StaffGreenRoots", 1 },
			{ "Recipe_StaffLightning", 1 }, { "Recipe_StaffRedTroll", 1 },

			// Deep North
			{ "Recipe_StaffSpiritCaller", 2 }, { "Recipe_StaffSpiritCaller_Upgrade", 2 },
			{ "Recipe_StaffFrostOrbs", 2 }, { "Recipe_StaffFrostOrbs_Upgrade", 2 },
			{ "Recipe_StaffOrbOfAhri", 2 }, { "Recipe_StaffOrbOfAhri_Upgrade", 2 },
			{ "Recipe_StaffThunderBlood", 2 }, { "Recipe_StaffThunderBlood_Upgrade", 2 }
		};


		private static readonly Dictionary<string, string> originalRecipeStations = new Dictionary<string, string>();
		private static readonly Dictionary<string, string> newRecipeStations = new Dictionary<string, string>()
		{
			{ "Recipe_HelmetBerserkerUndead", "piece_workbench" }, { "Recipe_ArmorBerserkerUndeadChest", "piece_workbench" }, { "Recipe_ArmorBerserkerUndeadLegs", "piece_workbench" }
		};

		public static void UpdateGearRecipeUnlock(ObjectDB objDB, bool wasChanged)
		{
			if (ConfigManager.GearUpgradeUnlockEnabled.Value)
			{
				foreach (var entry in newRecipeStationLevels)
				{
					string recipeName = entry.Key;
					int newStationLevel = entry.Value;

					Recipe recipe = objDB.m_recipes.Find(r => r.name == recipeName);
					if (recipe == null) continue;

					// Backup original value if not already stored
					if (!originalRecipeStationLevels.ContainsKey(recipeName))
					{
						log.Info($"Backing up {recipeName} min station level value: {recipe.m_minStationLevel}");
						originalRecipeStationLevels[recipeName] = recipe.m_minStationLevel;
					}

					// Apply new value
					log.Info($"Applying new min station level for {recipeName}: {newStationLevel}");
					recipe.m_minStationLevel = newStationLevel;
				}
			}
			else if (wasChanged)
			{
				foreach (var entry in originalRecipeStationLevels)
				{
					string recipeName = entry.Key;
					int originalLevel = entry.Value;

					Recipe recipe = objDB.m_recipes.Find(r => r.name == recipeName);
					if (recipe == null) continue;

					// Restore original value
					log.Info($"Restoring {recipeName} min station level value: {originalLevel}");
					recipe.m_minStationLevel = originalLevel;
				}

				// Remove backups after restoring
				originalRecipeStationLevels.Clear();
			}
		}

		public static void UpdateGearRecipeStations(ObjectDB objDB, bool wasChanged)
		{
			if (ConfigManager.GearUpgradeUnlockEnabled.Value)
			{
				foreach (var entry in newRecipeStations)
				{
					string recipeName = entry.Key;
					string newStation = entry.Value;

					Recipe recipe = objDB.m_recipes.Find(r => r.name == recipeName);
					if (recipe == null) continue;

					// Backup original value if not already stored
					if (!originalRecipeStations.ContainsKey(recipeName))
					{
						string original = recipe.m_craftingStation != null ? recipe.m_craftingStation.name : "null";
						log.Info($"Backing up {recipeName} crafting station value: {original}");
						originalRecipeStations[recipeName] = original;
					}

					// Apply new value
					CraftingStation stationPrefab = ZNetScene.instance.GetPrefab(newStation)?.GetComponent<CraftingStation>();
					if (stationPrefab == null)
					{
						log.Warn($"Could not find crafting station '{newStation}' for {recipeName}");
						continue;
					}

					log.Info($"Applying new crafting station for {recipeName}: {newStation}");
					recipe.m_craftingStation = stationPrefab;
				}
			}
			else if (wasChanged)
			{
				foreach (var entry in originalRecipeStations)
				{
					string recipeName = entry.Key;
					string originalStation = entry.Value;

					Recipe recipe = objDB.m_recipes.Find(r => r.name == recipeName);
					if (recipe == null) continue;

					CraftingStation stationPrefab = null;
					if (originalStation != "null")
					{
						stationPrefab = ZNetScene.instance.GetPrefab(originalStation)?.GetComponent<CraftingStation>();
					}

					log.Info($"Restoring {recipeName} crafting station value: {originalStation}");
					recipe.m_craftingStation = stationPrefab;
				}

				// Remove backups after restoring
				originalRecipeStations.Clear();
			}
		}
	}
}
