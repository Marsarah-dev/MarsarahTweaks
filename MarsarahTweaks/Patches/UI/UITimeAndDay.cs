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
		//public static string TimeEmoji;
		public static int CurrentDay;

		// UI elements
		private static Text UITimeText;
		private static Text UIDayText;

		//private static TMPro.TextMeshProUGUI UITimeEmojiTMP = null;
		//private static Color UITimeEmojiColor;

		/*private static readonly Dictionary<(Heightmap.Biome, string), string> WeatherEmojis = new Dictionary<(Heightmap.Biome, string), string>()
		{
			// Meadows
			{ (Heightmap.Biome.Meadows, "Clear"), "☀️" },
			{ (Heightmap.Biome.Meadows, "Rain"), "🌧" },
			{ (Heightmap.Biome.Meadows, "Misty"), "🌫" },
			{ (Heightmap.Biome.Meadows, "ThunderStorm"), "⛈" },
			{ (Heightmap.Biome.Meadows, "LightRain"), "🌦" },

			// BlackForest
			{ (Heightmap.Biome.BlackForest, "DeepForest Mist"), "🌫" },
			{ (Heightmap.Biome.BlackForest, "Rain"), "🌧" },
			{ (Heightmap.Biome.BlackForest, "Misty"), "🌫" },
			{ (Heightmap.Biome.BlackForest, "ThunderStorm"), "⛈" },

			// Swamp
			{ (Heightmap.Biome.Swamp, "SwampRain"), "🌧" },

			// Mountain
			{ (Heightmap.Biome.Mountain, "SnowStorm"), "🌨️" },
			{ (Heightmap.Biome.Mountain, "Snow"), "❄️" },

			// DeepNorth
			{ (Heightmap.Biome.DeepNorth, "Twilight_SnowStorm"), "🌨️" },
			{ (Heightmap.Biome.DeepNorth, "Twilight_Snow"), "❄️" },
			{ (Heightmap.Biome.DeepNorth, "Twilight_Clear"), "☀️" },

			// Plains
			{ (Heightmap.Biome.Plains, "Heath clear"), "☀️" },
			{ (Heightmap.Biome.Plains, "Misty"), "🌫" },
			{ (Heightmap.Biome.Plains, "LightRain"), "🌦" },

			// Ocean
			{ (Heightmap.Biome.Ocean, "Clear"), "☀️" },
			{ (Heightmap.Biome.Ocean, "Rain"), "🌧" },
			{ (Heightmap.Biome.Ocean, "LightRain"), "🌦" },
			{ (Heightmap.Biome.Ocean, "Misty"), "🌫" },
			{ (Heightmap.Biome.Ocean, "ThunderStorm"), "⛈" },
			{ (Heightmap.Biome.Ocean, "Ashlands_SeaStorm"), "🌪" },

			// Mistlands
			{ (Heightmap.Biome.Mistlands, "Mistlands_clear"), "☀️" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_rain"), "🌧" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_thunder"), "⛈" },

			// AshLands
			{ (Heightmap.Biome.AshLands, "Ashlands_ashrain"), "☀️" },  // 🔥 
			{ (Heightmap.Biome.AshLands, "Ashlands_misty"), "🌫" },
			{ (Heightmap.Biome.AshLands, "Ashlands_CinderRain"), "🌋" }, // cinder rain
			{ (Heightmap.Biome.AshLands, "Ashlands_storm"), "🌪" }
		};*/

		[HarmonyPatch(typeof(EnvMan), "Update")]
		class TimeAndDay_EnvManPatch
		{
			private static void Prefix(EnvMan __instance, ref float ___m_smoothDayFraction)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.TimeAndDayChoice.Value != TimeAndDayMode.Off)
				{
					CurrentDay = Traverse.Create((object)EnvMan.instance).Method("GetCurrentDay", Array.Empty<object>()).GetValue<int>();

					if (ConfigManager.TimeAndDayChoice.Value == TimeAndDayMode.DayPhases)
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
					//TimeEmoji = GetEmojiForCurrentWeather(___m_smoothDayFraction);
					//UITimeEmojiColor = GetColorFromFraction(___m_smoothDayFraction);
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

			/*private static string GetEmojiFromFraction(float dayFraction)
			{
				if (dayFraction < 0.20f) return "🌙";
				if (dayFraction < 0.25f) return "🌅";
				if (dayFraction < 0.33f) return "🌅";
				if (dayFraction < 0.50f) return "☀️";
				if (dayFraction < 0.66f) return "☀️";
				if (dayFraction < 0.75f) return "🌄"; // 🌤
				if (dayFraction < 0.80f) return "🌄";
				return "🌙";
			}*/

			/*private static Color GetColorFromFraction(float dayFraction)
			{
				if (dayFraction < 0.20f) return Color.white;
				if (dayFraction < 0.25f) return new Color(1f, 0.549019f, 0f);
				if (dayFraction < 0.33f) return Color.yellow;
				if (dayFraction < 0.50f) return Color.green;
				if (dayFraction < 0.66f) return Color.green;
				if (dayFraction < 0.75f) return Color.yellow;
				if (dayFraction < 0.80f) return new Color(1f, 0.549019f, 0f);
				return Color.white;
			}*/

			/*private static string GetEmojiForCurrentWeather(float dayFraction)
			{
				var envMan = EnvMan.instance;
				if (envMan == null)
					return "❓";

				var currentEnvField = typeof(EnvMan).GetField("m_currentEnv", BindingFlags.NonPublic | BindingFlags.Instance);
				if (currentEnvField == null)
					return "❓";

				var currentEnv = currentEnvField.GetValue(envMan) as EnvSetup;
				if (currentEnv == null)
					return "❓";

				// Use the biome directly from EnvMan
				var currentBiomeField = typeof(EnvMan).GetField("m_currentBiome", BindingFlags.NonPublic | BindingFlags.Instance);
				Heightmap.Biome biome = Heightmap.Biome.Meadows; // fallback
				if (currentBiomeField != null)
					biome = (Heightmap.Biome)currentBiomeField.GetValue(envMan);

				string emoji = WeatherEmojis.TryGetValue((biome, currentEnv.m_name), out var e) ? e : "❓";

				if (emoji == "☀️") // only override for clear-weather types
					emoji = GetEmojiFromFraction(dayFraction);

				return emoji ?? "❓";
			}*/
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		class InventoryWeightAndSlots_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.TimeAndDayChoice.Value != TimeAndDayMode.Off)
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

				if (ConfigManager.TimeAndDayChoice.Value != TimeAndDayMode.Off)
				{
					bool newUI = ConfigManager.UILayoutChoice.Value == UIMode.New;

					CreateUI(__instance); // Create UI if missing

					// Handle where to display the time text when toggling
					/*if (ConfigManager.UILayoutChoice.Value != lastUImode)
					{
						lastUImode = ConfigManager.UILayoutChoice.Value;
						UpdateTimePosition(newUI);
					}*/

					bool showTimeUI = Game.m_noMap ? ShowUI : ShowUI && Minimap.instance != null && Minimap.instance.m_mapSmall != null && Minimap.instance.m_mapSmall.activeInHierarchy;

					UITimeText.enabled = showTimeUI;
					UIDayText.enabled = showTimeUI;
					//UITimeEmojiTMP.enabled = newUI ? showTimeUI : false;

					if (showTimeUI)
					{
						UITimeText.color = GetColorFromString(TimeString);
						UIDayText.color = Color.white;
						//UITimeEmojiTMP.color = UITimeEmojiColor;

						UITimeText.text = TimeString;
						UIDayText.text = "Day " + CurrentDay.ToString();
						//UITimeEmojiTMP.text = TimeEmoji;
					}
				}
				else
				{
					if (UITimeText != null)
						UITimeText.enabled = false;
					if (UIDayText != null)
						UIDayText.enabled = false;
					//if (UITimeEmojiTMP != null)
						//UITimeEmojiTMP.enabled = false;
				}
			}

			/*private static void UpdateTimePosition(bool newUI)
			{
				float xOffset = newUI ? 20f : 40f;
				UITimeText.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, 0f);
			}*/

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
			if (UITimeText != null && UIDayText != null /*&& UITimeEmojiTMP != null*/)
				return;  // UI already exists, no need to create again

			//bool newUI = ConfigManager.UILayoutChoice.Value == UIMode.New;

			int UITextFontSize = 16;
			string UITextFontName = "AveriaSansLibre-Bold";
			//string UIEmojiFontName = "NotoEmoji-Regular SDF"; // NotoEmoji-Regular
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
			//float timeTextXPos = newUI ? 20f : 40f;
			float timeTextXPos = 40f;

			// Time text
			UITimeText = CreateTextObject("TimeText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(timeTextXPos, 0f), UITimeAreaSize);

			// Day text
			UIDayText = CreateTextObject("DayText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(-40f, 0f), UITimeAreaSize);

			// Time emoji
			//UITimeEmojiTMP = CreateTMPTextObject("TimeEmojiTMP", UITimeArea, Color.white, UIEmojiFontName, UITextFontSize + 2, TextAlignmentOptions.MidlineRight, new Vector2(80f, 0f), UITimeAreaEmojiSize, log);
		}
	}
}
