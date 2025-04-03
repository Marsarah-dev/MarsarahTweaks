using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class MinibossWeightChanges
	{
		[HarmonyPatch(typeof(Player), "OnSpawned")]
		public class MinibossCarryWeight_Patch
		{
			public static void Postfix(Player __instance)
			{
				if (__instance == null || !ConfigManager.minibossWeightEnabled.Value) return;

				int defeatedMinibosses = 0;
				if (GlobalKeyChecker.brennaDefeated) defeatedMinibosses++;
				if (GlobalKeyChecker.geirrhafaDefeated) defeatedMinibosses++;
				if (GlobalKeyChecker.thungrNZilDefeated) defeatedMinibosses++;

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
