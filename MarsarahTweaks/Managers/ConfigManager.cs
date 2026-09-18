
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MarsarahTweaks.Patches;
using MarsarahTweaks.Patches.Balance;
using MarsarahTweaks.Patches.Features;
using MarsarahTweaks.Patches.Grind;
using MarsarahTweaks.Patches.QOL;
using ServerSync;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using UnityEngine;

namespace MarsarahTweaks.Managers
{
	public static class ConfigManager
	{
		private static readonly LogManager log = new LogManager("Config Manager", LogManager.LogLevel.Warning);

		private static ConfigFile Config;
		private static readonly ConfigSync configSync = new ConfigSync(MarsarahTweaks.ModGUID)
		{
			DisplayName = MarsarahTweaks.ModName,
			CurrentVersion = MarsarahTweaks.ModVersion,
			MinimumRequiredVersion = MarsarahTweaks.ModVersion
		};

		public static bool IsAdminOrHost => configSync.IsAdmin;

		// Config file stuff
		private static string ConfigFileName => MarsarahTweaks.ModGUID + ".cfg";
		private static string ConfigFileFullPath => Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

		// Struct for Config Sections
		public static class ConfigSections
		{
			public const string Main = "1 - Main";
			public const string GrindReduction = "2 - Grind Reduction (Synced with Server)";
			public const string Balance = "3 - Balance (Synced with Server)";
			public const string Features = "4 - Features (Synced with Server)";
			public const string QOL = "5 - QOL (Synced with Server)";
		}

		// Struct for Config Metadata
		public struct ConfigMetadata
		{
			public string Name;
			public string Description;
			public string Section;
			public int Order;

			public ConfigMetadata(string name, string description, string section, int order)
			{
				Name = name;
				Description = description;
				Section = section;
				Order = order;
			}
		}

		private sealed class ConfigurationManagerAttributes
		{
			public int? Order;
		}


		// Grouped Config Metadata (for easy expansion)
		public static class Configs
		{
			public static readonly ConfigMetadata ServerConfig = new ConfigMetadata("Lock Configuration", "If on, only server admins can change the configuration.", ConfigSections.Main, 10);

			// Grind Reduction
			public static readonly ConfigMetadata DoubleBronzeCrafting = new ConfigMetadata("Double Bronze Crafting", "Doubles the amount of crafted Bronze at the Forge", ConfigSections.GrindReduction, 60);
			public static readonly ConfigMetadata GearRecipeAmountsModifications = new ConfigMetadata("Cheaper Gear Recipe Amounts", "Reduces costs for crafting and upgrading gear for metal. Balances other resources amounts", ConfigSections.GrindReduction, 50);
			public static readonly ConfigMetadata GearRecipeMaterialsModifications = new ConfigMetadata("Alternate Gear Recipe Materials", "Modifies gear recipe materials for some items (more materials from current respective biomes)", ConfigSections.GrindReduction, 40);
			public static readonly ConfigMetadata BuildPieceAmountsModifications = new ConfigMetadata("Cheaper Build Pieces Amounts", "Reduces costs for build pieces", ConfigSections.GrindReduction, 30);
			public static readonly ConfigMetadata BuildPieceMaterialsModifications = new ConfigMetadata("Alternate Build Pieces Materials", "Modifies build pieces materials (This will move the Workbench Toolrack extension from Mountain to Swamp biome)", ConfigSections.GrindReduction, 20);
			public static readonly ConfigMetadata FoodAndMeadModifications = new ConfigMetadata("Food And Mead Modifications", "Modifies food and mead recipes costs, crafted amounts and stacks. (Toggling mid-game requires CLIENT relog for food stacks)", ConfigSections.GrindReduction, 10);

