using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Features
{
	internal class BrighterLanterns
	{
		private static readonly LogManager log = new LogManager("Brighter Lanterns", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class BrighterLanterns_Patch
		{
			private static void Postfix(ZNetScene __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateLanterns(__instance);
			}
		}

		private static Dictionary<string, (float intensity, float range, float flickerIntensity, float flickerSpeed)> lanternBackups = new Dictionary<string, (float intensity, float range, float flickerIntensity, float flickerSpeed)>();

		public static void UpdateLanterns(ZNetScene znScene)
		{
			if (ConfigManager.BrighterLanternsEnabled.Value)
			{
				// Defaults: intensity = 1.5, range = 6, flickerIntensity = 0.1, flickerSpeed = 10

				UpdateLanternLight(znScene, "piece_dvergr_lantern", 2f, 9f, 0.05f, 5f);  // (2, 9) / (2, 12) / (3 / 12)
				UpdateLanternLight(znScene, "piece_dvergr_lantern_pole", 2f, 15f, 0.05f, 5f);  // (2, 12) / (2, 15) / (3 / 15)
			}
			else
			{
				RestoreLanternDefaults(znScene);
			}
		}

		private static void UpdateLanternLight(ZNetScene znScene, string prefabName, float lightIntensity, float lightRange, float flickerIntensity, float flickerSpeed)
		{
			GameObject lanternPrefab = znScene.GetPrefab(prefabName);
			if (lanternPrefab != null)
			{
				Light lightComponent = lanternPrefab.GetComponentInChildren<Light>();
				LightFlicker lightFlicker = lanternPrefab.GetComponentInChildren<LightFlicker>();

				// Backup only once (on first access)
				if (!lanternBackups.ContainsKey(prefabName) && lightComponent != null && lightFlicker != null)
				{
					lanternBackups[prefabName] = (
						lightComponent.intensity,
						lightComponent.range,
						lightFlicker.m_flickerIntensity,
						lightFlicker.m_flickerSpeed
					);
					log.Info($"Backed up light data for {prefabName}");
				}

				// Apply new values
				if (lightComponent != null)
				{
					lightComponent.intensity = lightIntensity;
					lightComponent.range = lightRange;
					log.Info($"Applied new light data for {prefabName}");
				}

				if (lightFlicker != null)
				{
					lightFlicker.m_flickerIntensity = flickerIntensity;
					lightFlicker.m_flickerSpeed = flickerSpeed;
					log.Info($"Applied new light flicker data for {prefabName}");
				}
			}
		}

		private static void RestoreLanternDefaults(ZNetScene znScene)
		{
			foreach (var kvp in lanternBackups)
			{
				string prefabName = kvp.Key;
				var (intensity, range, flickerIntensity, flickerSpeed) = kvp.Value;

				GameObject lanternPrefab = znScene.GetPrefab(prefabName);
				if (lanternPrefab != null)
				{
					Light lightComponent = lanternPrefab.GetComponentInChildren<Light>();
					if (lightComponent != null)
					{
						lightComponent.intensity = intensity;
						lightComponent.range = range;
						log.Info($"Restored light data for {prefabName}");
					}

					LightFlicker lightFlicker = lanternPrefab.GetComponentInChildren<LightFlicker>();
					if (lightFlicker != null)
					{
						lightFlicker.m_flickerIntensity = flickerIntensity;
						lightFlicker.m_flickerSpeed = flickerSpeed;
						log.Info($"Restored light flicker data for {prefabName}");
					}
				}
			}
		}
	}
}
