using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.QOL
{
	internal class WispRadiusChanges
	{
		[HarmonyPatch(typeof(Demister), "OnEnable")]
		class BiggerWispRadius_Patch
		{
			static void Postfix([NotNull] ref Demister __instance)
			{
				if (ConfigManager.biggerWispRadiusEnabled.Value)
				{
					__instance.m_forceField.endRange = 30; // default 6, 8, 10, 15 (it changes) (30 in my mod)
				}
			}
		}
	}
}