			// Balance
			public static readonly ConfigMetadata LinenCapeModifications = new ConfigMetadata("Early Linen Cape", "Renames the Linen Cape to Fine Cape, moves it to the Swamp biome, and adds poison resist to it. (Toggling mid-game requires CLIENT relog if the item is equipped)", ConfigSections.Balance, 140);
			public static readonly ConfigMetadata GearSpeedModifications = new ConfigMetadata("Gear Speed Modifications", "Removes speed penalty for heavy and mage armors; adds speed bonus to light armor. (Toggling mid-game requires CLIENT relog)", ConfigSections.Balance, 130);
			public static readonly ConfigMetadata ExtraArmorStatsModifications = new ConfigMetadata("Extra Armor Stats", "Heavy armor provides extra HP, light armor provides extra stamina, mage armor provides extra eitr", ConfigSections.Balance, 120);
			public static readonly ConfigMetadata GearUpgradeModifications = new ConfigMetadata("Gear Upgrade Unlock", "Gear from Meadows, Black Forest, Mistlands and Ashlands can be upgraded to max level within their respective biomes. Moves the Vilecage set to the Workbench", ConfigSections.Balance, 110);
			public static readonly ConfigMetadata ForsakenPowersModifications = new ConfigMetadata("Forsaken Powers Cooldowns", "Reduce Forsaken Powers cooldowns and increase durations (different for each power)", ConfigSections.Balance, 100);
			public static readonly ConfigMetadata CharacterSpeedModifications = new ConfigMetadata("Faster Character Speed", "Faster character jog, walk, swim and crouch speeds (run excluded)", ConfigSections.Balance, 90);
			public static readonly ConfigMetadata StatusEffectsModifications = new ConfigMetadata("Shorter Wet And Potion Cooldowns", "Wet effect and potions cooldown timers reduced", ConfigSections.Balance, 80);
			public static readonly ConfigMetadata LessStaminaModifications = new ConfigMetadata("Less Stamina Usage", "Stamina use of all actions is reduced by 15%", ConfigSections.Balance, 70);
			public static readonly ConfigMetadata DeathRaiserModifications = new ConfigMetadata("Better Death Raiser", "Adds a secondary attack that spawns archer skeletons. Primary attack will only spawn melee skeletons", ConfigSections.Balance, 60);
			public static readonly ConfigMetadata DeathRaiserSummonsModifications = new ConfigMetadata("Better Summoned Skeletons", "Increases summoned skeleton speed and add better looking gear according to Death Raiser level (stats not affected). (Toggling mid-game requires CLIENT relog for speed changes)", ConfigSections.Balance, 50);
			public static readonly ConfigMetadata FrostStaffModifications = new ConfigMetadata("Better Frost Staff Accuracy", "Improves Staff of Frost Accuracy", ConfigSections.Balance, 40);
			public static readonly ConfigMetadata CrossbowsReloadModifications = new ConfigMetadata("Reduced Crossbows Reload Time", "Crossbows Reload Time Reduced by 1s. (Toggling mid-game requires CLIENT relog)", ConfigSections.Balance, 30);
			public static readonly ConfigMetadata TrophyDropsModifications = new ConfigMetadata("Better Trophy Drop Rates", "Increases trophy drop rate for the following creatures: Rancid Remains, Ghost, Bear, Surtling, Draugr Elite, Wraith, Cultist, Fenring, Stone Golem, Deathsquito, Fuling Berserker, Vile, Tick, Dverger, Seeker Soldier, Charred Warlock", ConfigSections.Balance, 20);
			public static readonly ConfigMetadata DropsModifications = new ConfigMetadata("Better Creature Drops", "Modifies the drops for the following creatures: Fenring (adds Fenris Hair and Fenris Claw, removes Wolf Fang), Bat (adds 50% Bloodbag drop), Dvergr (increases chance of Soft Tissue to 100% from 25%)", ConfigSections.Balance, 10);

