using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class OtherPatches
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class OthersSection_Patch
		{
			static void Prefix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				// Only run on the server or if playing in a local world
				if (ZNet.instance == null || ZNet.instance.IsServer())
				{
					SetOtherModifications(__instance);
				}
			}
		}

		public static void SetOtherModifications(ObjectDB objDB)
		{
			// Original Dictionaries ====================================================
			var doubleBronzeOriginals = new Dictionary<string, int>();

			if (!ConfigManager.doubleBronzeEnabled.Value && doubleBronzeOriginals.Count == 0)
			{
				// Store original values before modifying them
				foreach (Recipe recipe in objDB.m_recipes)
				{
					if (!doubleBronzeOriginals.ContainsKey(recipe.name))
					{
						doubleBronzeOriginals[recipe.name] = recipe.m_amount;
					}
				}
			}

			// Dictionaries =============================================================
			var doubleBronzeChanges = new Dictionary<string, int>()
			{
				{ "Recipe_Bronze", 2 },
				{ "Recipe_Bronze5", 10 }
			};

			// Apply changes ============================================================
			foreach (Recipe recipe in objDB.m_recipes)
			{
				if (ConfigManager.doubleBronzeEnabled.Value)
				{
					// Apply double bronze modifications
					if (doubleBronzeChanges.TryGetValue(recipe.name, out int newAmount))
					{
						recipe.m_amount = newAmount;
					}
				}
				else if (originalRecipeAmounts.TryGetValue(recipe.name, out int defaultAmount))
				{
					// Restore default values when feature is turned off
					recipe.m_amount = defaultAmount;
				}
			}
		}
	}
}
