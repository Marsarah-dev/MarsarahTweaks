using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.Balance
{
	internal class CrossbowsChanges
	{
		private static readonly LogManager log = new LogManager("Crossbow Changes", LogManager.LogLevel.Warning);

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

		// Crossbow reload movement settings
		private const float MovingReloadSpeed = 0.5f;
		private static bool sprintRequested;
		private static bool jumpRequested;
		private static bool blockOrDodgeRequested;
		private static bool movingReloadLogged;
		private static float lastPreservationLogTime = -100f;

		private static bool IsMovementReloadEnabled()
		{
			return ConfigManager.CrossbowReloadWhileMovingEnabled != null
				&& ConfigManager.CrossbowReloadWhileMovingEnabled.Value;
		}

		private static bool IsMovementInterruption(Player player)
		{
			if (!IsMovementReloadEnabled()) return false;
			if (player != Player.m_localPlayer) return false;
			if (blockOrDodgeRequested) return false;

			return sprintRequested
				|| jumpRequested
				|| player.IsRunning()
				|| !player.IsOnGround();
		}

		private static Player.MinorActionData GetCrossbowReloadAction(Player player, List<Player.MinorActionData> queue)
		{
			if (queue == null) return null;

			foreach (Player.MinorActionData action in queue)
			{
				if (action == null) continue;
				if (action.m_type != Player.MinorActionData.ActionType.Reload) continue;
				if (!IsCrossbow(action.m_item)) continue;
				if (!player.IsItemEquiped(action.m_item)) continue;

				return action;
			}

			return null;
		}

		private static void RestoreReloadAction(Player player, List<Player.MinorActionData> queue, Player.MinorActionData action)
		{
			if (action == null || queue == null) return;
			if (!IsMovementReloadEnabled()) return;
			if (player != Player.m_localPlayer) return;
			if (!IsCrossbow(action.m_item)) return;
			if (!player.IsItemEquiped(action.m_item)) return;
			if (queue.Count != 0) return;

			queue.Add(action);

			if (Time.time - lastPreservationLogTime >= 1f)
			{
				log.Info($"Preserved crossbow reload action: {action.m_item.m_dropPrefab.name}");
				lastPreservationLogTime = Time.time;
			}
		}

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

				// Apply new reload time
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

		// Track movement controls so we can recognize interruptions
		// before the actual sprint or jump movement begins.
		[HarmonyPatch(typeof(Player), "SetControls")]
		private static class CrossbowMovementControls_Patch
		{
			private static void Prefix(Player __instance, Vector3 movedir, bool run, ref bool jump, bool blockHold, bool dodge)
			{
				if (__instance != Player.m_localPlayer) return;

				sprintRequested = run && movedir.sqrMagnitude > 0.01f;
				jumpRequested = jump;
				blockOrDodgeRequested = blockHold || dodge;
			}
		}

		// Preserve reload actions when sprinting changes the action queue.
		[HarmonyPatch(typeof(Player), "CheckRun")]
		private static class CrossbowCheckRun_Patch
		{
			private static void Prefix(Player __instance, List<Player.MinorActionData> ___m_actionQueue, ref Player.MinorActionData __state)
			{
				__state = null;

				if (!IsMovementInterruption(__instance)) return;

				__state = GetCrossbowReloadAction(__instance, ___m_actionQueue);
			}

			private static void Postfix(Player __instance, List<Player.MinorActionData> ___m_actionQueue, Player.MinorActionData __state)
			{
				RestoreReloadAction(__instance, ___m_actionQueue, __state);
			}
		}

		// Preserve partial reload progress if movement clears the queue.
		[HarmonyPatch(typeof(Player), "ClearActionQueue")]
		private static class CrossbowClearActionQueue_Patch
		{
			private static void Prefix(Player __instance, List<Player.MinorActionData> ___m_actionQueue, ref Player.MinorActionData __state)
			{
				__state = null;

				if (!IsMovementInterruption(__instance)) return;

				__state = GetCrossbowReloadAction(__instance, ___m_actionQueue);
			}

			private static void Postfix(Player __instance, List<Player.MinorActionData> ___m_actionQueue, Player.MinorActionData __state)
			{
				RestoreReloadAction(__instance, ___m_actionQueue, __state);
			}
		}

		// Allow sprinting to proceed while a crossbow reload is queued.
		[HarmonyPatch(typeof(Player), "InMinorActionSlowdown")]
		private static class CrossbowSprintReload_Patch
		{
			private static bool Prefix(Player __instance, List<Player.MinorActionData> ___m_actionQueue, ref bool __result)
			{
				if (!IsMovementReloadEnabled()) return true;
				if (__instance != Player.m_localPlayer) return true;
				if (!sprintRequested || blockOrDodgeRequested) return true;
				if (GetCrossbowReloadAction(__instance, ___m_actionQueue) == null) return true;

				__result = false;
				return false;
			}
		}

		// Apply half-speed progress while sprinting or airborne.
		[HarmonyPatch(typeof(Player), "UpdateActionQueue")]
		private static class CrossbowMovementReloadSpeed_Patch
		{
			private static void Prefix(Player __instance, ref float dt, List<Player.MinorActionData> ___m_actionQueue)
			{
				if (__instance != Player.m_localPlayer) return;

				Player.MinorActionData action = GetCrossbowReloadAction(__instance, ___m_actionQueue);

				bool movingReload = IsMovementReloadEnabled()
					&& action != null
					&& (__instance.IsRunning() || !__instance.IsOnGround());

				if (movingReload)
				{
					dt *= MovingReloadSpeed;

					if (!movingReloadLogged)
					{
						log.Info($"Crossbow reload slowed to {MovingReloadSpeed:P0} during movement");
						movingReloadLogged = true;
					}
				}
				else if (movingReloadLogged)
				{
					log.Info("Crossbow reload returned to normal speed");
					movingReloadLogged = false;
				}
			}
		}
	}
}