			// Features
			public static readonly ConfigMetadata ProgressionHalt = new ConfigMetadata("Automatic Progression Halt", "Creatures and objects do not drop any items unless the previous biome boss has been defeated. (Toggling mid-game requires CLIENT relog or reloading area)", ConfigSections.Features, 160);
			public static readonly ConfigMetadata ProgressionHaltOceanElder = new ConfigMetadata("Halt Ocean Behind Elder", "Ocean resources are halted by The Elder instead of the default Bonemass. Requires Progression Halt Enabled. (Toggling mid-game requires CLIENT relog or reloading area)", ConfigSections.Features, 150);
			public static readonly ConfigMetadata CreatureUnleveler = new ConfigMetadata("Creature Unleveler By Boss", "Increases chance of creatures to spawn with a star or two after defeating their relevant biome boss", ConfigSections.Features, 140);
			public static readonly ConfigMetadata RaidsModifications = new ConfigMetadata("No End-game Raids in Early Biomes", "Mistlands and Ashlands raids do not occur in the first four biomes", ConfigSections.Features, 130);
			public static readonly ConfigMetadata StopNighttimeInvasion = new ConfigMetadata("Stop Nighttime Invasion", "Fulings, Seeker and Charred no longer spawn in early biomes at night. (Fulings spawn in Mountains and Seeker and Charred spawn in Plains)", ConfigSections.Features, 120);
			public static readonly ConfigMetadata AshlandsEnemiesModifications = new ConfigMetadata("Less Ashlands Enemies", "Numbers and spawn chance reduced for Ashlands enemies. (Toggling mid-game requires reloading area)", ConfigSections.Features, 110);
			public static readonly ConfigMetadata ClearMistlands = new ConfigMetadata("Clear Mistlands after Queen", "Clear Mistlands mist after defeating the Queen. (Disabling mid-game requires CLIENT relog)", ConfigSections.Features, 100);
			public static readonly ConfigMetadata WeatherModifications = new ConfigMetadata("Clearer Weather", "Reduces chance for mist and snowstorms in Meadows, Plains, Ocean and Mountains respectively. (Toggling mid-game requires CLIENT relog)", ConfigSections.Features, 90);
			public static readonly ConfigMetadata CraftableChain = new ConfigMetadata("Craftable Chain", "Chain craftable at Black Forge", ConfigSections.Features, 80);
			public static readonly ConfigMetadata BrighterLanterns = new ConfigMetadata("Brighter Lanterns", "Dvergr lanterns are brighter. (Toggling mid-game requires CLIENT relog or reloading area)", ConfigSections.Features, 70);
			public static readonly ConfigMetadata ExtensionsModifications = new ConfigMetadata("Station Extensions Changes", "Decreases space requirement for workstation extensions and increases build distance to workstations (This does not increase workstation radius)", ConfigSections.Features, 60);
			public static readonly ConfigMetadata MinibossWeight = new ConfigMetadata("Hildir Weight Rewards", "Increases base carry weight by 25 when turning in Hildir chests (for each chest). (Toggling mid-game requires CLIENT relog)", ConfigSections.Features, 50);
			public static readonly ConfigMetadata FleeAIModifications = new ConfigMetadata("Stop Running Away", "Boars and Necks won't flee when alerted. (Toggling mid-game only affects new creatures)", ConfigSections.Features, 40);
			public static readonly ConfigMetadata TougherShips = new ConfigMetadata("Tougher Ships", "Increases Ships HP. Raft: 300 -> 400, Karve: 500 -> 650, Longship: 1000 -> 1250, Drakkar: 3000 -> 4000 (Toggling mid-game requires CLIENT relog or reloading area)", ConfigSections.Features, 30);
			public static readonly ConfigMetadata PortalsPerPlayer = new ConfigMetadata("Max Portals Per Player", "Set the maximum number of portals each player can build per world. The limit applies individually to the vanilla normal portal, stone portal, and Glacial Stone Portal when available from MarsarahBuildPieces. Set to 0 to prevent portal placement or -1 for unlimited portals. Server admins are exempt.", ConfigSections.Features, 20);
			public static readonly ConfigMetadata OtherModifications = new ConfigMetadata("Other Section", "Tankard costs reduced and Iron Nails crafting output doubled", ConfigSections.Features, 10);

