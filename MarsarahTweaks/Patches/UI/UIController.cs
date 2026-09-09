using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MarsarahTweaks.Managers.ConfigManager;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIController
	{
		private static readonly LogManager log = new LogManager("UI Controller", LogManager.LogLevel.Warning);

		// UI data
		internal static bool ShowUI = true;
		internal static bool ShowPlayerList = true;
		private static UIMode LastLayout = UIMode.New;

		// This is needed since Unity 6 update to set the default font for TMP fonts since LiberationSans is missing and was default
		[HarmonyPatch(typeof(Hud), "Awake")]
		static class HudAwakePatch
		{
			static void Prefix()
			{
				var notoEmoji = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "NotoEmoji-Regular SDF");
				if (notoEmoji != null)
				{
					TMP_Settings.defaultFontAsset = notoEmoji;
					log.Info("Set default TMP font asset to NotoEmoji-Regular SDF");
				}
				else
				{
					log.Warn("NotoEmoji font not found.");
				}
			}
		}

		public static void LogAllTMPFonts()
		{
			// Finds all TMP_FontAsset objects loaded in memory
			var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();

			if (fonts.Length == 0)
			{
				log.Warn("[TMPDebug] No TMP_FontAssets found in memory!");
				return;
			}

			log.Info($"[TMPDebug] {fonts.Length} TMP_FontAssets found in memory:");
			foreach (var font in fonts.OrderBy(f => f.name))
			{
				// TMP_FontAsset is a ScriptableObject, so no gameObject exists
				string info = font != null ? $"name: '{font.name}', hideFlags: {font.hideFlags}" : "null font";
				log.Info($"[TMPDebug] {info}");
			}
		}

		public static void UpdateUIDisplay()
		{
			UIMode currentLayout = ConfigManager.UILayoutChoice.Value;

			/*if (currentLayout == UIMode.Off)
			{

				ShowUI = false;
				ShowPlayerList = false;
			}
			else
			{*/
			if (LastLayout != currentLayout)
			{
				ShowUI = true;
				ShowPlayerList = true;
			}

			if (Input.GetKeyDown(KeyCode.Insert))
			{
				ShowUI = !ShowUI;
			}
			if (Input.GetKeyDown(KeyCode.Home))
			{
				ShowPlayerList = !ShowPlayerList;
			}
			//}

			LastLayout = currentLayout;
		}

		public static Text CreateTextObject(string name, GameObject parent, Color textColor, string fontName, int fontSize, TextAnchor alignment, Vector2 position, Vector2 sizeDelta)
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

		public static TextMeshProUGUI CreateTMPTextObject(string name, GameObject parent, Color textColor, string fontName, int fontSize, TextAlignmentOptions alignment, Vector2 position, Vector2 sizeDelta, LogManager specificLog)
		{
			//LogAllTMPFonts();

			GameObject textObject = new GameObject(name);
			textObject.layer = 5;
			textObject.transform.SetParent(parent.transform, false);

			RectTransform rectTransform = textObject.AddComponent<RectTransform>();
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = sizeDelta;
			rectTransform.localScale = Vector3.one;

			TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
			tmpText.color = textColor;
			tmpText.font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == fontName);
			tmpText.fontSize = fontSize;
			tmpText.alignment = alignment;
			tmpText.text = ""; // default

			if (tmpText.font != null)
			{
				// Ensure TMP has a proper material
				tmpText.fontMaterial = tmpText.fontMaterial != null ? new Material(tmpText.fontMaterial) : tmpText.font.material;

				if (tmpText.fontMaterial.HasProperty(ShaderUtilities.ID_OutlineWidth) && tmpText.fontMaterial.HasProperty(ShaderUtilities.ID_OutlineColor))
				{
					tmpText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.125f);
					tmpText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
				}

				// Force TMP to register the font material immediately
				tmpText.havePropertiesChanged = true;
				tmpText.SetAllDirty();
				tmpText.ForceMeshUpdate();
			}
			else
			{
				specificLog.Warn($"Font material for '{fontName}' is null or font is missing. Skipping outline setup.");
			}

			return tmpText;
		}

		public static Image CreateUIImageObject(string name, GameObject parent, Vector2 position, Vector2 sizeDelta)
		{
			GameObject iconObject = new GameObject(name);
			iconObject.layer = 5;
			iconObject.transform.SetParent(parent.transform, false);

			RectTransform rectTransform = iconObject.AddComponent<RectTransform>();
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = sizeDelta;
			rectTransform.localScale = Vector3.one;

			Image image = iconObject.AddComponent<Image>();
			image.color = Color.white;

			return image;
		}

		public static void UpdateUIPositions()
		{
			bool newUI = ConfigManager.UILayoutChoice.Value == UIMode.New;

			if (!newUI)
			{
				// Base position
				float xOffset = -45f;
				float yOffset = -230f;

				bool inventoryUIActive = UIInventoryWeightAndSlots.UIInventoryArea != null && ConfigManager.ShowInventoryWeightAndSlots.Value;
				bool enemyDetectorUIActive = UIEnemyDetector.UIEnemyArea != null && ConfigManager.ShowEnemyDetector.Value;
				bool boatSpeedUIActive = UIBoatSpeed.UIBoatArea != null && ConfigManager.ShowBoatSpeed.Value;

				if (inventoryUIActive)
				{
					// -45
					xOffset += 115f;  // Move next element to the right 
					if (!enemyDetectorUIActive && boatSpeedUIActive)
					{
						// Move more if the next element is the boat speed
						xOffset += 30;
					}
				}

				if (enemyDetectorUIActive)
				{
					// -40 / 70
					UIEnemyDetector.UIEnemyArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, yOffset);
					xOffset += 133f;
				}

				if (boatSpeedUIActive)
				{
					// -40 / 100 / 203
					UIBoatSpeed.UIBoatArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, yOffset);
				}
			}
			else
			{
				// Base position
				float xOffset = -10f; // -40
				float yOffset = -230f;

				bool inventoryUIActive = UIInventoryWeightAndSlots.UIWeightBarArea != null && UIInventoryWeightAndSlots.UISlotsArea != null && ConfigManager.ShowInventoryWeightAndSlots.Value;
				bool enemyDetectorUIActive = UIEnemyDetector.UIEnemyArea2 != null && UIEnemyDetector.UIFriendlyArea != null && ConfigManager.ShowEnemyDetector.Value;

				if (inventoryUIActive)
				{
					xOffset += 132f;
				}
				else if (enemyDetectorUIActive)
				{
					xOffset -= 55f;
				}

				if (enemyDetectorUIActive)
				{
					UIEnemyDetector.UIEnemyArea2.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, yOffset);
					UIEnemyDetector.UIFriendlyArea.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset + 54f, yOffset);
				}
			}
		}
	}
}
