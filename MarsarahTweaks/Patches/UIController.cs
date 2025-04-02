using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahTweaks.Patches
{
	internal class UIController
	{
		public static bool showUI = true;

		public static void UpdateUIDisplay()
		{
			if (Input.GetKeyDown(KeyCode.Insert))
			{
				UIController.showUI = !UIController.showUI;
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

		public static void UpdateUIPositions()
		{
			// Base position
			float xOffset = -45f;
			float yOffset = -230f;

			bool inventoryUIActive = UIInventoryWeightAndSlots.UIInventoryArea != null && ConfigManager.showInventoryWeightAndSlots.Value;
			bool enemyDetectorUIActive = UIEnemyDetector.UIEnemyArea != null && ConfigManager.showEnemyDetector.Value;
			bool boatSpeedUIActive = UIBoatSpeed.UIBoatArea != null && ConfigManager.showBoatSpeed.Value;

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
	}
}
