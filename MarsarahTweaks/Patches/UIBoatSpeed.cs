using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahTweaks.Patches
{
	internal class UIBoatSpeed
	{
		// UI data
		private static float boatSpeed;
		private static bool showBoatSpeedUI;

		// UI elements
		private static Text UIBoatText = null;
		private static Text UIBoatTextTitle = null;
		private static Image boatAreaBackground = null;

		[HarmonyPatch(typeof(Ship), "GetSpeed")]
		class ShowBoatSpeed_Patch
		{
			static void Prefix(Ship __instance, ref Rigidbody ___m_body)
			{
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
		public static class MarsarahMod_HUD_Update_Patch
		{
			public static void Postfix(Hud __instance)
			{
				if (__instance == null) return;

				if (ConfigManager.showBoatSpeed.Value)
				{
					CreateUI(__instance); // Create UI if missing

					UIBoatText.enabled = showBoatSpeedUI && UIController.showUI;
					UIBoatTextTitle.enabled = showBoatSpeedUI && UIController.showUI;
					boatAreaBackground.enabled = showBoatSpeedUI && UIController.showUI;

					if (showBoatSpeedUI && UIController.showUI)
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
				GameObject UIBoatArea = new GameObject("BoatArea");
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
				boatAreaBackground.enabled = UIController.showUI;

				// Boat area title
				UIBoatTextTitle = CreateTextObject("BoatTextTitle", UIBoatArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(-25f, 0f), UIBoatAreaSize);
				// Boat area speed text
				UIBoatText = CreateTextObject("BoatText", UIBoatArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(45f, 0f), UIBoatAreaSize);
			}

			private static Text CreateTextObject(string name, GameObject parent, Color textColor, string fontName, int fontSize, TextAnchor alignment, Vector2 position, Vector2 sizeDelta)
			{
				GameObject textObject = new GameObject(name);
				textObject.layer = 5;
				textObject.transform.SetParent(parent.transform);
				RectTransform textTransform = textObject.AddComponent<RectTransform>();
				textTransform.anchoredPosition = position;
				textTransform.sizeDelta = sizeDelta;
				textTransform.localScale = Vector3.one;  // Ensure correct scale

				Text text = textObject.AddComponent<Text>();
				text.color = textColor;
				text.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name == fontName);
				text.fontSize = fontSize;
				text.alignment = alignment;

				Outline outline = textObject.AddComponent<Outline>();
				outline.effectColor = Color.black;
				outline.effectDistance = new Vector2(1f, -1f);
				outline.useGraphicAlpha = true;
				outline.useGUILayout = true;

				return text;
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
