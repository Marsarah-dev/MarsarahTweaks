using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches
{
	internal class BrighterLanterns
	{
		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class BrighterLanterns_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ZNetScene Awake: Updating {ConfigManager.Configs.BrighterLanterns.Name}...");

						UpdateLanterns(__instance);
					}
					else
					{
						MarsarahTweaks.MLog($"ZNetScene Awake: I am a server. No changes made to {ConfigManager.Configs.BrighterLanterns.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ZNetScene Awake: Too early to do anything. No changes made to {ConfigManager.Configs.BrighterLanterns.Name}...");
				}
			}
		}

		private static Dictionary<string, (float intensity, float range, float flickerIntensity, float flickerSpeed)> lanternBackups = new Dictionary<string, (float intensity, float range, float flickerIntensity, float flickerSpeed)>();

		private static void UpdateLanterns(ZNetScene znScene)
		{
			if (ConfigManager.brighterLanternsEnabled.Value)
			{
				// Defaults: intensity = 1.5, range = 6, flickerIntensity = 0.1, flickerSpeed = 10

				UpdateLanternLight(znScene, "piece_dvergr_lantern", 2f, 9f, 0.05f, 5f);  // (2, 9) / (2, 12) / (3 / 12)
				UpdateLanternLight(znScene, "piece_dvergr_lantern_pole", 2f, 15f, 0.05f, 5f);  // (2, 12) / (2, 15) / (3 / 15)
			}
		}

		private static void UpdateLanternLight(ZNetScene znScene, string prefabName, float lightIntensity, float lightRange, float flickerIntensity, float flickerSpeed)
		{
			GameObject lanternPrefab = znScene.GetPrefab(prefabName);
			if (lanternPrefab != null)
			{
				Light lightComponent = lanternPrefab.GetComponentInChildren<Light>();
				if (lightComponent != null)
				{
					lightComponent.intensity = lightIntensity;
					lightComponent.range = lightRange;
					//MarsarahTweaks.MLog($"Light Component updated for {prefabName}");
				}

				LightFlicker lightFlicker = lanternPrefab.GetComponentInChildren<LightFlicker>();
				if (lightFlicker != null)
				{
					lightFlicker.m_flickerIntensity = flickerIntensity;
					lightFlicker.m_flickerSpeed = flickerSpeed;
					//MarsarahTweaks.MLog($"Light Flicker updated for {prefabName}");
				}
			}
		}
	}
}
