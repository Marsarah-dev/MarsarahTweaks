using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MarsarahTweaks.Managers.ConfigManager;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIWeather : UIController
	{
		private static readonly LogManager log = new LogManager("UI Weather", LogManager.LogLevel.Warning);

		// UI data
		private static string UIForecastTimer;

		// UI elements
		private static Text UINextWeatherTimerText = null;
		//private static Image UIWeatherIcon = null;
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
			{ (Heightmap.Biome.Meadows, "Heath clear"), "☀️" }, // Seasons
			{ (Heightmap.Biome.Meadows, "SwampRain"), "🌧" },  // Seasons
			{ (Heightmap.Biome.Meadows, "Snow"), "❄️" },        // Seasons
			{ (Heightmap.Biome.Meadows, "SnowStorm"), "🌨️" },  // Seasons

			// BlackForest
			{ (Heightmap.Biome.BlackForest, "DeepForest Mist"), "🌫" },
			{ (Heightmap.Biome.BlackForest, "Rain"), "🌧" },
			{ (Heightmap.Biome.BlackForest, "Misty"), "🌫" },
			{ (Heightmap.Biome.BlackForest, "ThunderStorm"), "⛈" },
			{ (Heightmap.Biome.BlackForest, "Clear"), "☀️" },      // Seasons
			{ (Heightmap.Biome.BlackForest, "LightRain"), "🌦" }, // Seasons
			{ (Heightmap.Biome.BlackForest, "SwampRain"), "🌧" }, // Seasons
			{ (Heightmap.Biome.BlackForest, "Snow"), "❄️" },       // Seasons
			{ (Heightmap.Biome.BlackForest, "SnowStorm"), "🌨️" }, // Seasons


			// Swamp
			{ (Heightmap.Biome.Swamp, "SwampRain"), "🌧" },
			{ (Heightmap.Biome.Swamp, "ThunderStorm"), "⛈" }, // Seasons
			{ (Heightmap.Biome.Swamp, "Snow"), "❄️" },          // Seasons
			{ (Heightmap.Biome.Swamp, "SnowStorm"), "🌨️" },    // Seasons

			// Mountain
			{ (Heightmap.Biome.Mountain, "SnowStorm"), "🌨️" },
			{ (Heightmap.Biome.Mountain, "Snow"), "❄️" },
			{ (Heightmap.Biome.Mountain, "Clear"), "☀️" }, // Seasons

			// Plains
			{ (Heightmap.Biome.Plains, "Heath clear"), "☀️" },
			{ (Heightmap.Biome.Plains, "Misty"), "🌫" },
			{ (Heightmap.Biome.Plains, "LightRain"), "🌦" },
			{ (Heightmap.Biome.Plains, "Rain"), "🌧" },         // Seasons
			{ (Heightmap.Biome.Plains, "ThunderStorm"), "⛈" }, // Seasons
			{ (Heightmap.Biome.Plains, "SwampRain"), "🌧" },    // Seasons
			{ (Heightmap.Biome.Plains, "Snow"), "❄️" },          // Seasons
			{ (Heightmap.Biome.Plains, "SnowStorm"), "🌨️" },    // Seasons

			// Mistlands
			{ (Heightmap.Biome.Mistlands, "Mistlands_clear"), "☀️" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_rain"), "🌧" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_thunder"), "⛈" },
			{ (Heightmap.Biome.Mistlands, "Heath clear"), "☀️" },      // Seasons
			{ (Heightmap.Biome.Mistlands, "DeepForest Mist"), "🌫" }, // or ☀️ for "clear-ish" // Seasons
			{ (Heightmap.Biome.Mistlands, "SwampRain"), "🌧" },       // Seasons
			{ (Heightmap.Biome.Mistlands, "Snow"), "❄️" },             // Seasons
			{ (Heightmap.Biome.Mistlands, "SnowStorm"), "🌨️" },       // Seasons

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
			{ (Heightmap.Biome.Ocean, "Ashlands_SeaStorm"), "🌪" },
			{ (Heightmap.Biome.Ocean, "Snow"), "❄️" },      // Seasons
			{ (Heightmap.Biome.Ocean, "SnowStorm"), "🌨️" } // Seasons
		};

		private static readonly Dictionary<(Heightmap.Biome, string), string> WeatherIcons = new Dictionary<(Heightmap.Biome, string), string>()
		{
			// Meadows
			{ (Heightmap.Biome.Meadows, "Clear"), "Clear"},
			{ (Heightmap.Biome.Meadows, "Rain"), "Rain"},
			{ (Heightmap.Biome.Meadows, "Misty"), "Misty"},
			{ (Heightmap.Biome.Meadows, "ThunderStorm"), "Thunderstorm"},
			{ (Heightmap.Biome.Meadows, "LightRain"), "LightRain"},
			{ (Heightmap.Biome.Meadows, "Heath clear"), "Clear" },   // Seasons
			{ (Heightmap.Biome.Meadows, "SwampRain"), "LightRain" }, // Seasons
			{ (Heightmap.Biome.Meadows, "Snow"), "Snow" },           // Seasons
			{ (Heightmap.Biome.Meadows, "SnowStorm"), "Snowstorm" }, // Seasons

			
			// BlackForest
			{ (Heightmap.Biome.BlackForest, "DeepForest Mist"), "Clear" },
			{ (Heightmap.Biome.BlackForest, "Rain"), "Rain" },
			{ (Heightmap.Biome.BlackForest, "Misty"), "Misty" },
			{ (Heightmap.Biome.BlackForest, "ThunderStorm"), "Thunderstorm" },
			{ (Heightmap.Biome.BlackForest, "Clear"), "Clear" },         // Seasons
			{ (Heightmap.Biome.BlackForest, "LightRain"), "LightRain" }, // Seasons
			{ (Heightmap.Biome.BlackForest, "SwampRain"), "LightRain" }, // Seasons
			{ (Heightmap.Biome.BlackForest, "Snow"), "Snow" },           // Seasons
			{ (Heightmap.Biome.BlackForest, "SnowStorm"), "Snowstorm" }, // Seasons


			// Swamp
			{ (Heightmap.Biome.Swamp, "SwampRain"), "LightRain" },
			{ (Heightmap.Biome.Swamp, "ThunderStorm"), "Thunderstorm" }, // Seasons
			{ (Heightmap.Biome.Swamp, "Snow"), "Snow" },                 // Seasons
			{ (Heightmap.Biome.Swamp, "SnowStorm"), "Snowstorm" },       // Seasons

			// Mountain
			{ (Heightmap.Biome.Mountain, "SnowStorm"), "Snowstorm" },
			{ (Heightmap.Biome.Mountain, "Snow"), "Snow" },
			{ (Heightmap.Biome.Mountain, "Clear"), "Clear" }, // Seasons

			// Plains
			{ (Heightmap.Biome.Plains, "Heath clear"), "Clear" },
			{ (Heightmap.Biome.Plains, "Misty"), "Misty" },
			{ (Heightmap.Biome.Plains, "LightRain"), "LightRain" },
			{ (Heightmap.Biome.Plains, "Rain"), "Rain" },                 // Seasons
			{ (Heightmap.Biome.Plains, "ThunderStorm"), "Thunderstorm" }, // Seasons
			{ (Heightmap.Biome.Plains, "SwampRain"), "LightRain" },       // Seasons
			{ (Heightmap.Biome.Plains, "Snow"), "Snow" },                 // Seasons
			{ (Heightmap.Biome.Plains, "SnowStorm"), "Snowstorm" },       // Seasons

			// Mistlands
			{ (Heightmap.Biome.Mistlands, "Mistlands_clear"), "Clear" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_rain"), "Rain" },
			{ (Heightmap.Biome.Mistlands, "Mistlands_thunder"), "Thunderstorm" },
			{ (Heightmap.Biome.Mistlands, "Heath clear"), "Clear" },     // Seasons
			{ (Heightmap.Biome.Mistlands, "DeepForest Mist"), "Clear" }, // Seasons
			{ (Heightmap.Biome.Mistlands, "SwampRain"), "LightRain" },   // Seasons
			{ (Heightmap.Biome.Mistlands, "Snow"), "Snow" },             // Seasons
			{ (Heightmap.Biome.Mistlands, "SnowStorm"), "Snowstorm" },   // Seasons

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
			{ (Heightmap.Biome.Ocean, "Ashlands_SeaStorm"), "AshStorm" },
			{ (Heightmap.Biome.Ocean, "Snow"), "Snow" },           // Seasons
			{ (Heightmap.Biome.Ocean, "SnowStorm"), "Snowstorm" }  // Seasons
		};


		[HarmonyPatch(typeof(EnvMan), "Update")]
		class Weather_EnvManPatch
		{
			private static void Prefix(EnvMan __instance, ref float ___m_smoothDayFraction, ref EnvSetup ___m_currentEnv, ref long ___m_environmentPeriod, ref double ___m_totalSeconds)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;
				if (__instance == null) return;
				if (!ShouldShowWeatherUI()) return;

				if (ConfigManager.ShowWeatherForecast.Value == true)
				{
					// Current weather
					//UpdateCurrentWeather(__instance, ___m_currentEnv, ___m_currentBiome);

					// Forecast
					UpdateForecastData(__instance, ___m_currentEnv, ___m_environmentPeriod, ___m_totalSeconds);
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

				if (ConfigManager.ShowWeatherForecast.Value)
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

				if (ConfigManager.ShowWeatherForecast.Value)
				{
					CreateUI(__instance);

					bool showWeatherUI = ShouldShowWeatherUI();
					//UIWeatherIcon.enabled = showWeatherUI;
					UIForecastIcon.enabled = showWeatherUI && UIForecastIcon.sprite != null;
					UINextWeatherTimerText.enabled = showWeatherUI;

					if (showWeatherUI)
					{
						UINextWeatherTimerText.text = UIForecastTimer;
						UINextWeatherTimerText.color = Color.white;
					}
				}
				else
				{
					if (UINextWeatherTimerText != null)
						UINextWeatherTimerText.enabled = false;

					/*if (UIWeatherIcon != null)
					{
						UIWeatherIcon.sprite = null;
						UIWeatherIcon.enabled = false;
					}*/

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
			if (/*UIWeatherIcon != null && */UIForecastIcon != null && UINextWeatherTimerText != null)
				return;  // UI already exists

			int UITextFontSize = 16;
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
			//UIWeatherIcon = CreateUIImageObject("WeatherIcon", UIWeatherWidgetArea, new Vector2(0f, 0f), new Vector2(30f, 30f));

			// --- Forecast icon ---
			UIForecastIcon = CreateUIImageObject("ForecastWeatherIcon", UIWeatherWidgetArea, new Vector2(25f, 0f), new Vector2(30f, 30f));

			// --- Timer text ---
			UINextWeatherTimerText = CreateTextObject("WeatherTimerTMP", UIWeatherWidgetArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleLeft, new Vector2(80f, 0f), new Vector2(80f, 30f));
		}

		/*private static void UpdateCurrentWeather(EnvMan envMan, EnvSetup currentEnv, Heightmap.Biome biome)
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
		}*/

		private static void UpdateForecastData(EnvMan envMan, EnvSetup currentEnv, long currentEnvironmentPeriod, double totalSeconds)
		{
			if (envMan == null || currentEnv == null)
			{
				UIForecastTimer = "--:--";
				return;
			}

			// Player position → biome context
			Vector3 position = Vector3.zero;
			if (Player.m_localPlayer != null)
				position = Player.m_localPlayer.transform.position;

			Heightmap.Biome currentBiome = Heightmap.Biome.None;
			if (Player.m_localPlayer != null)
			{
				currentBiome = Player.m_localPlayer.GetCurrentBiome();
			}

			BiomeSector biomeSector = WorldGenerator.instance != null ? WorldGenerator.instance.GetBiomeSector(position, false) : null;

			if (biomeSector == null)
			{
				log.Warn("Could not determine biome sector for weather forecast.");
				UIForecastTimer = "--:--";
				return;
			}

			bool isAshlands = WorldGenerator.IsAshlands(position.x, position.z);
			bool isDeepNorth = WorldGenerator.IsDeepnorth(position.x, position.z);

			EnvSetup forecastEnv = null;
			long forecastPeriod = -1;

			// Look ahead up to 50 periods
			for (int i = 1; i <= 50; i++)
			{
				long periodToCheck = currentEnvironmentPeriod + i;
				EnvSetup nextEnv = GetEnvironment(periodToCheck, biomeSector, isAshlands, isDeepNorth);

				if (nextEnv != null && nextEnv.m_name != currentEnv.m_name)
				{
					forecastEnv = nextEnv;
					forecastPeriod = periodToCheck;
					break;
				}
			}

			if (forecastEnv == null)
			{
				// No upcoming weather change within 50 periods
				// → show the current environment icon and a neutral timer ("--:--")

				Sprite iconSpriteCurrent = null;
				string currentNameNormalized = NormalizeWeatherName(currentBiome, currentEnv.m_name);
				if (WeatherIcons.TryGetValue((currentBiome, currentNameNormalized), out var iconKeyCurrent))
				{
					string resourcePath = $"MarsarahTweaks.Assets.Icons.Weather.{iconKeyCurrent}.png";
					iconSpriteCurrent = IconManager.LoadEmbeddedIcon(resourcePath);
				}

				if (UIForecastIcon != null)
				{
					UIForecastIcon.sprite = iconSpriteCurrent;
					UIForecastIcon.enabled = iconSpriteCurrent != null;
				}

				UIForecastTimer = "--:--";
				return;
			}

			string normalizedForecastName = NormalizeWeatherName(currentBiome, forecastEnv.m_name);

			// pick emoji
			string emoji = WeatherEmojis.TryGetValue((currentBiome, normalizedForecastName), out var e) ? e : "❓";
			string timerStr = GetNextWeatherTimer(forecastPeriod, totalSeconds);

			// pick icons
			Sprite iconSprite = null;
			if (WeatherIcons.TryGetValue((currentBiome, normalizedForecastName), out var iconKey))
			{
				string resourcePath = $"MarsarahTweaks.Assets.Icons.Weather.{iconKey}.png";
				iconSprite = IconManager.LoadEmbeddedIcon(resourcePath);

				if (iconSprite == null)
				{
					log.Warn($"Failed to load weather icon '{resourcePath}'.");
				}
			}
			else
			{
				log.Info($"No weather icon mapping for biome '{currentBiome}' and environment '{normalizedForecastName}'.");
			}

			// assign globals
			UIForecastTimer = timerStr;

			// Update UI element
			if (UIForecastIcon != null)
			{
				UIForecastIcon.sprite = iconSprite;
				UIForecastIcon.enabled = iconSprite != null;
			}

			// log only when forecast changes
			if (forecastEnv != _lastForecastEnv || forecastPeriod != _lastForecastPeriod)
			{
				// Current weather emoji (for summary log)
				string normalizedCurrentName = NormalizeWeatherName(currentBiome, currentEnv.m_name);
				string currentEmoji = WeatherEmojis.TryGetValue((currentBiome, normalizedCurrentName), out var curE) ? curE : "❓";

				// Collect all lookahead environments for debug
				StringBuilder sequenceLog = new StringBuilder();
				sequenceLog.AppendLine("Next 50 forecast environments:");

				for (int i = 1; i <= 50; i++)
				{
					long periodToCheck = currentEnvironmentPeriod + i;
					EnvSetup nextEnv = GetEnvironment(periodToCheck, biomeSector, isAshlands, isDeepNorth);

					if (nextEnv == null)
					{
						sequenceLog.AppendLine($"  +{i,2} → null (normalized: -, emoji=❓)");
						continue;
					}

					string rawName = nextEnv.m_name;
					string normalizedNextName = NormalizeWeatherName(currentBiome, rawName);
					string emojiNext = WeatherEmojis.TryGetValue((currentBiome, normalizedNextName), out var em) ? em : "❓";

					sequenceLog.AppendLine(
						$"  +{i,2} → {rawName} (normalized: {normalizedNextName}, emoji={emojiNext})"
					);
				}

				log.Info(sequenceLog.ToString());

				log.Info($"Current weather: {currentEnv.m_name} (normalized: {normalizedCurrentName}), biome={currentBiome}, emoji={currentEmoji}");
				log.Info($"Next forecast: {forecastEnv.m_name} (normalized: {normalizedForecastName}, emoji={emoji}), ETA {timerStr}");

				_lastForecastEnv = forecastEnv;
				_lastForecastEmoji = emoji;
				_lastForecastPeriod = forecastPeriod;
			}
		}

		private static EnvSetup GetEnvironment(long period, BiomeSector biomeSector, bool isAshlands, bool isDeepNorth)
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
					List<EnvEntry> availableEnvironments = envMan.GetAvailableEnvironments(biomeSector);

					if (availableEnvironments != null && availableEnvironments.Count > 0)
					{
						// Calculate total weight
						float totalWeight = availableEnvironments
							.Where(e => e != null && e.m_env != null)
							.Sum(e => e.m_weight);

						// Build debug log for available weathers
						StringBuilder envListLog = new StringBuilder();
						envListLog.AppendLine($"Available weathers for biome sector {biomeSector}:");

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

			string normalizedName = NormalizeWeatherName(currentBiome, currentEnv.m_name);
			string emoji = WeatherEmojis.TryGetValue((currentBiome, normalizedName), out var e) ? e : "❓";

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

		private static string NormalizeSeasonSuffix(string env)
		{
			// Remove seasonal suffixes
			env = env.Replace(" Summer", "")
					 .Replace(" Fall", "")
					 .Replace(" Winter", "");

			return env;
		}

		private static string NormalizeWeatherName(Heightmap.Biome biome, string env)
		{
			// Seasonality "warm" snow variants
			if (env == "WarmSnow")
				return "Snow";

			if (env == "WarmSnowStorm")
				return "SnowStorm";

			// Seasons: winter "rain" behaves like snow in most biomes
			if (env.EndsWith(" Winter", StringComparison.Ordinal))
			{
				bool isSnowInsteadOfRain =
					biome != Heightmap.Biome.Mountain &&
					biome != Heightmap.Biome.AshLands &&
					biome != Heightmap.Biome.DeepNorth;

				if (isSnowInsteadOfRain)
				{
					if (env.StartsWith("LightRain", StringComparison.Ordinal) || env.StartsWith("Rain", StringComparison.Ordinal))
					{
						// LightRain Winter / Rain Winter → light snow
						return "Snow";
					}

					if (env.StartsWith("ThunderStorm", StringComparison.Ordinal))
					{
						// ThunderStorm Winter → heavy snow
						return "SnowStorm";
					}
				}
			}

			// Seasons: Swamp Summer special case
			// Handle before stripping the suffix, because NormalizeSeasonSuffix would
			// turn "Swamp Summer" into just "Swamp".
			if (env == "Swamp Summer")
				env = "SwampRain";

			// Strip generic seasonal suffixes (" Summer", " Fall", " Winter")
			env = NormalizeSeasonSuffix(env);

			// Twilight variants
			// DeepNorth uses Twilight_* as its canonical environment names in vanilla so we don’t normalize there.
			if (biome != Heightmap.Biome.DeepNorth)
			{
				if (env == "Twilight_Snow")
					env = "Snow";
				else if (env == "Twilight_SnowStorm")
					env = "SnowStorm";
				else if (env == "Twilight_Clear")
					env = "Clear";
			}

			return env;
		}

		private static bool ShouldShowWeatherUI()
		{
			if (!ShowUI) return false;
			if (Game.m_noMap) return true;

			return Minimap.instance != null &&
				   Minimap.instance.m_mapSmall != null &&
				   Minimap.instance.m_mapSmall.activeInHierarchy;
		}
	}
}
