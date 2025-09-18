using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class WeatherChanges
	{
		private static readonly LogManager log = new LogManager("Weather Changes", LogManager.LogLevel.Info);

		private static bool applied = false;

		// New weather values
		public static readonly Dictionary<(Heightmap.Biome, string), float> weatherWeightChanges = new Dictionary<(Heightmap.Biome, string), float>()
		{
			{ (Heightmap.Biome.Meadows, "Misty"), 0.1f },        // 0.2
			{ (Heightmap.Biome.Meadows, "Rain"), 0.1f },         // 0.2
			{ (Heightmap.Biome.Meadows, "ThunderStorm"), 0.1f }, // 0.2

			{ (Heightmap.Biome.Mountain, "SnowStorm"), 0.5f },   // 1

			{ (Heightmap.Biome.Plains, "Heath clear"), 3f },     // 2
			{ (Heightmap.Biome.Plains, "Misty"), 0.1f },         // 0.4
			{ (Heightmap.Biome.Plains, "LightRain"), 0.1f },     // 0.4

			{ (Heightmap.Biome.Ocean, "Misty"), 0.05f },         // 0.1
		};

		// Backup dictionary
		private static readonly Dictionary<EnvEntry, float> originalWeights = new Dictionary<EnvEntry, float>();

		[HarmonyPatch(typeof(EnvMan), "SelectWeightedEnvironment")]
		class Patch_SelectWeightedEnvironment
		{
			static void Prefix(List<EnvEntry> environments, EnvMan __instance)
			{
				if (environments == null || environments.Count == 0) return;

				// Only proceed if config enabled
				if (ConfigManager.ClearerWeatherEnabled.Value)
				{
					if (!applied)
					{
						var biome = GetBiomeForEnvironments(environments, __instance);

						foreach (var e in environments)
						{
							// Backup original if not already
							if (!originalWeights.ContainsKey(e))
								originalWeights[e] = e.m_weight;

							if (biome.HasValue && weatherWeightChanges.TryGetValue((biome.Value, e.m_env.m_name), out float newWeight))
							{
								log.Info($"Changing weight for {e.m_env.m_name} in {biome.Value}: {e.m_weight} -> {newWeight}");
								e.m_weight = newWeight;
							}
						}
					}

					applied = true;
				}
				else if (applied)
				{
					// Restore original weights if config disabled
					foreach (var e in environments)
					{
						if (originalWeights.TryGetValue(e, out float originalWeight))
						{
							e.m_weight = originalWeight;
							log.Info($"Restored original weight for {e.m_env.m_name}: {originalWeight}");
						}
					}

					applied = false;
				}
			}

			static void Postfix(EnvSetup __result, List<EnvEntry> environments, EnvMan __instance)
			{
				if (!ConfigManager.ClearerWeatherEnabled.Value) return;

				if (__result != null && environments != null)
				{
					var biome = GetBiomeForEnvironments(environments, __instance);

					if (biome.HasValue)
					{
						log.Info($"Selected environment: {__result.m_name} in biome {biome.Value}");
						MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft,	$"Selected environment: {__result.m_name} in {biome.Value}");
					}
				}
			}
		}

		private static Heightmap.Biome? GetBiomeForEnvironments(List<EnvEntry> envs, EnvMan envMan)
		{
			foreach (var biomeSetup in envMan.m_biomes)
			{
				if (biomeSetup.m_environments == envs)
					return biomeSetup.m_biome;
			}
			return null;
		}

		/*[HarmonyPatch(typeof(EnvMan), "Awake")]
		class Patch_EnvMan_Awake
		{
			static void Postfix(EnvMan __instance)
			{
				log.Info("---- Biome → Environment list ----", header: true);

				foreach (var biomeSetup in __instance.m_biomes)
				{
					string biomeName = biomeSetup.m_biome.ToString();

					foreach (var entry in biomeSetup.m_environments)
					{
						log.Info($"Biome: {biomeName}, Env: {entry.m_env.m_name}, Default weight: {entry.m_weight}");
					}
				}

				log.Info("---- End list ----", footer: true);
			}
		}*/
	}
}
