using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal class WispRadiusChanges
	{
		private static readonly LogManager log = new LogManager("Wisp Radius", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Demister), "OnEnable")]
		class BiggerWispRadius_Patch
		{
			private static void Postfix([NotNull] ref Demister __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.BiggerWispRadiusEnabled.Value)
				{
					log.Info("Wisplight end range set to 30");
					__instance.m_forceField.endRange = 30; // default 6, 8, 10, 15 (it changes) (30 in my mod)
				}
			}
		}
	}
}
