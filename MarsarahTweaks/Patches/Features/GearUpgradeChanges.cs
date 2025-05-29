using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class GearUpgradeChanges
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class GearUpgradeUnlock_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateGearRecipeUnlock(__instance, false);
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
			// Station Level 0
			{ "Recipe_HelmetBronze", 0 }, { "Recipe_ArmorBronzeChest", 0 }, { "Recipe_ArmorBronzeLegs", 0 },
			{ "Recipe_AtgeirBronze", 0 }, { "Recipe_AxeBronze", 0 }, { "Recipe_KnifeCopper", 0 },
			{ "Recipe_MaceBronze", 0 }, { "Recipe_PickaxeBronze", 0 }, { "Recipe_SpearBronze", 0 },
			{ "Recipe_SwordBronze", 0 }, { "Recipe_HelmetMage", 0 }, { "Recipe_ArmorMageChest", 0 },
			{ "Recipe_ArmorMageLegs", 0 }, { "Recipe_CapeFeather", 0 }, { "Recipe_StaffFireball", 0 },
			{ "Recipe_StaffIceShards", 0 }, { "Recipe_StaffShield", 0 }, { "Recipe_StaffSkeleton", 0 },
			{ "Recipe_ArmorCarapaceChest", 0 }, { "Recipe_ArmorCarapaceLegs", 0 }, { "Recipe_HelmetCarapace", 0 },
			{ "Recipe_AxeJotunBane", 0 }, { "Recipe_SpearCarapace", 0 }, { "Recipe_SwordMistwalker", 0 },
			{ "Recipe_AtgeirHimminAfl", 0 }, { "Recipe_KnifeSkollAndHati", 0 }, { "Recipe_SledgeDemolisher", 0 },
			{ "Recipe_SwordKrom", 0 }, { "Recipe_BowSpineSnap", 0 }, { "Recipe_CrossbowArbalest", 0 },

			// Station Level 1
			{ "Recipe_HelmetLeather", 1 }, { "Recipe_ArmorLeatherChest", 1 }, { "Recipe_ArmorLeatherLegs", 1 },
			{ "Recipe_CapeDeerHide", 1 }, { "Recipe_HelmetTrollLeather", 1 }, { "Recipe_ArmorTrollLeatherChest", 1 },
			{ "Recipe_ArmorTrollLeatherLegs", 1 }, { "Recipe_CapeTrollHide", 1 }, { "Recipe_SledgeStagbreaker", 1 },
			{ "Recipe_HelmetMage_Ashlands", 1 }, { "Recipe_ArmorMageChest_Ashlands", 1 }, { "Recipe_ArmorMageLegs_Ashlands", 1 },
			{ "Recipe_CapeAsksvin", 1 }, { "Recipe_StaffClusterbomb", 1 }, { "Recipe_StaffGreenRoots", 1 },
			{ "Recipe_StaffLightning", 1 }, { "Recipe_StaffRedTroll", 1 },

			// Station Level 2
			{ "Recipe_HelmetMedium_Ashlands", 2 }, { "Recipe_ArmorMediumChest_Ashlands", 2 }, { "Recipe_ArmorMediumLegs_Ashlands", 2 },
			{ "Recipe_HelmetFlametal", 2 }, { "Recipe_ArmorFlametalChest", 2 }, { "Recipe_ArmorFlametalLegs", 2 },
			{ "Recipe_CapeAsh", 2 }, { "Recipe_MaceEldner", 2 }, { "Recipe_SpearSplitner", 2 },
			{ "Recipe_SwordNiedhogg", 2 }, { "Recipe_AxeBerzerkr", 2 }, { "Recipe_SwordSlayer", 2 },
			{ "Recipe_BowAshlands", 2 }, { "Recipe_CrossbowRipper", 2 },

			// Station Level 3
			{ "Recipe_MaceEldner_Blood", 3 }, { "Recipe_MaceEldner_Lightning", 3 }, { "Recipe_MaceEldner_Nature", 3 },
			{ "Recipe_SpearSplitner_Blood", 3 }, { "Recipe_SpearSplitner_Lightning", 3 }, { "Recipe_SpearSplitner_Nature", 3 },
			{ "Recipe_SwordNiedhogg_Blood", 3 }, { "Recipe_SwordNiedhogg_Lightning", 3 }, { "Recipe_SwordNiedhogg_Nature", 3 },
			{ "Recipe_AxeBerzerkr_Blood", 3 }, { "Recipe_AxeBerzerkr_Lightning", 3 }, { "Recipe_AxeBerzerkr_Nature", 3 },
			{ "Recipe_SwordSlayer_Blood", 3 }, { "Recipe_SwordSlayer_Lightning", 3 }, { "Recipe_SwordSlayer_Nature", 3 },
			{ "Recipe_BowAshlands_Blood", 3 }, { "Recipe_BowAshlands_Lightning", 3 }, { "Recipe_BowAshlands_Nature", 3 },
			{ "Recipe_CrossbowRipper_Blood", 3 }, { "Recipe_CrossbowRipper_Lightning", 3 }, { "Recipe_CrossbowRipper_Nature", 3 },
			{ "Recipe_SwordFire", 3 }, // Dyrnwyn
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
						//MarsarahTweaks.LogInfo($"Backing up {recipeName} min station level value: {recipe.m_minStationLevel}");
						originalRecipeStationLevels[recipeName] = recipe.m_minStationLevel;
					}

					// Apply new value
					//MarsarahTweaks.LogInfo($"Applying new min station level for {recipeName}: {newStationLevel}");
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
					//MarsarahTweaks.LogInfo($"Restoring {recipeName} min station level value: {originalLevel}");
					recipe.m_minStationLevel = originalLevel;
				}

				// Remove backups after restoring
				originalRecipeStationLevels.Clear();
			}
		}
	}
}
