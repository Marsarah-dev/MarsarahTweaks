using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Features
{
	internal class AIFleeChanges
	{
		[HarmonyPatch(typeof(MonsterAI), "Awake")]
		class MonsterAIFlee_Patch
		{
			private static void Prefix(MonsterAI __instance, ref bool ___m_fleeIfNotAlerted)
			{
				// Run on both server and client - no checks made

				if (__instance == null || !ConfigManager.NoFleeEnabled.Value) return;

				BaseAI thisBaseAI = __instance.GetComponentInParent<BaseAI>();
				if (thisBaseAI == null) return;	

				if (thisBaseAI.name == "Boar(Clone)" || thisBaseAI.name == "Neck(Clone)")
				{
					//MarsarahTweaks.LogInfo($"This AI won't flee");
					___m_fleeIfNotAlerted = false;
				}
			}
		}
	}
}
