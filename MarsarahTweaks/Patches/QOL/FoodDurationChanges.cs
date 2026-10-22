using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.QOL
{
	internal class FoodDurationChanges
	{
		private static readonly LogManager log = new LogManager("Food Durations", LogManager.LogLevel.Warning);

		// Only foods whose duration differs from vanilla are listed here.
		// Values are minutes. No Deep North foods or feasts are modified.
		private static readonly Dictionary<string, int> foodDurations = new Dictionary<string, int>()
		{
			// Meadows: directly edible ingredients -> 15 minutes
			{ "Raspberry", 15 },

			// Black Forest: directly edible ingredients -> 15; prepared dishes -> 30
			{ "Blueberries", 15 },
			{ "MushroomYellow", 15 },
			{ "QueensJam", 30 },
			{ "DeerStew", 30 },
			{ "MinceMeatSauce", 30 },
			{ "CarrotSoup", 30 },

			// Swamp: prepared dishes -> 30 minutes
			{ "BlackSoup", 30 },
			{ "ShocklateSmoothie", 30 }, // Muckshake
			{ "Sausages", 30 },
			{ "TurnipStew", 30 },

			// Mountains: prepared dishes -> 30 minutes
			{ "OnionSoup", 30 },
			{ "WolfMeatSkewer", 30 },
			{ "Eyescream", 30 },

			// Plains: prepared dishes -> 30 minutes
			{ "FishWraps", 30 },
			{ "Bread", 30 },

			// Mistlands: prepared dishes -> 30 minutes
			{ "MisthareSupreme", 30 },
			{ "MushroomOmelette", 30 },
			{ "Salad", 30 },
			{ "YggdrasilPorridge", 30 },
			{ "MagicallyStuffedShroom", 30 },

			// Ashlands: simply cooked meat -> 25; prepared dishes -> 30
			{ "CookedAsksvinMeat", 25 },
			{ "CookedVoltureMeat", 25 },
			{ "FierySvinstew", 30 },
			{ "MashedMeat", 30 },
			{ "SpicyMarmalade", 30 },
			{ "ScorchingMedley", 30 },
			{ "SizzlingBerryBroth", 30 },
			{ "SparklingShroomshake", 30 }
		};

		private static readonly Dictionary<string, float> originalFoodDurations = new Dictionary<string, float>();

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		private class FoodDurationChanges_Patch
		{
			private static void Postfix(ObjectDB __instance)
			{
				if (__instance == null || ZNet.instance == null) return;

				UpdateFoodDurations(__instance, false);
			}
		}

		public static void UpdateFoodDurations(ObjectDB objDB, bool wasModified)
		{
			if (objDB == null || objDB.m_items == null) return;

			bool enabled = ConfigManager.StandardizedFoodDurationsEnabled.Value;
			if (!enabled && (!wasModified || originalFoodDurations.Count == 0)) return;

			foreach (KeyValuePair<string, int> entry in foodDurations)
			{
				string prefabName = entry.Key;
				GameObject prefab = objDB.m_items.Find(item => item != null && item.name == prefabName);

				if (prefab == null)
				{
					continue;
				}

				ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
				if (itemDrop == null || itemDrop.m_itemData == null) continue;

				ItemDrop.ItemData.SharedData shared = itemDrop.m_itemData.m_shared;

				if (enabled)
				{
					if (!originalFoodDurations.ContainsKey(prefabName))
					{
						originalFoodDurations[prefabName] = shared.m_foodBurnTime;
					}

					float newDuration = entry.Value * 60f;
					if (shared.m_foodBurnTime < newDuration)
					{
						log.Info($"Setting {prefabName} duration from {shared.m_foodBurnTime}s to {newDuration}s");
						shared.m_foodBurnTime = newDuration;
					}
				}
				else if (originalFoodDurations.TryGetValue(prefabName, out float originalDuration))
				{
					if (shared.m_foodBurnTime != originalDuration)
					{
						log.Info($"Restoring {prefabName} duration to {originalDuration}s");
						shared.m_foodBurnTime = originalDuration;
					}
				}
			}

			if (!enabled) originalFoodDurations.Clear();
		}
	}
}
