using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.QOL
{
	internal class FriendlyBallistas
	{
		[HarmonyPatch(typeof(Turret), "UpdateTarget")]
		class FriendlyBallistas_Patch
		{
			private static bool originalTargetsFriendly = true;
			private static readonly bool newTargetsFriendly = false;

			private static void Postfix([NotNull] ref bool ___m_targetPlayers, [NotNull] ref bool ___m_targetTamed)
			{
				// Needs to run on both client and server - no checks needed

				if (ConfigManager.FriendlyBallistasEnabled.Value)
				{

					if (___m_targetPlayers != newTargetsFriendly)
					{
						//MarsarahTweaks.LogInfo($"Applying new player targeting: {newTargetsFriendly}");
						___m_targetPlayers = newTargetsFriendly;
					}
					if (___m_targetTamed != newTargetsFriendly)
					{
						//MarsarahTweaks.LogInfo($"Applying new tamed targeting: {newTargetsFriendly}");
						___m_targetTamed = newTargetsFriendly;
					}
				}
				else
				{
					if (___m_targetPlayers != originalTargetsFriendly)
					{
						//MarsarahTweaks.LogInfo($"Restoring player targeting: {originalTargetsFriendly}");
						___m_targetPlayers = originalTargetsFriendly;
					}
					if (___m_targetTamed != originalTargetsFriendly)
					{
						//MarsarahTweaks.LogInfo($"Restoring tamed targeting: {originalTargetsFriendly}");
						___m_targetTamed = originalTargetsFriendly;
					}
				}
			}
		}
	}
}
