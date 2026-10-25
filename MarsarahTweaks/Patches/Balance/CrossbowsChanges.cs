using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.Balance
{
	internal class CrossbowsChanges
	{
		private static readonly LogManager log = new LogManager("Crossbow Changes", LogManager.LogLevel.Info);

		private const string LoadedStateKey = "MarsarahTweaks.CrossbowLoaded";
		private static readonly AccessTools.FieldRef<Player, ItemDrop.ItemData> loadedWeaponField = AccessTools.FieldRefAccess<Player, ItemDrop.ItemData>("m_weaponLoaded");
		private static readonly Action<Player, ItemDrop.ItemData> setWeaponLoaded = AccessTools.MethodDelegate<Action<Player, ItemDrop.ItemData>>(AccessTools.Method(typeof(Player), "SetWeaponLoaded", new[] { typeof(ItemDrop.ItemData) }));

		private static readonly Dictionary<string, float> newCrossbowsReloadTimes = new Dictionary<string, float>()
		{
			{ "CrossbowArbalest", 2.5f },
			{ "CrossbowRipper", 2.5f },
			{ "CrossbowRipperBlood", 2.5f },
			{ "CrossbowRipperLightning", 2.5f },
			{ "CrossbowRipperNature", 2.5f },
		};

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class CrossbowsChanges_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateCrossbowsReloadSpeed(__instance);
			}
		}		

		private static void UpdateCrossbowsReloadSpeed(ObjectDB objDB)
		{
			if (!ConfigManager.ReducedCrossbowsReloadTimeEnabled.Value) return;
			
			foreach (var kvp in newCrossbowsReloadTimes)
			{
				string crossbowName = kvp.Key;
				float newCrossbowReloadTime = kvp.Value;

				// Get the prefab
				GameObject crossbowPrefab = objDB.GetItemPrefab(crossbowName);

				if (crossbowPrefab == null) continue;
				
				ItemDrop crossbowItem = crossbowPrefab.GetComponent<ItemDrop>();

				if (crossbowItem == null) continue;

				// Apply new relod time
				crossbowItem.m_itemData.m_shared.m_attack.m_reloadTime = newCrossbowReloadTime;
				log.Info($"Applied new Crossbow reload time of {newCrossbowReloadTime}s to {crossbowName}");
			}
		}

		private static bool IsCrossbow(ItemDrop.ItemData item)
		{
			return item != null
				&& item.m_shared != null
				&& item.m_shared.m_attack != null
				&& item.m_shared.m_attack.m_requiresReload
				&& item.m_shared.m_skillType == Skills.SkillType.Crossbows;
		}

		[HarmonyPatch(typeof(Humanoid), "UnequipItem")]
		private static class CrossbowUnequip_Patch
		{
			private static void Prefix(Humanoid __instance, ItemDrop.ItemData item)
			{
				if (!ConfigManager.KeepCrossbowsLoadedEnabled.Value) return;
				if (!(__instance is Player player) || player != Player.m_localPlayer) return;
				if (!IsCrossbow(item) || !__instance.IsItemEquiped(item)) return;
				if (!ReferenceEquals(loadedWeaponField(player), item)) return;

				item.m_customData[LoadedStateKey] = "1";

				log.Info($"Saved loaded state: {item.m_dropPrefab.name}");
			}
		}

		[HarmonyPatch(typeof(Player), "UpdateWeaponLoading")]
		private static class CrossbowRestore_Patch
		{
			private static void Prefix(Player __instance, ItemDrop.ItemData weapon)
			{
				if (__instance != Player.m_localPlayer) return;
				if (weapon == null || weapon.m_customData == null) return;
				if (!weapon.m_customData.Remove(LoadedStateKey)) return;

				if (!ConfigManager.KeepCrossbowsLoadedEnabled.Value) return;
				if (!IsCrossbow(weapon)) return;

				if (ReferenceEquals(loadedWeaponField(__instance), weapon)) return;

				setWeaponLoaded(__instance, weapon);

				log.Info($"Restored loaded state: {weapon.m_dropPrefab.name}");
			}
		}
	}
}
