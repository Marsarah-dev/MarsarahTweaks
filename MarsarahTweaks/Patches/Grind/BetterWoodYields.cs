
using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.Grind
{
	internal class BetterWoodYields
	{
		private static readonly LogManager log = new LogManager("Better Wood Yields", LogManager.LogLevel.Warning);

		// Target distribution: 30% regular Wood, 70% specialty wood
		private const float WoodWeight = 0.3f;
		private const float SpecialtyWoodWeight = 0.7f;

		// Half-log prefab -> Specialty wood prefab
		private static readonly Dictionary<string, string> woodYieldChanges = new Dictionary<string, string>()
		{
			// Core Wood
			{ "PineTree_log_half", "RoundLog" },

			// Fine Wood
			{ "Birch_log_half", "FineWood" },
			{ "Oak_log_half", "FineWood" },

			// Ashwood
			{ "AshlandsTreeLogHalf1", "Blackwood" },
			{ "AshlandsTreeLogHalf2", "Blackwood" },

			// Timberwood
			{ "FirTree_big_log_half", "Frostwood" },
			{ "FirTree_Snow_log_half", "Frostwood" },
			{ "PineTree_Snow_log_half", "Frostwood" },
			{ "PineTree_Snow_log_XL_half", "Frostwood" },
			{ "PineTree_Snow_log_half_frost_troll", "Frostwood" }
		};

		// Original weights are stored by DropTable reference so they remain
		// accessible even when Progression Halt temporarily removes a drop table.
		private static readonly Dictionary<DropTable, (string prefabName, int woodIndex, int specialtyIndex, float woodWeight, float specialtyWeight)> originalDropWeights = new Dictionary<DropTable, (string prefabName, int woodIndex, int specialtyIndex, float woodWeight, float specialtyWeight)>();

		private static ZNetScene cachedScene;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class BetterWoodYields_Patch
		{
			private static void Postfix(ZNetScene __instance)
			{
				if (__instance == null) return;

				UpdateBetterWoodYields(__instance, false);
			}
		}

		public static void UpdateBetterWoodYields(ZNetScene instance, bool wasChanged)
		{
			if (instance == null) return;

			// Clear backups when a new scene is initialized
			if (cachedScene != instance)
			{
				cachedScene = instance;
				originalDropWeights.Clear();
			}

			// Find and back up original drop tables
			foreach (var entry in woodYieldChanges)
			{
				string prefabName = entry.Key;
				string specialtyWoodName = entry.Value;

				GameObject prefab = instance.GetPrefab(prefabName);
				if (prefab == null) continue;

				TreeLog treeLog = prefab.GetComponent<TreeLog>();
				if (treeLog == null) continue;

				DropTable dropTable = treeLog.m_dropWhenDestroyed;

				// Do not recreate drop tables disabled by Progression Halt
				if (dropTable == null || dropTable.m_drops == null) continue;

				// The same DropTable may be shared by multiple prefabs
				if (originalDropWeights.ContainsKey(dropTable)) continue;

				// Only modify the expected two-item drop tables
				if (dropTable.m_drops.Count != 2)
				{
					log.Warn($"Unexpected drop table size for {prefabName}: {dropTable.m_drops.Count}");
					continue;
				}

				int woodIndex = -1;
				int specialtyIndex = -1;

				for (int i = 0; i < dropTable.m_drops.Count; i++)
				{
					DropTable.DropData drop = dropTable.m_drops[i];
					if (drop.m_item == null) continue;

					if (drop.m_item.name == "Wood")
						woodIndex = i;
					else if (drop.m_item.name == specialtyWoodName)
						specialtyIndex = i;
				}

				if (woodIndex < 0 || specialtyIndex < 0)
				{
					log.Warn($"Could not identify Wood and {specialtyWoodName} in {prefabName}");
					continue;
				}

				float originalWoodWeight = dropTable.m_drops[woodIndex].m_weight;
				float originalSpecialtyWeight = dropTable.m_drops[specialtyIndex].m_weight;

				originalDropWeights[dropTable] = (prefabName, woodIndex, specialtyIndex, originalWoodWeight, originalSpecialtyWeight);

				log.Info($"Backing up wood weights for {prefabName}: Wood {originalWoodWeight}, {specialtyWoodName} {originalSpecialtyWeight}");
			}

			// Apply or restore the cached drop tables
			foreach (var entry in originalDropWeights)
			{
				DropTable dropTable = entry.Key;
				var original = entry.Value;

				if (dropTable == null || dropTable.m_drops == null) continue;

				if (ConfigManager.BetterWoodYieldsEnabled.Value)
				{
					float totalWeight = original.woodWeight + original.specialtyWeight;

					SetWoodWeights(dropTable, original.woodIndex, original.specialtyIndex, totalWeight * WoodWeight, totalWeight * SpecialtyWoodWeight);

					log.Info($"Applied Better Wood Yields to {original.prefabName}: 30% Wood, 70% specialty wood");
				}
				else if (wasChanged)
				{
					SetWoodWeights(dropTable, original.woodIndex, original.specialtyIndex, original.woodWeight, original.specialtyWeight);

					log.Info($"Restored vanilla wood weights for {original.prefabName}");
				}
			}
		}

		private static void SetWoodWeights(DropTable dropTable, int woodIndex, int specialtyIndex, float woodWeight, float specialtyWeight)
		{
			// DropData is a struct, so modified entries must be assigned back to the list
			DropTable.DropData woodDrop = dropTable.m_drops[woodIndex];
			woodDrop.m_weight = woodWeight;
			dropTable.m_drops[woodIndex] = woodDrop;

			DropTable.DropData specialtyDrop = dropTable.m_drops[specialtyIndex];
			specialtyDrop.m_weight = specialtyWeight;
			dropTable.m_drops[specialtyIndex] = specialtyDrop;
		}

		// Debug helper: prints all half-log drop tables.
		// Intentionally not called during normal operation.
		private static void LogHalfLogDrops(ZNetScene instance)
		{
			log.Info("=== HALF-LOG DROP TABLES ===");

			foreach (GameObject prefab in instance.m_prefabs)
			{
				if (prefab == null) continue;
				if (prefab.name.IndexOf("half", StringComparison.OrdinalIgnoreCase) < 0) continue;

				TreeLog treeLog = prefab.GetComponent<TreeLog>();
				if (treeLog == null) continue;

				DropTable dropTable = treeLog.m_dropWhenDestroyed;

				if (dropTable == null || dropTable.m_drops == null)
				{
					log.Info($"{prefab.name}: No drop table");
					continue;
				}

				float totalWeight = 0f;

				foreach (DropTable.DropData drop in dropTable.m_drops)
				{
					if (drop.m_item == null) continue;
					if (drop.m_weight > 0f) totalWeight += drop.m_weight;
				}

				log.Info($"Half-log: {prefab.name} | Rolls: {dropTable.m_dropMin}-{dropTable.m_dropMax} | Chance: {dropTable.m_dropChance:P1} | OneOfEach: {dropTable.m_oneOfEach}");

				foreach (DropTable.DropData drop in dropTable.m_drops)
				{
					if (drop.m_item == null) continue;

					float probability = totalWeight > 0f ? drop.m_weight / totalWeight * 100f : 0f;

					log.Info($"  {drop.m_item.name}: Weight {drop.m_weight}, Probability {probability:0.##}%, Amount {drop.m_stackMin}-{drop.m_stackMax}");
				}
			}

			log.Info("=== END HALF-LOG DROP TABLES ===");
		}
	}
}