			// QOL
			public static readonly ConfigMetadata LighterMetalWeight = new ConfigMetadata("Lighter Metal Weight", "All metal ore and bars weight decreased to 8. (Toggling mid-game requires CLIENT relog or reloading area)", ConfigSections.QOL, 140);
			public static readonly ConfigMetadata LargerPickupArea = new ConfigMetadata("Larger Pickup Area", "Item pickup area slightly increased", ConfigSections.QOL, 130);
			public static readonly ConfigMetadata NoSkillLoss = new ConfigMetadata("No Skill Levels Loss On Death", "Skills won't go down the current level upon death (progress in that skill is still lost). (Toggling mid-game requires CLIENT relog)", ConfigSections.QOL, 120);
			public static readonly ConfigMetadata LargerBoatExploreRadius = new ConfigMetadata("Larger Boat Explore Radius", "Double explore radius on a boat", ConfigSections.QOL, 110);
			public static readonly ConfigMetadata BiggerWispRadius = new ConfigMetadata("Bigger Wisp Radius", "Increases wisp radius", ConfigSections.QOL, 100);
			public static readonly ConfigMetadata FriendlyBallistas = new ConfigMetadata("Friendly Ballistas", "Ballistas won't target players and tame animals", ConfigSections.QOL, 90);
			public static readonly ConfigMetadata LessFallDamage = new ConfigMetadata("Less Fall Damage", "Fall damage reduced by 40%", ConfigSections.QOL, 80);
			public static readonly ConfigMetadata FasterResourceDrops = new ConfigMetadata("Faster Resource Drops", "Enemies drop resources faster when dying", ConfigSections.QOL, 70);
			public static readonly ConfigMetadata FasterEquip = new ConfigMetadata("Faster Equip", "Equipping weapons is instant. Armor equip timers reduced to 1s (from 1s/2s). (Toggling mid-game requires CLIENT relog)", ConfigSections.QOL, 60);
			public static readonly ConfigMetadata CameraSailingPosition = new ConfigMetadata("Move Camera Up While Sailing", "Moves the camera a bit upwards when sailing to better see in front of the boat", ConfigSections.QOL, 50);
			public static readonly ConfigMetadata ShorterRestedDelay = new ConfigMetadata("Shorter Rested Delay", "Reduces the amount of time needed to get the rested buff from 20 to 10 seconds (Toggling mid-game requires re-entering the resting area)", ConfigSections.QOL, 40);
			public static readonly ConfigMetadata MoreUsableFuel = new ConfigMetadata("More Usable Fuel", "Ancient Bark can be used as fuel for Kilns and Withered Bones for Shield Generators", ConfigSections.QOL, 30);
			public static readonly ConfigMetadata PermanentLightsModifications = new ConfigMetadata("Permanent Lights", "Makes all light sources permanent, but the build costs of light source pieces use maximum amount of their respective fuel type", ConfigSections.QOL, 20);
			public static readonly ConfigMetadata PlayerLogoutAnnounce = new ConfigMetadata("Player Logout Announce", "Displays a message in the top-left corner and chat when a player logs out.", ConfigSections.QOL, 10);
		}

		// Config entries
		public static ConfigEntry<bool> ServerConfigLocked;
		//public static ConfigEntry<bool> testJumpEnabled;

		// Grind Reduction
		public static ConfigEntry<bool> DoubleBronzeEnabled;
		public static ConfigEntry<bool> GearRecipeAmountsEnabled;
		public static ConfigEntry<bool> GearRecipeMaterialsEnabled;
		public static ConfigEntry<bool> BuildPieceAmountsEnabled;
		public static ConfigEntry<bool> BuildPieceMaterialsEnabled;
		public static ConfigEntry<bool> FoodAndMeadModificationsEnabled;

		// Balance
		public static ConfigEntry<bool> EarlyLinenCapeEnabled;
		public static ConfigEntry<bool> GearSpeedModifiersEnabled;
		public static ConfigEntry<bool> ExtraArmorStatsEnabled;
		public static ConfigEntry<bool> GearUpgradeUnlockEnabled;
		public static ConfigEntry<bool> LongerForsakenPowersEnabled;
		public static ConfigEntry<bool> FasterCharacterSpeedEnabled;
		public static ConfigEntry<bool> ShorterStatusEffectsEnabled;
		public static ConfigEntry<bool> LessStaminaUsageEnabled;
		public static ConfigEntry<bool> BetterDeathRaiserEnabled;
		public static ConfigEntry<bool> BetterDeathRaiserSummonsEnabled;
		public static ConfigEntry<bool> BetterFrostStaffAccuracyEnabled;
		//public static ConfigEntry<bool> HealStaffEnabled;
		public static ConfigEntry<bool> ReducedCrossbowsReloadTimeEnabled;
		public static ConfigEntry<bool> BetterTrophyDropsEnabled;
		public static ConfigEntry<bool> BetterDropsEnabled;

