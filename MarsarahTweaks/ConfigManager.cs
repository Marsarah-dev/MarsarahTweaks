
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

			public static readonly ConfigMetadata LinenCapeModifications = new ConfigMetadata("01 - Early Linen Cape", "Renames the Linen Cape to Fine Cape, moves it to the Swamp biome, and adds poison resist to it. (Toggling mid-game requires CLIENT relog if the item is equipped)");
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
		public static ConfigEntry<bool> ServerConfigLocked;
		//public static ConfigEntry<bool> testJumpEnabled;

		public static ConfigEntry<bool> DoubleBronzeEnabled;
		public static ConfigEntry<bool> GearRecipeAmountsEnabled;
		public static ConfigEntry<bool> GearRecipeMaterialsEnabled;
		public static ConfigEntry<bool> BuildPieceAmountsEnabled;
		public static ConfigEntry<bool> BuildPieceMaterialsEnabled;
		public static ConfigEntry<bool> FoodAndMeadModificationsEnabled;

		public static ConfigEntry<bool> EarlyLinenCapeEnabled;
		public static ConfigEntry<bool> GearSpeedModifiersEnabled;
		public static ConfigEntry<bool> LongerForsakenPowersEnabled;
		public static ConfigEntry<bool> FasterCharacterSpeedEnabled;
		public static ConfigEntry<bool> ShorterStatusEffectsEnabled;
		public static ConfigEntry<bool> LessStaminaUsageEnabled;
		public static ConfigEntry<bool> ExtraArmorStatsEnabled;
		public static ConfigEntry<bool> BetterDeathRaiserEnabled;
		public static ConfigEntry<bool> BetterDeathRaiserSummonsEnabled;
		public static ConfigEntry<bool> BetterFrostStaffAccuracyEnabled;
		public static ConfigEntry<bool> ReducedCrossbowsReloadTimeEnabled;
		public static ConfigEntry<bool> LessAshlandsEnemiesEnabled;
		public static ConfigEntry<bool> GearUpgradeUnlockEnabled;
		public static ConfigEntry<bool> PermanentLightsEnabled;
		public static ConfigEntry<bool> ClearMistlandsEnabled;
		public static ConfigEntry<bool> CraftableChainEnabled;
		public static ConfigEntry<bool> BrighterLanternsEnabled;
		public static ConfigEntry<bool> ClearerWeatherEnabled;
		public static ConfigEntry<bool> CreatureUnlevelerEnabled;
		public static ConfigEntry<bool> MinibossWeightEnabled;
		public static ConfigEntry<bool> ExtensionsChangesEnabled;
		public static ConfigEntry<bool> NoFleeEnabled;
		public static ConfigEntry<bool> AutomaticProgressionHaltEnabled;
		public static ConfigEntry<bool> BetterTrophyDropsEnabled;
		public static ConfigEntry<bool> TougherShipsEnabled;
		public static ConfigEntry<bool> OtherEnabled;

		public static ConfigEntry<bool> LighterMetalWeightEnabled;
		public static ConfigEntry<bool> LargerPickupAreaEnabled;
		public static ConfigEntry<bool> NoSkillLowerOnDeathEnabled;
		public static ConfigEntry<bool> LargerBoatExploreRadiusEnabled;
		public static ConfigEntry<bool> BiggerWispRadiusEnabled;
		public static ConfigEntry<bool> FriendlyBallistasEnabled;
		public static ConfigEntry<bool> LessFallDamageEnabled;
		public static ConfigEntry<bool> FasterResourceDropsEnabled;
		public static ConfigEntry<bool> FasterEquipEnabled;

		public static ConfigEntry<bool> MoreLoadingTipsEnabled;
		public static ConfigEntry<bool> ShowInventoryWeightAndSlots;
		public static ConfigEntry<bool> ShowEnemyDetector;
		public static ConfigEntry<bool> ShowBoatSpeed;
		public static ConfigEntry<bool> ShowTimeAndDay;
		public static ConfigEntry<bool> TimeFormat24H;
		public static ConfigEntry<bool> ShowSmartBiome;
		public static ConfigEntry<bool> ShowSummonCounter;
		public static ConfigEntry<bool> ShowOnlinePlayers;
		public static ConfigEntry<bool> OnlinePlayersUnderMinimap;

		public static void Init(ConfigFile configFile)
		{
			Config = configFile;

			ServerConfigLocked = CreateConfig(ConfigSections.Main, Configs.ServerConfig.Name, true, Configs.ServerConfig.Description);
			_ = configSync.AddLockingConfigEntry(ServerConfigLocked);
			//testJumpEnabled = CreateConfig("1 - Main", "Test Jump", true, "Test Jump Patch");

			// ===== Grind Reduction
			DoubleBronzeEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.DoubleBronzeCrafting.Name, true, Configs.DoubleBronzeCrafting.Description);
			GearRecipeAmountsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.GearRecipeAmountsModifications.Name, true, Configs.GearRecipeAmountsModifications.Description);
			GearRecipeMaterialsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.GearRecipeMaterialsModifications.Name, true, Configs.GearRecipeMaterialsModifications.Description);
			BuildPieceAmountsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.BuildPieceAmountsModifications.Name, true, Configs.BuildPieceAmountsModifications.Description);
			BuildPieceMaterialsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.BuildPieceMaterialsModifications.Name, true, Configs.BuildPieceMaterialsModifications.Description);
			FoodAndMeadModificationsEnabled = CreateConfig(ConfigSections.GrindReduction, Configs.FoodAndMeadModifications.Name, true, Configs.FoodAndMeadModifications.Description);

			// ===== Features
			EarlyLinenCapeEnabled = CreateConfig(ConfigSections.Features, Configs.LinenCapeModifications.Name, true, Configs.LinenCapeModifications.Description);
			GearSpeedModifiersEnabled = CreateConfig(ConfigSections.Features, Configs.GearSpeedModifications.Name, true, Configs.GearSpeedModifications.Description);
			LongerForsakenPowersEnabled = CreateConfig(ConfigSections.Features, Configs.ForsakenPowersModifications.Name, true, Configs.ForsakenPowersModifications.Description);
			FasterCharacterSpeedEnabled = CreateConfig(ConfigSections.Features, Configs.CharacterSpeedModifications.Name, true, Configs.CharacterSpeedModifications.Description);
			ShorterStatusEffectsEnabled = CreateConfig(ConfigSections.Features, Configs.StatusEffectsModifications.Name, true, Configs.StatusEffectsModifications.Description);
			LessStaminaUsageEnabled = CreateConfig(ConfigSections.Features, Configs.LessStaminaModifications.Name, true, Configs.LessStaminaModifications.Description);
			ExtraArmorStatsEnabled = CreateConfig(ConfigSections.Features, Configs.ExtraArmorStatsModifications.Name, true, Configs.ExtraArmorStatsModifications.Description);
			BetterDeathRaiserEnabled = CreateConfig(ConfigSections.Features, Configs.DeathRaiserModifications.Name, true, Configs.DeathRaiserModifications.Description);
			BetterDeathRaiserSummonsEnabled = CreateConfig(ConfigSections.Features, Configs.DeathRaiserSummonsModifications.Name, true, Configs.DeathRaiserSummonsModifications.Description);
			BetterFrostStaffAccuracyEnabled = CreateConfig(ConfigSections.Features, Configs.FrostStaffModifications.Name, true, Configs.FrostStaffModifications.Description);
			ReducedCrossbowsReloadTimeEnabled = CreateConfig(ConfigSections.Features, Configs.CrossbowsReloadModifications.Name, true, Configs.CrossbowsReloadModifications.Description);
			LessAshlandsEnemiesEnabled = CreateConfig(ConfigSections.Features, Configs.AshlandsEnemiesModifications.Name, true, Configs.AshlandsEnemiesModifications.Description);
			GearUpgradeUnlockEnabled = CreateConfig(ConfigSections.Features, Configs.GearUpgradeModifications.Name, true, Configs.GearUpgradeModifications.Description);
			PermanentLightsEnabled = CreateConfig(ConfigSections.Features, Configs.PermanentLightsModifications.Name, true, Configs.PermanentLightsModifications.Description);
			ClearMistlandsEnabled = CreateConfig(ConfigSections.Features, Configs.ClearMistlands.Name, true, Configs.ClearMistlands.Description);
			CraftableChainEnabled = CreateConfig(ConfigSections.Features, Configs.CraftableChain.Name, true, Configs.CraftableChain.Description);
			BrighterLanternsEnabled = CreateConfig(ConfigSections.Features, Configs.BrighterLanterns.Name, true, Configs.BrighterLanterns.Description);
			ClearerWeatherEnabled = CreateConfig(ConfigSections.Features, Configs.WeatherModifications.Name, true, Configs.WeatherModifications.Description);
			CreatureUnlevelerEnabled = CreateConfig(ConfigSections.Features, Configs.CreatureUnleveler.Name, true, Configs.CreatureUnleveler.Description);
			MinibossWeightEnabled = CreateConfig(ConfigSections.Features, Configs.MinibossWeight.Name, true, Configs.MinibossWeight.Description);
			ExtensionsChangesEnabled = CreateConfig(ConfigSections.Features, Configs.ExtensionsModifications.Name, true, Configs.ExtensionsModifications.Description);
			NoFleeEnabled = CreateConfig(ConfigSections.Features, Configs.FleeAIModifications.Name, true, Configs.FleeAIModifications.Description);
			AutomaticProgressionHaltEnabled = CreateConfig(ConfigSections.Features, Configs.ProgressionHalt.Name, true, Configs.ProgressionHalt.Description);
			BetterTrophyDropsEnabled = CreateConfig(ConfigSections.Features, Configs.TrophyDropsModifications.Name, true, Configs.TrophyDropsModifications.Description);
			TougherShipsEnabled = CreateConfig(ConfigSections.Features, Configs.TougherShips.Name, true, Configs.TougherShips.Description);
			OtherEnabled = CreateConfig(ConfigSections.Features, Configs.OtherModifications.Name, true, Configs.OtherModifications.Description);

			// ===== QOL
			LighterMetalWeightEnabled = CreateConfig(ConfigSections.QOL, Configs.LighterMetalWeight.Name, true, Configs.LighterMetalWeight.Description);
			LargerPickupAreaEnabled = CreateConfig(ConfigSections.QOL, Configs.LargerPickupArea.Name, true, Configs.LargerPickupArea.Description);
			NoSkillLowerOnDeathEnabled = CreateConfig(ConfigSections.QOL, Configs.NoSkillLoss.Name, true, Configs.NoSkillLoss.Description);
			LargerBoatExploreRadiusEnabled = CreateConfig(ConfigSections.QOL, Configs.LargerBoatExploreRadius.Name, true, Configs.LargerBoatExploreRadius.Description);
			BiggerWispRadiusEnabled = CreateConfig(ConfigSections.QOL, Configs.BiggerWispRadius.Name, true, Configs.BiggerWispRadius.Description);
			FriendlyBallistasEnabled = CreateConfig(ConfigSections.QOL, Configs.FriendlyBallistas.Name, true, Configs.FriendlyBallistas.Description);
			LessFallDamageEnabled = CreateConfig(ConfigSections.QOL, Configs.LessFallDamage.Name, true, Configs.LessFallDamage.Description);
			FasterResourceDropsEnabled = CreateConfig(ConfigSections.QOL, Configs.FasterResourceDrops.Name, true, Configs.FasterResourceDrops.Description);
			FasterEquipEnabled = CreateConfig(ConfigSections.QOL, Configs.FasterEquip.Name, true, Configs.FasterEquip.Description);

			// ===== UI
			MoreLoadingTipsEnabled = CreateConfig(ConfigSections.UI, Configs.UIMoreLoadingTips.Name, true, Configs.UIMoreLoadingTips.Description, false);
			ShowInventoryWeightAndSlots = CreateConfig(ConfigSections.UI, Configs.UIInventoryWeightAndSlots.Name, true, Configs.UIInventoryWeightAndSlots.Description, false);
			ShowEnemyDetector = CreateConfig(ConfigSections.UI, Configs.UIEnemyDetector.Name, true, Configs.UIEnemyDetector.Description, false);
			ShowBoatSpeed = CreateConfig(ConfigSections.UI, Configs.UIBoatSpeed.Name, true, Configs.UIBoatSpeed.Description, false);
			ShowTimeAndDay = CreateConfig(ConfigSections.UI, Configs.UITimeAndDay.Name, true, Configs.UITimeAndDay.Description, false);
			TimeFormat24H = CreateConfig(ConfigSections.UI, Configs.UITimeAndDay24H.Name, false, Configs.UITimeAndDay24H.Description, false);
			ShowSmartBiome = CreateConfig(ConfigSections.UI, Configs.UISmartBiome.Name, true, Configs.UISmartBiome.Description, false);
			ShowSummonCounter = CreateConfig(ConfigSections.UI, Configs.UISummonCounter.Name, true, Configs.UISummonCounter.Description, false);
			ShowOnlinePlayers = CreateConfig(ConfigSections.UI, Configs.UIOnlinePlayers.Name, true, Configs.UIOnlinePlayers.Description, false);
			OnlinePlayersUnderMinimap = CreateConfig(ConfigSections.UI, Configs.UIOnlinePlayersUnderMinimap.Name, true, Configs.UIOnlinePlayersUnderMinimap.Description, false);

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
						if (ShowSmartBiome.Value)
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
						TougherShipsChanges.UpdateShipHP(ZNetScene.instance, true);
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
						TougherShipsChanges.UpdateShipHP(ZNetScene.instance, true);
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
