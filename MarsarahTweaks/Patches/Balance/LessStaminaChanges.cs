using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class LessStaminaChanges
	{
		private static readonly LogManager log = new LogManager("Less Stamina", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
		class LessStaminaUsage_Patch
		{
			private static void Prefix(ref float v)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.LessStaminaUsageEnabled.Value)
				{
					v *= 0.85f;
				}
			}
		}
	}
}
