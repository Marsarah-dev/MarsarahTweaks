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
	internal class FriendlyBallistas
	{
		private static readonly LogManager log = new LogManager("Friendly Ballistas", LogManager.LogLevel.Warning);

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
						log.Info($"Applying new player targeting: {newTargetsFriendly}");
						___m_targetPlayers = newTargetsFriendly;
					}
					if (___m_targetTamed != newTargetsFriendly)
					{
						log.Info($"Applying new tamed targeting: {newTargetsFriendly}");
						___m_targetTamed = newTargetsFriendly;
					}
				}
				else
				{
					if (___m_targetPlayers != originalTargetsFriendly)
					{
						log.Info($"Restoring player targeting: {originalTargetsFriendly}");
						___m_targetPlayers = originalTargetsFriendly;
					}
					if (___m_targetTamed != originalTargetsFriendly)
					{
						log.Info($"Restoring tamed targeting: {originalTargetsFriendly}");
						___m_targetTamed = originalTargetsFriendly;
					}
				}
			}
		}
	}
}
