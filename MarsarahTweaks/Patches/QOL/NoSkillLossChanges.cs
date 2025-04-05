using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.QOL
{
	internal class NoSkillLossChanges
	{
		[HarmonyPatch(typeof(Skills), "Awake")]
		class NoSkilllLowerOnDeath_Patch
		{
			private static void Prefix(ref float ___m_DeathLowerFactor)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.NoSkillLowerOnDeathEnabled.Value)
				{
					___m_DeathLowerFactor = 0f;
				}
			}
		}
	}
}
