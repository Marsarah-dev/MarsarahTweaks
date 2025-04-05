//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using static Terminal;

//namespace MarsarahTweaks.Patches
//{
//	internal class CustomConsoleCommandHandler
//	{
//		public static void Init()
//		{
//			// Weather command
//			new ConsoleCommand("currentweather", "Prints the current biome's weather", args =>
//			{
//				PrintCurrentWeather(args);
//			});

//			// Ship HP command
//			new ConsoleCommand("currentshiphp", "Prints the HP of the ship you're currently controlling", args =>
//			{
//				PrintCurrentShipHP(args);
//			});
//		}

//		private static void PrintCurrentWeather(ConsoleEventArgs args)
//		{
//			if (EnvMan.instance != null)
//			{
//				FieldInfo currentWeatherField = typeof(EnvMan).GetField("m_currentEnv", BindingFlags.NonPublic | BindingFlags.Instance);
//				object currentWeatherObj = currentWeatherField?.GetValue(EnvMan.instance);

//				if (currentWeatherObj != null)
//				{
//					FieldInfo weatherNameField = typeof(EnvSetup).GetField("m_name", BindingFlags.Public | BindingFlags.Instance);
//					FieldInfo weatherWeightField = typeof(EnvSetup).GetField("m_weight", BindingFlags.Public | BindingFlags.Instance);

//					string currentWeather = weatherNameField?.GetValue(currentWeatherObj) as string ?? "Unknown";
//					float weatherWeight = weatherWeightField != null ? (float)weatherWeightField.GetValue(currentWeatherObj) : -1f;

//					string message = $"[Weather Debug] Current Weather: {currentWeather}, Weight: {weatherWeight}";
//					args.Context?.AddString(message);
//				}
//				else
//				{
//					args.Context?.AddString("[Weather Debug] Could not retrieve weather.");
//				}
//			}
//			else
//			{
//				args.Context?.AddString("[Weather Debug] EnvMan instance is null.");
//			}
//		}

//		private static void PrintCurrentShipHP(ConsoleEventArgs args)
//		{
//			if (Player.m_localPlayer == null)
//			{
//				args.Context?.AddString("[Ship Debug] No player found.");
//				return;
//			}

//			Ship controlledShip = Player.m_localPlayer.GetControlledShip();
//			if (controlledShip == null)
//			{
//				args.Context?.AddString("[Ship Debug] You are not controlling a ship.");
//				return;
//			}

//			WearNTear wearNTear = controlledShip.GetComponent<WearNTear>();
//			if (wearNTear == null)
//			{
//				args.Context?.AddString("[Ship Debug] This ship has no WearNTear component.");
//				return;
//			}

//			string message = $"[Ship Debug] Current Ship HP: {wearNTear.m_health}";
//			args.Context?.AddString(message);
//		}
//	}
//}
