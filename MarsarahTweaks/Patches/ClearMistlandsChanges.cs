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

		// Check queen status on startup
		[HarmonyPatch(typeof(ZoneSystem), "Update")]
		class ClearMistlandsUpdate_Patch
		{
			static void Postfix(ZoneSystem __instance)
			{
				if (ConfigManager.clearMistlandsEnabled.Value)
				{
					List<string> globalKeys = __instance.GetGlobalKeys();
					if (queenDefeated != globalKeys.Contains("defeated_queen"))
					{
						queenDefeated = globalKeys.Contains("defeated_queen");
						//MarsarahTweaks.MLog($"ZoneSystem Update - Queen defeated: {queenDefeated}");
					}
				}
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
