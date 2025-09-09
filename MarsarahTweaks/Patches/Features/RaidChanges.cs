using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Linq;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class RaidChanges
	{
		private static readonly LogManager log = new LogManager("Raid Changes", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(RandEventSystem), "Start")]
		public static class RandEventSystem_Start_Patch
		{
			private static void Postfix(RandEventSystem __instance)
			{
				foreach (RandomEvent raid in __instance.m_events)
				{
					//DumpEvent(raid);
					
					if (raid.m_name == "army_seekers" || raid.m_name == "army_charred" || raid.m_name == "army_charredspawners")
					{
						// Remove BlackForest from their biome mask
						raid.m_biome &= ~Heightmap.Biome.BlackForest;

						log.Info($"[Raid Adjust] {raid.m_name} -> New Biomes: {raid.m_biome}");
					}
				}
			}
		}

		/*private static void DumpEvent(RandomEvent raid)
		{
			string name = raid.m_name;

			// Convert biome mask to readable string
			string biomeStr = raid.m_biome == (Heightmap.Biome)(-1) ? "All" : raid.m_biome.ToString();

			// Required keys
			string reqKeys = raid.m_requiredGlobalKeys.Count > 0 ? string.Join(", ", raid.m_requiredGlobalKeys) : "None";

			// Not-required keys
			string notReqKeys = raid.m_notRequiredGlobalKeys.Count > 0 ? string.Join(", ", raid.m_notRequiredGlobalKeys) : "None";

			log.Info($"Raid '{name}' -> Biomes: {biomeStr} | Required Keys: {reqKeys} | Not Required Keys: {notReqKeys}");
		}*/
	}
}
