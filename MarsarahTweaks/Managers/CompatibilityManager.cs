using BepInEx.Bootstrap;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace MarsarahTweaks.Managers
{
	internal static class CompatibilityManager
	{
		private static readonly LogManager log = new LogManager("Compatibility Manager", LogManager.LogLevel.Warning);

		// All loaded mods (GUID -> Name)
		private static readonly Dictionary<string, string> LoadedMods = new Dictionary<string, string>();

		// Struct representing a mod we care about for compatibility
		public struct ConflictMod
		{
			public string Guid;
			public string Name;
			public bool Loaded;

			public ConflictMod(string guid)
			{
				Guid = guid;
				Name = null;
				Loaded = false;
			}
		}

		// Static readonly variables for each mod we track
		public static ConflictMod MinimalStatusEffects = new ConflictMod("randyknapp.mods.minimalstatuseffects");
		public static ConflictMod BetterUI = new ConflictMod("MK_BetterUI");
		public static ConflictMod CraftFromContainers = new ConflictMod("aedenthorn.CraftFromContainers");
		public static ConflictMod DeezMistyBalls = new ConflictMod("Azumatt.DeezMistyBalls");
		public static ConflictMod MistBeGone = new ConflictMod("Azumatt.MistBeGone");
		public static ConflictMod InstantMonsterDrop = new ConflictMod("cjayride.InstantMonsterDrop");
		public static ConflictMod InstantEquip = new ConflictMod("org.bepinex.plugins.instantequip");
		public static ConflictMod EternalFire = new ConflictMod("digitalroot.mods.eternalfire");
		public static ConflictMod FuelEternal = new ConflictMod("Marfinator.FuelEternal");
		public static ConflictMod TorchesEternal = new ConflictMod("wildbill22.TorchesEternal");
		public static ConflictMod TorchesEternal2 = new ConflictMod("Xenofell.TorchesEternal");
		public static ConflictMod ForsakenPowerOverhaul = new ConflictMod("JuneGame.Valheim.ForsakenPowerOverhaul");
		public static ConflictMod TripleBronze = new ConflictMod("LolmanXDXD.TripleBronze");
		public static ConflictMod BiggerPickupRadius = new ConflictMod("net.mtnewton.biggerpickupradius");
		public static ConflictMod CreatureLevelLootControl = new ConflictMod("org.bepinex.plugins.creaturelevelcontrol");
		public static ConflictMod Sailing = new ConflictMod("org.bepinex.plugins.sailing");
		public static ConflictMod Seasonality = new ConflictMod("RustyMods.Seasonality");
		public static ConflictMod Seasons = new ConflictMod("shudnal.Seasons");

		// Initialize the manager
		public static void Initialize()
		{
			LoadedMods.Clear();

			// Populate all loaded mods (GUID -> Name)
			foreach (var plugin in Chainloader.PluginInfos.Values)
				LoadedMods[plugin.Metadata.GUID] = plugin.Metadata.Name;

			// Update all ConflictMods
			UpdateConflictMod(ref MinimalStatusEffects);
			UpdateConflictMod(ref BetterUI);
			UpdateConflictMod(ref CraftFromContainers);
			UpdateConflictMod(ref DeezMistyBalls);
			UpdateConflictMod(ref MistBeGone);
			UpdateConflictMod(ref InstantMonsterDrop);
			UpdateConflictMod(ref InstantEquip);
			UpdateConflictMod(ref EternalFire);
			UpdateConflictMod(ref FuelEternal);
			UpdateConflictMod(ref TorchesEternal);
			UpdateConflictMod(ref TorchesEternal2);
			UpdateConflictMod(ref ForsakenPowerOverhaul);
			UpdateConflictMod(ref TripleBronze);
			UpdateConflictMod(ref BiggerPickupRadius);
			UpdateConflictMod(ref CreatureLevelLootControl);
			UpdateConflictMod(ref Sailing);
			UpdateConflictMod(ref Seasonality);
			UpdateConflictMod(ref Seasons);
		}

		// Helper to update the Name and Loaded flag of a ConflictMod
		private static void UpdateConflictMod(ref ConflictMod mod)
		{
			if (LoadedMods.TryGetValue(mod.Guid, out var name))
			{
				mod.Name = name;
				mod.Loaded = true;
				log.Info($"{name} detected (GUID: {mod.Guid})");
			}
			else
			{
				mod.Name = mod.Guid; // fallback to GUID if name not found
				mod.Loaded = false;
			}
		}

		// Disable a config if a conflict mod is loaded
		public static void DisableIfIncompatible(ConflictMod mod, ConfigEntry<bool> config, string additionalReason = "")
		{
			if (mod.Loaded && config.Value)
			{
				log.Warn($"Automatically disabling '{config.Definition.Key}' because '{mod.Name}' mod is loaded. {additionalReason}");
				config.Value = false;
			}
		}

		public static void UpdateIncompatibilities()
		{
			/*DisableIfIncompatible(MinimalStatusEffects, ConfigManager.OnlinePlayersUnderMinimap);
			DisableIfIncompatible(BetterUI, ConfigManager.BetterEnemyNameplates, "Letting BetterUI handle enemy nameplates.");
			DisableIfIncompatible(BetterUI, ConfigManager.BetterItemQualityIndicator, "Letting BetterUI handle the quality indicator.");
			DisableIfIncompatible(BetterUI, ConfigManager.ColoredItemDurabilityBar, "Letting BetterUI handle the durability indicator.");
			DisableIfIncompatible(BetterUI, ConfigManager.DetailedHoverInfo, "Letting BetterUI handle hover information.");
			DisableIfIncompatible(CraftFromContainers, ConfigManager.ShowOwnedResources);
			DisableIfIncompatible(DeezMistyBalls, ConfigManager.BiggerWispRadiusEnabled);
			DisableIfIncompatible(MistBeGone, ConfigManager.ClearMistlandsEnabled);
			DisableIfIncompatible(InstantMonsterDrop, ConfigManager.FasterResourceDropsEnabled);
			DisableIfIncompatible(InstantEquip, ConfigManager.FasterEquipEnabled);
			DisableIfIncompatible(EternalFire, ConfigManager.PermanentLightsEnabled);
			DisableIfIncompatible(FuelEternal, ConfigManager.PermanentLightsEnabled);
			DisableIfIncompatible(TorchesEternal, ConfigManager.PermanentLightsEnabled);
			DisableIfIncompatible(TorchesEternal2, ConfigManager.PermanentLightsEnabled);
			DisableIfIncompatible(ForsakenPowerOverhaul, ConfigManager.LongerForsakenPowersEnabled);
			DisableIfIncompatible(TripleBronze, ConfigManager.DoubleBronzeEnabled);
			DisableIfIncompatible(BiggerPickupRadius, ConfigManager.LargerPickupAreaEnabled);
			DisableIfIncompatible(CreatureLevelLootControl, ConfigManager.CreatureUnlevelerEnabled);
			DisableIfIncompatible(Sailing, ConfigManager.LargerBoatExploreRadiusEnabled);
			DisableIfIncompatible(Sailing, ConfigManager.CameraUpWhenSailingEnabled);
			DisableIfIncompatible(Seasonality, ConfigManager.ClearerWeatherEnabled);
			DisableIfIncompatible(Seasons, ConfigManager.ClearerWeatherEnabled);*/
		}

		// Optional: log all loaded mods for debugging
		public static void DumpAllLoadedMods()
		{
			foreach (var plugin in Chainloader.PluginInfos.Values)
			{
				var meta = plugin.Metadata;
				log.Info($"Plugin: {meta.Name} v{meta.Version} (GUID: {meta.GUID})");
			}
		}
	}
}
