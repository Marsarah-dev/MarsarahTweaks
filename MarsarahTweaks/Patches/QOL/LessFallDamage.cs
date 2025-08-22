using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal class LessFallDamage
	{
		private static readonly LogManager log = new LogManager("Less Fall Damage", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(SEMan), "ModifyFallDamage")]
		class FallDamage_Patch
		{
			private static void Prefix(ref float damage)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.LessFallDamageEnabled.Value)
				{
					log.Info($"Reducing fall damage by 40%");
					damage = damage * 0.6f;
				}
	}
		}
	}
}
