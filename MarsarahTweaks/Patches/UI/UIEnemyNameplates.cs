using HarmonyLib;
using MarsarahTweaks.Managers;
using MarsarahTweaks.Patches.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace MarsarahTweaks.Features.UI
{
	internal class UIEnemyNameplates : UIController
	{
		private static readonly LogManager log = new LogManager("UI Enemy Nameplates", LogManager.LogLevel.Warning);

		// New sizes
		private const float BarHeight = 12f;
		private const float BarHeightBoss = 18f;

		// Backups
		private static float DefaultDistance = -1f;
		private static float DefaultBarHeight = -1f;
		private static float DefaultBarHeightBoss = -1f;
		private static readonly Dictionary<object, float> _lastBarHeight = new Dictionary<object, float>();

		// Reflection cache
		private static readonly FieldInfo m_hudsField;
		private static readonly Type hudDataType;
		private static readonly FieldInfo hud_m_gui_Field;
		private static readonly FieldInfo hud_m_character_Field;
		private static readonly FieldInfo hud_m_healthFast_Field;
		private static readonly FieldInfo hud_m_healthSlow_Field;
		private static readonly FieldInfo hud_m_name_Field;
		private static readonly FieldInfo hud_m_alerted_Field;
		private static readonly FieldInfo hud_m_aware_Field;

		// GuiBar internals
		private static readonly FieldInfo guiBar_m_width_Field;

		// Text 
		private class HpTexts
		{
			public TextMeshProUGUI HP;
			public TextMeshProUGUI HpPercent;
		}

		private static readonly ConditionalWeakTable<object, HpTexts> _hpTextCache = new ConditionalWeakTable<object, HpTexts>();

		static UIEnemyNameplates()
		{
			m_hudsField = typeof(EnemyHud).GetField("m_huds", BindingFlags.NonPublic | BindingFlags.Instance);
			hudDataType = typeof(EnemyHud).GetNestedType("HudData", BindingFlags.NonPublic | BindingFlags.Instance);

			if (m_hudsField == null || hudDataType == null)
			{
				log.Error("Failed to locate EnemyHud.m_huds or nested type HudData via reflection.");
				return;
			}

			hud_m_gui_Field = hudDataType.GetField("m_gui", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_character_Field = hudDataType.GetField("m_character", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_healthFast_Field = hudDataType.GetField("m_healthFast", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_healthSlow_Field = hudDataType.GetField("m_healthSlow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_name_Field = hudDataType.GetField("m_name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			hud_m_alerted_Field = hudDataType.GetField("m_alerted", BindingFlags.Public | BindingFlags.Instance);
			hud_m_aware_Field = hudDataType.GetField("m_aware", BindingFlags.Public | BindingFlags.Instance);

			guiBar_m_width_Field = typeof(GuiBar).GetField("m_width", BindingFlags.NonPublic | BindingFlags.Instance);
			if (guiBar_m_width_Field == null)
			{
				log.Warn("Could not resolve GuiBar.m_width via reflection. Resizing may fallback to default width.");
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "Awake")]
		public static class EmenyHud_Awake_Patch
		{
			private static void Postfix(ref EnemyHud __instance)
			{
				if (__instance == null) return;
				if (CompatibilityManager.BetterUI.Loaded) return;

				if (ConfigManager.BetterEnemyNameplates.Value)
				{
					float distanceMultiplier = 2f;
					DefaultDistance = __instance.m_maxShowDistance;
					__instance.m_maxShowDistance *= distanceMultiplier;
				}
				else if (DefaultDistance != -1f)
				{
					__instance.m_maxShowDistance = DefaultDistance;
				}
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
		public static class EnemyHud_ShowHud_CustomBar_Patch
		{
			private static void Postfix(EnemyHud __instance, Character c)
			{
				if (c == null || m_hudsField == null) return;
				if (CompatibilityManager.BetterUI.Loaded) return;

				var huds = m_hudsField.GetValue(__instance) as IDictionary;
				if (huds == null || !huds.Contains(c)) return;

				var hudData = huds[c];
				if (hudData == null) return;

				var guiObj = hud_m_gui_Field?.GetValue(hudData) as GameObject;
				if (guiObj == null) return;

				var healthTransform = guiObj.transform.Find("Health") as RectTransform;
				if (healthTransform == null) return;

				GuiBar fastBar = hud_m_healthFast_Field?.GetValue(hudData) as GuiBar;
				GuiBar slowBar = hud_m_healthSlow_Field?.GetValue(hudData) as GuiBar;

				ApplyBarSettings(c, healthTransform, fastBar, slowBar, ConfigManager.BetterEnemyNameplates.Value);
				AddHpText(hudData, healthTransform, ConfigManager.BetterEnemyNameplates.Value);
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
		public static class EnemyHud_UpdateHuds_CustomBar_Patch
		{
			private static void Postfix(EnemyHud __instance)
			{
				if (m_hudsField == null) return;

				var huds = m_hudsField.GetValue(__instance) as IDictionary;
				if (huds == null) return;

				foreach (DictionaryEntry entry in huds)
				{
					var hudData = entry.Value;
					if (hudData == null) continue;

					var character = hud_m_character_Field?.GetValue(hudData) as Character;
					if (character == null || character.IsDead()) continue;

					// Update HP text
					UpdateHpText(character, hudData, ConfigManager.BetterEnemyNameplates.Value);

					// Update alerted/aware
					UpdateAlertAndName(character, hudData, ConfigManager.BetterEnemyNameplates.Value);
				}
			}
		}

		private static void ApplyBarSettings(Character c, RectTransform health, GuiBar fastBar, GuiBar slowBar, bool enable)
		{
			// Backup
			if (!c.IsBoss() && DefaultBarHeight == -1f)
			{
				DefaultBarHeight = health.sizeDelta.y;
			}
			if (c.IsBoss() && DefaultBarHeightBoss == -1f)
			{
				DefaultBarHeightBoss = health.sizeDelta.y;
			}

			float targetHeight;
			if (enable)
			{
				targetHeight = c.IsBoss() ? BarHeightBoss : BarHeight;
			}
			else
			{
				targetHeight = c.IsBoss() ? DefaultBarHeightBoss : DefaultBarHeight;
			}

			if (_lastBarHeight.TryGetValue(health, out float lastHeight) && lastHeight == targetHeight)
				return; // nothing to do

			health.sizeDelta = new Vector2(health.sizeDelta.x, targetHeight);
			fastBar.m_bar.sizeDelta = new Vector2(fastBar.m_bar.sizeDelta.x, targetHeight);
			slowBar.m_bar.sizeDelta = new Vector2(slowBar.m_bar.sizeDelta.x, targetHeight);

			_lastBarHeight[health] = targetHeight;

			if (enable)
			{
				if (c.IsBoss())
					fastBar.SetColor(Color.magenta);
				else if (c.IsTamed() || c.IsPlayer())
					fastBar.SetColor(Color.green);
				else
					fastBar.SetColor(Color.red);
			}
			else
			{
				fastBar.SetColor(Color.red);
			}
		}

		private static void AddHpText(object hudData, RectTransform healthTransform, bool enableMainBars)
		{
			if (_hpTextCache.TryGetValue(hudData, out var existing))
				return; // already created

			bool enableHpText = ConfigManager.ShowEnemyHp.Value && enableMainBars;
			bool enableHpPercent = ConfigManager.ShowEnemyHpPercent.Value && enableMainBars;
			bool enableBothHpTexts = enableHpText && enableHpPercent;

			string UITMPFontName = "Valheim-AveriaSansLibre";
			Vector2 UITextAreaSize = new Vector2(100f, 14f); // width, height
			int UITextFontSize = 12;

			// HP text (cur / max)
			var hpText = CreateTMPTextObject("HpText", healthTransform.gameObject, Color.white, UITMPFontName, UITextFontSize, enableBothHpTexts ? TextAlignmentOptions.Left : TextAlignmentOptions.Center, Vector2.zero, UITextAreaSize, log);
			hpText.gameObject.SetActive(enableHpText);

			RectTransform hpTextRect = hpText.GetComponent<RectTransform>();
			if (enableBothHpTexts)
			{
				hpTextRect.anchorMin = new Vector2(0f, 0.5f);
				hpTextRect.anchorMax = new Vector2(0f, 0.5f);
				hpTextRect.pivot = new Vector2(0f, 0.5f);
				hpTextRect.anchoredPosition = new Vector2(3f, 0f);
			}
			else
			{
				hpTextRect.anchorMin = new Vector2(0.5f, 0.5f);
				hpTextRect.anchorMax = new Vector2(0.5f, 0.5f);
				hpTextRect.pivot = new Vector2(0.5f, 0.5f);
				hpTextRect.anchoredPosition = Vector2.zero;
			}

			// HpPercent text (%)
			var hpPercentText = CreateTMPTextObject("HpPercentText", healthTransform.gameObject, Color.white, UITMPFontName, UITextFontSize, enableBothHpTexts ? TextAlignmentOptions.Right : TextAlignmentOptions.Center, Vector2.zero, UITextAreaSize, log);
			hpPercentText.gameObject.SetActive(enableHpPercent);

			RectTransform hpPercentRect = hpPercentText.GetComponent<RectTransform>();
			if (enableBothHpTexts)
			{
				hpPercentRect.anchorMin = new Vector2(1f, 0.5f);
				hpPercentRect.anchorMax = new Vector2(1f, 0.5f);
				hpPercentRect.pivot = new Vector2(1f, 0.5f);
				hpPercentRect.anchoredPosition = new Vector2(-3f, 0f);
			}
			else
			{
				hpPercentRect.anchorMin = new Vector2(0.5f, 0.5f);
				hpPercentRect.anchorMax = new Vector2(0.5f, 0.5f);
				hpPercentRect.pivot = new Vector2(0.5f, 0.5f);
				hpPercentRect.anchoredPosition = Vector2.zero;
			}

			// Store cache
			_hpTextCache.Add(hudData, new HpTexts
			{
				HP = hpText,
				HpPercent = hpPercentText
			});
		}

		private static void UpdateHpText(Character character, object hudData, bool enableMainBars)
		{
			if (!_hpTextCache.TryGetValue(hudData, out var hpTexts)) return;

			bool enableHpText = ConfigManager.ShowEnemyHp.Value && enableMainBars;
			bool enableHpPercent = ConfigManager.ShowEnemyHpPercent.Value && enableMainBars;
			bool enableBothHpTexts = enableHpText && enableHpPercent;

			hpTexts.HP.gameObject.SetActive(enableHpText);
			hpTexts.HpPercent.gameObject.SetActive(enableHpPercent);

			if (!enableMainBars) return;

			UpdateHpTextLayout(hpTexts, enableBothHpTexts);

			// Update left / right text
			float currentHealth = character.GetHealth();
			float maxHealth = character.GetMaxHealth();
			float frac = Mathf.Clamp01(currentHealth / Math.Max(1f, maxHealth));

			//hpTexts.HP.text = $"{Mathf.CeilToInt(currentHealth)} - {Mathf.CeilToInt(maxHealth)}";
			hpTexts.HP.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
			hpTexts.HpPercent.text = $"{Mathf.RoundToInt(frac * 100f)}%";
		}

		private static void UpdateHpTextLayout(HpTexts hpTexts, bool enableBoth)
		{
			// HP (left or centered)
			var hpRect = hpTexts.HP.rectTransform;
			if (enableBoth)
			{
				hpRect.anchorMin = hpRect.anchorMax = new Vector2(0f, 0.5f);
				hpRect.pivot = new Vector2(0f, 0.5f);
				hpRect.anchoredPosition = new Vector2(3f, 0f);
				hpTexts.HP.alignment = TextAlignmentOptions.Left;
			}
			else
			{
				hpRect.anchorMin = hpRect.anchorMax = new Vector2(0.5f, 0.5f);
				hpRect.pivot = new Vector2(0.5f, 0.5f);
				hpRect.anchoredPosition = Vector2.zero;
				hpTexts.HP.alignment = TextAlignmentOptions.Center;
			}

			// HP percent (right or centered)
			var hpPercentRect = hpTexts.HpPercent.rectTransform;
			if (enableBoth)
			{
				hpPercentRect.anchorMin = hpPercentRect.anchorMax = new Vector2(1f, 0.5f);
				hpPercentRect.pivot = new Vector2(1f, 0.5f);
				hpPercentRect.anchoredPosition = new Vector2(-3f, 0f);
				hpTexts.HpPercent.alignment = TextAlignmentOptions.Right;
			}
			else
			{
				hpPercentRect.anchorMin = hpPercentRect.anchorMax = new Vector2(0.5f, 0.5f);
				hpPercentRect.pivot = new Vector2(0.5f, 0.5f);
				hpPercentRect.anchoredPosition = Vector2.zero;
				hpTexts.HpPercent.alignment = TextAlignmentOptions.Center;
			}
		}

		private static void UpdateAlertAndName(Character character, object hudData, bool enable)
		{
			var alertedObj = hud_m_alerted_Field?.GetValue(hudData) as RectTransform;
			var awareObj = hud_m_aware_Field?.GetValue(hudData) as RectTransform;
			if (enable)
			{
				alertedObj?.gameObject.SetActive(false);
				awareObj?.gameObject.SetActive(false);
			}

			BaseAI ai = character.GetBaseAI();
			bool isAlerted = ai?.IsAlerted() ?? false;
			bool hasTarget = ai?.HaveTarget() ?? false;

			var nameText = hud_m_name_Field?.GetValue(hudData) as TextMeshProUGUI;
			if (nameText != null)
			{
				if (enable)
				{
					if (isAlerted)
						nameText.color = Color.red;
					else if (hasTarget)
						nameText.color = Color.yellow;
					else
						nameText.color = Color.white;
				}
				else
				{
					nameText.color = Color.white;
				}
			}
		}
	}
}
