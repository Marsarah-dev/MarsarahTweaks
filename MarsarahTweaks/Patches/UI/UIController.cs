using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIController
	{
		public static bool showUI = true;		

		public static void UpdateUIDisplay()
		{
			if (Input.GetKeyDown(KeyCode.Insert))
			{
				showUI = !showUI;
			}
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

		public static TextMeshProUGUI CreateTMPTextObject(string name, GameObject parent, Color textColor, string fontName, int fontSize, TextAlignmentOptions alignment, Vector2 position, Vector2 sizeDelta)
		{
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

			if (tmpText.font != null && tmpText.fontMaterial != null)
			{
				tmpText.fontMaterial = new Material(tmpText.fontMaterial);

				if (tmpText.fontMaterial.HasProperty(ShaderUtilities.ID_OutlineWidth) && tmpText.fontMaterial.HasProperty(ShaderUtilities.ID_OutlineColor))
				{
					tmpText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.125f);
					tmpText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
				}
			}
			/*else
			{
				MarsarahTweaks.LogWarn($"[Boat UI] Font material for '{fontName}' is null or font is missing. Skipping outline setup.");
			}*/

			return tmpText;
		}

		public static void UpdateUIPositions()
		{
			if (!ConfigManager.UseSymbolsForUI.Value)
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
