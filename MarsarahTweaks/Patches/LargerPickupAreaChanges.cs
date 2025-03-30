using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class LargerPickupAreaChanges
	{
		[HarmonyPatch(typeof(Player), "Awake")]
		class LargerPickupArea_Patch
		{
			static void Prefix(Player __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"Player Awake: Updating {ConfigManager.Configs.LargerPickupArea.Name}...");
						UpdatePickupArea(__instance, false);
					}
					else
					{
						MarsarahTweaks.MLog($"Player Awake: I am a server. No changes made to {ConfigManager.Configs.LargerPickupArea.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"Player Awake: Too early to do anything. No changes made to {ConfigManager.Configs.LargerPickupArea.Name}...");
				}
			}
		}

		private static float originalPickupArea = -1f;

		public static void UpdatePickupArea(Player player, bool wasChanged)
		{
			if (ConfigManager.largerPickupAreaEnabled.Value)
			{
				if (originalPickupArea == -1f)
				{
					//MarsarahTweaks.MLog($"Backing up pickup area ({player.m_autoPickupRange})");
					originalPickupArea = player.m_autoPickupRange;
				}

				//MarsarahTweaks.MLog($"Assigning new pickup area (3) - from {player.m_autoPickupRange}");
				player.m_autoPickupRange = 3f;
			}
			else if (wasChanged && originalPickupArea != -1f)
			{
				//MarsarahTweaks.MLog($"Restoring pickup area: {originalPickupArea} - from {player.m_autoPickupRange}");
				player.m_autoPickupRange = originalPickupArea;
			}
		}
	}
}
