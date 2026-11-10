
using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using UnityEngine;
using static DropTable;

namespace MarsarahTweaks.Patches.Grind
{
	internal class RareResourceChanges
	{
		private static readonly LogManager log = new LogManager("Rare Resource Changes", LogManager.LogLevel.Warning);

		private const string CharredFortressChest = "TreasureChest_charredfortress";
		private const int GemMinAmount = 2;
		private const int GemMaxAmount = 3;

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

		private static bool IsGem(string prefabName)
		{
			return prefabName == "GemstoneGreen"
				|| prefabName == "GemstoneBlue"
				|| prefabName == "GemstoneRed";
		}
	}
}
