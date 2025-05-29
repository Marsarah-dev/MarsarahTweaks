using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.QOL
{
	internal class LargerPickupAreaChanges
	{
		[HarmonyPatch(typeof(Player), "Awake")]
		class LargerPickupArea_Patch
		{
			private static void Prefix(Player __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdatePickupArea(__instance, false);
			}
		}

		private static float originalPickupArea = -1f;

		public static void UpdatePickupArea(Player player, bool wasChanged)
		{
			if (ConfigManager.LargerPickupAreaEnabled.Value)
			{
				if (originalPickupArea == -1f)
				{
					//MarsarahTweaks.LogInfo($"Backing up pickup area ({player.m_autoPickupRange})");
					originalPickupArea = player.m_autoPickupRange;
				}

				//MarsarahTweaks.LogInfo($"Assigning new pickup area (3) - from {player.m_autoPickupRange}");
				player.m_autoPickupRange = 3f;
			}
			else if (wasChanged && originalPickupArea != -1f)
			{
				//MarsarahTweaks.LogInfo($"Restoring pickup area: {originalPickupArea} - from {player.m_autoPickupRange}");
				player.m_autoPickupRange = originalPickupArea;
			}
		}
	}
}
