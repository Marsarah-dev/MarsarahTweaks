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
		public static bool EikthyrDefeated = false;
		public static bool ElderDefeated = false;
		public static bool BonemassDefeated = false;
		public static bool ModerDefeated = false;
		public static bool YagluthDefeated = false;
		public static bool QueenDefeated = false;
		public static bool FaderDefeated = false;
		public static bool BrennaDefeated = false;
		public static bool GeirrhafaDefeated = false;
		public static bool ThungrNZilDefeated = false;

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
				UpdateDefeatedStates(__instance);

				EikthyrDefeated = CheckGlobalKey("defeated_eikthyr");
				ElderDefeated = CheckGlobalKey("defeated_gdking");
				BonemassDefeated = CheckGlobalKey("defeated_bonemass");
				ModerDefeated = CheckGlobalKey("defeated_dragon");
				YagluthDefeated = CheckGlobalKey("defeated_goblinking");
				QueenDefeated = CheckGlobalKey("defeated_queen");
				FaderDefeated = CheckGlobalKey("defeated_fader");
				BrennaDefeated = CheckGlobalKey("hildir1");
				GeirrhafaDefeated = CheckGlobalKey("hildir2");
				ThungrNZilDefeated = CheckGlobalKey("hildir3");
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
				}
			}
		}

		private static bool CheckGlobalKey(string globalKey) => globalKeyStates.ContainsKey(globalKey) && globalKeyStates[globalKey];

		public static bool IsBossDefeated(string boss)
		{
			if (bossToGlobalKey.TryGetValue(boss, out string globalKey))
			{
				return CheckGlobalKey(globalKey);
			}

			MarsarahTweaks.MLog($"[Warning] IsBossDefeated called with unknown boss: {boss}");
			return false; // Default to false if boss name is not found
		}
	}
}
