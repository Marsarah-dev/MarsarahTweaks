using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Grind
{
	internal class DoubleBronzeCrafting
	{
		private static readonly LogManager log = new LogManager("Double Bronze", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class OthersSection_Patch
		{
			private static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateDoubleBronzeCrafting(__instance, false);
			}
		}

		// Dictionaries
		private static readonly Dictionary<string, int> doubleBronzeOriginals = new Dictionary<string, int>();
		private static readonly Dictionary<string, int> doubleBronzeChanges = new Dictionary<string, int>()
		{
			{ "Recipe_Bronze", 2 },
			{ "Recipe_Bronze5", 10 }
		};

		// Double Bronze Crafting ==================================================================
		public static void UpdateDoubleBronzeCrafting(ObjectDB objDB, bool wasChanged)
		{
			if (ConfigManager.DoubleBronzeEnabled.Value)
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
						log.Info($"Backed up {recipe.name}");
					}

					// Apply the modified amount
					recipe.m_amount = doubleBronzeChanges[recipe.name];
					log.Info($"Applied new values for {recipe.name}");
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
					log.Info($"Restored {recipe.name}");
				}

				// Clear stored originals when disabling to free memory
				doubleBronzeOriginals.Clear();
			}
		}
	}
}
