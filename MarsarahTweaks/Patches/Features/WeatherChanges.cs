using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class WeatherChanges
	{
		[HarmonyPatch(typeof(EnvMan), "InitializeBiomeEnvSetup")]
		class LessFog_Patch
		{
			static void Postfix(EnvMan __instance)
			{
				if (!ZNet.instance || !ZNet.instance.IsServer()) return; // Prevent running on clients

				if (ConfigManager.clearerWeatherEnabled.Value)
				{
					foreach (BiomeEnvSetup biome in __instance.m_biomes)
					{
						//float totalWeightBefore = 0f;
						//float totalWeightAfter = 0f;

						foreach (EnvEntry environment in biome.m_environments)
						{
							if (environment?.m_env == null) continue; // Prevent null reference errors

							//totalWeightBefore += environment.m_weight;

							if (weatherWeightChanges.TryGetValue((biome.m_name, environment.m_env.m_name), out float newWeight))
							{
								//MarsarahTweaks.MLog($"Changing old weather weight {environment.m_weight} -> {newWeight} for {environment.m_env.m_name} in biome {biome.m_name}");
								environment.m_weight = newWeight;
							}

							//totalWeightAfter += environment.m_weight;
						}

						//MarsarahTweaks.MLog($"Biome {biome.m_name}: Total weight before: {totalWeightBefore}, after: {totalWeightAfter}");
					}
				}
			}

			// Store weather weight changes in a dictionary
			static readonly Dictionary<(string biome, string env), float> weatherWeightChanges = new Dictionary<(string biome, string env), float>()
			{
				{ ("Meadows", "Misty"), 0.1f },
				{ ("Meadows", "Rain"), 0.1f },
				{ ("Meadows", "ThunderStorm"), 0.1f },

				{ ("Mountain", "SnowStorm"), 0.5f },
				{ ("Mountain", "Snow"), 2.5f },

				{ ("Plains", "Heath clear"), 3f },
				{ ("Plains", "Misty"), 0.1f },
				{ ("Plains", "LightRain"), 0.1f },

				{ ("Ocean", "Misty"), 0.05f }
			};
		}
	}
}