		// Features
		public static ConfigEntry<bool> AutomaticProgressionHaltEnabled;
		public static ConfigEntry<bool> HaltOceanBehindElderEnabled;
		public static ConfigEntry<bool> CreatureUnlevelerEnabled;
		public static ConfigEntry<bool> NoEndRaidsInEarlyBiomesEnabled;
		public static ConfigEntry<bool> StopNighttimeInvasionEnabled;
		public static ConfigEntry<bool> LessAshlandsEnemiesEnabled;		
		public static ConfigEntry<bool> ClearMistlandsEnabled;
		public static ConfigEntry<bool> ClearerWeatherEnabled;
		public static ConfigEntry<bool> CraftableChainEnabled;
		public static ConfigEntry<bool> BrighterLanternsEnabled;
		public static ConfigEntry<bool> ExtensionsChangesEnabled;
		public static ConfigEntry<bool> MinibossWeightEnabled;
		public static ConfigEntry<bool> NoFleeEnabled;		
		public static ConfigEntry<bool> TougherShipsEnabled;
		public static ConfigEntry<int> MaxPortalsPerPlayer;
		public static ConfigEntry<bool> OtherEnabled;

		// QOL
		public static ConfigEntry<bool> LighterMetalWeightEnabled;
		public static ConfigEntry<bool> LargerPickupAreaEnabled;
		public static ConfigEntry<bool> NoSkillLowerOnDeathEnabled;
		public static ConfigEntry<bool> LargerBoatExploreRadiusEnabled;
		public static ConfigEntry<bool> BiggerWispRadiusEnabled;
		public static ConfigEntry<bool> FriendlyBallistasEnabled;
		public static ConfigEntry<bool> LessFallDamageEnabled;
		public static ConfigEntry<bool> FasterResourceDropsEnabled;
		public static ConfigEntry<bool> FasterEquipEnabled;
		public static ConfigEntry<bool> CameraUpWhenSailingEnabled;
		public static ConfigEntry<bool> ShorterRestedDelayEnabled;
		public static ConfigEntry<bool> MoreUsableFuelEnabled;
		public static ConfigEntry<bool> PermanentLightsEnabled;
		public static ConfigEntry<bool> AnnouncePlayerLogout;

