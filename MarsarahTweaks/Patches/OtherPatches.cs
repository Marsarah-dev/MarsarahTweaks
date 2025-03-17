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
		private static bool stuffApplied = false;

		[HarmonyPatch(typeof(ZNetScene), "Update")]
		class OthersSection_Patch
		{
			static void Prefix(ZNetScene __instance)
			{
				if (!stuffApplied)
				{
					if (!ZNet.instance.IsServer())
					{
						MarsarahTweaks.MLog("ZNetScene Update: Before checking ObjectDB...");
						if (ObjectDB.instance == null) return;

						MarsarahTweaks.MLog("ZNetScene Update: Applying recipe modifications...");
						SetOtherModifications(ObjectDB.instance);
						stuffApplied = true;
					}
					else
					{
						MarsarahTweaks.MLog("ZNetScene Update: I am a server. Not applying ObjDB stuff...");
						stuffApplied = true;
					}
				}
			}
		}

		public static void SetOtherModifications(ObjectDB objDB)
		{
			// Dictionaries =============================================================
			var doubleBronzeOriginals = new Dictionary<string, int>();
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
						doubleBronzeOriginals[recipe.name] = recipe.m_amount;
						recipe.m_amount = newAmount;
						MarsarahTweaks.MLog($"{recipe.name} new amount set to: {newAmount}");
					}
				}
				else if (doubleBronzeOriginals.Count != 0)
				{
					if (doubleBronzeOriginals.TryGetValue(recipe.name, out int defaultAmount))
					{
						recipe.m_amount = defaultAmount;
						MarsarahTweaks.MLog($"{recipe.name} default amount set to: {defaultAmount}");
					}
				}
			}
			if (doubleBronzeOriginals.Count == 0)
			{
				MarsarahTweaks.MLog("Double Bronze was originally false. No originals set.");
			}
		}
	}
}
