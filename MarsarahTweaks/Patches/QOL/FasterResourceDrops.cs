using HarmonyLib;
using System;
using System.Collections.Generic;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal class FasterResourceDrops
	{
		private static readonly LogManager log = new LogManager("Faster Resource Drops", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Ragdoll), "Awake")]
		class FasterResourceDrops_Patch
		{
			public static void Prefix(Ragdoll __instance, ref float ___m_ttl)
			{
				if (__instance == null) return;

				// This never gets called on a dedicated server, so we make no checks

				if (ConfigManager.FasterResourceDropsEnabled.Value)
				{
					if (___m_ttl > 5f)
					{
						___m_ttl = 5f;
						log.Info("Large monster ttl set to 5s");
					}
					else if (___m_ttl > 0.5f)
					{
						___m_ttl = 0.5f;
						log.Info("Small monster ttl set to 0.5s");
					}
				}
			}
		}
	}
}
