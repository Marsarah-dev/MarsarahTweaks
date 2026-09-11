
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MarsarahTweaks.Managers;
using MarsarahTweaks.Patches;
using MarsarahTweaks.Patches.Balance;
using MarsarahTweaks.Patches.Features;
using MarsarahTweaks.Patches.QOL;
using ServerSync;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MarsarahTweaks
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahTweaks : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahTweaks";
		internal const string ModVersion = "1.6.0";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		private readonly Harmony harmony = new Harmony(ModGUID);

		private void Awake()
		{
			LogManager.SetGlobalLogLevel(LogManager.LogLevel.Info); // None, Error, Warning, Info
			ConfigManager.Init(Config);
			//CustomConsoleCommandHandler.Init(); // Register new console commands

			harmony.PatchAll();
		}

		private void Start()
		{
			CompatibilityManager.Initialize();
			CompatibilityManager.UpdateIncompatibilities();
			CompatibilityManager.DumpAllLoadedMods();
		}

		private void OnDestroy()
		{
			Config.Save();
		}
	}
}