		public static void Init(ConfigFile configFile)
		{
			Config = configFile;

			ServerConfigLocked = CreateConfig(Configs.ServerConfig, true);
			_ = configSync.AddLockingConfigEntry(ServerConfigLocked);
			//testJumpEnabled = CreateConfig("1 - Main", "Test Jump", true, "Test Jump Patch");

			// ===== Grind Reduction
			DoubleBronzeEnabled = CreateConfig(Configs.DoubleBronzeCrafting, true);
			GearRecipeAmountsEnabled = CreateConfig(Configs.GearRecipeAmountsModifications, true);
			GearRecipeMaterialsEnabled = CreateConfig(Configs.GearRecipeMaterialsModifications, true);
			BuildPieceAmountsEnabled = CreateConfig(Configs.BuildPieceAmountsModifications, true);
			BuildPieceMaterialsEnabled = CreateConfig(Configs.BuildPieceMaterialsModifications, true);
			FoodAndMeadModificationsEnabled = CreateConfig(Configs.FoodAndMeadModifications, true);

			// ===== Balance
			EarlyLinenCapeEnabled = CreateConfig(Configs.LinenCapeModifications, true);
			GearSpeedModifiersEnabled = CreateConfig(Configs.GearSpeedModifications, true);
			ExtraArmorStatsEnabled = CreateConfig(Configs.ExtraArmorStatsModifications, true);
			GearUpgradeUnlockEnabled = CreateConfig(Configs.GearUpgradeModifications, true);
			LongerForsakenPowersEnabled = CreateConfig(Configs.ForsakenPowersModifications, true);
			FasterCharacterSpeedEnabled = CreateConfig(Configs.CharacterSpeedModifications, true);
			ShorterStatusEffectsEnabled = CreateConfig(Configs.StatusEffectsModifications, true);
			LessStaminaUsageEnabled = CreateConfig(Configs.LessStaminaModifications, true);
			BetterDeathRaiserEnabled = CreateConfig(Configs.DeathRaiserModifications, true);
			BetterDeathRaiserSummonsEnabled = CreateConfig(Configs.DeathRaiserSummonsModifications, true);
			BetterFrostStaffAccuracyEnabled = CreateConfig(Configs.FrostStaffModifications, true);
			//HealStaffEnabled = CreateConfig(ConfigSections.Balance, Configs.HealStaffModifications.Name, true, Configs.HealStaffModifications.Description);
			ReducedCrossbowsReloadTimeEnabled = CreateConfig(Configs.CrossbowsReloadModifications, true);
			BetterTrophyDropsEnabled = CreateConfig(Configs.TrophyDropsModifications, true);
			BetterDropsEnabled = CreateConfig(Configs.DropsModifications, true);

			// ===== Features
			AutomaticProgressionHaltEnabled = CreateConfig(Configs.ProgressionHalt, true);
			HaltOceanBehindElderEnabled = CreateConfig(Configs.ProgressionHaltOceanElder, false);
			CreatureUnlevelerEnabled = CreateConfig(Configs.CreatureUnleveler, true);
			NoEndRaidsInEarlyBiomesEnabled = CreateConfig(Configs.RaidsModifications, true);
			StopNighttimeInvasionEnabled = CreateConfig(Configs.StopNighttimeInvasion, true);
			LessAshlandsEnemiesEnabled = CreateConfig(Configs.AshlandsEnemiesModifications, true);
			ClearMistlandsEnabled = CreateConfig(Configs.ClearMistlands, true);
			ClearerWeatherEnabled = CreateConfig(Configs.WeatherModifications, true);
			CraftableChainEnabled = CreateConfig(Configs.CraftableChain, true);
			BrighterLanternsEnabled = CreateConfig(Configs.BrighterLanterns, true);
			ExtensionsChangesEnabled = CreateConfig(Configs.ExtensionsModifications, true);
			MinibossWeightEnabled = CreateConfig(Configs.MinibossWeight, true);
			NoFleeEnabled = CreateConfig(Configs.FleeAIModifications, true);
			TougherShipsEnabled = CreateConfig(Configs.TougherShips, true);
			MaxPortalsPerPlayer = CreateConfig(Configs.PortalsPerPlayer, -1, true, new AcceptableValueRange<int>(-1, 50));
			OtherEnabled = CreateConfig(Configs.OtherModifications, true);

			// ===== QOL
			LighterMetalWeightEnabled = CreateConfig(Configs.LighterMetalWeight, true);
			LargerPickupAreaEnabled = CreateConfig(Configs.LargerPickupArea, true);
			NoSkillLowerOnDeathEnabled = CreateConfig(Configs.NoSkillLoss, true);
			LargerBoatExploreRadiusEnabled = CreateConfig(Configs.LargerBoatExploreRadius, true);
			BiggerWispRadiusEnabled = CreateConfig(Configs.BiggerWispRadius, true);
			FriendlyBallistasEnabled = CreateConfig(Configs.FriendlyBallistas, true);
			LessFallDamageEnabled = CreateConfig(Configs.LessFallDamage, true);
			FasterResourceDropsEnabled = CreateConfig(Configs.FasterResourceDrops, true);
			FasterEquipEnabled = CreateConfig(Configs.FasterEquip, true);
			CameraUpWhenSailingEnabled = CreateConfig(Configs.CameraSailingPosition, true);
			ShorterRestedDelayEnabled = CreateConfig(Configs.ShorterRestedDelay, true);
			MoreUsableFuelEnabled = CreateConfig(Configs.MoreUsableFuel, true);
			PermanentLightsEnabled = CreateConfig(Configs.PermanentLightsModifications, false);
			AnnouncePlayerLogout = CreateConfig(Configs.PlayerLogoutAnnounce, true);

			SetupWatcher();
		}

		private static ConfigEntry<T> CreateConfig<T>(ConfigMetadata metadata, T defaultValue, bool synchronizedSetting = true, AcceptableValueBase acceptableValues = null)
		{
			ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes
			{
				Order = metadata.Order
			};

			ConfigEntry<T> configEntry = Config.Bind(metadata.Section, metadata.Name, defaultValue, new ConfigDescription(metadata.Description, acceptableValues, attributes));

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			configEntry.SettingChanged += (_, __) => OnConfigChanged(metadata.Name);

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
				log.Error($"There was an issue loading {ConfigFileName}");
				return;
			}
		}

