using HarmonyLib;
using JetBrains.Annotations;
using MarsarahTweaks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.QOL
{
	internal class LighterMetalWeight
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class OthersSection_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateLighterMetalWeight(__instance, false);
			}
		}

		// Dictionaries
		private static Dictionary<string, float> metalWeightOriginals = new Dictionary<string, float>();
		private static Dictionary<string, float> metalWeightChanges = new Dictionary<string, float>()
		{
			{ "TinOre", 8 },
			{ "Tin", 8 },
			{ "CopperOre", 8 },
			{ "Copper", 8 },
			{ "Bronze", 8 },
			{ "IronOre", 8 },
			{ "IronScrap", 8 },
			{ "Iron", 8 },
			{ "SilverOre", 8 },
			{ "Silver", 8 },
			{ "BlackMetalScrap", 8 },
			{ "BlackMetal", 8 },
			{ "CopperScrap", 8 },
			{ "BronzeScrap", 8 },
			{ "Flametal", 8 },
			{ "FlametalNew", 8 },
			{ "FlametalOre", 8 },
			{ "FlametalOreNew", 8 }
		};


		// Lighter Metal Weight" ===================================================================
		public static void UpdateLighterMetalWeight(ObjectDB objDB, bool wasChanged)
		{
			if (ConfigManager.lighterMetalWeightEnabled.Value)
			{
				// Apply weight reduction
				foreach (var itemName in metalWeightChanges.Keys)
				{
					GameObject item = objDB.m_items.Find(i => i.name == itemName);
					if (item == null) continue;

					ItemDrop itemDrop = item.GetComponent<ItemDrop>();
					if (itemDrop == null) continue;

					// Store the original weight
					if (!metalWeightOriginals.ContainsKey(item.name))
					{
						metalWeightOriginals[item.name] = itemDrop.m_itemData.m_shared.m_weight;
					}

					// Set the reduced weight
					if (metalWeightChanges.TryGetValue(item.name, out float newWeight))
					{
						//MarsarahTweaks.MLog($"Applying new weight for {item.name} from {itemDrop.m_itemData.m_shared.m_weight} to {newWeight}");
						itemDrop.m_itemData.m_shared.m_weight = newWeight;
					}
				}
			}
			else if (wasChanged)
			{
				// Restore original weights
				foreach (var itemName in metalWeightOriginals.Keys)
				{
					GameObject item = objDB.m_items.Find(i => i.name == itemName);
					if (item == null) continue;

					ItemDrop itemDrop = item.GetComponent<ItemDrop>();
					if (itemDrop == null) continue;

					if (metalWeightOriginals.TryGetValue(item.name, out float originalWeight))
					{
						//MarsarahTweaks.MLog($"Restoring original value for {item.name} from {itemDrop.m_itemData.m_shared.m_weight} to {originalWeight}");
						itemDrop.m_itemData.m_shared.m_weight = originalWeight;
					}
				}

				// Clear stored originals when disabling to save memory
				metalWeightOriginals.Clear();
			}
		}
	}
}
