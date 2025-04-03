using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahTweaks.Patches.UI
{
	internal class UITimeAndDay : UIController
	{
		// UI data
		public static string dayString;
		public static int currentDay;

		// UI elements
		private static Text UITimeText;
		private static Text UIDayText;

		[HarmonyPatch(typeof(EnvMan), "Update")]
		class TimeAndDay_EnvManPatch
		{
			static void Prefix(EnvMan __instance, ref float ___m_smoothDayFraction)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.showTimeAndDay.Value)
				{
					currentDay = Traverse.Create((object)EnvMan.instance).Method("GetCurrentDay", Array.Empty<object>()).GetValue<int>();

					if (!ConfigManager.timeFormat24H.Value)
					{
						dayString = GetStringFromFraction(___m_smoothDayFraction);
					}
					else
					{
						int hours = (int)(___m_smoothDayFraction * 24f);
						int minutes = (int)((___m_smoothDayFraction * 24f - (float)hours) * 60f);
						string hoursString = hours < 10 ? "0" + hours.ToString() : hours.ToString();
						string minutesString = minutes < 10 ? "0" + minutes.ToString() : minutes.ToString();
						dayString = "Time " + hoursString + ":" + minutesString;
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

		[HarmonyPatch(typeof(Hud), "Update")]
		public static class TimeAndDay_HUDUpdatePatch
		{
			public static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.showTimeAndDay.Value)
				{
					CreateUI(__instance); // Create UI if missing

					UITimeText.enabled = showUI && Minimap.instance.m_mapSmall.activeInHierarchy;
					UIDayText.enabled = showUI && Minimap.instance.m_mapSmall.activeInHierarchy;

					if (showUI && Minimap.instance.m_mapSmall.activeInHierarchy)
					{
						UITimeText.color = GetColorFromString(dayString);
						UIDayText.color = Color.white;

						UITimeText.text = dayString;
						UIDayText.text = "Day " + currentDay.ToString();
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

			private static void CreateUI(Hud hud)
			{
				if (UITimeText != null && UIDayText != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UITimeAreaSize = new Vector2(200f, 30f); // width, height

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

				// Time text
				UITimeText = CreateTextObject("TimeText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(40f, 0f), UITimeAreaSize);

				// Day text
				UIDayText = CreateTextObject("DayText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(-40f, 0f), UITimeAreaSize);
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
	}
}
