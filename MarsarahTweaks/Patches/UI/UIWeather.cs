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
	internal class UIWeather : UIController
	{
		private static readonly LogManager log = new LogManager("UI Weather", LogManager.LogLevel.Info);

		// UI data
		private static string TimeEmoji;

		// UI elements
		private static TMPro.TextMeshProUGUI UITimeEmojiTMP = null;
		private static Color UITimeEmojiColor;

		private static readonly Dictionary<(Heightmap.Biome, string), string> WeatherEmojis = new Dictionary<(Heightmap.Biome, string), string>()
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
		};

		[HarmonyPatch(typeof(EnvMan), "Update")]
		class Weather_EnvManPatch
		{
			private static void Prefix(EnvMan __instance, ref float ___m_smoothDayFraction, ref Heightmap.Biome ___m_currentBiome, ref EnvSetup ___m_currentEnv)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowWeatherIndicator.Value == true)
				{
					TimeEmoji = GetEmojiForCurrentWeather(___m_smoothDayFraction, ___m_currentBiome, ___m_currentEnv);
					UITimeEmojiColor = GetColorFromFraction(___m_smoothDayFraction);
				}
			}

			private static string GetEmojiForCurrentWeather(float dayFraction, Heightmap.Biome currentBiome, EnvSetup currentEnv)
			{
				var envMan = EnvMan.instance;
				if (envMan == null)
					return "❓";

				/*var currentEnvField = typeof(EnvMan).GetField("m_currentEnv", BindingFlags.NonPublic | BindingFlags.Instance);
				if (currentEnvField == null)
					return "❓";

				var currentEnv = currentEnvField.GetValue(envMan) as EnvSetup;*/
				if (currentEnv == null)
					return "❓";

				// Use the biome directly from EnvMan
				/*var currentBiomeField = typeof(EnvMan).GetField("m_currentBiome", BindingFlags.NonPublic | BindingFlags.Instance);
				Heightmap.Biome biome = Heightmap.Biome.Meadows; // fallback
				if (currentBiomeField != null)
					biome = (Heightmap.Biome)currentBiomeField.GetValue(envMan);*/

				string emoji = WeatherEmojis.TryGetValue((currentBiome, currentEnv.m_name), out var e) ? e : "❓";

				if (emoji == "☀️") // only override for clear-weather types
					emoji = GetEmojiFromFraction(dayFraction);

				return emoji ?? "❓";
			}

			private static string GetEmojiFromFraction(float dayFraction)
			{
				if (dayFraction < 0.20f) return "🌙";
				if (dayFraction < 0.25f) return "🌅";
				if (dayFraction < 0.33f) return "🌅";
				if (dayFraction < 0.50f) return "☀️";
				if (dayFraction < 0.66f) return "☀️";
				if (dayFraction < 0.75f) return "🌄"; // 🌤
				if (dayFraction < 0.80f) return "🌄";
				return "🌙";
			}

			private static Color GetColorFromFraction(float dayFraction)
			{
				if (dayFraction < 0.20f) return Color.white;
				if (dayFraction < 0.25f) return new Color(1f, 0.549019f, 0f);
				if (dayFraction < 0.33f) return Color.yellow;
				if (dayFraction < 0.50f) return Color.green;
				if (dayFraction < 0.66f) return Color.green;
				if (dayFraction < 0.75f) return Color.yellow;
				if (dayFraction < 0.80f) return new Color(1f, 0.549019f, 0f);
				return Color.white;
			}
		}

		[HarmonyPatch(typeof(Hud), "Awake")]
		class Weather_HUDAwakePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowWeatherIndicator.Value)
				{
					CreateUI(__instance);
				}
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		class Weather_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowWeatherIndicator.Value)
				{
					CreateUI(__instance);

					bool showWeatherUI = Game.m_noMap ? ShowUI : ShowUI && Minimap.instance != null && Minimap.instance.m_mapSmall != null && Minimap.instance.m_mapSmall.activeInHierarchy;
					UITimeEmojiTMP.enabled = showWeatherUI;

					if (showWeatherUI)
					{
						UITimeEmojiTMP.color = UITimeEmojiColor;
						UITimeEmojiTMP.text = TimeEmoji;
					}
				}
				else
				{
					if (UITimeEmojiTMP != null)
						UITimeEmojiTMP.enabled = false;
				}
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UITimeEmojiTMP != null)
				return;  // UI already exists, no need to create again

			int UITextFontSize = 16;
			//string UITextFontName = "AveriaSansLibre-Bold";
			string UIEmojiFontName = "NotoEmoji-Regular SDF"; // NotoEmoji-Regular
			//Vector2 UITimeAreaSize = new Vector2(200f, 30f); // width, height
			Vector2 UIWeatherAreaEmojiSize = new Vector2(30f, 30f); // width, height
			Vector2 UIWeatherAreaEmojiPosition = new Vector2(0f, 0f);

			// Weather area object
			GameObject UIWeatherArea = new GameObject("WeatherArea");
			UIWeatherArea.layer = 5;
			UIWeatherArea.transform.SetParent(hud.m_rootObject.transform);
			RectTransform timeAreaTransform = UIWeatherArea.AddComponent<RectTransform>();
			timeAreaTransform.anchorMin = new Vector2(1f, 1f);
			timeAreaTransform.anchorMax = new Vector2(1f, 1f);
			timeAreaTransform.anchoredPosition = new Vector2(-225f, -60f); // -140f, -25f
			timeAreaTransform.sizeDelta = UIWeatherAreaEmojiSize;
			UIWeatherArea.transform.localScale = Vector3.one;  // Ensure correct scale

			// Special modification for text sizeDelta
			//UITimeAreaSize.x = UITimeAreaSize.x / 2;
			//float timeTextXPos = newUI ? 20f : 40f;
			//float timeTextXPos = 40f;

			// Time text
			//UITimeText = CreateTextObject("TimeText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(timeTextXPos, 0f), UITimeAreaSize);

			// Day text
			//UIDayText = CreateTextObject("DayText", UITimeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(-40f, 0f), UITimeAreaSize);

			// Time emoji
			UITimeEmojiTMP = CreateTMPTextObject("TimeEmojiTMP", UIWeatherArea, Color.white, UIEmojiFontName, UITextFontSize + 2, TextAlignmentOptions.MidlineRight, UIWeatherAreaEmojiPosition, UIWeatherAreaEmojiSize, log);
		}
	}
}
