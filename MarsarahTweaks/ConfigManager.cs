
/*
 "Alternate Cooking Recipes" + "Alternate Mead Recipes" + "More Food Stacks" - "Food and Mead Modifications"
Si
"More Armor Stats" + "Mage Gear Eitr" - "More Armor Stats"
Si
"Better Death Raiser" + "Better Summoned Skeleton Gear" - "Better Death Raiser Summons" 
 */


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
			public static readonly ConfigMetadata GearRecipeAmountsModifications = new ConfigMetadata("2 - Cheaper Gear Recipe Amounts", "Reduces costs for crafting and upgrading gear for metal. Balances other resources amounts");
			public static readonly ConfigMetadata GearRecipeMaterialsModifications = new ConfigMetadata("3 - Alternate Gear Recipe Materials", "Modifies gear recipe materials for some items (more materials from current respective biomes)");
			public static readonly ConfigMetadata BuildPieceAmountsModifications = new ConfigMetadata("4 - Cheaper Build Pieces Amounts", "Reduces costs for build pieces");
			public static readonly ConfigMetadata BuildPieceMaterialsModifications = new ConfigMetadata("5 - Alternate Build Pieces Materials", "Modifies build pieces materials (This will move the Workbench Toolrack extension from Mountain to Swamp biome)");
			public static readonly ConfigMetadata FoodAndMeadModifications = new ConfigMetadata("6 - Food And Mead Modifications", "(Toggling mid-game requires CLIENT relog for food stacks) Modifies food and mead recipes costs, crafted amounts and stacks");

			public static readonly ConfigMetadata LinenCapeModifications = new ConfigMetadata("01 - Early Linen Cape", "Renames the Linen Cape to Fine Cape, moves it to the Swamp biome, and adds poison resist to it");
			public static readonly ConfigMetadata GearSpeedModifications = new ConfigMetadata("02 - Gear Speed Modifications", "(Toggling mid-game requires CLIENT relog) Removes speed penalty for heavy and mage armors; adds speed bonus to light armor");
			public static readonly ConfigMetadata ForsakenPowersModifications = new ConfigMetadata("03 - Forsaken Powers Modifications", "Reduce Forsaken Powers cooldowns and increase durations (different for each power)");
			public static readonly ConfigMetadata CharacterSpeedModifications = new ConfigMetadata("04 - Faster Character Speed", "Faster character jog, walk, swim and crouch speeds (run excluded)");
			public static readonly ConfigMetadata StatusEffectsModifications = new ConfigMetadata("05 - Shorter Wet Effect And Potion Cooldowns", "Wet effect and potions cooldown timers reduced");
			public static readonly ConfigMetadata LessStaminaModifications = new ConfigMetadata("06 - Less Stamina Usage", "Stamina use of all actions is reduced by 15%");
			public static readonly ConfigMetadata ExtraArmorStatsModifications = new ConfigMetadata("07 - Extra Armor Stats", "Heavy armor provides extra HP, light armor provides extra stamina, mage armor provides extra base eitr");
			public static readonly ConfigMetadata DeathRaiserModifications = new ConfigMetadata("08 - Better Death Raiser", "Adds secondary attack that spawns archer skeletons. Primary attack will only spawn melee skeletons");
			public static readonly ConfigMetadata DeathRaiserSummonsModifications = new ConfigMetadata("09 - Better Summoned Skeletons", "(Toggling mid-game requires CLIENT relog for speed changes) Increases summoned skeleton speed and add better looking gear according to Death Raiser level (stats not affected)");
			public static readonly ConfigMetadata FrostStaffModifications = new ConfigMetadata("10 - Better Frost Staff Accuracy", "Improves Staff of Frost Accuracy");
			public static readonly ConfigMetadata CrossbowsReloadModifications = new ConfigMetadata("11 - Reduced Crossbows Reload Time", "(Toggling mid-game requires CLIENT relog) Crossbows Reload Time Reduced by 1s");
			public static readonly ConfigMetadata AshlandsEnemiesModifications = new ConfigMetadata("12 - Less Ashlands Enemies", "(Toggling requires SERVER restart) Less Enemies in Ashlands");
			public static readonly ConfigMetadata GearUpgradeModifications = new ConfigMetadata("13 - Gear Upgrade Unlock", "Gear from Meadows, Black Forest, Mistlands and Ashlands can be upgraded to max level within their respective biomes");
			public static readonly ConfigMetadata PermanentLightsModifications = new ConfigMetadata("14 - Permanent Lights", "Makes all light sources permanent, but the build costs of light source pieces use maximum amount of their respective fuel type");
			public static readonly ConfigMetadata ClearMistlands = new ConfigMetadata("15 - Clear Mistlands", "(Disabling mid-game requires SERVER restart) Clear Mistlands mist after defeating the Queen");
			public static readonly ConfigMetadata CraftableChain = new ConfigMetadata("16 - Craftable Chain", "Chain craftable at Black Forge");
			public static readonly ConfigMetadata BrighterLanterns = new ConfigMetadata("17 - Brighter Lanterns", "(Toggling mid-game requires CLIENT relog) Dvergr lanterns are brighter"); // TODO: Test on server - disabling and relogging on local does not revert to default
			public static readonly ConfigMetadata WeatherModifications = new ConfigMetadata("18 - Clearer Weather", "(Toggling requires SERVER restart) Reduces chance for mist and snowstorms");
			public static readonly ConfigMetadata CreatureUnleveler = new ConfigMetadata("19 - Creature Unleveler By Boss", "(Toggling requires SERVER restart) Increases chance of creatures to spawn with a star or two after defeating their relevant biome boss");
			public static readonly ConfigMetadata MinibossWeight = new ConfigMetadata("20 - Hildir Weight Rewards", "(Toggling mid-game requires CLIENT relog) Increases base carry weight by 25 when turning in Hildir chests (for each chest)");

			public static readonly ConfigMetadata LighterMetalWeight = new ConfigMetadata("1 - Lighter Metal Weight", "(Toggling mid-game requires CLIENT relog) All metal ore and bars weight decreased to 8");
		}

		// Config entries
		public static ConfigEntry<bool> serverConfigLocked;
		//public static ConfigEntry<bool> testJumpEnabled;

		public static ConfigEntry<bool> doubleBronzeEnabled;
		public static ConfigEntry<bool> gearRecipeAmountsEnabled;
		public static ConfigEntry<bool> gearRecipeMaterialsEnabled;
		public static ConfigEntry<bool> buildPieceAmountsEnabled;
		public static ConfigEntry<bool> buildPieceMaterialsEnabled;
		public static ConfigEntry<bool> foodAndMeadModificationsEnabled;

		public static ConfigEntry<bool> earlyLinenCapeEnabled;
		public static ConfigEntry<bool> gearSpeedModifiersEnabled;
		public static ConfigEntry<bool> longerForsakenPowersEnabled;
		public static ConfigEntry<bool> fasterCharacterSpeedEnabled;
		public static ConfigEntry<bool> shorterStatusEffectsEnabled;
		public static ConfigEntry<bool> lessStaminaUsageEnabled;
		public static ConfigEntry<bool> extraArmorStatsEnabled;
		public static ConfigEntry<bool> betterDeathRaiserEnabled;
		public static ConfigEntry<bool> betterDeathRaiserSummonsEnabled;
		public static ConfigEntry<bool> betterFrostStaffAccuracyEnabled;
		public static ConfigEntry<bool> reducedCrossbowsReloadTimeEnabled;
		public static ConfigEntry<bool> lessAshlandsEnemiesEnabled;
		public static ConfigEntry<bool> gearUpgradeUnlockEnabled;
		public static ConfigEntry<bool> permanentLightsEnabled;
		public static ConfigEntry<bool> clearMistlandsEnabled;
		public static ConfigEntry<bool> craftableChainEnabled;
		public static ConfigEntry<bool> brighterLanternsEnabled;
		public static ConfigEntry<bool> clearerWeatherEnabled;
		public static ConfigEntry<bool> creatureUnlevelerEnabled;
		public static ConfigEntry<bool> minibossWeightEnabled;

		public static ConfigEntry<bool> lighterMetalWeightEnabled;

		public static void Init(ConfigFile configFile)
		{
			Config = configFile;

			serverConfigLocked = CreateConfig(ConfigSections.Main, Configs.ServerConfig.Name, true, Configs.ServerConfig.Description);
			_ = configSync.AddLockingConfigEntry(serverConfigLocked);
			//testJumpEnabled = CreateConfig("1 - Main", "Test Jump", true, "Test Jump Patch");

			// ===== Grind Reduction
			doubleBronzeEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.DoubleBronzeCrafting.Name, true, Configs.DoubleBronzeCrafting.Description);
			gearRecipeAmountsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.GearRecipeAmountsModifications.Name, true, Configs.GearRecipeAmountsModifications.Description);
			gearRecipeMaterialsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.GearRecipeMaterialsModifications.Name, true, Configs.GearRecipeMaterialsModifications.Description);
			buildPieceAmountsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.BuildPieceAmountsModifications.Name, true, Configs.BuildPieceAmountsModifications.Description);
			buildPieceMaterialsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.BuildPieceMaterialsModifications.Name, true, Configs.BuildPieceMaterialsModifications.Description);
			foodAndMeadModificationsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.FoodAndMeadModifications.Name, true, Configs.FoodAndMeadModifications.Description);

			// ===== Features
			earlyLinenCapeEnabled = CreateConfig(ConfigSections.Features, Configs.LinenCapeModifications.Name, true, Configs.LinenCapeModifications.Description);
			gearSpeedModifiersEnabled = CreateConfig(ConfigSections.Features, Configs.GearSpeedModifications.Name, true, Configs.GearSpeedModifications.Description);
			longerForsakenPowersEnabled = CreateConfig(ConfigSections.Features, Configs.ForsakenPowersModifications.Name, true, Configs.ForsakenPowersModifications.Description);
			fasterCharacterSpeedEnabled = CreateConfig(ConfigSections.Features, Configs.CharacterSpeedModifications.Name, true, Configs.CharacterSpeedModifications.Description);
			shorterStatusEffectsEnabled = CreateConfig(ConfigSections.Features, Configs.StatusEffectsModifications.Name, true, Configs.StatusEffectsModifications.Description);
			lessStaminaUsageEnabled = CreateConfig(ConfigSections.Features, Configs.LessStaminaModifications.Name, true, Configs.LessStaminaModifications.Description);
			extraArmorStatsEnabled = CreateConfig(ConfigSections.Features, Configs.ExtraArmorStatsModifications.Name, true, Configs.ExtraArmorStatsModifications.Description);
			betterDeathRaiserEnabled = CreateConfig(ConfigSections.Features, Configs.DeathRaiserModifications.Name, true, Configs.DeathRaiserModifications.Description);
			betterDeathRaiserSummonsEnabled = CreateConfig(ConfigSections.Features, Configs.DeathRaiserSummonsModifications.Name, true, Configs.DeathRaiserSummonsModifications.Description);
			betterFrostStaffAccuracyEnabled = CreateConfig(ConfigSections.Features, Configs.FrostStaffModifications.Name, true, Configs.FrostStaffModifications.Description);
			reducedCrossbowsReloadTimeEnabled = CreateConfig(ConfigSections.Features, Configs.CrossbowsReloadModifications.Name, true, Configs.CrossbowsReloadModifications.Description);
			lessAshlandsEnemiesEnabled = CreateConfig(ConfigSections.Features, Configs.AshlandsEnemiesModifications.Name, true, Configs.AshlandsEnemiesModifications.Description);
			gearUpgradeUnlockEnabled = CreateConfig(ConfigSections.Features, Configs.GearUpgradeModifications.Name, true, Configs.GearUpgradeModifications.Description);
			permanentLightsEnabled = CreateConfig(ConfigSections.Features, Configs.PermanentLightsModifications.Name, true, Configs.PermanentLightsModifications.Description);
			clearMistlandsEnabled = CreateConfig(ConfigSections.Features, Configs.ClearMistlands.Name, true, Configs.ClearMistlands.Description);
			craftableChainEnabled = CreateConfig(ConfigSections.Features, Configs.CraftableChain.Name, true, Configs.CraftableChain.Description);
			brighterLanternsEnabled = CreateConfig(ConfigSections.Features, Configs.BrighterLanterns.Name, true, Configs.BrighterLanterns.Description);
			clearerWeatherEnabled = CreateConfig(ConfigSections.Features, Configs.WeatherModifications.Name, true, Configs.WeatherModifications.Description);
			creatureUnlevelerEnabled = CreateConfig(ConfigSections.Features, Configs.CreatureUnleveler.Name, true, Configs.CreatureUnleveler.Description);
			minibossWeightEnabled = CreateConfig(ConfigSections.Features, Configs.MinibossWeight.Name, true, Configs.MinibossWeight.Description);

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
							DoubleBronzeCrafting.UpdateDoubleBronzeCrafting(ObjectDB.instance, true);
							break;

						case var name when name == Configs.LighterMetalWeight.Name:
							LighterMetalWeight.UpdateLighterMetalWeight(ObjectDB.instance, true);
							break;

						case var name when name == Configs.GearRecipeAmountsModifications.Name:
							GearRecipeChanges.UpdateGearRecipes(ObjectDB.instance, true, false);
							break;

						case var name when name == Configs.GearRecipeMaterialsModifications.Name:
							GearRecipeChanges.UpdateGearRecipes(ObjectDB.instance, false, true);
							break;

						case var name when name == Configs.BuildPieceAmountsModifications.Name:
							BuildPieceChanges.UpdateBuildPieces(ZNetScene.instance, true, false);
							break;

						case var name when name == Configs.BuildPieceMaterialsModifications.Name:
							BuildPieceChanges.UpdateBuildPieces(ZNetScene.instance, false, true);
							break;

						case var name when name == Configs.FoodAndMeadModifications.Name:
							FoodAndMeadChanges.UpdateFoodAndMead(ObjectDB.instance, true);
							break;

						case var name when name == Configs.LinenCapeModifications.Name:
							GearRecipeChanges.UpdateLinenCapeRecipe(ObjectDB.instance, true);
							EarlyLinenCape.UpdateLinenCapeStats(true);
							break;

						case var name when name == Configs.GearSpeedModifications.Name:
							GearSpeedChanges.UpdateGearSpeed(ObjectDB.instance, true);
							break;

						case var name when name == Configs.ForsakenPowersModifications.Name:
							ForsakenPowersChanges.UpdateForsakenPowers(ObjectDB.instance, true);
							break;

						case var name when name == Configs.CharacterSpeedModifications.Name:
							if (Player.m_localPlayer != null)
							{
								CharacterSpeedChanges.UpdateCharacterSpeed(Player.m_localPlayer, true);								
							}
							break;

						case var name when name == Configs.StatusEffectsModifications.Name:
							StatusEffectChanges.UpdateStatusEffects(ObjectDB.instance, true);
							break;

						case var name when name == Configs.GearUpgradeModifications.Name:
							GearUpgradeChanges.UpdateGearRecipeUnlock(ObjectDB.instance, true);
							break;

						case var name when name == Configs.PermanentLightsModifications.Name:
							PermanentLightsChanges.UpdateLightBuildPiecesAmounts(ZNetScene.instance, true);
							break;

						case var name when name == Configs.CraftableChain.Name:
							CraftableChain.UpdateChainRecipe(ObjectDB.instance, true);
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
