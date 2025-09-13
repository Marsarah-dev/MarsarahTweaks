using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahTweaks.Features.UI
{
	internal class UIEnemyNameplates
	{
		private static readonly LogManager log = new LogManager("UI Enemy Nameplates", LogManager.LogLevel.Info);

		private const float BarYOffset = 0f;
		private const float BarWidth = 100f;
		private const float BarHeight = 12f;
		private const string CustomBarName = "MarsarahCustomHealthBar";
		private const string CustomSFillName = CustomBarName + "_SFill";
		private const string CustomBGName = CustomBarName + "_BG";

		// Track created bars per HudData (not Character)
		private static readonly Dictionary<object, (Image fill, Image sFill, Image background, RectTransform fillRect)> _customBars
			= new Dictionary<object, (Image, Image, Image, RectTransform)>();

		private static readonly Color FillColor = Color.red;
		private static readonly Color StaticFillColor = new Color(0.3f, 0.3f, 0.3f, 1f);
		private static readonly Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

		[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
		public static class EnemyHud_ShowHud_CustomBar_Patch
		{
			private static void Postfix(EnemyHud __instance, Character c)
			{
				if (c == null) return;

				var hudsField = typeof(EnemyHud).GetField("m_huds", BindingFlags.NonPublic | BindingFlags.Instance);
				if (!(hudsField?.GetValue(__instance) is System.Collections.IDictionary huds)) return;
				if (!huds.Contains(c)) return;

				var hudData = huds[c];
				if (hudData == null) return;

				var hudDataType = typeof(EnemyHud).GetNestedType("HudData", BindingFlags.NonPublic);
				if (hudDataType == null) return;

				var guiField = hudDataType.GetField("m_gui", BindingFlags.Public | BindingFlags.Instance);
				var hudGO = guiField?.GetValue(hudData) as GameObject;
				if (hudGO == null) return;

				// Disable vanilla container
				var healthTransform = hudGO.transform.Find("Health");
				if (healthTransform != null)
					healthTransform.gameObject.SetActive(false);

				// Only create if missing or destroyed
				if (_customBars.TryGetValue(hudData, out var existing))
				{
					if (existing.fill == null || existing.fill.gameObject == null)
					{
						_customBars.Remove(hudData);
					}
					else
					{
						return;
					}
				}

				float horizontalPadding = 6f;
				float verticalPadding = 6f;

				// --- Background ---
				var bgGO = new GameObject(CustomBGName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				bgGO.transform.SetParent(hudGO.transform, false);

				var bgRect = bgGO.GetComponent<RectTransform>();
				bgRect.anchoredPosition = new Vector2(0f, BarYOffset);
				bgRect.sizeDelta = new Vector2(BarWidth + horizontalPadding, BarHeight + verticalPadding);

				var bgImage = bgGO.GetComponent<Image>();
				bgImage.color = BackgroundColor;
				bgImage.type = Image.Type.Sliced;

				// --- Static Fill ---
				var sFillGO = new GameObject(CustomSFillName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				sFillGO.transform.SetParent(bgGO.transform, false);
				sFillGO.transform.SetAsFirstSibling();

				var sFillRect = sFillGO.GetComponent<RectTransform>();
				sFillRect.anchorMin = new Vector2(0f, 0.5f);
				sFillRect.anchorMax = new Vector2(0f, 0.5f);
				sFillRect.pivot = new Vector2(0f, 0.5f);
				sFillRect.anchoredPosition = new Vector2(horizontalPadding / 2f, 0f);
				sFillRect.sizeDelta = new Vector2(BarWidth, BarHeight);

				var sFillImage = sFillGO.GetComponent<Image>();
				sFillImage.color = StaticFillColor;
				sFillImage.type = Image.Type.Sliced;

				// --- Main Fill ---
				var fillGO = new GameObject(CustomBarName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				fillGO.transform.SetParent(bgGO.transform, false);

				var fillRect = fillGO.GetComponent<RectTransform>();
				fillRect.anchorMin = new Vector2(0f, 0.5f);
				fillRect.anchorMax = new Vector2(0f, 0.5f);
				fillRect.pivot = new Vector2(0f, 0.5f);
				fillRect.anchoredPosition = new Vector2(horizontalPadding / 2f, 0f);
				fillRect.sizeDelta = new Vector2(BarWidth, BarHeight);

				var fillImage = fillGO.GetComponent<Image>();
				Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "bar_monster_hp_5");
				if (sprite != null) fillImage.sprite = sprite;
				fillImage.fillMethod = Image.FillMethod.Horizontal;
				fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
				fillImage.color = FillColor;
				fillImage.type = Image.Type.Filled;

				// Hook into vanilla fast/slow
				var healthFast = healthTransform?.Find("fast")?.GetComponent<Image>();
				var healthSlow = healthTransform?.Find("slow")?.GetComponent<Image>();

				// Store all references for UpdateHuds
				_customBars[hudData] = (fillImage, sFillImage, bgImage, fillRect);

				log.Info($"Created custom HP bar for {c.name}");
			}
		}


		[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
		public static class EnemyHud_UpdateHuds_CustomBar_Patch
		{
			private static void Postfix(EnemyHud __instance)
			{
				foreach (var kvp in _customBars)
				{
					var hudData = kvp.Key;
					var (fill, sFill, bg, fillRect) = kvp.Value;

					if (hudData == null || fill == null || !fill || sFill == null || !sFill || bg == null || !bg)
						continue;

					fill.enabled = true;
					sFill.enabled = true;
					bg.enabled = true;

					// Get Character from HudData
					var hudDataType = hudData.GetType();
					var charField = hudDataType.GetField("m_character", BindingFlags.Public | BindingFlags.Instance);
					var c = charField?.GetValue(hudData) as Character;
					if (c == null) continue;

					float currentHealth = c.GetHealth();
					float maxHealth = c.GetMaxHealth();
					float fillPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;

					// Just update fillAmount instead of resizing
					fill.fillAmount = fillPercent;

					// Optional: static fill can follow too (e.g. background dimming)
					sFill.fillAmount = 1f; // keep full, or set = fillPercent if you want it to shrink
				}
			}
		}











		// Logs

		/*[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
		public static class EnemyHud_ShowHud_Log_Patch
		{
			private static void Postfix(EnemyHud __instance, Character c)
			{
				// Access private m_huds field
				var hudsField = typeof(EnemyHud).GetField("m_huds", BindingFlags.NonPublic | BindingFlags.Instance);
				if (hudsField == null) { log.Warn("Failed to get m_huds field"); return; }

				if (!(hudsField.GetValue(__instance) is System.Collections.IDictionary huds))
				{ log.Warn("m_huds is null or not IDictionary"); return; }

				if (!huds.Contains(c))
				{
					log.Warn($"m_huds does not contain character {c?.name ?? "null"}");
					return;
				}

				var hudData = huds[c];
				if (hudData == null)
				{
					log.Warn($"hudData for {c?.name ?? "null"} is null");
					return;
				}

				var hudDataType = typeof(EnemyHud).GetNestedType("HudData", BindingFlags.NonPublic);
				if (hudDataType == null) { log.Warn("Failed to get HudData type"); return; }

				var characterField = hudDataType.GetField("m_character", BindingFlags.Public | BindingFlags.Instance);
				var hudCharacter = characterField?.GetValue(hudData) as Character;

				var guiField = hudDataType.GetField("m_gui", BindingFlags.Public | BindingFlags.Instance);
				var hudGO = guiField?.GetValue(hudData) as GameObject;
				if (hudGO == null) { log.Warn($"hudGO for {c?.name ?? "null"} is null"); return; }

				log.Info($"[ShowHud] character: {c?.name ?? "null"}");

				var nameField = hudDataType.GetField("m_name", BindingFlags.Public | BindingFlags.Instance);
				var nameText = nameField?.GetValue(hudData) as TextMeshProUGUI;
				if (nameText != null)
					log.Info($"  Name: text='{nameText.text}', pos={nameText.rectTransform.anchoredPosition}, size={nameText.rectTransform.sizeDelta}");

				void LogBar(string fieldName)
				{
					var m_barField = typeof(GuiBar).GetField("m_bar", BindingFlags.Public | BindingFlags.Instance);
					var bar = hudDataType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(hudData) as GuiBar;
					if (bar == null) { log.Warn($"Bar {fieldName} is null"); return; }

					float barWidth = 0f;
					var m_widthField = typeof(GuiBar).GetField("m_width", BindingFlags.NonPublic | BindingFlags.Instance);
					if (m_widthField != null) barWidth = (float)m_widthField.GetValue(bar);

					float currentValue = 0f;
					var m_valueField = typeof(GuiBar).GetField("m_value", BindingFlags.NonPublic | BindingFlags.Instance);
					if (m_valueField != null) currentValue = (float)m_valueField.GetValue(bar);

					float maxValue = 0f;
					var m_maxField = typeof(GuiBar).GetField("m_maxValue", BindingFlags.NonPublic | BindingFlags.Instance);
					if (m_maxField != null) maxValue = (float)m_maxField.GetValue(bar);

					var barRT = m_barField.GetValue(bar) as RectTransform;

					log.Info($"    {fieldName}: m_width={barWidth}, sizeDelta={barRT?.sizeDelta ?? Vector2.zero}, localScale={barRT?.localScale ?? Vector3.zero}, active={bar?.gameObject.activeSelf}, currentValue={currentValue}, maxValue={maxValue}, fill={(maxValue > 0f ? currentValue / maxValue : 0f):P0}");
				}

				LogBar("m_healthFast");
				LogBar("m_healthSlow");
				LogBar("m_healthFastFriendly");

				if (hudCharacter != null)
					log.Info($"  CurrentHealth={hudCharacter.GetHealth():0}/{hudCharacter.GetMaxHealth():0}");
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
		public static class EnemyHud_UpdateHuds_Log_Patch
		{
			private static void Postfix(EnemyHud __instance)
			{
				// Just log frame info and number of HUDs
				var hudsField = typeof(EnemyHud).GetField("m_huds", BindingFlags.NonPublic | BindingFlags.Instance);
				if (!(hudsField?.GetValue(__instance) is System.Collections.IDictionary huds)) return;

				log.Info($"[UpdateHuds] frame={Time.frameCount}, hudCount={huds.Count}");
			}
		}*/
	}
}
