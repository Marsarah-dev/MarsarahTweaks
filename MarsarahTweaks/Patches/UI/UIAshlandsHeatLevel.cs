using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIAshlandsHeatLevel : UIController
	{
		private static readonly LogManager log = new LogManager("UI Ashlands Heat", LogManager.LogLevel.Warning);

		// UI data
		private static Image heatBarFill = null;
		private static Image heatBarBGImage = null;
		private static GameObject UIHeatBarArea = null;
		private static Text heatBarText = null;
		private static TMPro.TextMeshProUGUI heatBarEmojiTMP = null;
		private static float heatThreshold;
		private static float currentHeat;

		[HarmonyPatch(typeof(Character), "UpdateLava")]
		public class GetHeatLevel_Patch
		{
			static void Postfix(Character __instance, ref float ___m_lavaHeatLevel)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (!ConfigManager.ShowHeatLevelInAshlands.Value || !showUI) return;
				if (!(__instance is Player)) return;

				heatThreshold = __instance.m_heatLevelFirstDamageThreshold;
				currentHeat = ___m_lavaHeatLevel;
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
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		static class HeatLevelHUDUpdate_Patch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;

				// TODO: use showUI

				if (ConfigManager.ShowHeatLevelInAshlands.Value)
				{
					CreateUI(__instance); // Create UI if missing

					if (Player.m_localPlayer && heatBarFill != null && heatThreshold > 0f)
					{
						// Hide UI if not in Ashlands
						if (Player.m_localPlayer.GetCurrentBiome() != Heightmap.Biome.AshLands)
						{
							if (UIHeatBarArea != null && UIHeatBarArea.activeSelf)
								UIHeatBarArea.SetActive(false);
							return;
						}

						// Handle loadscreena and ui hidden cases
						bool shouldBeVisible = !IsUIHidden() && !IsLoadScreenActive(__instance);
						if (UIHeatBarArea.activeSelf != shouldBeVisible)
						{
							UIHeatBarArea.SetActive(shouldBeVisible);
						}

						heatBarFill.fillAmount = Mathf.Clamp01(currentHeat / heatThreshold);

						if (heatBarText != null)
						{
							float pct = Mathf.Clamp01(currentHeat / heatThreshold) * 100f;
							heatBarText.text = $"{pct:0}%";
						}

						// Gradual color change
						float heatPercent = heatBarFill.fillAmount;
						Color startColor = Color.yellow;
						Color midColor = new Color(1f, 0.549019f, 0f); // orange
						Color endColor = Color.red;

						// Blend from yellow to orange to red
						Color barColor = heatPercent < 0.5f
							? Color.Lerp(startColor, midColor, heatPercent * 2f)
							: Color.Lerp(midColor, endColor, (heatPercent - 0.5f) * 2f);

						// Soft pulse alpha if heat is high
						if (heatPercent > 0.8f)
						{
							float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 4f); // Range [0.2–1.0]
							barColor.a = pulse; // fade alpha
						}
						else
						{
							barColor.a = 0.8f; // default alpha
						}

						heatBarFill.color = barColor;

						if (heatBarFill.fillAmount == 0)
						{							
							if (heatBarFill != null) heatBarFill.enabled = false;
							if (heatBarBGImage != null) heatBarBGImage.enabled = false;
							if (heatBarText != null) heatBarText.enabled = false;
							if (heatBarEmojiTMP != null)
							{
								heatBarEmojiTMP.enabled = false;
								heatBarEmojiTMP.text = "";

							}
						}
						else
						{
							if (heatBarFill != null) heatBarFill.enabled = showUI;
							if (heatBarBGImage != null) heatBarBGImage.enabled = showUI;
							if (heatBarText != null) heatBarText.enabled = showUI;
							if (heatBarEmojiTMP != null)
							{
								heatBarEmojiTMP.enabled = showUI;
								heatBarEmojiTMP.text = "🔥";

							}
						}
					}
				}
				else
				{
					if (heatBarFill != null)
						heatBarFill.enabled = false;
					if (heatBarBGImage != null)
						heatBarBGImage.enabled = false;
					if (heatBarText != null)
						heatBarText.enabled = false;
					if (heatBarEmojiTMP != null)
						heatBarEmojiTMP.enabled = false;
				}
			}

			private static Sprite GenerateWhiteSprite()
			{
				Texture2D tex = new Texture2D(1, 1);
				tex.SetPixel(0, 0, Color.white);
				tex.Apply();

				return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
			}

			private static bool IsLoadScreenActive(Hud hud)
			{
				return Hud.instance && hud.m_loadingScreen && hud.m_loadingScreen.gameObject.activeSelf;
			}

			private static bool IsUIHidden()
			{
				return Hud.IsUserHidden();
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UIHeatBarArea != null)
				return;  // UI already exists, no need to create again

			//Vector2 UIHeatAreaSize = new Vector2(30f, 150f); // width, height
			Vector2 UIHeatAreaSize = new Vector2(200f, 25f); // width, height
			int UITextFontSize = 13;
			int UIEmojiFontSize = 10;
			string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF"; // NotoEmoji-Regular

			// Heat Area Object
			UIHeatBarArea = new GameObject("HeatBar");
			UIHeatBarArea.layer = 5;
			//UIHeatBarArea.transform.SetParent(hud.m_healthPanel.transform); // health
			UIHeatBarArea.transform.SetParent(hud.m_rootObject.transform.parent, false); // main hud

			RectTransform heatAreaTransform = UIHeatBarArea.AddComponent<RectTransform>();
			//heatAreaTransform.anchorMin = new Vector2(1f, 1f);
			heatAreaTransform.anchorMin = new Vector2(0.5f, 0.5f); // center
																   //heatAreaTransform.anchorMax = new Vector2(1f, 1f);
			heatAreaTransform.anchorMax = new Vector2(0.5f, 0.5f); // center
			heatAreaTransform.pivot = new Vector2(0.5f, 0.5f); // center
															   //heatAreaTransform.anchoredPosition = new Vector2(-66f, 55f); // above the food slots to the left of hp bar
															   //heatAreaTransform.anchoredPosition = new Vector2(0f, -420f); // under the stamina bar (using main root) 
			heatAreaTransform.anchoredPosition = new Vector2(0f, 350f); // middle-up (using main root) 
			heatAreaTransform.sizeDelta = UIHeatAreaSize;
			UIHeatBarArea.transform.localScale = Vector3.one; // Ensure correct scale

			// Background texture
			GameObject heatBackgroundArea = new GameObject("HeatBarBackground");
			heatBackgroundArea.transform.SetParent(UIHeatBarArea.transform, false);
			heatBarBGImage = heatBackgroundArea.AddComponent<Image>();
			RectTransform bgRect = heatBackgroundArea.GetComponent<RectTransform>();
			bgRect.anchorMin = Vector2.zero;
			bgRect.anchorMax = Vector2.one;
			bgRect.offsetMin = Vector2.zero;
			bgRect.offsetMax = Vector2.zero;
			heatBarBGImage.color = new Color(0f, 0f, 0f, 0.4f); // semi-transparent dark
			heatBarBGImage.enabled = false;

			// Foreground Fill
			GameObject fillArea = new GameObject("HeatBarFill");
			fillArea.transform.SetParent(UIHeatBarArea.transform, false);
			heatBarFill = fillArea.AddComponent<Image>();
			RectTransform fillRect = fillArea.GetComponent<RectTransform>();
			fillRect.anchorMin = Vector2.zero;
			fillRect.anchorMax = Vector2.one;
			fillRect.offsetMin = new Vector2(3f, 3f);   // left, bottom
			fillRect.offsetMax = new Vector2(-3f, -3f); // right, top

			Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "bar_monster_hp_5");
			heatBarFill.sprite = sprite;
			//heatBarFill.sprite = GenerateWhiteSprite();
			heatBarFill.type = Image.Type.Filled;
			//heatBarFill.fillMethod = Image.FillMethod.Vertical;
			heatBarFill.fillMethod = Image.FillMethod.Horizontal;
			//heatBarFill.fillOrigin = (int)Image.OriginVertical.Bottom;
			heatBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
			heatBarFill.fillAmount = 0f; // Initially empty
			heatBarFill.enabled = false;

			// Text overlay
			heatBarText = CreateTextObject("HeatText", UIHeatBarArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleCenter, new Vector2(0f, 0f), UIHeatAreaSize);

			// Flame Icon
			//heatBarEmojiTMP = CreateTMPTextObject("HeatEmojiTMP", UIHeatBarArea, Color.red, UIEmojiFontName, UITextFontSize, TextAlignmentOptions.Top, new Vector2(0f, -3f), UIHeatAreaSize);
			heatBarEmojiTMP = CreateTMPTextObject("HeatEmojiTMP", UIHeatBarArea, Color.red, UIEmojiFontName, UIEmojiFontSize, TextAlignmentOptions.MidlineRight, new Vector2(-2f, 0f), UIHeatAreaSize);
		}
	}
}
