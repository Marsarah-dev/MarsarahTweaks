using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class MinibossWeightChanges
	{
		private static bool brennaDefeated = false;
		private static bool geirrhafaDefeated = false;
		private static bool thungrNZilDefeated = false;

		// Check boss status 
		[HarmonyPatch(typeof(ZoneSystem), "Update")]
		class ClearMistlandsUpdate_Patch
		{
			static void Postfix(ZoneSystem __instance)
			{
				if (!ConfigManager.clearMistlandsEnabled.Value) return;

				BossStateChecker.UpdateDefeatedStates(__instance);

				brennaDefeated = BossStateChecker.IsBossDefeated("hildir1");
				geirrhafaDefeated = BossStateChecker.IsBossDefeated("hildir2");
				thungrNZilDefeated = BossStateChecker.IsBossDefeated("hildir3");
			}
		}

		[HarmonyPatch(typeof(Player), "OnSpawned")]
		public class MinibossCarryWeight_Patch
		{
			public static void Postfix(Player __instance)
			{
				if (__instance == null || !ConfigManager.minibossWeightEnabled.Value) return;

				int defeatedMinibosses = 0;
				if (brennaDefeated) defeatedMinibosses++;
				if (geirrhafaDefeated) defeatedMinibosses++;
				if (thungrNZilDefeated) defeatedMinibosses++;

				// Adjust carry weight based on minibosses defeated
				__instance.m_maxCarryWeight = defeatedMinibosses switch
				{
					3 => 375,  // All defeated
					2 => 350,  // Two defeated
					1 => 325,  // One defeated
					_ => 300   // None defeated
				};
			}
		}
	}
}
