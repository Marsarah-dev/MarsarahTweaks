using HarmonyLib;
using System.Linq;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal static class MoreUsableFuel
	{
		private static readonly LogManager log = new LogManager("More Usable Fuel", LogManager.LogLevel.Warning);

		private static readonly string[] kilnFuelPrefabs =
		{
			"ElderBark", // Ancient Bark
			"Blackwood", // Ashwood
			"Frostwood"  // Timberwood
		};

		[HarmonyPatch(typeof(Smelter), "Awake")]
		public static class AddWoodToKiln_Patch
		{
			private static void Postfix(Smelter __instance)
			{
				UpdateKiln(__instance);
			}
		}

		[HarmonyPatch(typeof(ShieldGenerator), "Start")]
		public static class AddWitheredBonesToShieldGenerator_Patch
		{
			private static void Postfix(ShieldGenerator __instance)
			{
				UpdateShieldGenerator(__instance);
			}
		}

		public static void UpdateMoreUsableFuel()
		{
			UpdateAllKilns();
			UpdateAllShieldGenerators();
		}

		private static void UpdateAllKilns()
		{
			foreach (var kiln in UnityEngine.Object.FindObjectsByType<Smelter>(FindObjectsSortMode.None))
			{
				UpdateKiln(kiln);
			}
		}

		private static void UpdateKiln(Smelter kiln)
		{
			if (kiln == null || kiln.m_conversion == null) return;

			// Only modify smelters that produce Coal.
			if (!kiln.m_conversion.Exists(conv => conv.m_to != null && conv.m_to.name == "Coal")) return;

			var coalPrefab = ObjectDB.instance?.GetItemPrefab("Coal");

			if (!coalPrefab)
			{
				log.Warn("Coal prefab not found.");
				return;
			}

			var coalDrop = coalPrefab.GetComponent<ItemDrop>();

			if (!coalDrop)
			{
				log.Warn("Coal does not have ItemDrop component.");
				return;
			}

			foreach (string fuelPrefabName in kilnFuelPrefabs)
			{
				var fuelPrefab = ObjectDB.instance?.GetItemPrefab(fuelPrefabName);

				if (!fuelPrefab)
				{
					log.Warn($"{fuelPrefabName} prefab not found.");
					continue;
				}

				var fuelDrop = fuelPrefab.GetComponent<ItemDrop>();

				if (!fuelDrop)
				{
					log.Warn($"{fuelPrefabName} does not have ItemDrop component.");
					continue;
				}

				var existingConversion = kiln.m_conversion.FirstOrDefault(conv => conv.m_from == fuelDrop && conv.m_to == coalDrop);

				if (ConfigManager.MoreUsableFuelEnabled.Value)
				{
					if (existingConversion == null)
					{
						var newConversion = new Smelter.ItemConversion
						{
							m_from = fuelDrop,
							m_to = coalDrop
						};

						kiln.m_conversion.Add(newConversion);
						log.Info($"Added {fuelPrefabName} as fuel for Kiln.");
					}
				}
				else
				{
					if (existingConversion != null)
					{
						kiln.m_conversion.Remove(existingConversion);
						log.Info($"Removed {fuelPrefabName} as fuel from Kiln.");
					}
				}
			}
		}

		private static void UpdateAllShieldGenerators()
		{
			foreach (var sg in UnityEngine.Object.FindObjectsByType<ShieldGenerator>(FindObjectsSortMode.None))
			{
				UpdateShieldGenerator(sg);
			}
		}

		private static void UpdateShieldGenerator(ShieldGenerator sg)
		{
			var prefab = ObjectDB.instance?.GetItemPrefab("WitheredBone");

			if (!prefab)
			{
				log.Warn("WitheredBone prefab not found.");
				return;
			}

			var itemDrop = prefab.GetComponent<ItemDrop>();

			if (!itemDrop)
			{
				log.Warn("WitheredBone does not have ItemDrop component.");
				return;
			}

			if (ConfigManager.MoreUsableFuelEnabled.Value)
			{
				if (!sg.m_fuelItems.Contains(itemDrop))
				{
					sg.m_fuelItems.Add(itemDrop);
					log.Info("Added WitheredBone as fuel for Shield Generator.");
				}
			}
			else
			{
				if (sg.m_fuelItems.Contains(itemDrop))
				{
					sg.m_fuelItems.Remove(itemDrop);
					log.Info("Removed WitheredBone as fuel from Shield Generator.");
				}
			}
		}
	}
}