using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIBoatSpeed : UIController
	{
		private static readonly LogManager log = new LogManager("UI Boat Speed", LogManager.LogLevel.Warning);

		// UI data
		private static float boatSpeed;
		private static bool showBoatSpeedUI;

		// UI elements
		internal static GameObject UIBoatArea = null;
		private static Text UIBoatText = null;
		private static Text UIBoatTextTitle = null;

		internal static GameObject UIBoatArea2 = null;
		private static Text UIBoatText2 = null;
		private static TMPro.TextMeshProUGUI UIBoatEmojiTMP = null;

		[HarmonyPatch(typeof(Ship), "GetSpeed")]
		class ShowBoatSpeed_Patch
		{
			private static void Prefix(Ship __instance, ref Rigidbody ___m_body)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (ConfigManager.ShowBoatSpeed.Value)
				{
					if (__instance && __instance.HasPlayerOnboard())
					{
						boatSpeed = Vector3.Dot(___m_body.linearVelocity, __instance.transform.forward);
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

		[HarmonyPatch(typeof(Hud), "Awake")]
		class InventoryWeightAndSlots_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowInventoryWeightAndSlots.Value)
				{
					CreateTextBasedUI(__instance); 
					CreateSymbolBasedUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		static class BoatSpeedHUDUpdate_Patch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowBoatSpeed.Value)
				{
					CreateTextBasedUI(__instance);
					CreateSymbolBasedUI(__instance);

					if (!ConfigManager.UseSymbolsForUI.Value)
					{
						UIBoatArea?.SetActive(showBoatSpeedUI && showUI);
						UIBoatArea2?.SetActive(false);

						if (showBoatSpeedUI && showUI)
						{
							UIBoatText.color = GetColorFromSpeed(boatSpeed);
							UIBoatText.text = boatSpeed > 0 ? boatSpeed.ToString("0.00") : "R " + (Math.Abs(boatSpeed)).ToString("0.00");
							UIBoatTextTitle.text = "Boat speed";
						}
					}
					else
					{
						UIBoatArea?.SetActive(false);
						UIBoatArea2?.SetActive(showBoatSpeedUI && showUI);

						if (showBoatSpeedUI && showUI)
						{
							UIBoatText2.color = GetColorFromSpeed(boatSpeed);
							UIBoatText2.text = boatSpeed > 0 ? boatSpeed.ToString("0.00") : "R " + (Math.Abs(boatSpeed)).ToString("0.00");
							UIBoatEmojiTMP.text = "⛵";
						}
					}
				}
				else
				{
					// We're calling create here to make sure objects are safe to access
					CreateTextBasedUI(__instance);
					CreateSymbolBasedUI(__instance);

					UIBoatArea?.SetActive(false);
					UIBoatArea2?.SetActive(false);
				}
			}			

			private static Color GetColorFromSpeed(float speed)
			{
				if (speed < 2.5f) return Color.red;
				if (speed < 5f) return new Color(1f, 0.549019f, 0f);
				if (speed < 7.5f) return Color.yellow;
				return Color.green;
			}
		}

		private static void CreateTextBasedUI(Hud hud)
		{
			if (UIBoatArea != null && UIBoatText != null && UIBoatTextTitle != null)
				return;  // UI already exists, no need to create again

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UIBoatAreaSize;
			Vector2 UIBoatAreaSizeDefault = new Vector2(155f, 30f); // width, height

			if (!ConfigManager.UseSymbolsForUI.Value)
				UIBoatAreaSize = UIBoatAreaSizeDefault;
			else
				UIBoatAreaSize = new Vector2(155f, 30f);

			// Boat area object
			UIBoatArea = new GameObject("BoatArea");
			UIBoatArea.layer = 5;
			UIBoatArea.transform.SetParent(hud.m_healthPanel.transform); // health
			RectTransform boatAreaTransform = UIBoatArea.AddComponent<RectTransform>();
			boatAreaTransform.anchorMin = new Vector2(1f, 1f);
			boatAreaTransform.anchorMax = new Vector2(1f, 1f);
			boatAreaTransform.anchoredPosition = new Vector2(ConfigManager.ShowInventoryWeightAndSlots.Value && ConfigManager.ShowEnemyDetector.Value ? 203f : ConfigManager.ShowInventoryWeightAndSlots.Value || ConfigManager.ShowEnemyDetector.Value ? 100f : -40f, -230f);
			boatAreaTransform.sizeDelta = UIBoatAreaSize;
			UIBoatArea.transform.localScale = Vector3.one;  // Ensure correct scale

			// Background texture
			Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
			Image boatAreaBackground = UIBoatArea.AddComponent<Image>();
			boatAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
			boatAreaBackground.sprite = sprite;
			boatAreaBackground.type = Image.Type.Sliced;

			// Boat area title
			UIBoatTextTitle = CreateTextObject("BoatTextTitle", UIBoatArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(-25f, 0f), UIBoatAreaSizeDefault);

			// Boat area speed text
			UIBoatText = CreateTextObject("BoatText", UIBoatArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(45f, 0f), UIBoatAreaSize);
		}

		private static void CreateSymbolBasedUI(Hud hud)
		{
			//if (hud.m_shipHudRoot == null) return;

			if (UIBoatArea2 != null && UIBoatText2 != null && UIBoatEmojiTMP != null) return;

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF"; // NotoEmoji-Regular
			Vector2 UIBoatAreaSize = new Vector2(80f, 30f); // width, height
			/*float xOffset = ConfigManager.ShowInventoryWeightAndSlots.Value && ConfigManager.ShowEnemyDetector.Value ? 214f : 
				ConfigManager.ShowInventoryWeightAndSlots.Value && !ConfigManager.ShowEnemyDetector.Value ? 105f :
				!ConfigManager.ShowInventoryWeightAndSlots.Value && ConfigManager.ShowEnemyDetector.Value ? 57f : - 51f;
			float yOffset = -230f;*/

			float xOffset = Game.m_noMap ? -145f : -283f;
			float yOffset = -225f;

			bool minimalStatusEffectsLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "MinimalStatusEffects");
			if (minimalStatusEffectsLoaded)
			{
				xOffset = -360f;
				yOffset = -25f; // -55f;
			}

			// Boat Area Object
			UIBoatArea2 = new GameObject("BoatArea2");
			UIBoatArea2.layer = 5;
			//UIBoatArea2.transform.SetParent(hud.m_healthPanel.transform); // health
			//UIBoatArea2.transform.SetParent(hud.m_shipHudRoot.transform); // ship hud (top right)
			UIBoatArea2.transform.SetParent(hud.m_rootObject.transform); // minimap

			RectTransform boatAreaTransform = UIBoatArea2.AddComponent<RectTransform>();
			boatAreaTransform.anchorMin = new Vector2(1f, 1f);
			boatAreaTransform.anchorMax = new Vector2(1f, 1f);
			boatAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
			boatAreaTransform.sizeDelta = UIBoatAreaSize;
			UIBoatArea2.transform.localScale = Vector3.one;

			// Background texture
			//Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
			//Image boatAreaBackground = UIBoatArea2.AddComponent<Image>();
			//boatAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
			//boatAreaBackground.sprite = sprite;
			//boatAreaBackground.type = Image.Type.Sliced;

			// Text overlay
			UIBoatText2 = CreateTextObject("BoatText2", UIBoatArea2, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), UIBoatAreaSize);

			// Enemy icon
			UIBoatEmojiTMP = CreateTMPTextObject("BoatEmojiTMP", UIBoatArea2, Color.white, UIEmojiFontName, UITextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), UIBoatAreaSize);
		}
	}
}
