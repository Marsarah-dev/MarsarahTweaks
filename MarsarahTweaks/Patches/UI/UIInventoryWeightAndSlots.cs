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
	internal class UIInventoryWeightAndSlots : UIController
	{
		private static readonly LogManager log = new LogManager("UI Inventory", LogManager.LogLevel.Warning);

		// UI data
		private static float currentWeight;
		private static float maxWeight;
		private static float freeSlots;
		private static float freeSlotsPercent;

		// UI elements
		internal static GameObject UIInventoryArea = null;
		private static Text UIWeightText = null;
		private static Text UISlotText = null;

		internal static GameObject UIWeightBarArea = null;
		private static GameObject UIWeightBarEmojiArea = null;
		private static Image weightBarFill = null;
		private static Text UIWeightBarText = null;
		private static TMPro.TextMeshProUGUI UIWeightBarEmojiTMP = null;

		internal static GameObject UISlotsArea = null;
		private static Text UISlotsText2 = null;
		private static TMPro.TextMeshProUGUI UISlotsEmojiTMP = null;

		[HarmonyPatch(typeof(Player), "Update")]
		class InventoryWeightAndSlots_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (___m_localPlayer == null) return;

				if (ConfigManager.ShowInventoryWeightAndSlots.Value)
				{
					currentWeight = ___m_localPlayer.GetInventory().GetTotalWeight();
					maxWeight = ___m_localPlayer.GetMaxCarryWeight();
					freeSlots = ___m_localPlayer.GetInventory().GetEmptySlots();
					freeSlotsPercent = ___m_localPlayer.GetInventory().SlotsUsedPercentage();
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		class InventoryWeightAndSlots_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowInventoryWeightAndSlots.Value)
				{
					CreateTextBasedUI(__instance); // Create the text version of the UI if missing
					CreateSymbolBasedUI(__instance); // Create the symbol/bar version of the UI if missing

					if (!ConfigManager.UseSymbolsForUI.Value)
					{
						UIInventoryArea?.SetActive(showUI);
						UIWeightBarArea?.SetActive(false);
						UIWeightBarEmojiArea?.SetActive(false);
						UISlotsArea?.SetActive(false);

						if (showUI)
						{
							float currentWeightPrecent = currentWeight * 100 / maxWeight;
							UIWeightText.color = GetColorFromPercent(currentWeightPrecent);
							UIWeightText.text = currentWeight.ToString("0.0") + "/" + maxWeight.ToString("0");
						}

						if (showUI)
						{
							UISlotText.color = GetColorFromPercent(freeSlotsPercent);
							UISlotText.text = "(" + freeSlots.ToString() + ")";
						}
					}
					else
					{
						UIInventoryArea?.SetActive(false);
						UIWeightBarArea?.SetActive(showUI);
						UIWeightBarEmojiArea?.SetActive(showUI);
						UISlotsArea?.SetActive(showUI);

						if (showUI)
						{
							// Weight section

							weightBarFill.fillAmount = Mathf.Clamp01(currentWeight / maxWeight);

							// Gradual color change
							float weightPercent = weightBarFill.fillAmount;
							
							Color barColor = GetColorBlendFromPercent(weightPercent);

							weightBarFill.color = barColor;

							if (UIWeightBarText != null)
							{
								UIWeightBarText.text = currentWeight.ToString("0.0") + "/" + maxWeight.ToString("0");
								//UIWeightBarText.color = barColor;
							}

							if (UIWeightBarEmojiTMP != null)
							{
								UIWeightBarEmojiTMP.text = "🏋️"; // 🏋️ 🎒 👜
								UIWeightBarEmojiTMP.color = barColor;
							}

							// Free slots section

							Color slotsColor = GetColorFromPercent(freeSlotsPercent);

							if (UISlotsText2 != null)
							{
								UISlotsText2.color = slotsColor;
								UISlotsText2.text = freeSlots.ToString();
							}

							if (UISlotsEmojiTMP != null)
							{
								UISlotsEmojiTMP.color = slotsColor;
								UISlotsEmojiTMP.text = "🎒";
							}
						}
					}
				}
				else
				{
					// We're calling create here to make sure objects are safe to access
					CreateTextBasedUI(__instance); // Create the text version of the UI if missing
					CreateSymbolBasedUI(__instance); // Create the symbol/bar version of the UI if missing

					UIInventoryArea?.SetActive(false);
					UIWeightBarArea?.SetActive(false);
					UIWeightBarEmojiArea?.SetActive(false);
					UISlotsArea?.SetActive(false);
				}
			}

			private static void CreateTextBasedUI(Hud hud)
			{
				if (UIInventoryArea != null && UIWeightText != null && UISlotText != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIInventoryAreaSize = new Vector2(115f, 30f); // width, height

				// Inventory area object
				UIInventoryArea = new GameObject("InventoryArea");
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
				Image inventoryAreaBackground = UIInventoryArea.AddComponent<Image>();
				inventoryAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				inventoryAreaBackground.sprite = sprite;
				inventoryAreaBackground.type = Image.Type.Sliced;

				// Inventory Weight Text
				UIWeightText = CreateTextObject("WeightText", UIInventoryArea, Color.green, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(5f, 0f), UIInventoryAreaSize);

				// Inventory Slots Text
				UISlotText = CreateTextObject("SlotText", UIInventoryArea, Color.green, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-5f, 0f), UIInventoryAreaSize);
			}

			private static void CreateSymbolBasedUI(Hud hud)
			{
				if (UIWeightBarArea != null) return;

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIWeightAreaSize = new Vector2(100f, 30f); // width, height
				Vector2 UIWeightAreaEmojiSize = new Vector2(30f, 30f); // width, height
				float xOffset = -10f; // -40f
				float yOffset = -230f;

				// ======= Weight ========

				// Inventory Area Object
				UIWeightBarArea = new GameObject("WeightAreaBar");
				UIWeightBarArea.layer = 5;
				UIWeightBarArea.transform.SetParent(hud.m_healthPanel.transform); // health

				RectTransform weightAreaTransform = UIWeightBarArea.AddComponent<RectTransform>();
				weightAreaTransform.anchorMin = new Vector2(1f, 1f);
				weightAreaTransform.anchorMax = new Vector2(1f, 1f);
				weightAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset); // -40f
				weightAreaTransform.sizeDelta = UIWeightAreaSize;
				UIWeightBarArea.transform.localScale = Vector3.one; // Ensure correct scale

				// Background texture
				GameObject weightBackgroundArea = new GameObject("WeightBarBackground");
				weightBackgroundArea.transform.SetParent(UIWeightBarArea.transform, false);
				Image weightBarBGImage = weightBackgroundArea.AddComponent<Image>();
				RectTransform bgRect = weightBackgroundArea.GetComponent<RectTransform>();
				bgRect.anchorMin = Vector2.zero;
				bgRect.anchorMax = Vector2.one;
				bgRect.offsetMin = Vector2.zero;
				bgRect.offsetMax = Vector2.zero;
				weightBarBGImage.color = new Color(0f, 0f, 0f, 0.4f); // semi-transparent dark

				// Foreground Fill
				GameObject fillArea = new GameObject("WeightBarFill");
				fillArea.transform.SetParent(UIWeightBarArea.transform, false);
				weightBarFill = fillArea.AddComponent<Image>();
				RectTransform fillRect = fillArea.GetComponent<RectTransform>();
				fillRect.anchorMin = Vector2.zero;
				fillRect.anchorMax = Vector2.one;
				fillRect.offsetMin = new Vector2(3f, 3f);   // left, bottom
				fillRect.offsetMax = new Vector2(-3f, -3f); // right, top

				Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "bar_monster_hp_5");
				weightBarFill.sprite = sprite;
				weightBarFill.type = Image.Type.Filled;
				weightBarFill.fillMethod = Image.FillMethod.Horizontal;
				weightBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
				weightBarFill.fillAmount = 0f; // Initially empty

				// Weight bar emoji object
				UIWeightBarEmojiArea = new GameObject("WeightAreaEmoji");
				UIWeightBarEmojiArea.layer = 5;
				UIWeightBarEmojiArea.transform.SetParent(hud.m_healthPanel.transform); // health

				xOffset -= 65f;
				RectTransform weightAreaEmojiTransform = UIWeightBarEmojiArea.AddComponent<RectTransform>();
				weightAreaEmojiTransform.anchorMin = new Vector2(1f, 1f);
				weightAreaEmojiTransform.anchorMax = new Vector2(1f, 1f);
				weightAreaEmojiTransform.anchoredPosition = new Vector2(xOffset, yOffset); // -105f
				weightAreaEmojiTransform.sizeDelta = UIWeightAreaEmojiSize;
				UIWeightBarEmojiArea.transform.localScale = Vector3.one; // Ensure correct scale

				// Background texture for emoji area
				GameObject weightEmojiBackgroundArea = new GameObject("WeightEmojiBackground");
				weightEmojiBackgroundArea.transform.SetParent(UIWeightBarEmojiArea.transform, false);
				Image weightEmojiBGImage = weightEmojiBackgroundArea.AddComponent<Image>();
				RectTransform bgRectEmo = weightEmojiBackgroundArea.GetComponent<RectTransform>();
				bgRectEmo.anchorMin = Vector2.zero;
				bgRectEmo.anchorMax = Vector2.one;
				bgRectEmo.offsetMin = Vector2.zero;
				bgRectEmo.offsetMax = Vector2.zero;
				weightEmojiBGImage.color = new Color(0f, 0f, 0f, 0.4f);

				// Text overlay
				UIWeightBarText = CreateTextObject("WeightText", UIWeightBarArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(0f, 0f), UIWeightAreaSize);

				// Bag icon
				UIWeightBarEmojiTMP = CreateTMPTextObject("WeightEmojiTMP", UIWeightBarEmojiArea, Color.green, UITextFontName, UITextFontSize + 4, TextAlignmentOptions.Midline, new Vector2(0f, 0f), UIWeightAreaSize);


				// ======= Inventory Slots ========

				Vector2 UISlotsAreaSize = new Vector2(50f, 30f); // width, height
				xOffset = 68f; // 38

				// Inventory Area Object
				UISlotsArea = new GameObject("SlotsArea");
				UISlotsArea.layer = 5;
				UISlotsArea.transform.SetParent(hud.m_healthPanel.transform); // health

				RectTransform slotsAreaTransform = UISlotsArea.AddComponent<RectTransform>();
				slotsAreaTransform.anchorMin = new Vector2(1f, 1f);
				slotsAreaTransform.anchorMax = new Vector2(1f, 1f);
				slotsAreaTransform.anchoredPosition = new Vector2(xOffset, yOffset);
				slotsAreaTransform.sizeDelta = UISlotsAreaSize;
				UISlotsArea.transform.localScale = Vector3.one; 

				Sprite spriteSlotsBackground = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite tempSprite) => tempSprite.name == "InputFieldBackground");
				Image slotsAreaBackground = UISlotsArea.AddComponent<Image>();
				slotsAreaBackground.color = new Color(0f, 0f, 0f, 0.4f);
				slotsAreaBackground.sprite = spriteSlotsBackground;
				slotsAreaBackground.type = Image.Type.Sliced;

				// Text overlay
				UISlotsText2 = CreateTextObject("slotsText", UISlotsArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(-4f, 0f), UISlotsAreaSize);

				// Bag icon
				UISlotsEmojiTMP = CreateTMPTextObject("SlotsEmojiTMP", UISlotsArea, Color.green, UITextFontName, UITextFontSize + 4, TextAlignmentOptions.MidlineLeft, new Vector2(4f, 0f), UISlotsAreaSize);
			}

			private static Color GetColorFromPercent(float percent)
			{
				if (percent < 33f) return Color.green;
				if (percent < 66f) return Color.yellow;
				if (percent < 100f) return new Color(1f, 0.549019f, 0f);
				return Color.red;
			}

			private static Color GetColorBlendFromPercent(float percent)
			{
				Color colorAt0 = Color.green;
				Color colorAt33 = Color.green;
				Color colorAt66 = Color.yellow;
				Color colorAt100 = new Color(1f, 0.549019f, 0f); // orange
				Color colorPast100 = Color.red;

				Color color;

				// Gradual color blend
				if (percent <= 0.33f)
				{
					// 0–33% → green to green
					color = Color.Lerp(colorAt0, colorAt33, percent / 0.33f);
				}
				else if (percent <= 0.66f)
				{
					// 33–66% → green to yellow
					color = Color.Lerp(colorAt33, colorAt66, (percent - 0.33f) / 0.33f);
				}
				else if (percent < 1.0f)
				{
					// 66–100% → yellow to orange
					color = Color.Lerp(colorAt66, colorAt100, (percent - 0.66f) / 0.34f);
				}
				else
				{
					// >=100% → fully red
					color = colorPast100;
				}

				return color;
			}
		}
	}
}
