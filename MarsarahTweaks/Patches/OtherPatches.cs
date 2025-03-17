using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsarahTweaks.Patches
{
	internal class OtherPatches
	{
		private static Dictionary<string, int> doubleBronzeOriginals = new Dictionary<string, int>();

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class OthersSection_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					if (!ZNet.instance.IsServer())
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.ConfigEntryName.DoubleBronzeCrafting}...");
						UpdateDoubleBronzeCrafting(__instance);
					}
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.ConfigEntryName.DoubleBronzeCrafting}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.ConfigEntryName.DoubleBronzeCrafting}...");
				}
			}
		}

		public static void UpdateDoubleBronzeCrafting(ObjectDB objDB)
		{
			var doubleBronzeChanges = new Dictionary<string, int>()
			{
				{ "Recipe_Bronze", 2 },
				{ "Recipe_Bronze5", 10 }
			};

			// Iterate only over the recipes we care about
			foreach (var recipeName in doubleBronzeChanges.Keys)
			{
				Recipe recipe = objDB.m_recipes.Find(r => r.name == recipeName);
				if (recipe == null) continue; // Skip if the recipe doesn't exist

				if (ConfigManager.doubleBronzeEnabled.Value)
				{
					// Store the original amount *only once*
					if (!doubleBronzeOriginals.ContainsKey(recipe.name))
					{
						doubleBronzeOriginals[recipe.name] = recipe.m_amount;
					}

					// Apply new values
					recipe.m_amount = doubleBronzeChanges[recipe.name];
					MarsarahTweaks.MLog($"{recipe.name} new amount set to: {recipe.m_amount}");
				}
				else
				{
					// Revert changes if we have a stored original
					if (doubleBronzeOriginals.TryGetValue(recipe.name, out int defaultAmount))
					{
						recipe.m_amount = defaultAmount;
						MarsarahTweaks.MLog($"{recipe.name} reverted to original amount: {defaultAmount}");
					}
				}
			}

			// Clear the dictionary when disabling to save memory
			if (!ConfigManager.doubleBronzeEnabled.Value)
			{
				doubleBronzeOriginals.Clear();
				MarsarahTweaks.MLog("Double Bronze Crafting disabled. Reverted changes.");
			}
		}
	}
}
