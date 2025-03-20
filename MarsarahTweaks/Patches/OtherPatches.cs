using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsarahTweaks.Patches
{
	internal class OtherPatches
	{
		private static Dictionary<string, int> doubleBronzeOriginals = new Dictionary<string, int>();
		private static Dictionary<string, float> metalWeightOriginals = new Dictionary<string, float>();

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class OthersSection_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					//bool isLocalWorld = ZNet.instance.IsServer() && !isDedicatedServer;
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.DoubleBronzeCrafting.Name}...");
						UpdateDoubleBronzeCrafting(__instance);

						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.LighterMetalWeight.Name}...");
						UpdateLighterMetalWeight(__instance);
					}
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.Configs.DoubleBronzeCrafting.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.Configs.DoubleBronzeCrafting.Name}...");
				}
			}
		}

		// Double Bronze Crafting ==================================================================
		public static void UpdateDoubleBronzeCrafting(ObjectDB objDB)
		{
			var doubleBronzeChanges = new Dictionary<string, int>()
			{
				{ "Recipe_Bronze", 2 },
				{ "Recipe_Bronze5", 10 }
			};

			if (ConfigManager.doubleBronzeEnabled.Value)
			{
				// Apply changes only if enabled
				foreach (var recipeName in doubleBronzeChanges.Keys)
				{
					Recipe recipe = objDB.m_recipes.Find(r => r.name == recipeName);
					if (recipe == null) continue;

					// Store the original amount only once
					if (!doubleBronzeOriginals.ContainsKey(recipe.name))
					{
						doubleBronzeOriginals[recipe.name] = recipe.m_amount;
					}

					// Apply the modified amount
					recipe.m_amount = doubleBronzeChanges[recipe.name];
				}
			}
			else
			{
				// Revert changes only if we have stored originals
				foreach (var recipeName in doubleBronzeOriginals.Keys)
				{
					Recipe recipe = objDB.m_recipes.Find(r => r.name == recipeName);
					if (recipe == null) continue;

					recipe.m_amount = doubleBronzeOriginals[recipe.name];
				}

				// Clear stored originals when disabling to free memory
				doubleBronzeOriginals.Clear();
			}
		}

		// Lighter Metal Weight" ===================================================================
		public static void UpdateLighterMetalWeight(ObjectDB objDB)
		{
			var metalWeightChanges = new Dictionary<string, float>()
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

			if (ConfigManager.lighterMetalWeightEnabled.Value)
			{
				// Apply weight reduction
				foreach (var itemName in metalWeightChanges.Keys)
				{
					GameObject item = objDB.m_items.Find(i => i.name == itemName);
					if (item == null) continue;

					ItemDrop itemDrop = item.GetComponent<ItemDrop>();
					if (itemDrop == null) continue;

					// Store the original weight (only once)
					if (!metalWeightOriginals.ContainsKey(item.name))
					{
						metalWeightOriginals[item.name] = itemDrop.m_itemData.m_shared.m_weight;
					}

					// Set the reduced weight
					if (metalWeightChanges.TryGetValue(item.name, out float newWeight))
					{
						itemDrop.m_itemData.m_shared.m_weight = newWeight;
					}
				}
			}
			else
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
						itemDrop.m_itemData.m_shared.m_weight = originalWeight;
					}
				}

				// Clear stored originals when disabling to save memory
				metalWeightOriginals.Clear();
			}
		}
	}
}
