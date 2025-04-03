using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	public static class GlobalKeyChecker
	{
		public static bool eikthyrDefeated = false;
		public static bool elderDefeated = false;
		public static bool bonemassDefeated = false;
		public static bool moderDefeated = false;
		public static bool yagluthDefeated = false;
		public static bool queenDefeated = false;
		public static bool faderDefeated = false;
		public static bool brennaDefeated = false;
		public static bool geirrhafaDefeated = false;
		public static bool thungrNZilDefeated = false;

		// Dictionary to store global key states
		private static readonly Dictionary<string, bool> globalKeyStates = new Dictionary<string, bool>()
		{
			{ "defeated_eikthyr", false },
			{ "defeated_gdking", false },
			{ "defeated_bonemass", false },
			{ "defeated_dragon", false },
			{ "defeated_goblinking", false },
			{ "defeated_queen", false },
			{ "defeated_fader", false },
			{ "hildir1", false }, // turning in Brenna chest
			{ "hildir2", false }, // turning in Geirrhafa chest
			{ "hildir3", false }  // turning in ThungrNZil chest
		};

		// Dictionary mapping boss names to global keys
		private static readonly Dictionary<string, string> bossToGlobalKey = new Dictionary<string, string>()
		{
			{ "Eikthyr", "defeated_eikthyr" },
			{ "The Elder", "defeated_gdking" },
			{ "Bonemass", "defeated_bonemass" },
			{ "Moder", "defeated_dragon" },
			{ "Yagluth", "defeated_goblinking" },
			{ "The Queen", "defeated_queen" },
			{ "Fader", "defeated_fader" },
			{ "Brenna", "hildir1" },
			{ "Geirrhafa", "hildir2" },
			{ "ThungrNZil", "hildir3" }
		};

		// Check global keys
		[HarmonyPatch(typeof(ZoneSystem), "Update")]
		class BossStatusUpdate_Patch
		{
			static void Postfix(ZoneSystem __instance)
			{
				if (!ConfigManager.clearMistlandsEnabled.Value) return;

				UpdateDefeatedStates(__instance);

				eikthyrDefeated = checkGlobalKey("defeated_eikthyr");
				elderDefeated = checkGlobalKey("defeated_gdking");
				bonemassDefeated = checkGlobalKey("defeated_bonemass");
				moderDefeated = checkGlobalKey("defeated_dragon");
				yagluthDefeated = checkGlobalKey("defeated_goblinking");
				queenDefeated = checkGlobalKey("defeated_queen");
				faderDefeated = checkGlobalKey("defeated_fader");
				brennaDefeated = checkGlobalKey("hildir1");
				geirrhafaDefeated = checkGlobalKey("hildir2");
				thungrNZilDefeated = checkGlobalKey("hildir3");
			}
		}

		private static void UpdateDefeatedStates(ZoneSystem zoneSystem)
		{
			List<string> globalKeys = zoneSystem.GetGlobalKeys();
			foreach (var key in globalKeyStates.Keys.ToList())
			{
				bool keyIsPresent = globalKeys.Contains(key);
				if (globalKeyStates[key] != keyIsPresent)
				{
					globalKeyStates[key] = keyIsPresent;
					//MarsarahTweaks.MLog($"Global key: {key} changed: {keyIsPresent}");
				}
			}
		}

		private static bool checkGlobalKey(string globalKey) => globalKeyStates.ContainsKey(globalKey) && globalKeyStates[globalKey];

		public static bool isBossDefeated(string boss)
		{
			if (bossToGlobalKey.TryGetValue(boss, out string globalKey))
			{
				return checkGlobalKey(globalKey);
			}
			//MarsarahTweaks.MLog($"[Warning] IsBossDefeated called with unknown boss: {boss}");
			return false; // Default to false if boss name is not found
		}
	}
}
