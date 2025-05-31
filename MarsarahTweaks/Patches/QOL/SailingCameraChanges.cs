using HarmonyLib;
using MarsarahTweaks.Patches.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Diagnostics;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal class SailingCameraChanges
	{
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

		[HarmonyPatch(typeof(Ship), "GetSpeed")]
		private class ShipCamera_Patch
		{
			private static void Prefix(Ship __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				UpdateShipControlledState(__instance);
			}
		}

		[HarmonyPatch(typeof(GameCamera), "GetCameraPosition")]
		private class GameCamera_Patch
		{
			private static void Postfix(GameCamera __instance, ref float dt, ref Vector3 pos, ref Quaternion rot)
			{
				if (!Player.m_localPlayer) return;

				// Set camera offset to move up if controlling a ship
				float offsetY = UpdateCameraOffset(dt);
				pos += Vector3.up * offsetY;
			}
		}

		private static void UpdateShipControlledState(Ship __instance)
		{
			if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

			if (ConfigManager.CameraUpWhenSailingEnabled.Value)
			{
				//bool isControlling = Traverse.Create(__instance).Method("HaveControllingPlayer").GetValue<bool>();
				Ship currentControlledShip = Player.m_localPlayer.GetControlledShip();
				bool isControlling = currentControlledShip != null;

				if (isControlling != moveCameraUp)
				{
					moveCameraUp = isControlling;

					if (moveCameraUp)
					{
						string prefabName = __instance.gameObject.name.Replace("(Clone)", "").Trim();

						if (!shipOffsets.TryGetValue(prefabName, out targetOffset))
						{
							targetOffset = 0f; // Default if unknown ship
						}

						//MarsarahTweaks.LogInfo($"[SailingCamera] Controlling {prefabName}, offset set to {targetOffset:F1}");
					}
					else
					{
						targetOffset = 0f;
					}
				}
			}
			else
			{
				moveCameraUp = false;
				targetOffset = 0f;
			}
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