		private static void OnConfigChanged(string configName)
		{
			log.Info($"Config setting '{configName}' changed!");
			Config.Save();

			CompatibilityManager.UpdateIncompatibilities();

			if (configName == Configs.WeatherModifications.Name)
			{
				WeatherChanges.UpdateWeatherWeights(EnvMan.instance);
			}

			if (ObjectDB.instance == null || ZNetScene.instance == null) return;
			if (ZNet.instance == null) return;
			var spawnSystem = Object.FindFirstObjectByType<SpawnSystem>();

			bool isDedicatedServer = ZNet.instance.IsDedicated();

			// Handle client-side (non-dedicated) configs
			if (!isDedicatedServer)
			{
				switch (configName)
				{
					case var name when name == Configs.DoubleBronzeCrafting.Name:
						DoubleBronzeCrafting.UpdateDoubleBronzeCrafting(ObjectDB.instance, true);
						break;

					case var name when name == Configs.GearRecipeAmountsModifications.Name:
						GearRecipeChanges.UpdateGearRecipes(ObjectDB.instance, true, false);
						break;

					case var name when name == Configs.GearRecipeMaterialsModifications.Name:
						GearRecipeChanges.UpdateGearRecipes(ObjectDB.instance, false, true);
						break;

					case var name when name == Configs.BuildPieceAmountsModifications.Name:
						BuildPieceChanges.UpdateBuildPieces(ZNetScene.instance, true, false);
						//SilverSconce.RefreshSilverSconceRequirements();
						//SilverTableTorch.RefreshSilverTorchRequirements();
						//GreenStandingBrazier.RefreshGreenBrazierRequirements();
						//SilverHangingBrazier.RefreshSilverHangingBrazierRequirements();
						//ColoredDvergerLanterns.RefreshColoredDvergrLanternsRequirements();
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
						GearUpgradeChanges.UpdateGearRecipeStations(ObjectDB.instance, true);
						break;

					case var name when name == Configs.PermanentLightsModifications.Name:
						PermanentLightsChanges.UpdateLightBuildPiecesAmounts(ZNetScene.instance, true);
						break;

					case var name when name == Configs.CraftableChain.Name:
						CraftableChain.UpdateChainRecipe(ObjectDB.instance, true);
						break;

					case var name when name == Configs.BrighterLanterns.Name:
						BrighterLanterns.UpdateLanterns(ZNetScene.instance);
						break;

					case var name when name == Configs.TougherShips.Name:
						TougherShipsChanges.UpdateShipHP(ZNetScene.instance, true);
						break;

					case var name when name == Configs.OtherModifications.Name:
						OtherChanges.UpdateOthers(ObjectDB.instance, true);
						break;

					case var name when name == Configs.LighterMetalWeight.Name:
						LighterMetalWeight.UpdateLighterMetalWeight(ObjectDB.instance, true);
						break;

					case var name when name == Configs.LargerPickupArea.Name:
						if (Player.m_localPlayer != null)
						{
							LargerPickupAreaChanges.UpdatePickupArea(Player.m_localPlayer, true);
						}
						break;
				}
			}

			// Handle server-side configs
			/*if (isServer)
			{
				switch (configName)
				{
					case var name when name == Configs.TougherShips.Name:
						log.Info($"ConfigManager Server: Reapplying modifications for {configName}...");
						TougherShipsChanges.UpdateShipHP(ZNetScene.instance, true);
						break;
				}
			}*/

			// Handle both client and server-side configs
			switch (configName)
			{
				case var name when name == Configs.AshlandsEnemiesModifications.Name:
					if (spawnSystem != null)
					{
						AshlandsEnemiesChanges.UpdateAshlandsSpawns(spawnSystem);
					}
					break;

				case var name when name == Configs.StopNighttimeInvasion.Name:
					if (spawnSystem != null)
					{
						NighttimeSpawnChanges.UpdateNighttimeSpawns(spawnSystem);
					}
					break;

				case var name when name == Configs.CreatureUnleveler.Name:
					if (spawnSystem != null)
					{
						CreatureUnleveler.ApplyCreatureLevelChanges(spawnSystem);
					}
					break;

				case var name when name == Configs.MoreUsableFuel.Name:
					MoreUsableFuel.UpdateMoreUsableFuel();
					break;
			}
		}
	}
}
