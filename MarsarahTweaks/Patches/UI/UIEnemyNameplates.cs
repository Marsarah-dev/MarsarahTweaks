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

		// GuiBar internals
		private static readonly Type guiBarType;
		private static readonly FieldInfo guiBar_m_bar_Field;
		private static readonly FieldInfo guiBar_m_width_Field;
		private static readonly MethodInfo guiBar_SetColor_Method;

		// Track previous health per character
		private class FloatWrapper { public float Value; }
		private static readonly ConditionalWeakTable<Character, FloatWrapper> _previousHealth = new ConditionalWeakTable<Character, FloatWrapper>();

		static UIEnemyNameplates()
		{
			try
			{
				var enemyHudType = typeof(EnemyHud);
				m_hudsField = enemyHudType.GetField("m_huds", BindingFlags.NonPublic | BindingFlags.Instance);
				hudDataType = enemyHudType.GetNestedType("HudData", BindingFlags.NonPublic | BindingFlags.Instance);

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

				guiBarType = hud_m_healthFast_Field?.FieldType
					?? AppDomain.CurrentDomain.GetAssemblies()
						.SelectMany(a => a.GetTypesSafe())
						.FirstOrDefault(t => t.Name.Equals("GuiBar", StringComparison.OrdinalIgnoreCase));

				if (guiBarType != null)
				{
					guiBar_m_bar_Field = guiBarType.GetField("m_bar", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					guiBar_m_width_Field = guiBarType.GetField("m_width", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					guiBar_SetColor_Method = guiBarType.GetMethod("SetColor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				}
				else
				{
					log.Warn("Could not resolve GuiBar type via reflection. Resizing / color calls will be limited.");
				}
			}
			catch (Exception ex)
			{
				log.Error($"Reflection static init failed: {ex}");
			}
		}

		private static IEnumerable<Type> GetTypesSafe(this Assembly asm)
		{
			try { return asm.GetTypes(); }
			catch { return Array.Empty<Type>(); }
		}

		// ---------- ShowHud patch ----------
		[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
		public static class EnemyHud_ShowHud_CustomBar_Patch
		{
			private static void Postfix(EnemyHud __instance, Character c)
			{
				try
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

					ApplyBarSizeAndColor(fastObj, BarHeight, Color.red);
					ApplyBarSizeAndColor(slowObj, BarHeight, new Color(0.3f, 0.3f, 0.3f, 1f));

					var bgImage = healthTransform.GetComponent<Image>();
					if (bgImage != null)
						bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
				}
				catch (Exception ex)
				{
					log.Error($"ShowHud postfix error: {ex}");
				}
			}
		}

		// ---------- UpdateHuds patch ----------
		[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
		public static class EnemyHud_UpdateHuds_CustomBar_Patch
		{
			private static void Postfix(EnemyHud __instance)
			{
				try
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

						// Fast bar
						UpdateGuiBar(fastObj, frac);

						// Slow bar
						UpdateGuiBar(slowObj, frac, smooth: true, speed: SlowFollowSpeed, color: new Color(0.3f, 0.3f, 0.3f, 1f));

						// Orange trail
						FloatWrapper wrapper;
						if (!_previousHealth.TryGetValue(character, out wrapper))
						{
							wrapper = new FloatWrapper { Value = currentHealth };
							_previousHealth.Add(character, wrapper);
						}

						if (currentHealth < wrapper.Value)
							SpawnTrailBar(fastObj, wrapper.Value / maxHealth, frac);

						wrapper.Value = currentHealth;

						// Color overrides
						if (character.IsTamed())
						{
							ApplyBarColor(fastObj, Color.green);
							ApplyBarColor(slowObj, new Color(0f, 0.5f, 0f, 1f));
						}
						else if (character.IsBoss())
						{
							ApplyBarColor(fastObj, Color.magenta);
							ApplyBarColor(slowObj, Color.gray);
						}
						else
						{
							ApplyBarColor(fastObj, Color.red);
						}
					}
				}
				catch (Exception ex)
				{
					log.Error($"UpdateHuds postfix error: {ex}");
				}
			}

			private static void UpdateGuiBar(object guiBarObj, float fraction, bool smooth = false, float speed = 3f, Color? color = null)
			{
				if (guiBarObj == null || guiBar_m_bar_Field == null) return;

				var rect = guiBar_m_bar_Field.GetValue(guiBarObj) as RectTransform;
				if (rect != null)
				{
					float baseWidth = GetGuiBarBaseWidth(guiBarObj);
					float targetWidth = baseWidth * fraction;
					rect.sizeDelta = smooth ? new Vector2(Mathf.Lerp(rect.sizeDelta.x, targetWidth, Time.deltaTime * speed), rect.sizeDelta.y) : new Vector2(targetWidth, rect.sizeDelta.y);
				}

				if (color.HasValue)
					ApplyBarColor(guiBarObj, color.Value);
			}

			private static void SpawnTrailBar(object fastObj, float startFrac, float endFrac)
			{
				if (fastObj == null || guiBar_m_bar_Field == null) return;

				var fastRect = guiBar_m_bar_Field.GetValue(fastObj) as RectTransform;
				if (fastRect == null) return;

				var trailGO = GameObject.Instantiate(fastRect.gameObject, fastRect.parent);
				var trailRect = trailGO.GetComponent<RectTransform>();
				trailRect.SetAsFirstSibling();
				trailRect.sizeDelta = new Vector2(GetGuiBarBaseWidth(fastObj) * startFrac, trailRect.sizeDelta.y);

				var image = trailGO.GetComponent<Image>();
				if (image != null) image.color = new Color(1f, 0.65f, 0f, 1f);

				trailGO.AddComponent<TrailBarAnimator>().Init(fastObj, startFrac, endFrac, TrailDuration);
			}

			private class TrailBarAnimator : MonoBehaviour
			{
				private RectTransform rect;
				private float startWidth;
				private float targetWidth;
				private float duration;
				private float elapsed;

				public void Init(object guiBarObj, float startFrac, float endFrac, float duration)
				{
					rect = GetComponent<RectTransform>();
					startWidth = rect.sizeDelta.x;
					targetWidth = GetGuiBarBaseWidth(guiBarObj) * endFrac;
					this.duration = duration;
					elapsed = 0f;
				}

				private void Update()
				{
					if (rect == null) { Destroy(this); return; }

					elapsed += Time.deltaTime;
					float t = Mathf.Clamp01(elapsed / duration);
					float width = Mathf.Lerp(startWidth, targetWidth, t);
					rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);

					if (t >= 1f) Destroy(gameObject);
				}
			}
		}

		// ---------- Common helpers ----------
		private static float GetGuiBarBaseWidth(object guiBarObj)
		{
			if (guiBarObj == null) return BarWidth;
			try
			{
				if (guiBar_m_width_Field != null)
				{
					var val = guiBar_m_width_Field.GetValue(guiBarObj);
					if (val is float f) return f;
					if (val is double d) return (float)d;
					if (val is int i) return i;
					if (val != null && float.TryParse(val.ToString(), out var parsed)) return parsed;
				}
			}
			catch { }
			return BarWidth;
		}
		
		private static void ApplyBarSizeAndColor(object guiBarObj, float height, Color color)
		{
			if (guiBarObj == null) return;
			try
			{
				if (guiBar_m_bar_Field != null)
				{
					var barRect = guiBar_m_bar_Field.GetValue(guiBarObj) as RectTransform;
					if (barRect != null)
						barRect.sizeDelta = new Vector2(GetGuiBarBaseWidth(guiBarObj), height);
				}

				ApplyBarColor(guiBarObj, color);
			}
			catch (Exception ex) { log.Warn($"ApplyBarSizeAndColor failed: {ex.Message}"); }
		}

		private static void ApplyBarColor(object guiBarObj, Color color)
		{
			if (guiBarObj == null) return;
			try
			{
				if (guiBar_SetColor_Method != null)
					guiBar_SetColor_Method.Invoke(guiBarObj, new object[] { color });
				else
				{
					var colorField = guiBarObj.GetType().GetField("m_color", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					if (colorField != null && colorField.FieldType == typeof(Color))
						colorField.SetValue(guiBarObj, color);
				}
			}
			catch (Exception ex) { log.Warn($"ApplyBarColor failed: {ex.Message}"); }
		}
	}
}
