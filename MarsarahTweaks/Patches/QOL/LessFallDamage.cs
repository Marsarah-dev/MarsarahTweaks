using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.QOL
{
	internal class LessFallDamage
	{
		[HarmonyPatch(typeof(SEMan), "ModifyFallDamage")]
		public class FallDamage_Patch
		{
			public static void Prefix(ref float damage)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.lessFallDamageEnabled.Value)
				{
					//MarsarahTweaks.MLog($"Reducing fall damage...");
					damage = damage * 0.6f;
				}
	}
		}
	}
}
