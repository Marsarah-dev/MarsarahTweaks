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
using static ItemDrop.ItemData;

namespace MarsarahTweaks.Features.UI
{
	internal static class UIEnemyNameplates
	{
		private static readonly LogManager log = new LogManager("UI Enemy Nameplates", LogManager.LogLevel.Info);

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
		private static readonly MethodInfo GetTamenessMethod;

		// GuiBar internals
		private static readonly FieldInfo guiBar_m_width_Field;

		// Text 
		private class HpTexts
		{
			public TextMeshProUGUI HP;
			public TextMeshProUGUI HpPercent;
			public TextMeshProUGUI Taming;
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

			GetTamenessMethod = typeof(Tameable).GetMethod("GetTameness", BindingFlags.NonPublic | BindingFlags.Instance);
	}

		[HarmonyPatch(typeof(EnemyHud), "Awake")]
		public static class EmenyHud_Awake_Patch
		{
			private static void Postfix(ref EnemyHud __instance)
			{
				if (__instance == null) return;

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

					// Update text
					UpdateText(character, hudData, ConfigManager.BetterEnemyNameplates.Value);

					// Custom alerted/aware handling
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

		private static void AddHpText(object hudData, RectTransform healthTransform, bool enable)
		{
			if (_hpTextCache.TryGetValue(hudData, out var existing))
				return; // already created

			var font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == "Valheim-AveriaSansLibre");
			if (font == null) log.Warn("Valheim-AveriaSansLibre not found!");

			// HP text (cur / max)
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
			//leftText.enabled = enable;

			// HpPercent text (%)
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
			//rightText.enabled = enable;

			// Taming text (bottom-right, below the bar)
			GameObject emojiObj = new GameObject("HpEmoji", typeof(RectTransform));
			emojiObj.transform.SetParent(healthTransform, false);

			RectTransform emojiRect = emojiObj.GetComponent<RectTransform>();
			emojiRect.anchorMin = new Vector2(1f, 0f);
			emojiRect.anchorMax = new Vector2(1f, 0f);
			emojiRect.pivot = new Vector2(1f, 0f);
			emojiRect.anchoredPosition = new Vector2(-3f, -14f); // slightly below the bar

			var emojiText = emojiObj.AddComponent<TextMeshProUGUI>();
			emojiText.font = font;
			emojiText.fontSize = 11f;
			emojiText.alignment = TextAlignmentOptions.BottomRight;
			emojiText.color = Color.white;
			//emojiText.enabled = enable;
			emojiText.text = ""; // start empty

			// Store all three
			_hpTextCache.Add(hudData, new HpTexts
			{
				HP = leftText,
				HpPercent = rightText,
				Taming = emojiText
			});
		}

		private static void UpdateText(Character character, object hudData, bool enable)
		{
			if (!_hpTextCache.TryGetValue(hudData, out var hpTexts)) return;

			hpTexts.HP.gameObject.SetActive(enable);
			hpTexts.HpPercent.gameObject.SetActive(enable);
			hpTexts.Taming.gameObject.SetActive(enable); 

			if (!enable) return;

			// Update left / right text
			float currentHealth = character.GetHealth();
			float maxHealth = character.GetMaxHealth();
			float frac = Mathf.Clamp01(currentHealth / Math.Max(1f, maxHealth));

			hpTexts.HP.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
			hpTexts.HpPercent.text = $"{Mathf.RoundToInt(frac * 100f)}%";

			// Update tamed creatures progress
			if (character.TryGetComponent<Tameable>(out var tameable))
			{
				UpdateTamingText(tameable, hpTexts.Taming);
			}
		}

		private static void UpdateTamingText(Tameable tameable, TextMeshProUGUI tamingText)
		{
			if (!tameable.IsTamed())
			{
				int tamingProgress = 0;
				if (GetTamenessMethod != null)
					tamingProgress = (int)GetTamenessMethod.Invoke(tameable, null);

				tamingText.gameObject.SetActive(tamingProgress != 0);

				if (tamingProgress != 0)
				{
					string status = tameable.GetStatusString();

					tamingText.text = $"Taming: {tamingProgress}%";
					tamingText.color = status switch
					{
						"$hud_tamehungry" => new Color(1f, 0.549f, 0f),
						"$hud_tamefrightened" => Color.red,
						_ => Color.cyan // $hud_tameinprogress, hud_tamehappy
					};
				}
			}
			else
			{
				tamingText.gameObject.SetActive(false);
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
					nameText.color = Color.white;
			}
		}
	}
}
