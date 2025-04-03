using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class CrossbowsChanges
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class GearSpeedChanges_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.CrossbowsReloadModifications.Name}...");

						UpdateCrossbowsReloadSpeed(__instance);
					}	
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.Configs.CrossbowsReloadModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.Configs.CrossbowsReloadModifications.Name}...");
				}
			}
		}

		private static Dictionary<string, float> newCrossbowsReloadTimes = new Dictionary<string, float>()
		{
			{ "CrossbowArbalest", 2.5f },
			{ "CrossbowRipper", 2.5f },
			{ "CrossbowRipperBlood", 2.5f },
			{ "CrossbowRipperLightning", 2.5f },
			{ "CrossbowRipperNature", 2.5f },
		};

		private static void UpdateCrossbowsReloadSpeed(ObjectDB objDB)
		{
			if (ConfigManager.reducedCrossbowsReloadTimeEnabled.Value)
			{
				foreach (var kvp in newCrossbowsReloadTimes)
				{
					string crossbowName = kvp.Key;
					float newCrossbowReloadTime = kvp.Value;

					// Get the prefab
					GameObject crossbowPrefab = objDB.GetItemPrefab(crossbowName);

					if (crossbowPrefab != null)
					{
						ItemDrop crossbowItem = crossbowPrefab.GetComponent<ItemDrop>();

						if (crossbowItem != null)
						{
							// Apply new relod time
							crossbowItem.m_itemData.m_shared.m_attack.m_reloadTime = newCrossbowReloadTime;
						}
					}
				}
			}
		}
	}
}
