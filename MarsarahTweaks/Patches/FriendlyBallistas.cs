using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class FriendlyBallistas
	{
		[HarmonyPatch(typeof(Turret), "UpdateTarget")]
		class FriendlyBallistas_Patch
		{
			private static bool originalTargetsFriendly = true;
			private static readonly bool newTargetsFriendly = false;

			static void Postfix([NotNull] ref bool ___m_targetPlayers, [NotNull] ref bool ___m_targetTamed)
			{
				if (!ZNet.instance || !ZNet.instance.IsServer()) return; // Prevent running on clients

				if (ConfigManager.friendlyBallistasEnabled.Value)
				{

					if (___m_targetPlayers != newTargetsFriendly)
					{
						//MarsarahTweaks.MLog($"Applying new player targeting: {newTargetsFriendly}");
						___m_targetPlayers = newTargetsFriendly;
					}
					if (___m_targetTamed != newTargetsFriendly)
					{
						//MarsarahTweaks.MLog($"Applying new tamed targeting: {newTargetsFriendly}");
						___m_targetTamed = newTargetsFriendly;
					}
				}
				else
				{
					if (___m_targetPlayers != originalTargetsFriendly)
					{
						//MarsarahTweaks.MLog($"Restoring player targeting: {originalTargetsFriendly}");
						___m_targetPlayers = originalTargetsFriendly;
					}
					if (___m_targetTamed != originalTargetsFriendly)
					{
						//MarsarahTweaks.MLog($"Restoring tamed targeting: {originalTargetsFriendly}");
						___m_targetTamed = originalTargetsFriendly;
					}
				}
			}
		}
	}
}
