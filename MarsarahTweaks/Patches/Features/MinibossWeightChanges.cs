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
		class MinibossCarryWeight_Patch
		{
			private static void Postfix(Player __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (__instance == null || !ConfigManager.MinibossWeightEnabled.Value) return;

				int defeatedMinibosses = 0;
				if (GlobalKeyChecker.BrennaDefeated) defeatedMinibosses++;
				if (GlobalKeyChecker.GeirrhafaDefeated) defeatedMinibosses++;
				if (GlobalKeyChecker.ThungrNZilDefeated) defeatedMinibosses++;

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
