using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIBoatSpeed : UIController
	{
		// UI data
		private static float boatSpeed;
		private static bool showBoatSpeedUI;

		// UI elements
		internal static GameObject UIBoatArea = null;
		private static Text UIBoatText = null;
		private static Text UIBoatTextTitle = null;
		private static Image boatAreaBackground = null;

		[HarmonyPatch(typeof(Ship), "GetSpeed")]
		class ShowBoatSpeed_Patch
		{
			static void Prefix(Ship __instance, ref Rigidbody ___m_body)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (ConfigManager.showBoatSpeed.Value)
				{
					if (__instance && __instance.HasPlayerOnboard())
					{
						boatSpeed = Vector3.Dot(___m_body.velocity, __instance.transform.forward);
					}
					if (Traverse.Create((object)__instance).Method("HaveControllingPlayer", Array.Empty<object>()).GetValue<bool>())
					{
						showBoatSpeedUI = true;
					}
					else
					{
						showBoatSpeedUI = false;
					}
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		public static class BoatSpeedHUDUpdate_Patch
		{
			public static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.showBoatSpeed.Value)
				{
					CreateUI(__instance); // Create UI if missing

					UIBoatText.enabled = showBoatSpeedUI && showUI;
					UIBoatTextTitle.enabled = showBoatSpeedUI && showUI;
					boatAreaBackground.enabled = showBoatSpeedUI && showUI;

					if (showBoatSpeedUI && showUI)
					{
						UIBoatText.color = GetColorFromSpeed(boatSpeed);
						UIBoatTextTitle.text = "Boat speed";
						UIBoatText.text = boatSpeed > 0 ? "F " + boatSpeed.ToString("0.00") : "R " + (Math.Abs(boatSpeed)).ToString("0.00");
					}
				}
				else
				{
					if (UIBoatText != null)
						UIBoatText.enabled = false;
					if (UIBoatTextTitle != null)
						UIBoatTextTitle.enabled = false;
					if (boatAreaBackground != null)
						boatAreaBackground.enabled = false;
				}
			}			

			private static void CreateUI(Hud hud)
			{
				if (UIBoatText != null && UIBoatTextTitle != null && boatAreaBackground != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIBoatAreaSize = new Vector2(155f, 30f); // width, height

				// Boat area object
				UIBoatArea = new GameObject("BoatArea");
				UIBoatArea.layer = 5;
				UIBoatArea.transform.SetParent(hud.m_healthPanel.transform); // health
				RectTransform boatAreaTransform = UIBoatArea.AddComponent<RectTransform>();
				boatAreaTransform.anchorMin = new Vector2(1f, 1f);
				boatAreaTransform.anchorMax = new Vector2(1f, 1f);
				boatAreaTransform.anchoredPosition = new Vector2(ConfigManager.showInventoryWeightAndSlots.Value && ConfigManager.showEnemyDetector.Value ? 203f : ConfigManager.showInventoryWeightAndSlots.Value || ConfigManager.showEnemyDetector.Value ? 100f : -40f, -230f);
				boatAreaTransform.sizeDelta = UIBoatAreaSize;
				UIBoatArea.transform.localScale = Vector3.one;  // Ensure correct scale

				// Background texture
				Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
				boatAreaBackground = UIBoatArea.AddComponent<Image>();
				boatAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				boatAreaBackground.sprite = sprite;
				boatAreaBackground.type = Image.Type.Sliced;
				boatAreaBackground.enabled = showUI;

				// Boat area title
				UIBoatTextTitle = CreateTextObject("BoatTextTitle", UIBoatArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(-25f, 0f), UIBoatAreaSize);

				// Boat area speed text
				UIBoatText = CreateTextObject("BoatText", UIBoatArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(45f, 0f), UIBoatAreaSize);
			}

			private static Color GetColorFromSpeed(float speed)
			{
				if (speed < 2.5f) return Color.red;
				if (speed < 5f) return new Color(1f, 0.549019f, 0f);
				if (speed < 7.5f) return Color.yellow;
				return Color.green;
			}
		}
	}
}
