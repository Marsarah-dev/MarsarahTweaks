/*using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Terminal;

namespace MarsarahTweaks.Patches
{
	internal class CustomConsoleCommandHandler
	{
		private static readonly LogManager log = new LogManager("Console Command Handler", LogManager.LogLevel.Info);

		public static void Init()
		{
			// Weather command
			new ConsoleCommand("currentweather", "Prints the current biome's weather", args =>
			{
				PrintCurrentWeather(args);
			});

			// Ship HP command
			new ConsoleCommand("currentshiphp", "Prints the HP of the ship you're currently controlling", args =>
			{
				PrintCurrentShipHP(args);
			});
		}

		private static void PrintCurrentWeather(ConsoleEventArgs args)
		{
			var envMan = EnvMan.instance;
			if (envMan == null)
			{
				log.Info("EnvMan instance is null.");
				return;
			}

			// Get current environment
			FieldInfo currentWeatherField = typeof(EnvMan).GetField("m_currentEnv", BindingFlags.NonPublic | BindingFlags.Instance);
			EnvSetup currentEnv = currentWeatherField?.GetValue(envMan) as EnvSetup;

			// Get current biome
			FieldInfo currentBiomeField = typeof(EnvMan).GetField("m_currentBiome", BindingFlags.NonPublic | BindingFlags.Instance);
			Heightmap.Biome? currentBiome = currentBiomeField != null ? (Heightmap.Biome?)currentBiomeField.GetValue(envMan) : null;

			if (currentEnv != null && currentBiome.HasValue)
			{
				log.Info($"Current Biome: {currentBiome.Value}, Current Weather: {currentEnv.m_name}");

				// Find all environments in this biome
				BiomeEnvSetup biomeSetup = envMan.m_biomes.FirstOrDefault(b => b.m_biome == currentBiome.Value);
				if (biomeSetup != null)
				{
					log.Info($"All possible weathers in {currentBiome.Value}:");
					foreach (var entry in biomeSetup.m_environments)
					{
						log.Info($"- {entry.m_env.m_name} (Weight: {entry.m_weight})");
					}
				}
				else
				{
					log.Info($"No BiomeEnvSetup found for {currentBiome.Value}");
				}
			}
			else
			{
				log.Info("Could not retrieve current weather or biome.");
			}
		}

		private static void PrintCurrentShipHP(ConsoleEventArgs args)
		{
			if (Player.m_localPlayer == null)
			{
				args.Context?.AddString("[Ship Debug] No player found.");
				return;
			}

			Ship controlledShip = Player.m_localPlayer.GetControlledShip();
			if (controlledShip == null)
			{
				args.Context?.AddString("[Ship Debug] You are not controlling a ship.");
				return;
			}

			WearNTear wearNTear = controlledShip.GetComponent<WearNTear>();
			if (wearNTear == null)
			{
				args.Context?.AddString("[Ship Debug] This ship has no WearNTear component.");
				return;
			}

			string message = $"[Ship Debug] Current Ship HP: {wearNTear.m_health}";
			args.Context?.AddString(message);
		}
	}
}
*/