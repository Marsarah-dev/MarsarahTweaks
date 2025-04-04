using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MarsarahTweaks.Patches.Features
{
	internal class TougherShipsChanges
	{
		private static readonly Dictionary<string, float> originalHealthValues = new Dictionary<string, float>();

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class ShipsHP_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				//if (!ZNet.instance || !ZNet.instance.IsServer()) return; // Only run on server
				//if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers
				// Run on both server and client

				if (__instance == null) return;

				//MarsarahTweaks.MLog($"ZNetScene Awake: Updating {ConfigManager.Configs.TougherShips.Name}...");
				updateShipHP(__instance, false);
			}
		}

		public static void updateShipHP(ZNetScene znScene, bool wasChanged)
		{
			if (ConfigManager.tougherShipsEnabled.Value)
			{
				ApplyHealthChange(znScene, "Raft", 400);
				ApplyHealthChange(znScene, "Karve", 650);
				ApplyHealthChange(znScene, "VikingShip", 1250);
				ApplyHealthChange(znScene, "VikingShip_Ashlands", 4000);
			}
			else if (wasChanged)
			{
				RestoreOriginalHealth(znScene, "Raft");
				RestoreOriginalHealth(znScene, "Karve");
				RestoreOriginalHealth(znScene, "VikingShip");
				RestoreOriginalHealth(znScene, "VikingShip_Ashlands");
			}
		}

		private static void ApplyHealthChange(ZNetScene scene, string prefabName, float newHealth)
		{
			GameObject shipPrefab = scene.GetPrefab(prefabName);
			if (shipPrefab == null) return;

			WearNTear wearNTear = shipPrefab.GetComponent<WearNTear>();
			if (wearNTear == null) return;

			if (!originalHealthValues.ContainsKey(prefabName))
			{
				//MarsarahTweaks.MLog($"[Tougher Ships] Backing up original HP ({wearNTear.m_health}) for ship {prefabName}");
				originalHealthValues[prefabName] = wearNTear.m_health; // Store original HP
			}

			//MarsarahTweaks.MLog($"[Tougher Ships] Setting new HP ({newHealth}) for ship {prefabName}");
			wearNTear.m_health = newHealth;
		}

		private static void RestoreOriginalHealth(ZNetScene scene, string prefabName)
		{
			if (!originalHealthValues.TryGetValue(prefabName, out float originalHealth)) return;

			GameObject shipPrefab = scene.GetPrefab(prefabName);
			if (shipPrefab == null) return;

			WearNTear wearNTear = shipPrefab.GetComponent<WearNTear>();
			if (wearNTear == null) return;

			//MarsarahTweaks.MLog($"[Tougher Ships] Restoring old HP ({originalHealth}) for ship {prefabName}");
			wearNTear.m_health = originalHealth;

			originalHealthValues.Remove(prefabName);
		}
	}
}
