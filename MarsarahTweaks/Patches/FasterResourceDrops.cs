using HarmonyLib;
using System;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches
{
	internal class FasterResourceDrops
	{
		private static Dictionary<string, float> ttlBackup = new Dictionary<string, float>();

		[HarmonyPatch(typeof(Ragdoll), "Awake")]
		class FasterResourceDrops_Patch
		{
			public static void Prefix(Ragdoll __instance, ref float ___m_ttl)
			{
				if (__instance == null) return;

				if (!ZNet.instance || !ZNet.instance.IsServer()) return; // Do not run on clients

				if (ConfigManager.fasterResourceDropsEnabled.Value)
				{
					if (___m_ttl > 5f)
					{
						___m_ttl = 5f;
					}
					else if (___m_ttl > 0.5f)
					{
						___m_ttl = 0.5f;
					}
				}
			}
		}
	}
}
