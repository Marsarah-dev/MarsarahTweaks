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
			public TextMeshProUGUI Left;
			public TextMeshProUGUI Right;
			public TextMeshProUGUI Emoji;
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

				/*float maxDistance = 10f; 
				__instance.m_maxShowDistance = Mathf.Max(__instance.m_maxShowDistance, maxDistance);*/

				float distanceMultiplier = 2f;
				__instance.m_maxShowDistance *= distanceMultiplier;
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

				if (fastObj is GuiBar fastBar && slowObj is GuiBar slowBar)
				{
					// Apply to bars (height only!)
					ApplyBarSize(fastBar.m_bar, BarHeight);
					ApplyBarColor(fastBar, Color.red);

					ApplyBarSize(slowBar.m_bar, BarHeight);
					ApplyBarColor(slowBar, Color.yellow);

					// Ensure layering
					slowBar.m_bar.SetAsFirstSibling();
					fastBar.m_bar.SetAsLastSibling();

					// For bosses: stretch background to full boss width
					if (c.IsBoss() && guiBar_m_width_Field?.GetValue(fastBar) is float bossWidth)
					{
						healthTransform.sizeDelta = new Vector2(bossWidth, BarHeight);
					}
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

					// Fast bar updates
					UpdateGuiBar(fastObj, frac);

					// Slow bar updates
					if (slowObj is GuiBar slowBar && fastObj is GuiBar fastBar)
					{
						RectTransform slowRect = slowBar.m_bar;
						RectTransform fastRect = fastBar.m_bar;
						if (slowRect != null && fastRect != null)
						{
							slowRect.sizeDelta = new Vector2(slowRect.sizeDelta.x, fastRect.sizeDelta.y);
						}
					}

					// Color overrides
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

					// Get chjaracter AI and status
					BaseAI ai = character.GetBaseAI();
					bool isAlerted = ai?.IsAlerted() ?? false;
					bool hasTarget = ai?.HaveTarget() ?? false;

					// Update text
					if (_hpTextCache.TryGetValue(hudData, out var hpTexts))
					{
						// Update left / right text
						hpTexts.Left.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
						hpTexts.Right.text = $"{Mathf.RoundToInt(frac * 100f)}%";

						// Update emoji for tamed creatures
						if (character.TryGetComponent<Tameable>(out var tameable))
						{
							// Show taming progress for untamed creatures
							if (!tameable.IsTamed())
							{
								int tamingProgress = 0;
								if (GetTamenessMethod != null)
									tamingProgress = (int)GetTamenessMethod.Invoke(tameable, null);

								string status = tameable.GetStatusString();

								if (tamingProgress == 0)
									hpTexts.Emoji.text = "";
								else
								{
									hpTexts.Emoji.text = $"Taming: {tamingProgress}%";
									log.Info($"Creature: {character.name} - Status: {status}");
									hpTexts.Emoji.color = status switch
									{
										"$hud_tamehungry" => new Color(1f, 0.549f, 0f),
										"$hud_tamefrightened" => Color.red,
										_ => Color.cyan // $hud_tameinprogress
									};
								}
							}
							else
							{
								// Post-tamed emoji logic
								bool hungry = tameable.IsHungry();
								if (hasTarget)
								{
									hpTexts.Emoji.text = "😡";
									hpTexts.Emoji.color = Color.red;
								}
								else if (hungry)
								{
									hpTexts.Emoji.text = "☹"; // 😋
									hpTexts.Emoji.color = new Color(1f, 0.549019f, 0f);
								}
								else if (!isAlerted)
								{
									hpTexts.Emoji.text = "🙂"; // 😄
									hpTexts.Emoji.color = Color.yellow;
								}
								else
									hpTexts.Emoji.text = "";
							}
						}
					}

					// Custom alerted/aware handling
					var alertedObj = hud_m_alerted_Field?.GetValue(hudData) as RectTransform;
					var awareObj = hud_m_aware_Field?.GetValue(hudData) as RectTransform;
					alertedObj?.gameObject.SetActive(false);
					awareObj?.gameObject.SetActive(false);

					var nameText = hud_m_name_Field?.GetValue(hudData) as TextMeshProUGUI;
					if (nameText != null)
					{
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
		private static void ApplyBarSize(RectTransform rect, float height)
		{
			if (rect == null) return;
			rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
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

			// Emoji text (bottom-right, below the bar)
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
			emojiText.enabled = true;
			emojiText.text = ""; // start empty

			// Store all three
			_hpTextCache.Add(hudData, new HpTexts
			{
				Left = leftText,
				Right = rightText,
				Emoji = emojiText
			});
		}
	}
}
