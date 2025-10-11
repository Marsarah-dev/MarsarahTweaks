using HarmonyLib;
using MarsarahTweaks.Managers;
using Splatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MarsarahTweaks.Managers.ConfigManager;

namespace MarsarahTweaks.Patches.UI
{
	internal class UITimeAndDay : UIController
	{
		private static readonly LogManager log = new LogManager("UI Time And Day", LogManager.LogLevel.Warning);

		// UI data
		public static string TimeString;
		public static int CurrentDay;

		// UI elements
		private static Text UITimeText;
		private static Text UIDayText;

		[HarmonyPatch(typeof(EnvMan), "Update")]
		class TimeAndDay_EnvManPatch
		{
			private static void Prefix(EnvMan __instance, ref float ___m_smoothDayFraction)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowCurrentDay.Value)
				{
					CurrentDay = Traverse.Create((object)EnvMan.instance).Method("GetCurrentDay", Array.Empty<object>()).GetValue<int>();
				}

				if (ConfigManager.TimeChoice.Value != TimeMode.Off)
				{
					if (ConfigManager.TimeChoice.Value == TimeMode.DayPhases)
					{
						TimeString = GetStringFromFraction(___m_smoothDayFraction);
					}
					else
					{
						int hours = (int)(___m_smoothDayFraction * 24f);
						int minutes = (int)((___m_smoothDayFraction * 24f - (float)hours) * 60f);
						string hoursString = hours < 10 ? "0" + hours.ToString() : hours.ToString();
						string minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
						TimeString = "Time " + hoursString + ":" + minutesString;
					}
				}
			}

			private static string GetStringFromFraction(float dayFraction)
			{
				if (dayFraction < 0.20f) return "Night";
				if (dayFraction < 0.25f) return "Dawn";
				if (dayFraction < 0.33f) return "Morning";
				if (dayFraction < 0.50f) return "Day";
				if (dayFraction < 0.66f) return "Afternoon";
				if (dayFraction < 0.75f) return "Evening";
				if (dayFraction < 0.80f) return "Dusk";
				return "Night";
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		class InventoryWeightAndSlots_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.TimeChoice.Value != TimeMode.Off || ConfigManager.ShowCurrentDay.Value != false)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		class TimeAndDay_HUDUpdatePatch
		{
			//private static UIMode lastUImode = ConfigManager.UILayoutChoice.Value;

			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.TimeChoice.Value != TimeMode.Off || ConfigManager.ShowCurrentDay.Value != false)
				{
					bool newUI = ConfigManager.UILayoutChoice.Value == UIMode.New;

					CreateUI(__instance); // Create UI if missing

					bool showTimeAndDayUI = Game.m_noMap ? ShowUI : ShowUI && Minimap.instance != null && Minimap.instance.m_mapSmall != null && Minimap.instance.m_mapSmall.activeInHierarchy;

					UITimeText.enabled = showTimeAndDayUI && ConfigManager.TimeChoice.Value != TimeMode.Off;
					UIDayText.enabled = showTimeAndDayUI && ConfigManager.ShowCurrentDay.Value != false;

					if (showTimeAndDayUI)
					{
						UITimeText.color = GetColorFromString(TimeString);
						UIDayText.color = Color.white;

						UITimeText.text = TimeString;
						UIDayText.text = "Day " + CurrentDay.ToString();
					}
				}
				else
				{
					if (UITimeText != null)
						UITimeText.enabled = false;
					if (UIDayText != null)
						UIDayText.enabled = false;
				}
			}

			private static Color GetColorFromString(string word)
			{
				if (word == "Night") return Color.red;
				if (word == "Dawn") return new Color(1f, 0.549019f, 0f);
				if (word == "Dusk") return new Color(1f, 0.549019f, 0f);
				if (word == "Morning") return Color.yellow;
				if (word == "Evening") return Color.yellow;
				if (word == "Day") return Color.green;
				if (word == "Afternoon") return Color.green;
				return Color.white;
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UITimeText != null && UIDayText != null)
				return;  // UI already exists, no need to create again

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UITimeAreaSize = new Vector2(200f, 30f); // width, height
			Vector2 UITimeAreaEmojiSize = new Vector2(30f, 30f); // width, height

			// Day-Time area object
			GameObject UITimeArea = new GameObject("TimeArea");
			UITimeArea.layer = 5;
			UITimeArea.transform.SetParent(hud.m_rootObject.transform);
			RectTransform timeAreaTransform = UITimeArea.AddComponent<RectTransform>();
			timeAreaTransform.anchorMin = new Vector2(1f, 1f);
			timeAreaTransform.anchorMax = new Vector2(1f, 1f);
			timeAreaTransform.anchoredPosition = new Vector2(-140f, -25f);
			timeAreaTransform.sizeDelta = UITimeAreaSize;
			UITimeArea.transform.localScale = Vector3.one;  // Ensure correct scale

			// Special modification for text sizeDelta
			UITimeAreaSize.x = UITimeAreaSize.x / 2;
			float timeTextXPos = 40f;
			float dayTextXPos = -40f;

			// Time text
			UITimeText = CreateTextObject("TimeText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(timeTextXPos, 0f), UITimeAreaSize);

			// Day text
			UIDayText = CreateTextObject("DayText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(dayTextXPos, 0f), UITimeAreaSize);
		}

		public static void UpdatePositions()
		{
			if (UITimeText == null || UIDayText == null) return;

			bool timeEnabled = ConfigManager.TimeChoice.Value != TimeMode.Off;
			bool dayEnabled = ConfigManager.ShowCurrentDay.Value;

			float timeTextXPos = 40f;
			float dayTextXPos = -40f;

			RectTransform timeTransform = UITimeText.rectTransform;
			RectTransform dayTransform = UIDayText.rectTransform;

			Vector2 tPos = timeTransform.anchoredPosition;
			Vector2 dPos = dayTransform.anchoredPosition;

			if (timeEnabled && dayEnabled)
			{
				tPos.x = timeTextXPos;
				dPos.x = dayTextXPos;
				UITimeText.alignment = TextAnchor.MiddleRight;
				UIDayText.alignment = TextAnchor.MiddleLeft;
			}
			else
			{
				tPos.x = timeEnabled ? 0f : 0f;
				dPos.x = dayEnabled ? 0f : 0f;
				UITimeText.alignment = TextAnchor.MiddleCenter;
				UIDayText.alignment = TextAnchor.MiddleCenter;
			}

			timeTransform.anchoredPosition = tPos;
			dayTransform.anchoredPosition = dPos;
		}
	}
}
