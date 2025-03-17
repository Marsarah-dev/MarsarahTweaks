using BepInEx;
using BepInEx.Configuration;
using MarsarahTweaks.Patches;
using ServerSync;
using System.IO;
using UnityEngine;

namespace MarsarahTweaks
{
	public static class ConfigManager
	{
		private static ConfigFile Config;
		private static readonly ConfigSync configSync = new ConfigSync(MarsarahTweaks.ModGUID)
		{
			DisplayName = MarsarahTweaks.ModName,
			CurrentVersion = MarsarahTweaks.ModVersion,
			MinimumRequiredVersion = MarsarahTweaks.ModVersion
		};

		// Config file stuff
		private static string ConfigFileName => MarsarahTweaks.ModGUID + ".cfg";
		private static string ConfigFileFullPath => Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

		// Config entries
		public static ConfigEntry<bool> serverConfigLocked;
		public static ConfigEntry<bool> testJumpEnabled;

		//public static ConfigEntry<bool> cheaperGearEnabled;
		//public static ConfigEntry<bool> doubleBronzeEnabled;

		//public static ConfigEntry<bool> altGearRecipesEnabled;

		public static void Init(ConfigFile configFile)
		{
			Config = configFile;
			MarsarahTweaks.MLog("Initializing ConfigManager...");

			serverConfigLocked = CreateConfig("1 - Main", "Lock Configuration", true, "If on, only server admins can change the configuration.");
			_ = configSync.AddLockingConfigEntry(serverConfigLocked);

			// ===== Grind Reduction
			//cheaperGearEnabled = CreateConfig("2 - Grind Reduction", "Cheaper Gear Recipes", true, "Reduces costs for crafting and upgrading gear (especially metal)");
			//doubleBronzeEnabled = CreateConfig("2 - Grind Reduction", "Double Bronze Crafting", true, "Doubles the amount of crafted Bronze at the Forge");

			// ===== Features
			//altGearRecipesEnabled = CreateConfig("3 - Features", "Alt Gear Recipes", true, "Modifies gear recipe materials for some items (more materials from current biomes)");

			MarsarahTweaks.MLog("ConfigManager initialization complete. ConfigSync ready.");

			SetupWatcher();
		}

		private static ConfigEntry<T> CreateConfig<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true)
		{
			var configEntry = Config.Bind(group, name, defaultValue, new ConfigDescription(description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]")));
			MarsarahTweaks.MLog($"Config created: {name} = {configEntry.Value} (Synchronized: {synchronizedSetting})");

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			return configEntry;
		}

		private static void SetupWatcher()
		{
			FileSystemWatcher watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName)
			{
				IncludeSubdirectories = true,
				SynchronizingObject = ThreadingHelper.SynchronizingObject,
				EnableRaisingEvents = true
			};

			watcher.Changed += ReadConfigValues;
			watcher.Created += ReadConfigValues;
			watcher.Renamed += ReadConfigValues;
		}

		private static void ReadConfigValues(object sender, FileSystemEventArgs e)
		{
			MarsarahTweaks.MLog($"ReadConfigValues triggered by file change: {e.FullPath}");

			if (!File.Exists(ConfigFileFullPath))
				return;

			try
			{
				MarsarahTweaks.MLog("ReadConfigValues called");
				Config.Reload();
			}
			catch
			{
				MarsarahTweaks.MLog("There was an issue loading " + ConfigFileName);
			}
		}
	}
}
