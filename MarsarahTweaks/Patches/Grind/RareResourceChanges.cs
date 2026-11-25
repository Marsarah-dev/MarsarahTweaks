
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
		private const string PutridHolePile = "morgenhole_pile";
		private const int BoneFragmentsMinAmount = 2;
		private const int BoneFragmentsMaxAmount = 5;
		private const int SoftTissueMinAmount = 2;
		private const int SoftTissueMaxAmount = 3;

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

		[HarmonyPatch(typeof(DropOnDestroyed), "OnDestroyed")]
		private static class DropOnDestroyed_OnDestroyedPatch
		{
			private static void Prefix(DropOnDestroyed __instance, out DropTable __state)
			{
				__state = null;

				if (__instance == null) return;
				if (ConfigManager.MoreRareResourcesEnabled?.Value != true) return;
				if (Utils.GetPrefabName(__instance.gameObject) != PutridHolePile) return;

				DropTable originalTable = __instance.m_dropWhenDestroyed;
				DropTable modifiedTable = GetModifiedPutridHoleLoot(originalTable);

				if (modifiedTable == null) return;

				__state = originalTable;
				__instance.m_dropWhenDestroyed = modifiedTable;
			}

			private static void Postfix(DropOnDestroyed __instance, DropTable __state)
			{
				if (__instance == null || __state == null) return;

				__instance.m_dropWhenDestroyed = __state;
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

		private static DropTable GetModifiedPutridHoleLoot(DropTable originalTable)
		{
			if (originalTable == null || originalTable.m_drops == null)
			{
				log.Warn("Putrid Hole pile has no valid loot table.");
				return null;
			}

			// Verify that both expected resources exist.
			HashSet<string> foundResources = new HashSet<string>();

			foreach (DropData drop in originalTable.m_drops)
			{
				if (drop.m_item == null) continue;

				if (drop.m_item.name == "BoneFragments" || drop.m_item.name == "Softtissue")
				{
					foundResources.Add(drop.m_item.name);
				}
			}

			if (foundResources.Count != 2)
			{
				log.Warn($"Putrid Hole pile loot table incomplete. Found {foundResources.Count}/2 expected resources. Changes skipped.");
				return null;
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

				if (modifiedDrop.m_item != null)
				{
					switch (modifiedDrop.m_item.name)
					{
						case "BoneFragments":
							modifiedDrop.m_stackMin = BoneFragmentsMinAmount;
							modifiedDrop.m_stackMax = BoneFragmentsMaxAmount;
							break;

						case "Softtissue":
							modifiedDrop.m_stackMin = SoftTissueMinAmount;
							modifiedDrop.m_stackMax = SoftTissueMaxAmount;
							break;
					}

					if (originalDrop.m_stackMin != modifiedDrop.m_stackMin || originalDrop.m_stackMax != modifiedDrop.m_stackMax)
					{
						log.Info($"[Putrid Hole] {modifiedDrop.m_item.name}: Amount {originalDrop.m_stackMin}-{originalDrop.m_stackMax} -> {modifiedDrop.m_stackMin}-{modifiedDrop.m_stackMax}");
					}
				}

				modifiedTable.m_drops.Add(modifiedDrop);
			}

			return modifiedTable;
		}

		private static bool IsGem(string prefabName)
		{
			return prefabName == "GemstoneGreen"
				|| prefabName == "GemstoneBlue"
				|| prefabName == "GemstoneRed";
		}

		private static void LogPutridHoleDrops(ZNetScene instance)
		{
			if (instance == null) return;

			GameObject prefab = instance.GetPrefab("morgenhole_pile");

			if (prefab == null)
			{
				log.Warn("Could not find morgenhole_pile prefab.");
				return;
			}

			DropOnDestroyed dropOnDestroyed = prefab.GetComponent<DropOnDestroyed>();

			if (dropOnDestroyed == null)
			{
				log.Warn("morgenhole_pile has no DropOnDestroyed component.");
				return;
			}

			DropTable dropTable = dropOnDestroyed.m_dropWhenDestroyed;

			if (dropTable == null || dropTable.m_drops == null)
			{
				log.Warn("morgenhole_pile has no valid drop table.");
				return;
			}

			log.Info("Original Putrid Hole Pile Drop Table", header: true);

			log.Info($"Prefab: {prefab.name}");
			log.Info($"DropMin: {dropTable.m_dropMin}");
			log.Info($"DropMax: {dropTable.m_dropMax}");
			log.Info($"DropChance: {dropTable.m_dropChance * 100f:0.##}%");
			log.Info($"OneOfEach: {dropTable.m_oneOfEach}");

			foreach (DropData drop in dropTable.m_drops)
			{
				if (drop.m_item == null) continue;

				log.Info($"  {drop.m_item.name} | Min: {drop.m_stackMin} | Max: {drop.m_stackMax} | Weight: {drop.m_weight} | DontScale: {drop.m_dontScale}");
			}

			log.Info("End of Putrid Hole Pile Drop Table", footer: true);
		}
	}
}
