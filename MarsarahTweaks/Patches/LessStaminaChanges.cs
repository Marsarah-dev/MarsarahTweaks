using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class LessStaminaChanges
	{
		[HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
		class LessStaminaUsage_Patch
		{
			static void Prefix(ref float v)
			{
				if (ConfigManager.lessStaminaUsageEnabled.Value)
				{
					v *= 0.85f;
				}
			}
		}
	}
}
