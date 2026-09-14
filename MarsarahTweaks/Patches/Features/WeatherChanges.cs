using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.Features
{
	internal class WeatherChanges
	{
		private static readonly LogManager log = new LogManager("Weather Changes", LogManager.LogLevel.Warning);

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

		// Original weather weights used for restoring config changes and Alt Biome weather
		private static readonly Dictionary<EnvEntry, float> originalWeights = new Dictionary<EnvEntry, float>();
		private static EnvMan appliedEnvMan = null;
		private static bool weightsApplied = false;

		private static readonly System.Reflection.MethodInfo memberwiseClone = AccessTools.Method(typeof(object), "MemberwiseClone");

		[HarmonyPatch(typeof(EnvMan), "GetAvailableEnvironments", new Type[] { typeof(BiomeSector) })]
		class Patch_GetAvailableEnvironments
		{
			static void Prefix(EnvMan __instance)
			{
				UpdateWeatherWeights(__instance);
			}

			static void Postfix(BiomeSector __0, List<EnvEntry> __result)
			{
				if (!ConfigManager.ClearerWeatherEnabled.Value) return;
				if (__0 == null || __result == null) return;
				if (!HasAlternateWeatherChanges(__0)) return;

				for (int i = 0; i < __result.Count; i++)
				{
					EnvEntry entry = __result[i];

					if (entry == null)
						continue;

					if (!originalWeights.TryGetValue(entry, out float originalWeight))
						continue;

					EnvEntry vanillaEntry = CloneEnvEntry(entry);
					vanillaEntry.m_weight = originalWeight;
					__result[i] = vanillaEntry;
				}
			}
		}

		public static void UpdateWeatherWeights(EnvMan envMan)
		{
			if (envMan == null || envMan.m_biomes == null)
				return;

			if (appliedEnvMan != envMan)
			{
				originalWeights.Clear();
				weightsApplied = false;
				appliedEnvMan = envMan;
			}

			if (ConfigManager.ClearerWeatherEnabled.Value)
			{
				if (!weightsApplied)
					ApplyWeatherWeights(envMan);
			}
			else
			{
				if (weightsApplied)
					RestoreWeatherWeights();
			}
		}

		private static void ApplyWeatherWeights(EnvMan envMan)
		{
			foreach (BiomeEnvSetup biomeSetup in envMan.m_biomes)
			{
				foreach (EnvEntry entry in biomeSetup.m_environments)
				{
					if (entry?.m_env == null)
						continue;

					if (!weatherWeightChanges.TryGetValue((biomeSetup.m_biome, entry.m_env.m_name), out float newWeight))
						continue;

					if (originalWeights.ContainsKey(entry))
						continue;

					originalWeights[entry] = entry.m_weight;

					log.Info($"Changing weight for {entry.m_env.m_name} in {biomeSetup.m_biome}: {entry.m_weight} -> {newWeight}");
					entry.m_weight = newWeight;
				}
			}

			weightsApplied = true;
		}

		private static void RestoreWeatherWeights()
		{
			foreach (var pair in originalWeights)
			{
				log.Info($"Restoring weight for {pair.Key.m_env.m_name}: {pair.Key.m_weight} -> {pair.Value}");
				pair.Key.m_weight = pair.Value;
			}

			originalWeights.Clear();
			weightsApplied = false;
		}

		private static bool HasAlternateWeatherChanges(BiomeSector biomeSector)
		{
			if (biomeSector.AltBiomes == null)
				return false;

			foreach (AltBiome altBiome in biomeSector.AltBiomes)
			{
				if (altBiome == null)
					continue;

				if (!string.IsNullOrEmpty(altBiome.m_forceEnvironment))
					return true;

				if (altBiome.m_addEnvironments != null && altBiome.m_addEnvironments.Count > 0)
					return true;

				if (altBiome.m_blockEnvironments != null && altBiome.m_blockEnvironments.Count > 0)
					return true;
			}

			return false;
		}

		private static EnvEntry CloneEnvEntry(EnvEntry entry)
		{
			return (EnvEntry)memberwiseClone.Invoke(entry, null);
		}

		/*
		// Debug helper: prints base biome weather weights.
		private static void LogWeatherWeights(EnvMan envMan)
		{
			if (envMan == null || envMan.m_biomes == null)
				return;

			log.Info("=== Weather Weights by Biome ===");

			foreach (BiomeEnvSetup biomeSetup in envMan.m_biomes)
			{
				foreach (EnvEntry entry in biomeSetup.m_environments)
				{
					if (entry?.m_env == null)
						continue;

					log.Info($"{biomeSetup.m_biome} - {entry.m_env.m_name}: {entry.m_weight}");
				}
			}

			log.Info("=== End Weather Weights ===");
		}
		*/
	}
}