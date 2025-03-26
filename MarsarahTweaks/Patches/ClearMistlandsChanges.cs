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
		private static bool queenDefeated = false;

		// Check queen status 
		[HarmonyPatch(typeof(ZoneSystem), "Update")]
		class ClearMistlandsUpdate_Patch
		{
			static void Postfix(ZoneSystem __instance)
			{
				if (!ConfigManager.clearMistlandsEnabled.Value) return;

				BossStateChecker.UpdateDefeatedStates(__instance);

				queenDefeated = BossStateChecker.IsBossDefeated("defeated_queen");
			}
		}

		// Disable Mist Emitter
		[HarmonyPatch(typeof(MistEmitter), "Update")]
		class ClearMistlandsEmitter_Patch
		{
			static void Prefix(MistEmitter __instance)
			{
				if (__instance == null) return;

				if (ConfigManager.clearMistlandsEnabled.Value && queenDefeated)
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

				if (ConfigManager.clearMistlandsEnabled.Value && queenDefeated)
				{
					__instance.gameObject.SetActive(false);
				}
			}
		}
	}
}
