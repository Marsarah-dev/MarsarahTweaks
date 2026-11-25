
using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using UnityEngine;
using System;
using static DropTable;

namespace MarsarahTweaks.Patches.Grind
{
	internal class RareResourceChanges
	{
		private static readonly LogManager log = new LogManager("Rare Resource Changes", LogManager.LogLevel.Info);

		private const string CharredFortressChest = "TreasureChest_charredfortress";
		private const int GemMinAmount = 2;
		private const int GemMaxAmount = 3;
		private const float KrigenArmorMouldChance = 0.15f;
		private const float HexenArmorMouldChance = 0.25f;

		private static readonly Dictionary<CharacterDrop.Drop, (float chance, bool levelMultiplier)> originalArmorMouldDrops = new Dictionary<CharacterDrop.Drop, (float chance, bool levelMultiplier)>();

		private static readonly Dictionary<string, float> lootWeights = new Dictionary<string, float>
		{
            // Gems
            { "GemstoneGreen", 0.75f }, // Jade
            { "GemstoneBlue", 0.75f },  // Iolite
            { "GemstoneRed", 0.75f },   // Bloodstone

            // Common resources
            { "Coins", 0.66f },
			{ "CharredBone", 0.66f },
			{ "Charredskull", 0.66f }
		};

		private static readonly HashSet<string> krigenArmorMoulds = new HashSet<string>
		{
			"MoldArmorGoldHelmet",
			"MoldArmorGoldChest",
			"MoldArmorGoldLegs",
			"MoldArmorMediumHelmet",
			"MoldArmormediumChest", // it's capitalized like this
			"MoldArmorMediumLegs"
		};

		private static readonly HashSet<string> hexenArmorMoulds = new HashSet<string>
		{
			"MoldArmorMageHelmet",
			"MoldArmorMageChest",
			"MoldArmorMageLegs"
		};

		[HarmonyPatch(typeof(Container), "Awake")]
		private static class Container_AwakePatch
		{
			private static void Prefix(Container __instance)
			{
				if (__instance == null) return;
				if (ConfigManager.MoreRareResourcesEnabled?.Value != true) return;

				if (Utils.GetPrefabName(__instance.gameObject) != CharredFortressChest)
					return;

				UpdateCharredFortressLoot(__instance);
			}
		}

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		private static class ZNetScene_AwakePatch
		{
			private static void Postfix(ZNetScene __instance)
			{
				if (__instance == null) return;

				originalArmorMouldDrops.Clear();

				//LogOriginalCreatureDrops(__instance);
				UpdateDeepNorthArmorMoulds(__instance);
			}
		}

		private static void UpdateCharredFortressLoot(Container chest)
		{
			DropTable originalTable = chest.m_defaultItems;

			if (originalTable == null || originalTable.m_drops == null)
			{
				log.Warn("Charred Fortress chest has no valid loot table.");
				return;
			}

			// Verify that all six expected resources exist before modifying the table.
			HashSet<string> foundResources = new HashSet<string>();

			foreach (DropData drop in originalTable.m_drops)
			{
				if (drop.m_item != null && lootWeights.ContainsKey(drop.m_item.name))
					foundResources.Add(drop.m_item.name);
			}

			if (foundResources.Count != lootWeights.Count)
			{
				log.Warn($"Charred Fortress loot table incomplete. Found {foundResources.Count}/{lootWeights.Count} expected resources. Changes skipped.");
				return;
			}

			// Create an independent copy to avoid modifying shared loot tables.
			DropTable modifiedTable = new DropTable
			{
				m_dropMin = originalTable.m_dropMin,
				m_dropMax = originalTable.m_dropMax,
				m_dropChance = originalTable.m_dropChance,
				m_oneOfEach = originalTable.m_oneOfEach,
				m_drops = new List<DropData>(originalTable.m_drops.Count)
			};

			foreach (DropData originalDrop in originalTable.m_drops)
			{
				DropData modifiedDrop = new DropData
				{
					m_item = originalDrop.m_item,
					m_weight = originalDrop.m_weight,
					m_stackMin = originalDrop.m_stackMin,
					m_stackMax = originalDrop.m_stackMax,
					m_dontScale = originalDrop.m_dontScale
				};

				if (modifiedDrop.m_item != null &&
					lootWeights.TryGetValue(modifiedDrop.m_item.name, out float newWeight))
				{
					modifiedDrop.m_weight = newWeight;

					if (IsGem(modifiedDrop.m_item.name))
					{
						modifiedDrop.m_stackMin = GemMinAmount;
						modifiedDrop.m_stackMax = GemMaxAmount;
					}
				}

				modifiedTable.m_drops.Add(modifiedDrop);
			}

			chest.m_defaultItems = modifiedTable;

			log.Info("Applied More Rare Resources to Charred Fortress chest loot.");
		}

