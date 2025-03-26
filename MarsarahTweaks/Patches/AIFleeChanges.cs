using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class AIFleeChanges
	{
		[HarmonyPatch(typeof(MonsterAI), "Awake")]
		class MonsterAIFlee_Patch
		{
			private static void Prefix(MonsterAI __instance, ref bool ___m_fleeIfNotAlerted)
			{
				if (__instance == null || !ConfigManager.noFleeEnabled.Value) return;

				BaseAI thisBaseAI = __instance.GetComponentInParent<BaseAI>();
				if (thisBaseAI == null) return;	

				if (thisBaseAI.name == "Boar(Clone)" || thisBaseAI.name == "Neck(Clone)")
				{
					___m_fleeIfNotAlerted = false;
					//MarsarahTweaks.MLog($"{thisBaseAI.name} will NOT flee");
				}
			}
		}
	}
}
