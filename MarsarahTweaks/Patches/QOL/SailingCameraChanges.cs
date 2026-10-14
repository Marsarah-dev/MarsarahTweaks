using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal class SailingCameraChanges
	{
		private static readonly LogManager log = new LogManager("Sailing Camera", LogManager.LogLevel.Warning);

		private static readonly Dictionary<string, float> shipOffsets = new Dictionary<string, float>()
		{
			{ "Raft", 2f },
			{ "Karve", 3f },
			{ "VikingShip", 4f },			// Longship
			{ "VikingShip_Ashlands", 5f }   // Drakkar
		};

		private static bool moveCameraUp = false;
		private static float currentOffset = 0f;
		private static float targetOffset = 0f;
		private const float smoothSpeed = 2f; // Higher = faster transition

		//private static float defaultMaxDistance = -1f; // Seems to be 8
		//private const float sailingMaxDistance = 20f;

		/*[HarmonyPatch(typeof(Ship), "GetSpeed")]
		private class ShipCamera_Patch
		{
			private static void Prefix(Ship __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				UpdateShipControlledState(__instance);
			}
		}*/

		[HarmonyPatch(typeof(GameCamera), "GetCameraPosition")]
		private class GameCamera_Patch
		{
			private static void Postfix(GameCamera __instance, ref float dt, ref Vector3 pos, ref Quaternion rot)
			{
				if (!Player.m_localPlayer) return;

				UpdateShipControlledState();

				float offsetY = UpdateCameraOffset(dt);
				pos += Vector3.up * offsetY;
			}
		}

		/*[HarmonyPatch(typeof(GameCamera), "GetCameraPosition")]
		private class GameCamera_Patch
		{
			private static void Postfix(GameCamera __instance, ref float dt, ref Vector3 pos, ref Quaternion rot)
			{
				if (!Player.m_localPlayer) return;

				// Set camera offset to move up if controlling a ship
				float offsetY = UpdateCameraOffset(dt);
				pos += Vector3.up * offsetY;
			}
		}*/

		private static void UpdateShipControlledState()
		{
			if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

			if (!ConfigManager.CameraUpWhenSailingEnabled.Value)
			{
				moveCameraUp = false;
				targetOffset = 0f;
				return;
			}

			Ship currentControlledShip = Player.m_localPlayer.GetControlledShip();

			if (!currentControlledShip)
			{
				moveCameraUp = false;
				targetOffset = 0f;
				return;
			}

			string prefabName = currentControlledShip.gameObject.name.Replace("(Clone)", "").Trim();

			if (!shipOffsets.TryGetValue(prefabName, out float newTargetOffset))
			{
				newTargetOffset = 0f;
			}

			if (!moveCameraUp || !Mathf.Approximately(targetOffset, newTargetOffset))
			{
				log.Info($"Controlling {prefabName}, offset set to {newTargetOffset:F1}");
			}

			moveCameraUp = true;
			targetOffset = newTargetOffset;
		}

		private static float UpdateCameraOffset(float dt)
		{
			if (ConfigManager.CameraUpWhenSailingEnabled.Value)
			{
				currentOffset = Mathf.Lerp(currentOffset, targetOffset, dt * smoothSpeed);
			}
			else
			{
				currentOffset = Mathf.Lerp(currentOffset, 0f, dt * smoothSpeed);
			}

			return currentOffset;
		}
	}
}
