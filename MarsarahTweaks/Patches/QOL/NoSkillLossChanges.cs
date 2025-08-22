using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal class NoSkillLossChanges
	{
		private static readonly LogManager log = new LogManager("No Skill Loss", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Skills), "Awake")]
		class NoSkilllLowerOnDeath_Patch
		{
			private static void Prefix(ref float ___m_DeathLowerFactor)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.NoSkillLowerOnDeathEnabled.Value)
				{
					log.Info("Updating death lower factor");
					___m_DeathLowerFactor = 0f;
				}
			}
		}
	}
}
