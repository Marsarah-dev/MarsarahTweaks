using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.QOL
{
	internal class NoSkillLossChanges
	{
		[HarmonyPatch(typeof(Skills), "Awake")]
		class NoSkilllLowerOnDeath_Patch
		{
			static void Prefix(ref float ___m_DeathLowerFactor)
			{
				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"Player Awake: Updating {ConfigManager.Configs.NoSkillLoss.Name}...");
						if (ConfigManager.noSkillLowerOnDeathEnabled.Value)
						{
							___m_DeathLowerFactor = 0f;
						}
					}
					else
					{
						MarsarahTweaks.MLog($"Player Awake: I am a server. No changes made to {ConfigManager.Configs.NoSkillLoss.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"Player Awake: Too early to do anything. No changes made to {ConfigManager.Configs.NoSkillLoss.Name}...");
				}
			}
		}
	}
}
