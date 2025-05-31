using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Features
{
	internal class ClearMistlandsChanges
	{
		// Disable Mist Emitter
		[HarmonyPatch(typeof(MistEmitter), "Update")]
		class ClearMistlandsEmitter_Patch
		{
			private static void Prefix(MistEmitter __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.ClearMistlandsEnabled.Value && GlobalKeyChecker.QueenDefeated)
				{
					__instance.gameObject.SetActive(false);
				}
			}
		}

		// Disable Particle Mist
		[HarmonyPatch(typeof(ParticleMist), "Update")]
		class ClearMistlandsParticle_Patch
		{
			private static void Postfix(ParticleMist __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.ClearMistlandsEnabled.Value && GlobalKeyChecker.QueenDefeated)
				{
					__instance.gameObject.SetActive(false);
				}
			}
		}
	}
}
