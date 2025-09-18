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

namespace MarsarahTweaks.Patches.UI
{
	internal class UITamingProgress : UIController
	{
		private static readonly LogManager log = new LogManager("UI Taming Progress", LogManager.LogLevel.Warning);

		// Reflection cache
		private static readonly FieldInfo m_hudsField;
		private static readonly Type hudDataType;
		private static readonly FieldInfo hud_m_gui_Field;
		private static readonly FieldInfo hud_m_character_Field;
		private static readonly MethodInfo GetTamenessMethod;

		// Cache for taming text
		private static readonly ConditionalWeakTable<object, TextMeshProUGUI> _tamingCache = new ConditionalWeakTable<object, TextMeshProUGUI>();

		static UITamingProgress()
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

			GetTamenessMethod = typeof(Tameable).GetMethod("GetTameness", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		[HarmonyPatch(typeof(EnemyHud), "ShowHud")]
		public static class EnemyHud_ShowHud_Taming_Patch
		{
			private static void Postfix(EnemyHud __instance, Character c)
			{
				if (!ConfigManager.ShowTamingProgress.Value) return;
				if (c == null || m_hudsField == null) return;

				var huds = m_hudsField.GetValue(__instance) as IDictionary;
				if (huds == null || !huds.Contains(c)) return;

				var hudData = huds[c];
				if (hudData == null) return;

				var guiObj = hud_m_gui_Field?.GetValue(hudData) as GameObject;
				if (guiObj == null) return;

				var healthTransform = guiObj.transform.Find("Health") as RectTransform;
				if (healthTransform == null) return;

				AddTamingText(hudData, healthTransform);
			}
		}

		[HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
		public static class EnemyHud_UpdateHuds_Taming_Patch
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

					UpdateTamingText(character, hudData);
				}
			}
		}

		private static void AddTamingText(object hudData, RectTransform healthTransform)
		{
			if (_tamingCache.TryGetValue(hudData, out _))
				return; // already created

			string UITMPFontName = "Valheim-AveriaSansLibre";
			Vector2 UITextAreaSize = new Vector2(100f, 14f); // width, height
			int UITextFontSize = 11;

			// Create text object
			var tamingText = CreateTMPTextObject("TamingText", healthTransform.gameObject, Color.white, UITMPFontName, UITextFontSize, TextAlignmentOptions.BottomRight, Vector2.zero, UITextAreaSize, log);

			tamingText.gameObject.SetActive(ConfigManager.ShowTamingProgress.Value);
			tamingText.text = "";

			// Positioning (bottom-right of health bar)
			RectTransform tamingRect = tamingText.rectTransform;
			tamingRect.anchorMin = new Vector2(1f, 0f);
			tamingRect.anchorMax = new Vector2(1f, 0f);
			tamingRect.pivot = new Vector2(1f, 0f);
			tamingRect.anchoredPosition = new Vector2(-3f, -14f);

			// Cache
			_tamingCache.Add(hudData, tamingText);
		}


		private static void UpdateTamingText(Character character, object hudData)
		{
			if (!_tamingCache.TryGetValue(hudData, out var tamingText)) return;

			if (!ConfigManager.ShowTamingProgress.Value)
			{
				tamingText.gameObject.SetActive(false);
				return;
			}

			if (character.TryGetComponent<Tameable>(out var tameable))
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
			else
			{
				tamingText.gameObject.SetActive(false);
			}
		}
	}
}
