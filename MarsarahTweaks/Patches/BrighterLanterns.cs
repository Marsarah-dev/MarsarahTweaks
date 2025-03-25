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

						UpdateLanterns(__instance, false);
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

		public static void UpdateLanterns(ZNetScene znScene, bool wasChanged)
		{
			if (ConfigManager.brighterLanternsEnabled.Value)
			{
				// Defaults: intensity = 1.5, range = 6, flickerIntensity = 0.1, flickerSpeed = 10

				CreateLanternBackup(znScene, "piece_dvergr_lantern");
				CreateLanternBackup(znScene, "piece_dvergr_lantern_pole");

				UpdateLanternLight(znScene, "piece_dvergr_lantern", 2f, 9f, 0.05f, 5f);  // (2, 9) / (2, 12) / (3 / 12)
				UpdateLanternLight(znScene, "piece_dvergr_lantern_pole", 2f, 15f, 0.05f, 5f);  // (2, 12) / (2, 15) / (3 / 15)

				ApplyChangesToExistingLanterns();
			}
			else if (wasChanged)
			{
				RestoreLanternBackup(znScene, "piece_dvergr_lantern");
				RestoreLanternBackup(znScene, "piece_dvergr_lantern_pole");

				ApplyChangesToExistingLanterns();
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
					MarsarahTweaks.MLog($"Light Component updated for {prefabName}");
				}

				LightFlicker lightFlicker = lanternPrefab.GetComponentInChildren<LightFlicker>();
				if (lightFlicker != null)
				{
					lightFlicker.m_flickerIntensity = flickerIntensity;
					lightFlicker.m_flickerSpeed = flickerSpeed;
					MarsarahTweaks.MLog($"Light Flicker updated for {prefabName}");
				}
			}
		}

		private static void CreateLanternBackup(ZNetScene znScene, string prefabName)
		{
			if (!lanternBackups.ContainsKey(prefabName))
			{
				GameObject lanternPrefab = znScene.GetPrefab(prefabName);
				if (lanternPrefab != null)
				{
					Light lightComponent = lanternPrefab.GetComponentInChildren<Light>();
					LightFlicker lightFlicker = lanternPrefab.GetComponentInChildren<LightFlicker>();

					if (lightComponent != null && lightFlicker != null)
					{
						lanternBackups[prefabName] = (lightComponent.intensity, lightComponent.range, lightFlicker.m_flickerIntensity, lightFlicker.m_flickerSpeed);
						MarsarahTweaks.MLog($"Backup created for {prefabName}");
					}
				}
			}
		}

		private static void RestoreLanternBackup(ZNetScene znScene, string prefabName)
		{
			if (lanternBackups.TryGetValue(prefabName, out var backup))
			{
				GameObject lanternPrefab = znScene.GetPrefab(prefabName);
				if (lanternPrefab != null)
				{
					Light lightComponent = lanternPrefab.GetComponentInChildren<Light>();
					LightFlicker lightFlicker = lanternPrefab.GetComponentInChildren<LightFlicker>();

					if (lightComponent != null)
					{
						lightComponent.intensity = backup.intensity;
						lightComponent.range = backup.range;
						MarsarahTweaks.MLog($"Light Component restored for {prefabName}");
					}
					if (lightFlicker != null)
					{
						lightFlicker.m_flickerIntensity = backup.flickerIntensity;
						lightFlicker.m_flickerSpeed = backup.flickerSpeed;
						MarsarahTweaks.MLog($"Light Flicker restored for {prefabName}");
					}
				}
				lanternBackups.Remove(prefabName);
			}
		}

		/// Applies changes to all existing lanterns in the world.
		private static void ApplyChangesToExistingLanterns()
		{
			foreach (Light light in UnityEngine.Object.FindObjectsOfType<Light>())
			{
				if (light.gameObject.name.Contains("piece_dvergr_lantern"))
				{
					if (ConfigManager.brighterLanternsEnabled.Value)
					{
						light.intensity = 2f;
						light.range = light.gameObject.name.Contains("pole") ? 12f : 9f;

						LightFlicker flicker = light.GetComponent<LightFlicker>();
						if (flicker != null)
						{
							flicker.m_flickerIntensity = 0.05f;
							flicker.m_flickerSpeed = 5f;
						}
					}
					else
					{
						if (lanternBackups.TryGetValue(light.gameObject.name, out var backup))
						{
							light.intensity = backup.intensity;
							light.range = backup.range;

							LightFlicker flicker = light.GetComponent<LightFlicker>();
							if (flicker != null)
							{
								flicker.m_flickerIntensity = backup.flickerIntensity;
								flicker.m_flickerSpeed = backup.flickerSpeed;
							}
						}
					}
				}
			}
		}
	}
}
