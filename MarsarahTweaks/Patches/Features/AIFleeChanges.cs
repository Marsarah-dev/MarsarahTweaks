using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class AIFleeChanges
	{
		[HarmonyPatch(typeof(MonsterAI), "Awake")]
		class MonsterAIFlee_Patch
		{
			private static void Prefix(MonsterAI __instance, ref bool ___m_fleeIfNotAlerted)
			{
				//if (!ZNet.instance || !ZNet.instance.IsServer()) return; // Prevent running on clients
				// Run on both server and client - no checks made

				if (__instance == null || !ConfigManager.noFleeEnabled.Value) return;

				BaseAI thisBaseAI = __instance.GetComponentInParent<BaseAI>();
				if (thisBaseAI == null) return;	

				if (thisBaseAI.name == "Boar(Clone)" || thisBaseAI.name == "Neck(Clone)")
				{
					//MarsarahTweaks.MLog($"This AI won't flee");
					___m_fleeIfNotAlerted = false;
				}
			}
		}
	}
}
