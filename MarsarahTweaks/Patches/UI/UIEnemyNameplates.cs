using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahTweaks.Features.UI
{
	internal static class UIEnemyNameplates
	{
		private static readonly LogManager log = new LogManager("UI Enemy Nameplates", LogManager.LogLevel.Info);

		// Default sizes (tweakable)
		private const float BarHeight = 12f;
		private const float BarWidth = 100f;
		private const float SlowFollowSpeed = 3f;
		private const float TrailDuration = 1f; // how long the orange bar takes to shrink

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
			public TextMeshProUGUI Left;
			public TextMeshProUGUI Right;
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

		[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
		public static class EnemyHud_ShowHud_CustomBar_Patch
		{
			private static void Postfix(EnemyHud __instance, Character c)
			{
				if (c == null || m_hudsField == null) return;

				var huds = m_hudsField.GetValue(__instance) as IDictionary;
				if (huds == null || !huds.Contains(c)) return;

				var hudData = huds[c];
				if (hudData == null) return;

				var guiObj = hud_m_gui_Field?.GetValue(hudData) as GameObject;
				if (guiObj == null) return;

				var healthTransform = guiObj.transform.Find("Health") as RectTransform;
				if (healthTransform == null) return;

				healthTransform.sizeDelta = new Vector2(BarWidth, BarHeight);

				var fastObj = hud_m_healthFast_Field?.GetValue(hudData);
				var slowObj = hud_m_healthSlow_Field?.GetValue(hudData);

				ApplyBarSize(fastObj, BarHeight);
				ApplyBarColor(fastObj, Color.red);

				ApplyBarSize(slowObj, BarHeight);
				ApplyBarColor(slowObj, Color.yellow);

				if (fastObj is GuiBar fastBar && slowObj is GuiBar slowBar)
				{
					// Ensure slow bar is behind fast bar
					slowBar.m_bar.SetAsFirstSibling(); // behind
					fastBar.m_bar.SetAsLastSibling();  // on top
				}

				var bgImage = healthTransform.GetComponent<Image>();
				if (bgImage != null)
					bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

				AddHpText(hudData, healthTransform);
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

					float currentHealth = character.GetHealth();
					float maxHealth = character.GetMaxHealth();
					float frac = Mathf.Clamp01(currentHealth / Math.Max(1f, maxHealth));

					var fastObj = hud_m_healthFast_Field?.GetValue(hudData);
					var slowObj = hud_m_healthSlow_Field?.GetValue(hudData);

					// --- Fast bar updates ---
					UpdateGuiBar(fastObj, frac);

					// --- Slow bar ---
					// Only update height to match fast bar; leave width and color alone
					if (slowObj is GuiBar slowBar && fastObj is GuiBar fastBar)
					{
						RectTransform slowRect = slowBar.m_bar;
						RectTransform fastRect = fastBar.m_bar;
						if (slowRect != null && fastRect != null)
						{
							slowRect.sizeDelta = new Vector2(slowRect.sizeDelta.x, fastRect.sizeDelta.y);
						}
					}

					// --- Color overrides ---
					if (character.IsTamed())
					{
						ApplyBarColor(fastObj, Color.green);
					}
					else if (character.IsBoss())
					{
						ApplyBarColor(fastObj, Color.magenta);
					}
					else
					{
						ApplyBarColor(fastObj, Color.red);
					}

					// Update text
					if (_hpTextCache.TryGetValue(hudData, out var hpTexts))
					{
						hpTexts.Left.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
						hpTexts.Right.text = $"{Mathf.RoundToInt(frac * 100f)}%";
					}

					// --- Custom alerted/aware handling ---
					var alertedObj = hud_m_alerted_Field?.GetValue(hudData) as RectTransform;
					var awareObj = hud_m_aware_Field?.GetValue(hudData) as RectTransform;
					alertedObj?.gameObject.SetActive(false);
					awareObj?.gameObject.SetActive(false);

					var nameText = hud_m_name_Field?.GetValue(hudData) as TextMeshProUGUI;
					if (nameText != null && character.GetBaseAI() is BaseAI ai)
					{
						bool hasTarget = ai.HaveTarget();
						bool isAlerted = ai.IsAlerted();

						if (isAlerted)
							nameText.color = Color.red;
						else if (hasTarget)
							nameText.color = Color.yellow;
						else
							nameText.color = Color.white;
					}
				}
			}
			private static void UpdateGuiBar(object guiBarObj, float fraction)
			{
				GuiBar guiBar = guiBarObj as GuiBar;
				if (guiBar == null) return;

				RectTransform rect = guiBar.m_bar;
				if (rect != null)
				{
					float baseWidth = guiBar_m_width_Field?.GetValue(guiBar) is float f ? f : BarWidth;
					rect.sizeDelta = new Vector2(baseWidth * fraction, rect.sizeDelta.y);
				}
			}
		}

		private static void ApplyBarSize(object guiBarObj, float height)
		{
			if (guiBarObj == null) return;
			if (guiBarObj is GuiBar guiBar)
			{
				RectTransform barRect = guiBar.m_bar;
				if (barRect != null)
				{
					barRect.sizeDelta = new Vector2(barRect.sizeDelta.x, height);
				}
			}
		}

		private static void ApplyBarColor(object guiBarObj, Color color)
		{
			if (guiBarObj == null) return;
			if (guiBarObj is GuiBar guiBar) guiBar.SetColor(color);
		}

		private static void AddHpText(object hudData, RectTransform healthTransform)
		{
			if (_hpTextCache.TryGetValue(hudData, out var existing))
				return; // already created

			var font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Valheim-AveriaSansLibre");
			if (font == null) log.Warn("Valheim-AveriaSansLibre not found!");

			// Left text (cur / max)
			GameObject leftObj = new GameObject("HpTextLeft", typeof(RectTransform));
			leftObj.transform.SetParent(healthTransform, false);

			RectTransform leftRect = leftObj.GetComponent<RectTransform>();
			leftRect.anchorMin = new Vector2(0f, 0.5f);
			leftRect.anchorMax = new Vector2(0f, 0.5f);
			leftRect.pivot = new Vector2(0f, 0.5f);
			leftRect.anchoredPosition = new Vector2(3f, 0f);

			/*leftRect.anchorMin = new Vector2(0.5f, 0.5f);
			leftRect.anchorMax = new Vector2(0.5f, 0.5f);
			leftRect.pivot = new Vector2(0.5f, 0.5f);
			leftRect.anchoredPosition = Vector2.zero;*/

			var leftText = leftObj.AddComponent<TextMeshProUGUI>();
			leftText.font = font;
			leftText.fontSize = 12f;
			leftText.alignment = TextAlignmentOptions.Left;
			//leftText.alignment = TextAlignmentOptions.Center;
			leftText.color = Color.white;
			leftText.enabled = true;

			// Right text (%)
			GameObject rightObj = new GameObject("HpTextRight", typeof(RectTransform));
			rightObj.transform.SetParent(healthTransform, false);

			RectTransform rightRect = rightObj.GetComponent<RectTransform>();
			rightRect.anchorMin = new Vector2(1f, 0.5f);
			rightRect.anchorMax = new Vector2(1f, 0.5f);
			rightRect.pivot = new Vector2(1f, 0.5f);
			rightRect.anchoredPosition = new Vector2(-3f, 0f);

			/*rightRect.anchorMin = new Vector2(0.5f, 0.5f);
			rightRect.anchorMax = new Vector2(0.5f, 0.5f);
			rightRect.pivot = new Vector2(0.5f, 0.5f);
			rightRect.anchoredPosition = Vector2.zero;*/

			var rightText = rightObj.AddComponent<TextMeshProUGUI>();
			rightText.font = font;
			rightText.fontSize = 12f;
			rightText.alignment = TextAlignmentOptions.Right;
			//rightText.alignment = TextAlignmentOptions.Center;
			rightText.color = Color.white;
			rightText.enabled = true;

			// Store both
			_hpTextCache.Add(hudData, new HpTexts
			{
				Left = leftText,
				Right = rightText
			});
		}
	}
}
