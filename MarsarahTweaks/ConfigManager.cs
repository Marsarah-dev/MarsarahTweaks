using BepInEx;
using BepInEx.Configuration;
using ServerSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks
{
	public class ConfigManager
	{
		private readonly BaseUnityPlugin plugin;

		public ConfigManager(BaseUnityPlugin plugin)
		{
			this.plugin = plugin;
		}

		private static readonly ConfigSync configSync = new ServerSync.ConfigSync(MarsarahTweaks.ModGUID) { DisplayName = MarsarahTweaks.ModName, CurrentVersion = MarsarahTweaks.ModVersion, MinimumRequiredVersion = MarsarahTweaks.ModVersion };
		//public static ConfigFile configBase = new ConfigFile(MarsarahTweaks.ConfigFileFullPath, saveOnInit: true); // Not sure about this one

		private static ConfigEntry<bool> serverConfigLocked;

		public static ConfigEntry<bool> cheaperGearEnabled;
		public static ConfigEntry<bool> doubleBronzeEnabled;
		public static ConfigEntry<bool> cheaperBuildPiecesEnabled;
		public static ConfigEntry<bool> altCookingRecipesEnabled;
		public static ConfigEntry<bool> altMeadRecipesEnabled;
		public static ConfigEntry<bool> moreFoodStacksEnabled;

		public static ConfigEntry<bool> altGearRecipesEnabled;
		public static ConfigEntry<bool> altBuildPiecesMaterialsEnabled;
		public static ConfigEntry<bool> altLinenCapeEnabled;
		public static ConfigEntry<bool> altGearSpeedModifiersEnabled;
		public static ConfigEntry<bool> altForsakenPowersEnabled;
		public static ConfigEntry<bool> fasterCharacterSpeedEnabled;
		public static ConfigEntry<bool> altStatusEffectsEnabled;
		public static ConfigEntry<bool> lessStaminaUsageEnabled;
		public static ConfigEntry<bool> moreArmorStatsEnabled;
		public static ConfigEntry<bool> mageGearGivesEitrEnabled;
		public static ConfigEntry<bool> betterDeathRaiserEnabled;
		public static ConfigEntry<bool> betterSummonedSkeletonGearEnabled;
		public static ConfigEntry<bool> betterFrostStaffAccuracyEnabled;
		public static ConfigEntry<bool> reducedCrossbowsReloadTimeEnabled;
		public static ConfigEntry<bool> lessAshlandsEnemiesEnabled;
		public static ConfigEntry<bool> gearUpgradeUnlockEnabled;
		public static ConfigEntry<bool> permanentLightsEnabled;
		public static ConfigEntry<bool> clearMistlandsEnabled;
		public static ConfigEntry<bool> craftableChainEnabled;
		public static ConfigEntry<bool> brighterLanternsEnabled;
		public static ConfigEntry<bool> lessFogEnabled;
		public static ConfigEntry<bool> creatureUnlevelerEnabled;
		public static ConfigEntry<bool> minibossWeightEnabled;
		public static ConfigEntry<bool> extensionsChangesEnabled;
		public static ConfigEntry<bool> noFleeEnabled;
		public static ConfigEntry<bool> automaticProgressionHaltEnabled;
		public static ConfigEntry<bool> seekerSoldierTrophyEnabled;
		public static ConfigEntry<bool> tougherShipsEnabled;
		public static ConfigEntry<bool> otherEnabled;

		public static ConfigEntry<bool> lighterMetalWeightEnabled;
		public static ConfigEntry<bool> largerPickupAreaEnabled;
		public static ConfigEntry<bool> noSkillLowerOnDeathEnabled;
		public static ConfigEntry<bool> largerBoatExploreRadiusEnabled;
		public static ConfigEntry<bool> biggerWhispRadiusEnabled;
		public static ConfigEntry<bool> friendlyBallistasEnabled;
		public static ConfigEntry<bool> lessFallDamageEnabled;
		public static ConfigEntry<bool> fasterResourceDropsEnabled;
		public static ConfigEntry<bool> fasterEquipEnabled;
		public static ConfigEntry<bool> instantEatAndDrinkEnabled;

		public static ConfigEntry<bool> altLoadingTipsEnabled;
		public static ConfigEntry<bool> showInventoryWeight;
		public static ConfigEntry<bool> showInventorySlots;
		public static ConfigEntry<bool> showBoatSpeed;
		public static ConfigEntry<bool> showTimeAndDay;
		public static ConfigEntry<bool> timeFormat24H;
		public static ConfigEntry<bool> smartBiomeEnabled;
		public static ConfigEntry<bool> showEnemyDetector;
		public static ConfigEntry<bool> showSummonCounter;
		public static ConfigEntry<bool> showOnlinePartyIndicator;
		public static ConfigEntry<bool> partyIndicatorUnderMinimap;
		public static ConfigEntry<bool> hideOnlineIndicatorWhenSolo;

		// Read Config File =================================================
		public void ReadConfigFile()
		{
			serverConfigLocked = config("1 - Main", "Lock Configuration", true, "If on, only server admins can change the configuration.");
			_ = configSync.AddLockingConfigEntry<bool>(serverConfigLocked);

			// ===== Grindyness Reduction
			cheaperGearEnabled					= config("2 - Grindyness Reduction", "Cheaper Gear Recipes",		true, "Reduces costs for crafting and upgrading gear (especially metal)");
			doubleBronzeEnabled					= config("2 - Grindyness Reduction", "Double Bronze Crafting",		true, "Doubles the amount of crafted Bronze at the Forge");
			cheaperBuildPiecesEnabled			= config("2 - Grindyness Reduction", "Cheaper Build Pieces",		true, "Reduces costs for build pieces");
			altCookingRecipesEnabled			= config("2 - Grindyness Reduction", "Alternate Cooking Recipes",	true, "Modifies food recipes costs and crafted amounts");
			altMeadRecipesEnabled				= config("2 - Grindyness Reduction", "Alternate Mead Recipes",		true, "Modifies mead recipes costs and crafted amounts");
			moreFoodStacksEnabled				= config("2 - Grindyness Reduction", "More Food Stacks",			true, "Increases food item stacks to make them more uniform");

			// ===== Features
			altGearRecipesEnabled				= config("3 - Features", "Alt Gear Recipes",						true, "Modifies gear recipe materials for some items (more materials from current biomes)");
			altBuildPiecesMaterialsEnabled		= config("3 - Features", "Alt Build Piece Materials",				true, "Modifies build pieces materials (more materials from current biome)");
			altLinenCapeEnabled					= config("3 - Features", "Alt Linen & Lox Cape",					true, "Moves Linen Cape to Swamp Era and adds poison resist to both capes");
			altGearSpeedModifiersEnabled		= config("3 - Features", "Alt Gear Speed Modifiers",				true, "Gear speed penalty reduced (light armors have increased move speed, heavy armors have no penalty)");
			altForsakenPowersEnabled			= config("3 - Features", "Alt Forsaken Powers",						true, "Reduce Forsaken Powers cooldowns and increase durations");
			fasterCharacterSpeedEnabled			= config("3 - Features", "Faster Character Speed",					true, "Faster character jog, walk, swim and crouch speeds (run excluded)");
			altStatusEffectsEnabled				= config("3 - Features", "Alt Status Effects",						true, "Wet effect and potions timers reduced to 1 min");
			lessStaminaUsageEnabled				= config("3 - Features", "Less Stamina Usage",						true, "Less stamina usage in general");
			moreArmorStatsEnabled				= config("3 - Features", "More Armor Stats",						true, "Heavy armor gives extra HP, light armor gives extra stamina");
			mageGearGivesEitrEnabled			= config("3 - Features", "Mage Gear Eitr",							true, "Mage Gear Provides Base Eitr");
			betterDeathRaiserEnabled			= config("3 - Features", "Better Death Raiser",						true, "Increases chance of summoning archers, and increases summoned skeleton speed");
			betterSummonedSkeletonGearEnabled	= config("3 - Features", "Better Summoned Skeleton Gear",			true, "Summoned Skeletons have better looking gear (stats not affected)");
			betterFrostStaffAccuracyEnabled		= config("3 - Features", "Better Frost Staff Accuracy",				true, "Improves Staff of Frost Accuracy");
			reducedCrossbowsReloadTimeEnabled	= config("3 - Features", "Reduced Crossbows Reload Time",			true, "Crossbows Reload Time Reduced");
			lessAshlandsEnemiesEnabled			= config("3 - Features", "Less Ashlands Enemies",					false, "Less Enemies in Ashlands");
			gearUpgradeUnlockEnabled			= config("3 - Features", "Gear Upgrade Unlock",						true, "Unlock max levels for upgrading Mistlands and Ashlands gear, plus earlier max level upgrades for Leather and Bronze sets");
			permanentLightsEnabled				= config("3 - Features", "Permanent Lights",						false, "Makes all light sources permanent, but the build costs use max fuel type");
			clearMistlandsEnabled				= config("3 - Features", "Clear Mistlands",							true, "Clear Mistlands mist after defeating the Queen");
			craftableChainEnabled				= config("3 - Features", "Craftable Chain",							true, "Chain craftable at Black Forge");
			brighterLanternsEnabled				= config("3 - Features", "Brighter Lanterns",						true, "Dvergr lanterns are brighter");
			lessFogEnabled						= config("3 - Features", "Clearer Weather",							true, "Reduces chance for mist and snowstorms");
			creatureUnlevelerEnabled			= config("3 - Features", "Creature Unleveler By Boss",				true, "Increases chance of creatures to spawn with a star or two after defeating their relevant biome boss");
			minibossWeightEnabled				= config("3 - Features", "Hildir Weight Rewards",					true, "Increases base carry weight by 25 when turning in Hildir chests (for each chest)");
			extensionsChangesEnabled			= config("3 - Features", "Station Extensions Changes",				true, "Decreases space requirement for workstation extensions and increases build distance to workstations");
			noFleeEnabled						= config("3 - Features", "Stop Running Away",						true, "Boars and Necks won't flee when alerted");
			automaticProgressionHaltEnabled		= config("3 - Features", "Progression Halt",						true, "Creatures and objects do not drop any items unless the previous biome boss has been defeated.");
			seekerSoldierTrophyEnabled			= config("3 - Features", "Better Seeker Soldier Trophy Rate",		true, "Increased Seeker Soldier trophy drop rate to 0.2 (from 0.05) for easier Queen summons.");
			tougherShipsEnabled					= config("3 - Features", "Tougher Ships",							true, "Increases Ships HP. Raft: 300 -> 400, Karve: 500 -> 650, Longship: 1000 -> 1250, Drakkar: 3000 -> 4000");
			otherEnabled						= config("3 - Features", "Other Section",							true, "Tankard costs reduced and Iron Nails crafting output doubled");

			// ===== QOL
			lighterMetalWeightEnabled			= config("4 - QOL", "Lighter Metal Weight",							true, "All metal (ore and bar) weight decreased to 8 (equal to Tin Ore)");
			largerPickupAreaEnabled				= config("4 - QOL", "Larger Pickup Area",							true, "Item pickup area increased");
			noSkillLowerOnDeathEnabled			= config("4 - QOL", "No Skill Lower On Death",						true, "Skills won't go down the current level upon death (progress in that skill is still lost)");
			largerBoatExploreRadiusEnabled		= config("4 - QOL", "Larger Boat Explore Radius",					true, "Larger explore radius on a boat");
			biggerWhispRadiusEnabled			= config("4 - QOL", "Bigger Wisp Radius",							true, "Increases wisp radius");
			friendlyBallistasEnabled			= config("4 - QOL", "Friendly Ballistas",							true, "Ballistas won't target players and tame animals");
			lessFallDamageEnabled				= config("4 - QOL", "Less Fall Damage",								true, "Fall damage reduced by 40%");
			fasterResourceDropsEnabled			= config("4 - QOL", "Faster Resource Drops",						true, "Enemies drop resources faster when dying");
			fasterEquipEnabled					= config("4 - QOL", "Faster Equip",									false, "Equipping weapons is instant. Armor equip timers reduced to 1s (from 1s/2s)");
			instantEatAndDrinkEnabled			= config("4 - QOL", "Instant Eat And Drink",						false, "Eating food and drinking meads is instant (Skips animation completely)");

			// ===== UI
			altLoadingTipsEnabled				= config("5 - UI", "Alt Loading Tips",								true, "More loading screen tips", false);
			showInventoryWeight					= config("5 - UI", "Show Inventory Weight",							true, "Show inventory weight", false);
			showInventorySlots					= config("5 - UI", "Show Inventory Slots",							true, "Show free inventory slots", false);
			showBoatSpeed						= config("5 - UI", "Show Boat Speed",								true, "Show boat speed (only when driving a boat)", false);
			showTimeAndDay						= config("5 - UI", "Show Time And Day",								true, "Show time and day", false);
			timeFormat24H						= config("5 - UI", "Show Time And Day - 24 Hour Format",			false, "Use 24 Hour time format", false);
			smartBiomeEnabled					= config("5 - UI", "Show Smart Biome",								true, "Show smart biome text on minimap (colored according to armor value)", false);
			showEnemyDetector					= config("5 - UI", "Show Enemy Detector",							true, "Show enemy detector", false);
			showSummonCounter					= config("5 - UI", "Show Summon Counter",							true, "Show number of summoned skeletons from the Dead Raiser", false);
			showOnlinePartyIndicator			= config("5 - UI", "Show Online Party Indicator",					true, "Show online players", false);
			partyIndicatorUnderMinimap			= config("5 - UI", "Show Online Party Indicator - Indicator Under Minimap", false, "Show online players under minimap", false);
			hideOnlineIndicatorWhenSolo			= config("5 - UI", "Show Online Party Indicator - Hide Online Indicator When Solo", true, "Hide online players when alone in world", false);
		}

		// Config Template =================================================
		ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
		{
			ConfigDescription extendedDescription = new ConfigDescription(description.Description + (synchronizedSetting ? " [Synced with Server] " : " [Not Synced with Server] "), description.AcceptableValues, description.Tags);

			ConfigEntry<T> configEntry = plugin.Config.Bind(group, name, value, extendedDescription);

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			return configEntry;
		}

		ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

	}
}
