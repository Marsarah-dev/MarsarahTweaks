using BepInEx;
using BepInEx.Configuration;
using MarsarahTweaks.Patches;
using ServerSync;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
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
		/*public static class ConfigEntryName
		{
			public const string DoubleBronzeCrafting = "Double Bronze Crafting";
			public const string GearRecipeAmountsModifications = "Cheaper Gear Recipe Amounts";
			public const string GearRecipeMaterialsModifications = "Alternate Gear Recipe Materials";
			public const string BuildPiecesAmountsModifications = "Cheaper Build Pieces Amounts";
			public const string BuildPiecesMaterialsModifications = "Alternate Build Pieces Materials";

			public const string LighterMetalWeight = "Lighter Metal Weight";
		}*/

		// Struct for Config Sections
		public static class ConfigSections
		{
			public const string Main = "1 - Main";
			public const string GrindReduction = "2 - Grind Reduction";
			public const string Features = "3 - Features";
			public const string QOL = "4 - QOL";
			public const string UI = "5 - UI";
		}

		// Struct for Config Metadata
		public struct ConfigMetadata
		{
			public string Name;
			public string Description;

			public ConfigMetadata(string name, string description)
			{
				Name = name;
				Description = description;
			}
		}

		// Grouped Config Metadata (for easy expansion)
		public static class Configs
		{
			public static readonly ConfigMetadata ServerConfig = new ConfigMetadata("Lock Configuration", "If on, only server admins can change the configuration.");

			public static readonly ConfigMetadata DoubleBronzeCrafting = new ConfigMetadata("1 - Double Bronze Crafting", "Doubles the amount of crafted Bronze at the Forge");
			public static readonly ConfigMetadata GearRecipeAmounts = new ConfigMetadata("2 - Cheaper Gear Recipe Amounts", "Reduces costs for crafting and upgrading gear for metal. Balances other resources amounts");
			public static readonly ConfigMetadata GearRecipeMaterials = new ConfigMetadata("3 - Alternate Gear Recipe Materials", "Modifies gear recipe materials for some items (more materials from current respective biomes)");
			public static readonly ConfigMetadata BuildPiecesAmounts = new ConfigMetadata("4 - Cheaper Build Pieces Amounts", "Reduces costs for build pieces");
			public static readonly ConfigMetadata BuildPiecesMaterials = new ConfigMetadata("5 - Alternate Build Pieces Materials", "Modifies build pieces materials");

			public static readonly ConfigMetadata LighterMetalWeight = new ConfigMetadata("1 - Lighter Metal Weight", "All metal (ore and bars) weight decreased to 8 (Toggling this mid-game requires client relog to take effect)");
		}

		// Config entries
		public static ConfigEntry<bool> serverConfigLocked;
		//public static ConfigEntry<bool> testJumpEnabled;

		public static ConfigEntry<bool> doubleBronzeEnabled;
		public static ConfigEntry<bool> gearRecipeAmountsEnabled;
		public static ConfigEntry<bool> gearRecipeMaterialsEnabled;
		public static ConfigEntry<bool> buildPieceAmountsEnabled;
		public static ConfigEntry<bool> buildPieceMaterialsEnabled;

		public static ConfigEntry<bool> lighterMetalWeightEnabled;

		public static void Init(ConfigFile configFile)
		{
			Config = configFile;

			serverConfigLocked = CreateConfig(ConfigSections.Main, Configs.ServerConfig.Name, true, Configs.ServerConfig.Description);
			_ = configSync.AddLockingConfigEntry(serverConfigLocked);
			//testJumpEnabled = CreateConfig("1 - Main", "Test Jump", true, "Test Jump Patch");

			// ===== Grind Reduction
			doubleBronzeEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.DoubleBronzeCrafting.Name, true, Configs.DoubleBronzeCrafting.Description);
			gearRecipeAmountsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.GearRecipeAmounts.Name, true, Configs.GearRecipeAmounts.Description);
			gearRecipeMaterialsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.GearRecipeMaterials.Name, true, Configs.GearRecipeMaterials.Description);
			buildPieceAmountsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.BuildPiecesAmounts.Name, true, Configs.BuildPiecesAmounts.Description);
			buildPieceMaterialsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.BuildPiecesMaterials.Name, true, Configs.BuildPiecesMaterials.Description);

			// ===== Features


			// ===== QOL
			lighterMetalWeightEnabled = CreateConfig(ConfigSections.QOL, Configs.LighterMetalWeight.Name, true, Configs.LighterMetalWeight.Description);

			SetupWatcher();
		}

		private static ConfigEntry<T> CreateConfig<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true)
		{
			var configEntry = Config.Bind(group, name, defaultValue, new ConfigDescription(description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]")));

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

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

			if (ObjectDB.instance == null || ZNetScene.instance == null) return;

			if (ZNet.instance != null)
			{
				bool isDedicatedServer = ZNet.instance.IsDedicated();

				if (!isDedicatedServer)
				{
					MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
					switch (configName)
					{
						case var name when name == Configs.DoubleBronzeCrafting.Name:
							OtherPatches.UpdateDoubleBronzeCrafting(ObjectDB.instance);
							break;

						case var name when name == Configs.LighterMetalWeight.Name:
							OtherPatches.UpdateLighterMetalWeight(ObjectDB.instance);
							break;

						case var name when name == Configs.GearRecipeAmounts.Name:
							GearRecipeChanges.ModifyGearRecipes(ObjectDB.instance, true, false);
							break;

						case var name when name == Configs.GearRecipeMaterials.Name:
							GearRecipeChanges.ModifyGearRecipes(ObjectDB.instance, false, true);
							break;

						case var name when name == Configs.BuildPiecesAmounts.Name:
							BuildPieceChanges.ModifyBuildPieces(ZNetScene.instance, true, false);
							break;

						case var name when name == Configs.BuildPiecesMaterials.Name:
							BuildPieceChanges.ModifyBuildPieces(ZNetScene.instance, false, true);
							break;
					}
				}
                else
                {
					MarsarahTweaks.MLog($"ConfigManager: I am a server. No changes made to {configName}...");
				}
            }
		}
	}
}
