using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace MarsarahTweaks.Patches
{
	internal class UIInventoryWeightAndSlots
	{
		// UI data
		private static float currentWeight;
		private static float maxWeight;
		private static float freeSlots;
		private static float freeSlotsPercent;

		// UI elements
		private static Text UIWeightText = null;
		private static Text UISlotText = null;
		private static Image inventoryAreaBackground = null;

		[HarmonyPatch(typeof(Player), "Update")]
		class InventoryWeightAndSlots_PlayerPatch
		{
			static void Prefix(ref Player ___m_localPlayer)
			{
				if (___m_localPlayer == null) return;

				if (ConfigManager.showInventoryWeightAndSlots.Value)
				{
					currentWeight = ___m_localPlayer.GetInventory().GetTotalWeight();
					maxWeight = ___m_localPlayer.GetMaxCarryWeight();
					freeSlots = ___m_localPlayer.GetInventory().GetEmptySlots();
					freeSlotsPercent = ___m_localPlayer.GetInventory().SlotsUsedPercentage();
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		public static class MarsarahMod_HUD_Update_Patch
		{
			public static void Postfix(Hud __instance)
			{
				if (__instance == null) return;

				if (ConfigManager.showInventoryWeightAndSlots.Value)
				{
					CreateUI(__instance); // Create UI if missing

					// Inventory Weight section
					UIWeightText.enabled = UIController.showUI;
					inventoryAreaBackground.enabled = UIController.showUI;

					if (UIController.showUI)
					{
						float currentWeightPrecent = currentWeight * 100 / maxWeight;
						UIWeightText.color = GetColorFromPercent(currentWeightPrecent);
						UIWeightText.text = currentWeight.ToString("0.0") + "/" + maxWeight.ToString("0");
					}

					// Inventory Slots section
					UISlotText.enabled = UIController.showUI;
					inventoryAreaBackground.enabled = UIController.showUI;

					if (UIController.showUI)
					{
						UISlotText.color = GetColorFromPercent(freeSlotsPercent);
						UISlotText.text = "(" + freeSlots.ToString() + ")";
					}
				}
				else
				{
					if (UIWeightText != null)
						UIWeightText.enabled = false;
					if (UISlotText != null)
						UISlotText.enabled = false;
					if (inventoryAreaBackground != null)
						inventoryAreaBackground.enabled = false;					
				}
			}

			private static void CreateUI(Hud hud)
			{
				if (UIWeightText != null && UISlotText != null && inventoryAreaBackground != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIInventoryAreaSize = new Vector2(115f, 30f); // width, height

				// Inventory area object
				GameObject UIInventoryArea = new GameObject("InventoryArea");
				UIInventoryArea.layer = 5;
				UIInventoryArea.transform.SetParent(hud.m_healthPanel.transform);
				RectTransform inventoryAreaTransform = UIInventoryArea.AddComponent<RectTransform>();
				inventoryAreaTransform.anchorMin = new Vector2(1f, 1f);
				inventoryAreaTransform.anchorMax = new Vector2(1f, 1f);
				inventoryAreaTransform.anchoredPosition = new Vector2(-45f, -230f);
				inventoryAreaTransform.sizeDelta = UIInventoryAreaSize;
				UIInventoryArea.transform.localScale = Vector3.one;  // Ensure correct scale

				// Background texture
				Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
				inventoryAreaBackground = UIInventoryArea.AddComponent<Image>();
				inventoryAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				inventoryAreaBackground.sprite = sprite;
				inventoryAreaBackground.type = Image.Type.Sliced;
				inventoryAreaBackground.enabled = UIController.showUI;

				// Inventory Weight Text
				UIWeightText = CreateTextObject("WeightText", UIInventoryArea, Color.green, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(5f, 0f), UIInventoryAreaSize);
				// Inventory Slots Text
				UISlotText = CreateTextObject("SlotText", UIInventoryArea, Color.green, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-5f, 0f), UIInventoryAreaSize);
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

			private static Color GetColorFromPercent(float percent)
			{
				if (percent < 33f) return Color.green;
				if (percent < 66f) return Color.yellow;
				if (percent < 100f) return new Color(1f, 0.549019f, 0f);
				return Color.red;
			}
		}
	}
}
