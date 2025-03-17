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

		// Config entry names
		public static class ConfigEntryName
		{
			public const string DoubleBronzeCrafting = "Double Bronze Crafting";
			public const string GearRecipeAmountsModifications = "Gear Recipe Amounts Modification";
			public const string GearRecipeMaterialsModifications = "Gear Recipe Materials Modifications";
		}

		// Config entries
		public static ConfigEntry<bool> serverConfigLocked;
		//public static ConfigEntry<bool> testJumpEnabled;

		public static ConfigEntry<bool> doubleBronzeEnabled;
		//public static ConfigEntry<bool> gearRecipeAmountsEnabled;

		//public static ConfigEntry<bool> gearRecipeMaterialsEnabled;

		public static void Init(ConfigFile configFile)
		{
			Config = configFile;

			serverConfigLocked = CreateConfig("1 - Main", "Lock Configuration", true, "If on, only server admins can change the configuration.");
			_ = configSync.AddLockingConfigEntry(serverConfigLocked);
			//testJumpEnabled = CreateConfig("1 - Main", "Test Jump", true, "Test Jump Patch");

			// ===== Grind Reduction
			doubleBronzeEnabled = CreateConfig("2 - Grind Reduction", ConfigEntryName.DoubleBronzeCrafting, true, "Doubles the amount of crafted Bronze at the Forge");
			//gearRecipeAmountsEnabled = CreateConfig("2 - Grind Reduction", "Gear Recipe Amounts Modifications", true, "Reduces costs for crafting and upgrading gear for metal. Balances other resources amounts");

			// ===== Features
			//gearRecipeMaterialsEnabled = CreateConfig("3 - Features", "Gear Recipe Materials Modifications", true, "Modifies gear recipe materials for some items (more materials from current respective biomes)");

			SetupWatcher();
		}

		private static ConfigEntry<T> CreateConfig<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true)
		{
			var configEntry = Config.Bind(group, name, defaultValue, new ConfigDescription(description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]")));

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			//configEntry.SettingChanged += (_, __) => Config.Save();
			configEntry.SettingChanged += (_, __) => OnConfigChanged(name);

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
			if (!File.Exists(ConfigFileFullPath))
				return;

			try
			{
				Config.Reload();
			}
			catch
			{
				MarsarahTweaks.MLog($"There was an issue loading {ConfigFileName}");
			}
		}

		private static void OnConfigChanged(string configName)
		{
			MarsarahTweaks.MLog($"Config setting '{configName}' changed!");
			Config.Save();

			switch (configName)
			{
				case ConfigEntryName.DoubleBronzeCrafting:
					if (ObjectDB.instance != null)
					{
						if (ZNet.instance != null && !ZNet.instance.IsServer())
						{
							MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {ConfigEntryName.DoubleBronzeCrafting}...");
							OtherPatches.UpdateDoubleBronzeCrafting(ObjectDB.instance);
						}
						else
						{
							MarsarahTweaks.MLog($"ConfigManager: I am a server. No changes made to {ConfigManager.ConfigEntryName.DoubleBronzeCrafting}...");
						}
					}
					else
					{
						MarsarahTweaks.MLog("ObjectDB is not initialized yet.");
					}
					break;
			}
		}

	}
}
