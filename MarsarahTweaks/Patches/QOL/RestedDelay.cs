using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.QOL
{
	internal class RestedDelay
	{
		[HarmonyPatch(typeof(SE_Cozy), "Setup")] // OnEnable
		class RestedDelay_Patch
		{
			private static float originalRestedDelay = -1f;
			private static readonly float newRestedDelay = 10f;
			private static void Postfix(SE_Cozy __instance)
			{
				if (originalRestedDelay == -1f)
				{
					originalRestedDelay = __instance.m_delay;
					//MarsarahTweaks.MLog($"Original rested delay saved: {originalRestedDelay}");
				}

				if (ConfigManager.ShorterRestedDelayEnabled.Value)
				{
					if (__instance.m_delay != newRestedDelay)
					{
						__instance.m_delay = 10f;
						//MarsarahTweaks.MLog($"Rested delay set to {__instance.m_delay}");
					}
				}
				else
				{
					if (__instance.m_delay != originalRestedDelay)
					{
						__instance.m_delay = originalRestedDelay;
						//MarsarahTweaks.MLog($"Rested delay reverted to {__instance.m_delay}");
					}
					else // Apparently it's automatically set to 20 if the config is off, even mid-game
					{
						//MarsarahTweaks.MLog($"Rested delay is at {__instance.m_delay}");
					}
				}
			}
		}
	}
}
