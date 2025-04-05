using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class ArmorStatsChanges
	{
		[HarmonyPatch(typeof(Player), "UpdateStats", new Type[] { typeof(float) })]
		class eitrFromMageGear_Patch
		{
			private static void Prefix(Player __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateArmorStats(__instance);
			}
		}

		[HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
		class BaseEitr_Patch
		{
			private static void Postfix(ref float hp, ref float stamina, ref float eitr)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.ExtraArmorStatsEnabled.Value)
				{
					hp += hpFromGear;
					stamina += staminaFromGear;
					eitr += eitrFromGear;
				}
			}
		}

		[HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), new Type[] { typeof(ItemDrop.ItemData), typeof(int), typeof(bool), typeof(float), typeof(int) })]
		class ArmorTooltip_Patch
		{
			private static void Postfix(ref string __result, ItemDrop.ItemData item)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (item == null || item.m_shared == null) return;

				if (ConfigManager.ExtraArmorStatsEnabled.Value)
				{
					UpdateTooltips(item, ref __result);
				}
			}
		}


		private static float hpFromGear;
		private static float staminaFromGear;
		private static float eitrFromGear;

		static readonly Dictionary<string, (int HP, int Stamina, int Eitr)> newArmorStats = new Dictionary<string, (int HP, int Stamina, int Eitr)>()
		{
			// Heavy Armor (HP Bonus)
			{ "$item_helmet_bronze", (2, 0, 0) },
			{ "$item_chest_bronze", (4, 0, 0) },
			{ "$item_legs_bronze", (4, 0, 0) },
			{ "$item_helmet_iron", (4, 0, 0) },
			{ "$item_chest_iron", (8, 0, 0) },
			{ "$item_legs_iron", (8, 0, 0) },
			{ "$item_helmet_drake", (8, 0, 0) },
			{ "$item_chest_wolf", (12, 0, 0) },
			{ "$item_legs_wolf", (10, 0, 0) },
			{ "$item_helmet_padded", (10, 0, 0) },
			{ "$item_chest_pcuirass", (16, 0, 0) },
			{ "$item_legs_pgreaves", (14, 0, 0) },
			{ "$item_helmet_carapace", (12, 0, 0) },
			{ "$item_chest_carapace", (20, 0, 0) },
			{ "$item_legs_carapace", (18, 0, 0) },
			{ "$item_helmet_flametal", (14, 0, 0) },
			{ "$item_chest_flametal", (24, 0, 0) },
			{ "$item_legs_flametal", (22, 0, 0) },

			// Hybrid (HP & Stamina)
			{ "$item_helmet_medium_ashlands", (5, 5, 0) },
			{ "$item_chest_medium_ashlands", (10, 8, 0) },
			{ "$item_legs_medium_ashlands", (10, 7, 0) },

			// Light Armor (Stamina Bonus)
			{ "$item_helmet_trollleather", (0, 1, 0) },
			{ "$item_chest_trollleather", (0, 2, 0) },
			{ "$item_legs_trollleather", (0, 2, 0) },
			{ "$item_helmet_root", (0, 3, 0) },
			{ "$item_chest_root", (0, 4, 0) },
			{ "$item_legs_root", (0, 3, 0) },
			{ "$item_helmet_fenris", (0, 4, 0) },
			{ "$item_chest_fenris", (0, 6, 0) },
			{ "$item_legs_fenris", (0, 5, 0) },

			// Mage Gear (Eitr Bonus)
			{ "$item_helmet_mage", (0, 0, 10) },
			{ "$item_chest_mage", (0, 0, 20) },
			{ "$item_legs_mage", (0, 0, 20) },
			{ "$item_helmet_mage_ashlands", (0, 0, 15) },
			{ "$item_chest_mage_ashlands", (0, 0, 30) },
			{ "$item_legs_mage_ashlands", (0, 0, 30) }
		};

		private static void UpdateArmorStats(Player player)
		{
			eitrFromGear = 0f;
			hpFromGear = 0f;
			staminaFromGear = 0f;

			if (ConfigManager.ExtraArmorStatsEnabled.Value)
			{
				Inventory playerInventory = player.GetInventory();
				List<ItemDrop.ItemData> playerEquippedItems = playerInventory.GetEquippedItems();

				foreach (ItemDrop.ItemData equippedItem in playerEquippedItems)
				{
					if (newArmorStats.TryGetValue(equippedItem.m_shared.m_name, out var stats))
					{
						hpFromGear += stats.HP;
						staminaFromGear += stats.Stamina;
						eitrFromGear += stats.Eitr;
					}
				}
			}
		}

		private static void UpdateTooltips(ItemDrop.ItemData item, ref string result)
		{
			// Check if the item exists in the armor dictionary
			if (newArmorStats.TryGetValue(item.m_shared.m_name, out var stats))
			{
				List<string> statLines = new List<string>();

				// Append relevant stat increases
				if (stats.HP > 0)
				{
					statLines.Add($"<color=green>Max HP: +{stats.HP}</color>");
				}
				if (stats.Stamina > 0)
				{
					statLines.Add($"<color=yellow>Max Stamina: +{stats.Stamina}</color>");
				}
				if (stats.Eitr > 0)
				{
					statLines.Add($"<color=purple>Max Eitr: +{stats.Eitr}</color>");
				}

				// Append stat modifications to the tooltip
				if (statLines.Count > 0)
				{
					result += "\n" + string.Join("\n", statLines);
				}
			}
		}
	}
}
