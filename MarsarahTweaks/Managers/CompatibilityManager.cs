using BepInEx.Bootstrap;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using static MarsarahTweaks.Managers.ConfigManager;

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
		public static ConflictMod TripleBronzeJVL = new ConflictMod("digitalroot.mods.triplebronze.jvl");
		public static ConflictMod BiggerPickupRadius = new ConflictMod("net.mtnewton.biggerpickupradius");
		public static ConflictMod CreatureLevelLootControl = new ConflictMod("org.bepinex.plugins.creaturelevelcontrol");
		public static ConflictMod Sailing = new ConflictMod("org.bepinex.plugins.sailing");
		public static ConflictMod Seasonality = new ConflictMod("RustyMods.Seasonality");
		public static ConflictMod Seasons = new ConflictMod("shudnal.Seasons");
		public static ConflictMod ImFriendlyDammit = new ConflictMod("Azumatt.ImFRIENDLYDAMMIT");

		// Initialize the manager
		public static void Initialize()
		{
			LoadedMods.Clear();

			// Populate all loaded mods (GUID -> Name)
			foreach (var plugin in Chainloader.PluginInfos.Values)
				LoadedMods[plugin.Metadata.GUID] = plugin.Metadata.Name;

			// Update all ConflictMods
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
			UpdateConflictMod(ref TripleBronzeJVL);
			UpdateConflictMod(ref BiggerPickupRadius);
			UpdateConflictMod(ref CreatureLevelLootControl);
			UpdateConflictMod(ref Sailing);
			UpdateConflictMod(ref Seasonality);
			UpdateConflictMod(ref Seasons);
			UpdateConflictMod(ref ImFriendlyDammit);
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

		// Generic helper: if a given mod is loaded, force a config entry to a specific value.
		// Works for bools, enums, ints, strings, etc.
		public static void SetIfIncompatible<T>(ConflictMod mod, ConfigEntry<T> config, T forcedValue, string actionDescription, string additionalReason = "")
		{
			if (!mod.Loaded)
				return;

			// Avoid pointless logs if it's already at the forced value
			if (EqualityComparer<T>.Default.Equals(config.Value, forcedValue))
				return;

			string before = config.Value?.ToString() ?? "null";
			string after = forcedValue?.ToString() ?? "null";

			log.Warn(
				$"{actionDescription} '{config.Definition.Key}' because '{mod.Name}' mod is loaded. " +
				$"(was: {before}, now: {after}) {additionalReason}"
			);

			config.Value = forcedValue;
		}


		// Convenience wrapper for bool configs
		public static void DisableIfIncompatible(ConflictMod mod, ConfigEntry<bool> config, string additionalReason = "")
		{
			SetIfIncompatible(
				mod,
				config,
				false,
				"Automatically disabling",
				additionalReason
			);
		}

		public static void UpdateIncompatibilities()
		{
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
			DisableIfIncompatible(TripleBronzeJVL, ConfigManager.DoubleBronzeEnabled);
			DisableIfIncompatible(BiggerPickupRadius, ConfigManager.LargerPickupAreaEnabled);
			DisableIfIncompatible(CreatureLevelLootControl, ConfigManager.CreatureUnlevelerEnabled);
			DisableIfIncompatible(Sailing, ConfigManager.LargerBoatExploreRadiusEnabled);
			DisableIfIncompatible(Sailing, ConfigManager.CameraUpWhenSailingEnabled);
			DisableIfIncompatible(Sailing, ConfigManager.TougherShipsEnabled);
			DisableIfIncompatible(Seasonality, ConfigManager.ClearerWeatherEnabled);
			DisableIfIncompatible(Seasons, ConfigManager.ClearerWeatherEnabled);
			DisableIfIncompatible(ImFriendlyDammit, ConfigManager.FriendlyBallistasEnabled);
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
