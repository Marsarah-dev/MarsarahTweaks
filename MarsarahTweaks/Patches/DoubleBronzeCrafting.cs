using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsarahTweaks.Patches
{
	internal class DoubleBronzeCrafting
	{
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
						UpdateDoubleBronzeCrafting(__instance, false);
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

		// Dictionaries
		private static Dictionary<string, int> doubleBronzeOriginals = new Dictionary<string, int>();
		private static Dictionary<string, int> doubleBronzeChanges = new Dictionary<string, int>()
		{
			{ "Recipe_Bronze", 2 },
			{ "Recipe_Bronze5", 10 }
		};

		// Double Bronze Crafting ==================================================================
		public static void UpdateDoubleBronzeCrafting(ObjectDB objDB, bool wasChanged)
		{
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
			else if (wasChanged)
			{
				// Revert changes
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
	}
}
