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
		private static Text UIWeightText;
		private static Text UISlotText;
		private static Image inventoryAreaBackground;

		[HarmonyPatch(typeof(Player), "Update")]
		class InventoryWeightAndSlots_PlayerPatch
		{
			static void Prefix(ref Player ___m_localPlayer)
			{
				if (___m_localPlayer != null)
				{
					if (ConfigManager.showInventoryWeightAndSlots.Value)
					{
						currentWeight = ___m_localPlayer.GetInventory().GetTotalWeight();
						maxWeight = ___m_localPlayer.GetMaxCarryWeight();
						freeSlots = ___m_localPlayer.GetInventory().GetEmptySlots();
						freeSlotsPercent = ___m_localPlayer.GetInventory().SlotsUsedPercentage();
					}
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		class InventoryWeightAndSlots_HudAwakePatch
		{
			static void Postfix(Hud __instance)
			{
				if (__instance == null) return;

				// Show Inventory Weight & Slots section
				if (ConfigManager.showInventoryWeightAndSlots.Value)
				{
					int UITextFontSize = 16;
					string UITextFontName = "AveriaSansLibre-Bold";
					float UIInventoryArealWidth = 115f;
					float UIInventoryAreaHeight = 30f;
					GameObject UIInventoryArea;

					// Inventory area object
					UIInventoryArea = new GameObject("InventoryArea");
					UIInventoryArea.layer = 5;
					UIInventoryArea.transform.SetParent(__instance.m_healthPanel.transform);
					RectTransform inventoryAreaTransform = UIInventoryArea.AddComponent<RectTransform>();
					inventoryAreaTransform.anchorMin = new Vector2(1f, 1f);
					inventoryAreaTransform.anchorMax = new Vector2(1f, 1f);
					inventoryAreaTransform.anchoredPosition = new Vector2(-45f, -230f);
					inventoryAreaTransform.sizeDelta = new Vector2(UIInventoryArealWidth, UIInventoryAreaHeight);

					// Background texture
					Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
					inventoryAreaBackground = UIInventoryArea.AddComponent<Image>();
					inventoryAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
					inventoryAreaBackground.sprite = sprite;
					inventoryAreaBackground.type = Image.Type.Sliced;
					inventoryAreaBackground.enabled = UIController.showUI;

					// Inventory area weight text object
					GameObject weightTextObject = new GameObject("WeightText");
					weightTextObject.layer = 5;
					weightTextObject.transform.SetParent(UIInventoryArea.transform);
					RectTransform weightAreaTextTransform = weightTextObject.AddComponent<RectTransform>();
					weightAreaTextTransform.anchoredPosition = new Vector2(5f, 0f);
					weightAreaTextTransform.sizeDelta = new Vector2(UIInventoryArealWidth, UIInventoryAreaHeight);

					// Text in weight text object
					UIWeightText = weightTextObject.AddComponent<Text>();
					UIWeightText.color = Color.green;
					UIWeightText.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault((Font tempFont) => ((UnityEngine.Object)(object)tempFont).name == UITextFontName);
					UIWeightText.fontSize = UITextFontSize;
					UIWeightText.alignment = TextAnchor.MiddleLeft;

					// Weight text outline
					Outline weightOutline = weightTextObject.AddComponent<Outline>();
					weightOutline.effectColor = Color.black;
					weightOutline.effectDistance = new Vector2(1f, -1f);
					weightOutline.useGraphicAlpha = true;
					weightOutline.useGUILayout = true;

					// Inventory area slots text object
					GameObject slotTextObject = new GameObject("SlotText");
					slotTextObject.layer = 5;
					slotTextObject.transform.SetParent(UIInventoryArea.transform);
					RectTransform slotAreaTextTransform = slotTextObject.AddComponent<RectTransform>();
					slotAreaTextTransform.anchoredPosition = new Vector2(-5f, 0f);
					slotAreaTextTransform.sizeDelta = new Vector2(UIInventoryArealWidth, UIInventoryAreaHeight);

					// Text in text object
					UISlotText = slotTextObject.AddComponent<Text>();
					UISlotText.color = Color.green;
					UISlotText.font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault((Font tempFont) => ((UnityEngine.Object)(object)tempFont).name == UITextFontName);
					UISlotText.fontSize = UITextFontSize;
					UISlotText.alignment = TextAnchor.MiddleRight;

					// Text outline
					Outline slotOutline = slotTextObject.AddComponent<Outline>();
					slotOutline.effectColor = Color.black;
					slotOutline.effectDistance = new Vector2(1f, -1f);
					slotOutline.useGraphicAlpha = true;
					slotOutline.useGUILayout = true;
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
					// Inventory Weight section
					UIWeightText.enabled = UIController.showUI;
					inventoryAreaBackground.enabled = UIController.showUI;

					if (UIController.showUI)
					{
						float currentWeightPrecent = currentWeight * 100 / maxWeight;
						if (currentWeightPrecent < 33f)
							UIWeightText.color = Color.green;
						else if (currentWeightPrecent < 66f)
							UIWeightText.color = Color.yellow;
						else if (currentWeightPrecent < 100f)
							UIWeightText.color = new Color(1f, 0.549019f, 0f);
						else
							UIWeightText.color = Color.red;
						UIWeightText.text = currentWeight.ToString("0.0") + "/" + maxWeight.ToString("0");
					}

					// Inventory Slots section
					UISlotText.enabled = UIController.showUI;
					inventoryAreaBackground.enabled = UIController.showUI;

					if (UIController.showUI)
					{
						if (freeSlotsPercent < 33f)
							UISlotText.color = Color.green;
						else if (freeSlotsPercent < 66f)
							UISlotText.color = Color.yellow;
						else if (freeSlotsPercent < 100f)
							UISlotText.color = new Color(1f, 0.549019f, 0f);
						else
							UISlotText.color = Color.red;
						UISlotText.text = "(" + freeSlots.ToString() + ")";
					}
				}
				else
				{
					UIWeightText.enabled = false;
					inventoryAreaBackground.enabled = false;
					UISlotText.enabled = false;
					inventoryAreaBackground.enabled = false;
				}
			}
		}
	}
}