		private static void LogOriginalCreatureDrops(ZNetScene instance)
		{
			if (instance == null) return;

			string[] creatureNames =
			{
				"JotunWarrior",
				"JotunWarriorDualWield",
				"JotunWitch",
				"Moose"
			};

			log.Info("Original Creature Drop Tables", header: true);

			foreach (string creatureName in creatureNames)
			{
				GameObject prefab = instance.GetPrefab(creatureName);

				if (prefab == null)
				{
					log.Warn($"Could not find creature prefab: {creatureName}");
					continue;
				}

				CharacterDrop characterDrop = prefab.GetComponent<CharacterDrop>();

				if (characterDrop == null || characterDrop.m_drops == null)
				{
					log.Warn($"No valid CharacterDrop component found for {creatureName}");
					continue;
				}

				log.Info($"Creature: {creatureName}");

				foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
				{
					if (drop?.m_prefab == null) continue;

					log.Info($"  {drop.m_prefab.name} | Min: {drop.m_amountMin} | Max: {drop.m_amountMax} | Chance: {drop.m_chance * 100f:0.##}% | LevelMultiplier: {drop.m_levelMultiplier} | DontScale: {drop.m_dontScale} | OnePerPlayer: {drop.m_onePerPlayer}");
				}
			}

			log.Info("End of Creature Drop Tables", footer: true);
		}

		public static void UpdateDeepNorthArmorMoulds(ZNetScene instance)
		{
			if (instance == null) return;

			UpdateCreatureArmorMoulds(instance, "JotunWarrior", krigenArmorMoulds, KrigenArmorMouldChance);
			UpdateCreatureArmorMoulds(instance, "JotunWarriorDualWield", krigenArmorMoulds, KrigenArmorMouldChance);
			UpdateCreatureArmorMoulds(instance, "JotunWitch", hexenArmorMoulds, HexenArmorMouldChance);
		}

		private static void UpdateCreatureArmorMoulds(ZNetScene instance, string creatureName, HashSet<string> moulds, float newChance)
		{
			GameObject prefab = instance.GetPrefab(creatureName);

			if (prefab == null)
			{
				log.Warn($"Could not find creature prefab: {creatureName}");
				return;
			}

			CharacterDrop characterDrop = prefab.GetComponent<CharacterDrop>();

			if (characterDrop == null || characterDrop.m_drops == null)
			{
				log.Warn($"No valid CharacterDrop component found for {creatureName}");
				return;
			}

			bool enabled = ConfigManager.MoreRareResourcesEnabled?.Value == true;
			int modifiedCount = 0;

			foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
			{
				if (drop?.m_prefab == null) continue;

				string itemName = drop.m_prefab.name;
				if (!moulds.Contains(itemName)) continue;

				// Backup original drop values.
				if (!originalArmorMouldDrops.TryGetValue(drop, out var original))
				{
					original = (drop.m_chance, drop.m_levelMultiplier);
					originalArmorMouldDrops[drop] = original;
				}

				// Disable star scaling for all armor moulds when enabled.
				float targetChance = enabled ? newChance : original.chance;
				bool targetLevelMultiplier = enabled ? false : original.levelMultiplier;

				if (drop.m_chance != targetChance || drop.m_levelMultiplier != targetLevelMultiplier)
				{
					log.Info($"[Deep North] {creatureName} | {itemName} | Chance: {drop.m_chance * 100f:0.##}% -> {targetChance * 100f:0.##}% | LevelMultiplier: {drop.m_levelMultiplier} -> {targetLevelMultiplier}");

					drop.m_chance = targetChance;
					drop.m_levelMultiplier = targetLevelMultiplier;
				}

				modifiedCount++;
			}

			if (modifiedCount != moulds.Count)
			{
				log.Warn($"[Deep North] {creatureName}: Found {modifiedCount}/{moulds.Count} expected armor mould drops.");
			}
			else
			{
				log.Info($"[Deep North] {creatureName}: All {modifiedCount} armor moulds set to {(enabled ? "modified" : "original")} values.");
			}
		}

		private static bool IsGem(string prefabName)
		{
			return prefabName == "GemstoneGreen"
				|| prefabName == "GemstoneBlue"
				|| prefabName == "GemstoneRed";
		}
	}
}
