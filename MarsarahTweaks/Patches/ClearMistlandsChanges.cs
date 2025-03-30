using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class ClearMistlandsChanges
	{
		// Disable Mist Emitter
		[HarmonyPatch(typeof(MistEmitter), "Update")]
		class ClearMistlandsEmitter_Patch
		{
			static void Prefix(MistEmitter __instance)
			{
				if (__instance == null) return;

				if (ConfigManager.clearMistlandsEnabled.Value && GlobalKeyChecker.queenDefeated)
				{
					__instance.gameObject.SetActive(false);
				}
			}
		}

		// Disable Particle Mist
		[HarmonyPatch(typeof(ParticleMist), "Update")]
		class ClearMistlandsParticle_Patch
		{
			static void Postfix(ParticleMist __instance)
			{
				if (__instance == null) return;

				if (ConfigManager.clearMistlandsEnabled.Value && GlobalKeyChecker.queenDefeated)
				{
					__instance.gameObject.SetActive(false);
				}
			}
		}
	}
}
