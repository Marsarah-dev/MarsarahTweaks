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
		//private static string UIWeatherEmoji;
		//private static Color UIWeatherEmojiColor;

		//private static string UIForecastEmoji;
		private static string UIForecastTimer;
		//private static Color UIForecastEmojiColor = Color.cyan;

		// UI elements
		//private static TMPro.TextMeshProUGUI UIWeatherEmojiTMP = null;
		//private static TMPro.TextMeshProUGUI UIForecastEmojiTMP = null;
		private static Text UINextWeatherTimerText = null;
		private static Image UIWeatherIcon = null;
		private static Image UIForecastIcon = null;

		// Cache
		private static EnvSetup _lastForecastEnv = null;
		private static string _lastForecastEmoji = "❓";
		private static long _lastForecastPeriod = -1;
		private static string _lastAvailableWeathersLog = string.Empty;

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

			// Plains
			{ (Heightmap.Biome.Plains, "Heath clear"), "☀️" },
			{ (Heightmap.Biome.Plains, "Misty"), "🌫" },
			{ (Heightmap.Biome.Plains, "LightRain"), "🌦" },

			// Mistlands
			{ (Heightmap.Biome.Mistlands, "Mistlands_clear"), "☀️" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_rain"), "🌧" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_thunder"), "⛈" },

			// AshLands
			{ (Heightmap.Biome.AshLands, "Ashlands_ashrain"), "☀️" },  // 🔥 
			{ (Heightmap.Biome.AshLands, "Ashlands_misty"), "🌫" },
			{ (Heightmap.Biome.AshLands, "Ashlands_CinderRain"), "🌋" }, // cinder rain
			{ (Heightmap.Biome.AshLands, "Ashlands_storm"), "🌪" },

			// DeepNorth
			{ (Heightmap.Biome.DeepNorth, "Twilight_SnowStorm"), "🌨️" },
			{ (Heightmap.Biome.DeepNorth, "Twilight_Snow"), "❄️" },
			{ (Heightmap.Biome.DeepNorth, "Twilight_Clear"), "☀️" },

			// Ocean
			{ (Heightmap.Biome.Ocean, "Clear"), "☀️" },
			{ (Heightmap.Biome.Ocean, "Rain"), "🌧" },
			{ (Heightmap.Biome.Ocean, "LightRain"), "🌦" },
			{ (Heightmap.Biome.Ocean, "Misty"), "🌫" },
			{ (Heightmap.Biome.Ocean, "ThunderStorm"), "⛈" },
			{ (Heightmap.Biome.Ocean, "Ashlands_SeaStorm"), "🌪" }
		};

		private static readonly Dictionary<(Heightmap.Biome, string), string> WeatherIcons = new Dictionary<(Heightmap.Biome, string), string>()
		{
			// Meadows
			{(Heightmap.Biome.Meadows, "Clear"), "Clear"},
			{(Heightmap.Biome.Meadows, "Rain"), "Rain"},
			{(Heightmap.Biome.Meadows, "Misty"), "Misty"},
			{(Heightmap.Biome.Meadows, "ThunderStorm"), "Thunderstorm"},
			{(Heightmap.Biome.Meadows, "LightRain"), "LightRain"},
			
			// BlackForest
			{ (Heightmap.Biome.BlackForest, "DeepForest Mist"), "Clear" },
			{ (Heightmap.Biome.BlackForest, "Rain"), "Rain" },
			{ (Heightmap.Biome.BlackForest, "Misty"), "misty" },
			{ (Heightmap.Biome.BlackForest, "ThunderStorm"), "Thunderstorm" },

			// Swamp
			{ (Heightmap.Biome.Swamp, "SwampRain"), "LightRain" },

			// Mountain
			{ (Heightmap.Biome.Mountain, "SnowStorm"), "Snowstorm" },
			{ (Heightmap.Biome.Mountain, "Snow"), "Snow" },

			// Plains
			{ (Heightmap.Biome.Plains, "Heath clear"), "Clear" },
			{ (Heightmap.Biome.Plains, "Misty"), "Misty" },
			{ (Heightmap.Biome.Plains, "LightRain"), "LightRain" },

			// Mistlands
			{ (Heightmap.Biome.Mistlands, "Mistlands_clear"), "Clear" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_rain"), "Rain" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_thunder"), "Thunderstorm" },

			// AshLands
			{ (Heightmap.Biome.AshLands, "Ashlands_ashrain"), "AshRain" },
			{ (Heightmap.Biome.AshLands, "Ashlands_misty"), "AshMist" },
			{ (Heightmap.Biome.AshLands, "Ashlands_CinderRain"), "AshCinderRain" },
			{ (Heightmap.Biome.AshLands, "Ashlands_storm"), "AshStorm" },

			// DeepNorth
			{ (Heightmap.Biome.DeepNorth, "Twilight_SnowStorm"), "Snowstorm" },
			{ (Heightmap.Biome.DeepNorth, "Twilight_Snow"), "Snow" },
			{ (Heightmap.Biome.DeepNorth, "Twilight_Clear"), "Clear" },

			// Ocean
			{ (Heightmap.Biome.Ocean, "Clear"), "Clear" },
			{ (Heightmap.Biome.Ocean, "Rain"), "Rain" },
			{ (Heightmap.Biome.Ocean, "LightRain"), "LightRain" },
			{ (Heightmap.Biome.Ocean, "Misty"), "Misty" },
			{ (Heightmap.Biome.Ocean, "ThunderStorm"), "Thunderstorm" },
			{ (Heightmap.Biome.Ocean, "Ashlands_SeaStorm"), "AshStorm" }
		};


		[HarmonyPatch(typeof(EnvMan), "Update")]
		class Weather_EnvManPatch
		{
			private static void Prefix(EnvMan __instance, ref float ___m_smoothDayFraction, ref Heightmap.Biome ___m_currentBiome, ref EnvSetup ___m_currentEnv, ref long ___m_environmentPeriod, ref double ___m_totalSeconds)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowWeatherIndicator.Value == true)
				{
					// Current weather
					//UIWeatherEmoji = GetEmojiForCurrentWeather(___m_smoothDayFraction, ___m_currentBiome, ___m_currentEnv);
					//UIWeatherEmojiColor = GetColorFromFraction(___m_smoothDayFraction);
					UpdateCurrentWeather(__instance, ___m_currentEnv, ___m_currentBiome);

					// Forecast
					UpdateForecastData(__instance, ___m_currentEnv, ___m_currentBiome, ___m_environmentPeriod, ___m_totalSeconds);
				}
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
					//UIWeatherEmojiTMP.enabled = showWeatherUI;
					//UIForecastEmojiTMP.enabled = showWeatherUI;
					UIWeatherIcon.enabled = showWeatherUI;
					UIForecastIcon.enabled = showWeatherUI;
					UINextWeatherTimerText.enabled = showWeatherUI;

					if (showWeatherUI)
					{
						//UIWeatherEmojiTMP.text = UIWeatherEmoji;
						//UIWeatherEmojiTMP.color = UIWeatherEmojiColor;

						//UIForecastEmojiTMP.text = UIForecastEmoji;
						//UIForecastEmojiTMP.color = UIForecastEmojiColor;

						UINextWeatherTimerText.text = UIForecastTimer;
						UINextWeatherTimerText.color = Color.white;
					}
				}
				else
				{
					//if (UIWeatherEmojiTMP != null)
						//UIWeatherEmojiTMP.enabled = false;

					//if (UIForecastEmojiTMP != null)
						//UIForecastEmojiTMP.enabled = false;

					if (UINextWeatherTimerText != null)
						UINextWeatherTimerText.enabled = false;

					if (UIWeatherIcon != null)
					{
						UIWeatherIcon.sprite = null;
						UIWeatherIcon.enabled = false;
					}

					if (UIForecastIcon != null)
					{
						UIForecastIcon.sprite = null;
						UIForecastIcon.enabled = false;
					}
				}
			}
		}

		private static void CreateUI(Hud hud)
		{
			if (UIWeatherIcon != null && UIForecastIcon != null && UINextWeatherTimerText != null)
				return;  // UI already exists

			int UITextFontSize = 16;
			//string UIEmojiFontName = "NotoEmoji-Regular SDF";
			string UITextFontName = "AveriaSansLibre-Bold";
			Vector2 UIWeatherAreaSize = new Vector2(200f, 40f); // wider, since it holds all
			Vector2 UIWeatherAreaPos = new Vector2(-130f, -220f); // bottom-right corner

			// Parent container for the widget
			GameObject UIWeatherWidgetArea = new GameObject("WeatherWidgetArea");
			UIWeatherWidgetArea.layer = 5;
			UIWeatherWidgetArea.transform.SetParent(hud.m_rootObject.transform);
			RectTransform widgetTransform = UIWeatherWidgetArea.AddComponent<RectTransform>();
			widgetTransform.anchorMin = new Vector2(1f, 1f);
			widgetTransform.anchorMax = new Vector2(1f, 1f); 
			widgetTransform.anchoredPosition = UIWeatherAreaPos;
			widgetTransform.sizeDelta = UIWeatherAreaSize;
			UIWeatherWidgetArea.transform.localScale = Vector3.one;

			// --- Weather icon ---
			UIWeatherIcon = CreateUIImageObject("WeatherIcon", UIWeatherWidgetArea, new Vector2(0f, 0f), new Vector2(30f, 30f));

			// --- Current weather emoji ---
			//UIWeatherEmojiTMP = CreateTMPTextObject("CurrentWeatherTMP", UIWeatherWidgetArea, Color.white, UIEmojiFontName, UITextFontSize + 2, TextAlignmentOptions.MidlineLeft, new Vector2(0f, 0f), new Vector2(30f, 30f), log);

			// --- Forecast icon ---
			UIForecastIcon = CreateUIImageObject("ForecastWeatherIcon", UIWeatherWidgetArea, new Vector2(25f, 0f), new Vector2(30f, 30f));

			// --- Forecast emoji ---
			//UIForecastEmojiTMP = CreateTMPTextObject("ForecastWeatherTMP", UIWeatherWidgetArea, Color.white, UIEmojiFontName, UITextFontSize + 2, TextAlignmentOptions.MidlineLeft, new Vector2(25f, 0f), new Vector2(30f, 30f), log);

			// --- Timer text ---
			UINextWeatherTimerText = CreateTextObject("WeatherTimerTMP", UIWeatherWidgetArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(80f, 0f), new Vector2(80f, 30f));
		}

		private static void UpdateCurrentWeather(EnvMan envMan, EnvSetup currentEnv, Heightmap.Biome biome)
		{
			if (envMan == null || currentEnv == null) return;

			// Pick icon
			Sprite iconSprite = null;
			if (WeatherIcons.TryGetValue((biome, currentEnv.m_name), out var iconKey))
			{
				string resourcePath = $"MarsarahTweaks.Assets.Icons.Weather.{iconKey}.png";
				iconSprite = IconManager.LoadEmbeddedIcon(resourcePath);
			}

			// Update UI element
			if (UIWeatherIcon != null)
			{
				UIWeatherIcon.sprite = iconSprite;
				UIWeatherIcon.enabled = iconSprite != null;
			}
		}

		private static void UpdateForecastData(EnvMan envMan, EnvSetup currentEnv, Heightmap.Biome biome, long currentEnvironmentPeriod, double totalSeconds)
		{
			if (envMan == null || currentEnv == null)
			{
				//UIForecastEmoji = "❓";
				UIForecastTimer = "--:--";
				return;
			}

			// Player position → biome context
			Vector3 position = Vector3.zero;
			if (Player.m_localPlayer != null)
				position = Player.m_localPlayer.transform.position;

			bool isAshlands = WorldGenerator.IsAshlands(position.x, position.z);
			bool isDeepNorth = WorldGenerator.IsDeepnorth(position.x, position.z);

			EnvSetup forecastEnv = null;
			long forecastPeriod = -1;

			// Look ahead up to 50 periods
			for (int i = 1; i <= 50; i++)
			{
				long periodToCheck = currentEnvironmentPeriod + i;
				EnvSetup nextEnv = GetEnvironment(periodToCheck, biome, isAshlands, isDeepNorth);

				if (nextEnv != null && nextEnv.m_name != currentEnv.m_name)
				{
					forecastEnv = nextEnv;
					forecastPeriod = periodToCheck;
					break;
				}
			}

			if (forecastEnv == null)
			{
				// Handle static-weather biomes gracefully
				var availableEnvironments = Traverse.Create(envMan).Method("GetAvailableEnvironments", new object[] { biome }).GetValue<List<EnvEntry>>();

				if (EnvMan.instance != null && availableEnvironments != null && availableEnvironments.Count <= 1)
				{
					// Only one weather -> show current one again with 00:00
					//UIForecastEmoji = WeatherEmojis.TryGetValue((biome, currentEnv.m_name), out var env) ? env : "❓";
					Sprite iconSpriteConstant = null;
					if (WeatherIcons.TryGetValue((biome, currentEnv.m_name), out var iconKeyConstant))
					{
						string resourcePath = $"MarsarahTweaks.Assets.Icons.Weather.{iconKeyConstant}.png";
						iconSpriteConstant = IconManager.LoadEmbeddedIcon(resourcePath);
					}
					if (UIForecastIcon != null)
					{
						UIForecastIcon.sprite = iconSpriteConstant;
						UIForecastIcon.enabled = iconSpriteConstant != null;
					}
					UIForecastTimer = "00:00";
					//UIForecastEmojiColor = new Color(0.6f, 0.8f, 0.6f);
				}
				else
				{
					//UIForecastEmoji = "❓";
					UIForecastTimer = "--:--";
				}
				return;
			}

			// pick emoji
			/*string emoji = WeatherEmojis.TryGetValue((biome, forecastEnv.m_name), out var e) ? e : "❓";
			string timerStr = GetNextWeatherTimer(forecastPeriod, totalSeconds);

			// assign globals
			UIForecastEmoji = emoji;
			UIForecastTimer = timerStr;
			UIForecastEmojiColor = Color.cyan;*/

			// pick emoji
			string emoji = WeatherEmojis.TryGetValue((biome, forecastEnv.m_name), out var e) ? e : "❓";
			string timerStr = GetNextWeatherTimer(forecastPeriod, totalSeconds);

			// pick icons
			Sprite iconSprite = null;
			if (WeatherIcons.TryGetValue((biome, forecastEnv.m_name), out var iconKey))
			{
				string resourcePath = $"MarsarahTweaks.Assets.Icons.Weather.{iconKey}.png";
				iconSprite = IconManager.LoadEmbeddedIcon(resourcePath);
			}

			// assign globals
			//UIForecastEmoji = emoji;
			UIForecastTimer = timerStr;
			//UIForecastEmojiColor = Color.cyan;

			// Update UI element
			if (UIForecastIcon != null)
			{
				UIForecastIcon.sprite = iconSprite;
				UIForecastIcon.enabled = iconSprite != null;
			}
			/*if (UIForecastEmojiTMP != null)
			{
				UIForecastEmojiTMP.text = emoji; // optional fallback
				UIForecastEmojiTMP.enabled = iconSprite == null;
			}*/

			// log only when forecast changes
			if (forecastEnv != _lastForecastEnv || forecastPeriod != _lastForecastPeriod)
			{
				// Collect all lookahead environments for debug
				StringBuilder sequenceLog = new StringBuilder();
				sequenceLog.AppendLine("Next 50 forecast environments:");

				for (int i = 1; i <= 50; i++)
				{
					long periodToCheck = currentEnvironmentPeriod + i;
					EnvSetup nextEnv = GetEnvironment(periodToCheck, biome, isAshlands, isDeepNorth);

					string nextName = nextEnv != null ? nextEnv.m_name : "null";
					sequenceLog.AppendLine($"  +{i,2} → {nextName}");
				}

				log.Info(sequenceLog.ToString());

				log.Info($"Next forecast: {forecastEnv.m_name} ({emoji}), ETA {timerStr}");
				_lastForecastEnv = forecastEnv;
				_lastForecastEmoji = emoji;
				_lastForecastPeriod = forecastPeriod;
			}
		}

		private static EnvSetup GetEnvironment(long period, Heightmap.Biome biome, bool isAshlands, bool isDeepNorth)
		{
			// Save RNG state
			UnityEngine.Random.State state = UnityEngine.Random.state;

			// Deterministic seed
			UnityEngine.Random.InitState((int)period);

			EnvSetup result = null;

			try
			{
				var envMan = EnvMan.instance;
				if (envMan != null)
				{
					// Get all the available weathers for given biome
					var availableEnvironments = Traverse.Create(envMan).Method("GetAvailableEnvironments", new object[] { biome }).GetValue<List<EnvEntry>>();

					if (availableEnvironments != null && availableEnvironments.Count > 0)
					{
						// Calculate total weight
						float totalWeight = availableEnvironments
							.Where(e => e != null && e.m_env != null)
							.Sum(e => e.m_weight);

						// Build debug log for available weathers
						StringBuilder envListLog = new StringBuilder();
						envListLog.AppendLine($"Available weathers for biome {biome}:");

						foreach (var entry in availableEnvironments)
						{
							if (entry == null || entry.m_env == null)
								continue;

							string name = entry.m_env.m_name;
							float weight = entry.m_weight;
							float probability = totalWeight > 0 ? (weight / totalWeight) * 100f : 0f;
							bool ashlands = entry.m_ashlandsOverride;
							bool deepnorth = entry.m_deepnorthOverride;

							envListLog.AppendLine($"  - {name} (weight={weight}, probability={probability:F1}%)");
						}

						string logStr = envListLog.ToString();

						// Only log once per forecast change (avoid spam)
						if (logStr != _lastAvailableWeathersLog)
						{
							log.Info(logStr);
							_lastAvailableWeathersLog = logStr;
						}
					}

					if (availableEnvironments != null && availableEnvironments.Count > 0)
					{
						// From the list of available weathers, select one based on weights
						result = Traverse.Create(envMan).Method("SelectWeightedEnvironment", new object[] { availableEnvironments }).GetValue<EnvSetup>();

						// Apply Ashlands / DeepNorth overrides
						foreach (var entry in availableEnvironments)
						{
							if (entry == null) continue;

							if (entry.m_ashlandsOverride && isAshlands)
							{
								result = entry.m_env;
							}
							if (entry.m_deepnorthOverride && isDeepNorth)
							{
								result = entry.m_env;
							}
						}
					}
				}
			}
			finally
			{
				// Restore RNG state
				UnityEngine.Random.state = state;
			}

			return result;
		}

		// Returns a timer string until the given forecast period occurs.
		private static string GetNextWeatherTimer(long forecastPeriod, double totalSecondsToNow)
		{
			var envMan = EnvMan.instance;
			if (envMan == null) return "";

			// Period length in seconds (derived from EnvMan constant)
			float periodLength = envMan.m_environmentDuration;
			double forecastTime = forecastPeriod * periodLength;

			if (forecastTime <= totalSecondsToNow)
				return "0:00";

			double secondsLeft = forecastTime - totalSecondsToNow;
			TimeSpan ts = TimeSpan.FromSeconds(secondsLeft);

			if (ts.TotalHours >= 1.0)
				return $"{(int)ts.TotalHours}:{ts.Minutes:D2}h";
			return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
		}

		private static string GetEmojiForCurrentWeather(float dayFraction, Heightmap.Biome currentBiome, EnvSetup currentEnv)
		{
			var envMan = EnvMan.instance;
			if (envMan == null)
				return "❓";

			if (currentEnv == null)
				return "❓";

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
}
