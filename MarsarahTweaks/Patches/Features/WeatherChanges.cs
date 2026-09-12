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
		private static readonly LogManager log = new LogManager("Weather Changes", LogManager.LogLevel.Warning);

		//private static bool originalWeatherLogged = false;

		// New weather values
		public static readonly Dictionary<(Heightmap.Biome, string), float> weatherWeightChanges = new Dictionary<(Heightmap.Biome, string), float>()
		{
			{ (Heightmap.Biome.Meadows, "Clear"), 4.5f },           // 5
			{ (Heightmap.Biome.Meadows, "Misty"), 0.1f },           // 0.2
			{ (Heightmap.Biome.Meadows, "Rain"), 0.15f },           // 0.2
			{ (Heightmap.Biome.Meadows, "ThunderStorm"), 0.15f },   // 0.2
			{ (Heightmap.Biome.Meadows, "LightRain"), 0.175f },     // 0.2

			{ (Heightmap.Biome.BlackForest, "DeepForest Mist"), 3f }, // 2

			{ (Heightmap.Biome.Mountain, "SnowStorm"), 0.3f },      // 1

			{ (Heightmap.Biome.Plains, "Heath clear"), 3f },        // 2
			{ (Heightmap.Biome.Plains, "Misty"), 0.1f },            // 0.4
			{ (Heightmap.Biome.Plains, "LightRain"), 0.1f },        // 0.4

			{ (Heightmap.Biome.Ocean, "Misty"), 0.025f },           // 0.1

			{ (Heightmap.Biome.Mistlands, "Mistlands_clear"), 2f }, // 1.5

			{ (Heightmap.Biome.AshLands, "Ashlands_ashrain"), 2f }, // 1.5
		};

		// Backup dictionary and apply tracker
		private static readonly Dictionary<EnvEntry, float> originalWeights = new Dictionary<EnvEntry, float>();

		[HarmonyPatch(typeof(EnvMan), "SelectWeightedEnvironment")]
		class Patch_SelectWeightedEnvironment
		{
			static void Prefix(List<EnvEntry> environments, EnvMan __instance)
			{
				if (environments == null || environments.Count == 0) return;

				/*if (!originalWeatherLogged)
				{
					LogWeatherWeights(__instance);
					originalWeatherLogged = true;
				}*/

				var biome = GetBiomeForEnvironments(environments, __instance);

				if (ConfigManager.ClearerWeatherEnabled.Value)
				{
					if (!biome.HasValue)
						return;

					foreach (var e in environments)
					{
						// Store original value only once
						if (!originalWeights.ContainsKey(e))
							originalWeights[e] = e.m_weight;

						if (weatherWeightChanges.TryGetValue((biome.Value, e.m_env.m_name), out float newWeight))
						{
							if (Math.Abs(e.m_weight - newWeight) > 0.001f)
							{
								log.Info($"Changing weight for {e.m_env.m_name} in {biome.Value}: {e.m_weight} -> {newWeight}");
								e.m_weight = newWeight;
							}
						}
					}
				}
				else
				{
					bool restoredAny = false;

					foreach (var e in environments)
					{
						if (originalWeights.TryGetValue(e, out float originalWeight))
						{
							if (Math.Abs(e.m_weight - originalWeight) > 0.001f)
							{
								e.m_weight = originalWeight;
								log.Info($"Restored original weight for {e.m_env.m_name}: {originalWeight}");
								restoredAny = true;
							}

							// Clean up restored entry
							originalWeights.Remove(e);
						}
					}

					if (restoredAny)
						log.Info($"Restored weights for biome: {biome?.ToString() ?? "Unknown"}");
				}
			}

			/*static void Postfix(EnvSetup __result, List<EnvEntry> environments, EnvMan __instance)
			{
				if (!ConfigManager.ClearerWeatherEnabled.Value) return;

				if (__result != null && environments != null)
				{
					var biome = GetBiomeForEnvironments(environments, __instance);

					if (biome.HasValue)
					{
						log.Info($"Selected environment: {__result.m_name} in biome {biome.Value}");
						//MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft,	$"Selected environment: {__result.m_name} in {biome.Value}");
					}
				}
			}*/
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

		// Helper to print available weathers
		/*private static bool printed = false;
		 
		[HarmonyPatch(typeof(EnvMan), "Update")]
		class Patch_EnvMan_Awake
		{
			static void Postfix(EnvMan __instance)
			{
				if (!printed)
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

					printed = true;
				}				
			}
		}*/

		private static void LogWeatherWeights(EnvMan envMan)
		{
			if (envMan == null || envMan.m_biomes == null)
			{
				log.Warn("Cannot log weather weights because EnvMan or its biome list is null.");
				return;
			}

			log.Info("=== Vanilla Weather Weights by Biome ===");

			Dictionary<Heightmap.Biome, int> biomeCounts = new Dictionary<Heightmap.Biome, int>();
			Dictionary<Heightmap.Biome, int> biomeIndexes = new Dictionary<Heightmap.Biome, int>();

			foreach (var biomeSetup in envMan.m_biomes)
			{
				Heightmap.Biome biome = biomeSetup.m_biome;

				if (!biomeCounts.ContainsKey(biome))
					biomeCounts[biome] = 0;

				biomeCounts[biome]++;
			}

			foreach (var biomeSetup in envMan.m_biomes)
			{
				Heightmap.Biome biome = biomeSetup.m_biome;
				List<EnvEntry> environments = biomeSetup.m_environments;

				if (!biomeIndexes.ContainsKey(biome))
					biomeIndexes[biome] = 0;

				biomeIndexes[biome]++;

				int variantIndex = biomeIndexes[biome];
				int variantCount = biomeCounts[biome];

				string variantText = variantCount > 1 ? $" [Variant {variantIndex}/{variantCount}]" : "";

				if (environments == null || environments.Count == 0)
				{
					log.Info($"Biome: {biome}{variantText} | No environments");
					continue;
				}

				float totalWeight = 0f;

				foreach (EnvEntry entry in environments)
				{
					if (entry?.m_env == null) continue;

					totalWeight += entry.m_weight;
				}

				log.Info($"Biome: {biome}{variantText} | Environments: {environments.Count} | Total Weight: {totalWeight:0.###}");

				foreach (EnvEntry entry in environments)
				{
					if (entry?.m_env == null) continue;

					float probability = totalWeight > 0f ? entry.m_weight / totalWeight * 100f : 0f;

					log.Info($"   - {entry.m_env.m_name}: Weight {entry.m_weight:0.###} | Probability {probability:0.00}%");
				}
			}

			log.Info("=== End Vanilla Weather Weights ===");
		}
	}
}
