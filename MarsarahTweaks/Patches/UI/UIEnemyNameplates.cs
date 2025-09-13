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
		private static readonly FieldInfo guiBar_m_width_Field;

		// Track previous health per character
		private class FloatWrapper { public float Value; }
		private static readonly ConditionalWeakTable<Character, FloatWrapper> _previousHealth = new ConditionalWeakTable<Character, FloatWrapper>();

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

			guiBar_m_width_Field = typeof(GuiBar).GetField("m_width", BindingFlags.NonPublic | BindingFlags.Instance);
			if (guiBar_m_width_Field == null)
			{
				log.Warn("Could not resolve GuiBar.m_width via reflection. Resizing may fallback to default width.");
			}
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
				if (guiBarObj == null) return;

				GuiBar guiBar = guiBarObj as GuiBar;
				if (guiBar == null) return;

				RectTransform rect = guiBar.m_bar;
				if (rect != null)
				{
					float baseWidth = guiBar_m_width_Field?.GetValue(guiBar) is float f ? f : BarWidth;
					float targetWidth = baseWidth * fraction;
					rect.sizeDelta = smooth ? new Vector2(Mathf.Lerp(rect.sizeDelta.x, targetWidth, Time.deltaTime * speed), rect.sizeDelta.y) : new Vector2(targetWidth, rect.sizeDelta.y);
				}

				if (color.HasValue)
					guiBar.SetColor(color.Value);
			}

			private static void SpawnTrailBar(object fastObj, float startFrac, float endFrac)
			{
				if (fastObj == null) return;

				GuiBar fastGui = fastObj as GuiBar;
				if (fastGui == null) return;

				RectTransform fastRect = fastGui.m_bar;
				if (fastRect == null) return;

				var trailGO = GameObject.Instantiate(fastRect.gameObject, fastRect.parent);
				var trailRect = trailGO.GetComponent<RectTransform>();
				float baseWidth = guiBar_m_width_Field?.GetValue(fastGui) is float f ? f : BarWidth;
				trailRect.SetAsFirstSibling();
				trailRect.sizeDelta = new Vector2(baseWidth * startFrac, trailRect.sizeDelta.y);

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
					GuiBar guiBar = guiBarObj as GuiBar;
					if (guiBar == null) return;

					rect = GetComponent<RectTransform>();
					startWidth = rect.sizeDelta.x;
					float baseWidth = guiBar_m_width_Field?.GetValue(guiBar) is float f ? f : BarWidth;
					targetWidth = baseWidth * endFrac;
					this.duration = duration;
					elapsed = 0f;
				}

				private void Update() // MC: check this
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
		
		private static void ApplyBarSizeAndColor(object guiBarObj, float height, Color color)
		{
			if (guiBarObj == null) return;

			GuiBar guiBar = guiBarObj as GuiBar;
			if (guiBar == null) return;

			RectTransform barRect = guiBar.m_bar;
			if (barRect != null)
			{
				float baseWidth = guiBar_m_width_Field?.GetValue(guiBar) is float f ? f : BarWidth;
				barRect.sizeDelta = new Vector2(baseWidth, height);
			}

			guiBar.SetColor(color);
		}

		private static void ApplyBarColor(object guiBarObj, Color color)
		{
			if (guiBarObj == null) return;

			GuiBar guiBar = guiBarObj as GuiBar;
			if (guiBar == null) return;

			guiBar.SetColor(color);
		}
	}
}
