
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
using MarsarahTweaks.Patches.Grind;
using MarsarahTweaks.Patches.Features;
using MarsarahTweaks.Patches.QOL;
using MarsarahTweaks.Patches.UI;

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
			public const string GrindReduction = "2 - Grind Reduction (Synced with Server)";
			public const string Features = "3 - Features (Synced with Server)";
			public const string QOL = "4 - QOL (Synced with Server)";
			public const string UI = "5 - UI (NOT Synced with Server)";
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
			public static readonly ConfigMetadata FoodAndMeadModifications = new ConfigMetadata("6 - Food And Mead Modifications", "Modifies food and mead recipes costs, crafted amounts and stacks. (Toggling mid-game requires CLIENT relog for food stacks)");

			public static readonly ConfigMetadata LinenCapeModifications = new ConfigMetadata("01 - Early Linen Cape", "Renames the Linen Cape to Fine Cape, moves it to the Swamp biome, and adds poison resist to it");
			public static readonly ConfigMetadata GearSpeedModifications = new ConfigMetadata("02 - Gear Speed Modifications", "Removes speed penalty for heavy and mage armors; adds speed bonus to light armor. (Toggling mid-game requires CLIENT relog)");
			public static readonly ConfigMetadata ForsakenPowersModifications = new ConfigMetadata("03 - Forsaken Powers Modifications", "Reduce Forsaken Powers cooldowns and increase durations (different for each power)");
			public static readonly ConfigMetadata CharacterSpeedModifications = new ConfigMetadata("04 - Faster Character Speed", "Faster character jog, walk, swim and crouch speeds (run excluded)");
			public static readonly ConfigMetadata StatusEffectsModifications = new ConfigMetadata("05 - Shorter Wet And Potion Cooldowns", "Wet effect and potions cooldown timers reduced");
			public static readonly ConfigMetadata LessStaminaModifications = new ConfigMetadata("06 - Less Stamina Usage", "Stamina use of all actions is reduced by 15%");
			public static readonly ConfigMetadata ExtraArmorStatsModifications = new ConfigMetadata("07 - Extra Armor Stats", "Heavy armor provides extra HP, light armor provides extra stamina, mage armor provides extra eitr");
			public static readonly ConfigMetadata DeathRaiserModifications = new ConfigMetadata("08 - Better Death Raiser", "Adds a secondary attack that spawns archer skeletons. Primary attack will only spawn melee skeletons");
			public static readonly ConfigMetadata DeathRaiserSummonsModifications = new ConfigMetadata("09 - Better Summoned Skeletons", "Increases summoned skeleton speed and add better looking gear according to Death Raiser level (stats not affected). (Toggling mid-game requires CLIENT relog for speed changes)");
			public static readonly ConfigMetadata FrostStaffModifications = new ConfigMetadata("10 - Better Frost Staff Accuracy", "Improves Staff of Frost Accuracy");
			public static readonly ConfigMetadata CrossbowsReloadModifications = new ConfigMetadata("11 - Reduced Crossbows Reload Time", "Crossbows Reload Time Reduced by 1s. (Toggling mid-game requires CLIENT relog)");
			public static readonly ConfigMetadata AshlandsEnemiesModifications = new ConfigMetadata("12 - Less Ashlands Enemies", "Numbers and spawn chance reduced for Ashlands enemies. (Toggling mid-game requires reloading area)");
			public static readonly ConfigMetadata GearUpgradeModifications = new ConfigMetadata("13 - Gear Upgrade Unlock", "Gear from Meadows, Black Forest, Mistlands and Ashlands can be upgraded to max level within their respective biomes");
			public static readonly ConfigMetadata PermanentLightsModifications = new ConfigMetadata("14 - Permanent Lights", "Makes all light sources permanent, but the build costs of light source pieces use maximum amount of their respective fuel type");
			public static readonly ConfigMetadata ClearMistlands = new ConfigMetadata("15 - Clear Mistlands", "Clear Mistlands mist after defeating the Queen. (Disabling mid-game requires CLIENT relog)");
			public static readonly ConfigMetadata CraftableChain = new ConfigMetadata("16 - Craftable Chain", "Chain craftable at Black Forge");
			public static readonly ConfigMetadata BrighterLanterns = new ConfigMetadata("17 - Brighter Lanterns", "Dvergr lanterns are brighter. (Toggling mid-game requires CLIENT relog or reloading area)");
			public static readonly ConfigMetadata WeatherModifications = new ConfigMetadata("18 - Clearer Weather", "Reduces chance for mist and snowstorms in Meadows, Plains, Ocean and Mountains respectively. (Toggling mid-game requires CLIENT relog)");
			public static readonly ConfigMetadata CreatureUnleveler = new ConfigMetadata("19 - Creature Unleveler By Boss", "Increases chance of creatures to spawn with a star or two after defeating their relevant biome boss");
			public static readonly ConfigMetadata MinibossWeight = new ConfigMetadata("20 - Hildir Weight Rewards", "Increases base carry weight by 25 when turning in Hildir chests (for each chest). (Toggling mid-game requires CLIENT relog)");
			public static readonly ConfigMetadata ExtensionsModifications = new ConfigMetadata("21 - Station Extensions Changes", "Decreases space requirement for workstation extensions and increases build distance to workstations (This does not increase workstation radius)");
			public static readonly ConfigMetadata FleeAIModifications = new ConfigMetadata("22 - Stop Running Away", "Boars and Necks won't flee when alerted. (Toggling mid-game only affects new creatures)");
			public static readonly ConfigMetadata ProgressionHalt = new ConfigMetadata("23 - Automatic Progression Halt", "Creatures and objects do not drop any items unless the previous biome boss has been defeated. (Toggling mid-game requires CLIENT relog or reloading area)");
			public static readonly ConfigMetadata TrophyDropsModifications = new ConfigMetadata("24 - Better Trophy Drop Rates", "Increases trophy drop rate for the following creatures: Rancid Remains, Surtling, Draugr Elite, Wraith, Cultist, Fenring, Stone Golem, Deathsquito, Fuling Berserker, Tick, Dverger, Seeker Soldier, Charred Warlock");
			public static readonly ConfigMetadata TougherShips = new ConfigMetadata("25 - Tougher Ships", "Increases Ships HP. Raft: 300 -> 400, Karve: 500 -> 650, Longship: 1000 -> 1250, Drakkar: 3000 -> 4000 (Toggling mid-game requires CLIENT relog or reloading area)");
			public static readonly ConfigMetadata OtherModifications = new ConfigMetadata("26 - Other Section", "Tankard costs reduced and Iron Nails crafting output doubled");

			public static readonly ConfigMetadata LighterMetalWeight = new ConfigMetadata("1 - Lighter Metal Weight", "All metal ore and bars weight decreased to 8. (Toggling mid-game requires CLIENT relog or reloading area)");
			public static readonly ConfigMetadata LargerPickupArea = new ConfigMetadata("2 - Larger Pickup Area", "Item pickup area slightly increased");
			public static readonly ConfigMetadata NoSkillLoss = new ConfigMetadata("3 - No Skill Levels Loss On Death", "Skills won't go down the current level upon death (progress in that skill is still lost). (Toggling mid-game requires CLIENT relog)");
			public static readonly ConfigMetadata LargerBoatExploreRadius = new ConfigMetadata("4 - Larger Boat Explore Radius", "Double explore radius on a boat");
			public static readonly ConfigMetadata BiggerWispRadius = new ConfigMetadata("5 - Bigger Wisp Radius", "Increases wisp radius");
			public static readonly ConfigMetadata FriendlyBallistas = new ConfigMetadata("6 - Friendly Ballistas", "Ballistas won't target players and tame animals");
			public static readonly ConfigMetadata LessFallDamage = new ConfigMetadata("7 - Less Fall Damage", "Fall damage reduced by 40%");
			public static readonly ConfigMetadata FasterResourceDrops = new ConfigMetadata("8 - Faster Resource Drops", "Enemies drop resources faster when dying");
			public static readonly ConfigMetadata FasterEquip = new ConfigMetadata("9 - Faster Equip", "Equipping weapons is instant. Armor equip timers reduced to 1s (from 1s/2s). (Toggling mid-game requires CLIENT relog)");

			public static readonly ConfigMetadata UIMoreLoadingTips = new ConfigMetadata("01 - More Loading Tips", "More loading screen tips");
			public static readonly ConfigMetadata UIInventoryWeightAndSlots = new ConfigMetadata("02 - Show Inventory Weight and Free Slots", "Shows inventory weight and free slots on the bottom left of the screen");
			public static readonly ConfigMetadata UIEnemyDetector = new ConfigMetadata("03 - Show Enemy Detector", "Shows enemy detector on the bottom left of the screen");
			public static readonly ConfigMetadata UIBoatSpeed = new ConfigMetadata("04 - Show Boat Speed", "Shows boat speed when using a boat on the bottom left of the screen");
			public static readonly ConfigMetadata UITimeAndDay = new ConfigMetadata("05 - Show Time And Day", "Shows time and day above the minimap");
			public static readonly ConfigMetadata UITimeAndDay24H = new ConfigMetadata("06 - Time - 24 Hour Format", "Use 24 Hour time format when Show Time And Day is enabled");
			public static readonly ConfigMetadata UISmartBiome = new ConfigMetadata("07 - Smart Biome Indicator", "Shows smart biome text on minimap (colored according to worn armor relative to current biome)");
			public static readonly ConfigMetadata UISummonCounter = new ConfigMetadata("08 - Show Summon Counter", "Shows number of summoned skeletons from the Dead Raiser");
			public static readonly ConfigMetadata UIOnlinePlayers = new ConfigMetadata("09 - Show Online Players", "Shows online players on the bottom right of the screen (Not displayed if only one player is online)");
			public static readonly ConfigMetadata UIOnlinePlayersUnderMinimap = new ConfigMetadata("10 - Show Online Players Under Minimap", "Shows online players under minimap instead of bottom right when Show Online Players is enabled");
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
		public static ConfigEntry<bool> extensionsChangesEnabled;
		public static ConfigEntry<bool> noFleeEnabled;
		public static ConfigEntry<bool> automaticProgressionHaltEnabled;
		public static ConfigEntry<bool> betterTrophyDropsEnabled;
		public static ConfigEntry<bool> tougherShipsEnabled;
		public static ConfigEntry<bool> otherEnabled;

		public static ConfigEntry<bool> lighterMetalWeightEnabled;
		public static ConfigEntry<bool> largerPickupAreaEnabled;
		public static ConfigEntry<bool> noSkillLowerOnDeathEnabled;
		public static ConfigEntry<bool> largerBoatExploreRadiusEnabled;
		public static ConfigEntry<bool> biggerWispRadiusEnabled;
		public static ConfigEntry<bool> friendlyBallistasEnabled;
		public static ConfigEntry<bool> lessFallDamageEnabled;
		public static ConfigEntry<bool> fasterResourceDropsEnabled;
		public static ConfigEntry<bool> fasterEquipEnabled;

		public static ConfigEntry<bool> moreLoadingTipsEnabled;
		public static ConfigEntry<bool> showInventoryWeightAndSlots;
		public static ConfigEntry<bool> showEnemyDetector;
		public static ConfigEntry<bool> showBoatSpeed;
		public static ConfigEntry<bool> showTimeAndDay;
		public static ConfigEntry<bool> timeFormat24H;
		public static ConfigEntry<bool> showSmartBiome;
		public static ConfigEntry<bool> showSummonCounter;
		public static ConfigEntry<bool> showOnlinePlayers;
		public static ConfigEntry<bool> onlinePlayersUnderMinimap;

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
			extensionsChangesEnabled = CreateConfig(ConfigSections.Features, Configs.ExtensionsModifications.Name, true, Configs.ExtensionsModifications.Description);
			noFleeEnabled = CreateConfig(ConfigSections.Features, Configs.FleeAIModifications.Name, true, Configs.FleeAIModifications.Description);
			automaticProgressionHaltEnabled = CreateConfig(ConfigSections.Features, Configs.ProgressionHalt.Name, true, Configs.ProgressionHalt.Description);
			betterTrophyDropsEnabled = CreateConfig(ConfigSections.Features, Configs.TrophyDropsModifications.Name, true, Configs.TrophyDropsModifications.Description);
			tougherShipsEnabled = CreateConfig(ConfigSections.Features, Configs.TougherShips.Name, true, Configs.TougherShips.Description);
			otherEnabled = CreateConfig(ConfigSections.Features, Configs.OtherModifications.Name, true, Configs.OtherModifications.Description);

			// ===== QOL
			lighterMetalWeightEnabled = CreateConfig(ConfigSections.QOL, Configs.LighterMetalWeight.Name, true, Configs.LighterMetalWeight.Description);
			largerPickupAreaEnabled = CreateConfig(ConfigSections.QOL, Configs.LargerPickupArea.Name, true, Configs.LargerPickupArea.Description);
			noSkillLowerOnDeathEnabled = CreateConfig(ConfigSections.QOL, Configs.NoSkillLoss.Name, true, Configs.NoSkillLoss.Description);
			largerBoatExploreRadiusEnabled = CreateConfig(ConfigSections.QOL, Configs.LargerBoatExploreRadius.Name, true, Configs.LargerBoatExploreRadius.Description);
			biggerWispRadiusEnabled = CreateConfig(ConfigSections.QOL, Configs.BiggerWispRadius.Name, true, Configs.BiggerWispRadius.Description);
			friendlyBallistasEnabled = CreateConfig(ConfigSections.QOL, Configs.FriendlyBallistas.Name, true, Configs.FriendlyBallistas.Description);
			lessFallDamageEnabled = CreateConfig(ConfigSections.QOL, Configs.LessFallDamage.Name, true, Configs.LessFallDamage.Description);
			fasterResourceDropsEnabled = CreateConfig(ConfigSections.QOL, Configs.FasterResourceDrops.Name, true, Configs.FasterResourceDrops.Description);
			fasterEquipEnabled = CreateConfig(ConfigSections.QOL, Configs.FasterEquip.Name, true, Configs.FasterEquip.Description);

			// ===== UI
			moreLoadingTipsEnabled = CreateConfig(ConfigSections.UI, Configs.UIMoreLoadingTips.Name, true, Configs.UIMoreLoadingTips.Description, false);
			showInventoryWeightAndSlots = CreateConfig(ConfigSections.UI, Configs.UIInventoryWeightAndSlots.Name, true, Configs.UIInventoryWeightAndSlots.Description, false);
			showEnemyDetector = CreateConfig(ConfigSections.UI, Configs.UIEnemyDetector.Name, true, Configs.UIEnemyDetector.Description, false);
			showBoatSpeed = CreateConfig(ConfigSections.UI, Configs.UIBoatSpeed.Name, true, Configs.UIBoatSpeed.Description, false);
			showTimeAndDay = CreateConfig(ConfigSections.UI, Configs.UITimeAndDay.Name, true, Configs.UITimeAndDay.Description, false);
			timeFormat24H = CreateConfig(ConfigSections.UI, Configs.UITimeAndDay24H.Name, false, Configs.UITimeAndDay24H.Description, false);
			showSmartBiome = CreateConfig(ConfigSections.UI, Configs.UISmartBiome.Name, true, Configs.UISmartBiome.Description, false);
			showSummonCounter = CreateConfig(ConfigSections.UI, Configs.UISummonCounter.Name, true, Configs.UISummonCounter.Description, false);
			showOnlinePlayers = CreateConfig(ConfigSections.UI, Configs.UIOnlinePlayers.Name, true, Configs.UIOnlinePlayers.Description, false);
			onlinePlayersUnderMinimap = CreateConfig(ConfigSections.UI, Configs.UIOnlinePlayersUnderMinimap.Name, true, Configs.UIOnlinePlayersUnderMinimap.Description, false);

			SetupWatcher();
		}

		private static ConfigEntry<T> CreateConfig<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true)
		{
			//var configEntry = Config.Bind(group, name, defaultValue, new ConfigDescription(description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]")));
			var configEntry = Config.Bind(group, name, defaultValue, new ConfigDescription(description));

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
			//MarsarahTweaks.MLog($"Config setting '{configName}' changed!");
			Config.Save();

			if (ObjectDB.instance == null || ZNetScene.instance == null) return;
			if (ZNet.instance == null) return;

			
			bool isDedicatedServer = ZNet.instance.IsDedicated();
			bool isServer = ZNet.instance.IsServer();

			// Handle client-side (non-dedicated) configs
			if (!isDedicatedServer)
			{
				switch (configName)
				{
					case var name when name == Configs.DoubleBronzeCrafting.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						DoubleBronzeCrafting.UpdateDoubleBronzeCrafting(ObjectDB.instance, true);
						break;

					case var name when name == Configs.GearRecipeAmountsModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						GearRecipeChanges.UpdateGearRecipes(ObjectDB.instance, true, false);
						break;

					case var name when name == Configs.GearRecipeMaterialsModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						GearRecipeChanges.UpdateGearRecipes(ObjectDB.instance, false, true);
						break;

					case var name when name == Configs.BuildPieceAmountsModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						BuildPieceChanges.UpdateBuildPieces(ZNetScene.instance, true, false);
						break;

					case var name when name == Configs.BuildPieceMaterialsModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						BuildPieceChanges.UpdateBuildPieces(ZNetScene.instance, false, true);
						break;

					case var name when name == Configs.FoodAndMeadModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						FoodAndMeadChanges.UpdateFoodAndMead(ObjectDB.instance, true);
						break;

					case var name when name == Configs.LinenCapeModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						GearRecipeChanges.UpdateLinenCapeRecipe(ObjectDB.instance, true);
						EarlyLinenCape.UpdateLinenCapeStats(true);
						break;

					case var name when name == Configs.GearSpeedModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						GearSpeedChanges.UpdateGearSpeed(ObjectDB.instance, true);
						break;

					case var name when name == Configs.ForsakenPowersModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						ForsakenPowersChanges.UpdateForsakenPowers(ObjectDB.instance, true);
						break;

					case var name when name == Configs.CharacterSpeedModifications.Name:
						if (Player.m_localPlayer != null)
						{
							//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
							CharacterSpeedChanges.UpdateCharacterSpeed(Player.m_localPlayer, true);								
						}
						break;

					case var name when name == Configs.StatusEffectsModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						StatusEffectChanges.UpdateStatusEffects(ObjectDB.instance, true);
						break;

					case var name when name == Configs.GearUpgradeModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						GearUpgradeChanges.UpdateGearRecipeUnlock(ObjectDB.instance, true);
						if (showSmartBiome.Value)
						{
							UISmartBiome.UpdateBiomeWeights();
						}
						break;

					case var name when name == Configs.PermanentLightsModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						PermanentLightsChanges.UpdateLightBuildPiecesAmounts(ZNetScene.instance, true);
						break;

					case var name when name == Configs.CraftableChain.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						CraftableChain.UpdateChainRecipe(ObjectDB.instance, true);
						break;

					case var name when name == Configs.BrighterLanterns.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						BrighterLanterns.UpdateLanterns(ZNetScene.instance);
						break;

					case var name when name == Configs.TougherShips.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						TougherShipsChanges.updateShipHP(ZNetScene.instance, true);
						break;

					case var name when name == Configs.OtherModifications.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						OtherChanges.UpdateOthers(ObjectDB.instance, true);
						break;

					case var name when name == Configs.LighterMetalWeight.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						LighterMetalWeight.UpdateLighterMetalWeight(ObjectDB.instance, true);
						break;

					case var name when name == Configs.LargerPickupArea.Name:
						if (Player.m_localPlayer != null)
						{
							//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
							LargerPickupAreaChanges.UpdatePickupArea(Player.m_localPlayer, true);
						}
						break;

					case var name when name == Configs.UIInventoryWeightAndSlots.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						UIController.UpdateUIPositions();
						break;

					case var name when name == Configs.UIEnemyDetector.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						UIController.UpdateUIPositions();
						break;

					case var name when name == Configs.UIBoatSpeed.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						UIController.UpdateUIPositions();
						break;

					case var name when name == Configs.UISmartBiome.Name:
						//MarsarahTweaks.MLog($"ConfigManager: Reapplying modifications for {configName}...");
						UISmartBiome.UpdateBiomeWeights();
						break;
				}
			}

			// Handle server-side configs
			/*if (isServer)
			{
				switch (configName)
				{
					case var name when name == Configs.TougherShips.Name:
						MarsarahTweaks.MLog($"ConfigManager Server: Reapplying modifications for {configName}...");
						TougherShipsChanges.updateShipHP(ZNetScene.instance, true);
						break;
				}
			}*/

			// Handle both client aand server-side configs
			switch (configName)
			{
				case var name when name == Configs.AshlandsEnemiesModifications.Name:
					if (Object.FindObjectOfType<SpawnSystem>() is SpawnSystem spawnSystemA)
					{
						//MarsarahTweaks.MLog($"ConfigManager Server: Reapplying modifications for {configName}...");
						AshlandsEnemiesChanges.UpdateAshlandsSpawns(spawnSystemA);
					}
					break;

				case var name when name == Configs.CreatureUnleveler.Name:
					if (Object.FindObjectOfType<SpawnSystem>() is SpawnSystem spawnSystemB)
					{
						//MarsarahTweaks.MLog($"ConfigManager Server: Reapplying modifications for {configName}...");
						CreatureUnleveler.ApplyCreatureLevelChanges(spawnSystemB);
					}
					break;
			}


			/*else
            {
				MarsarahTweaks.MLog($"ConfigManager: I am a server. No changes made to {configName}...");
			}*/
		}
	}
}
