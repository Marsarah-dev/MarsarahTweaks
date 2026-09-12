using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class GearSpeedChanges
	{
		private static readonly LogManager log = new LogManager("Gear Speed", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class GearSpeedChanges_Patch
		{
			private static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				LogGearSpeeds(__instance);

				UpdateGearSpeed(__instance, false);
			}
		}

		// Dictionaries
		private static Dictionary<string, float> originalMovementModifiers = new Dictionary<string, float>();
		private static Dictionary<string, float> movementModifiers = new Dictionary<string, float>
		{
			// Chest
			{ "ArmorBronzeChest", 0f }, { "ArmorIronChest", 0f }, { "ArmorWolfChest", 0f },
			{ "ArmorPaddedCuirass", 0f }, { "ArmorCarapaceChest", 0f }, { "ArmorFlametalChest", 0f },
			{ "ArmorMageChest", 0f }, { "ArmorMageChest_Ashlands", 0f },
			{ "ArmorTrollLeatherChest", 0.02f }, { "ArmorAshlandsMediumChest", 0.02f },
			{ "ArmorRootChest", 0.01f }, { "ArmorBerserkerChest", 0.02f }, { "ArmorBerserkerUndeadChest", 0.02f },
			{ "ArmorLoxChest", 0.02f },

			// Legs
			{ "ArmorBronzeLegs", 0f }, { "ArmorIronLegs", 0f }, { "ArmorWolfLegs", 0f },
			{ "ArmorPaddedGreaves", 0f }, { "ArmorCarapaceLegs", 0f }, { "ArmorFlametalLegs", 0f },
			{ "ArmorMageLegs", 0f }, { "ArmorMageLegs_Ashlands", 0f },
			{ "ArmorTrollLeatherLegs", 0.02f }, { "ArmorAshlandsMediumlegs", 0.02f },
			{ "ArmorRootLegs", 0.01f }, { "ArmorBerserkerLegs", 0.02f }, { "ArmorBerserkerUndeadLegs", 0.02f },
			{ "ArmorLoxLegs", 0.02f },

			// Two-handed weapons
			{ "Battleaxe", -0.10f }, { "BattleaxeCrystal", -0.10f }, { "BattleaxeBlackmetal", -0.10f }, { "BattleaxeSkullSplittur", -0.10f }, 
			{ "SledgeStagbreaker", -0.10f }, { "SledgeIron", -0.10f }, { "SledgeDemolisher", -0.10f },

			// Shields
			/*{ "ShieldFlametalTower", -0.10f }, { "ShieldBlackmetalTower", -0.10f },
			{ "ShieldBoneTower", -0.10f }, { "ShieldIronTower", -0.10f }, { "ShieldWoodTower", -0.10f },*/
			{ "ShieldSerpentscale", -0.05f }
		};

		public static void UpdateGearSpeed(ObjectDB objDB, bool wasChanged)
		{
			if (objDB == null || objDB.m_items == null) return;

			foreach (GameObject prefab in objDB.m_items)
			{
				ItemDrop item = prefab.GetComponent<ItemDrop>();
				if (item == null) continue;

				string itemName = item.name;
				float currentModifier = item.m_itemData.m_shared.m_movementModifier;

				if (ConfigManager.GearSpeedModifiersEnabled.Value)
				{
					if (movementModifiers.TryGetValue(itemName, out float newModifier))
					{
						// Backup original value
						if (!originalMovementModifiers.ContainsKey(itemName))
						{
							originalMovementModifiers[itemName] = currentModifier;
							log.Info($"Backing up gear speed for {itemName}: {currentModifier}");
						}

						// Apply modification if different
						if (currentModifier != newModifier)
						{
							log.Info($"Applying gear speed modification for {itemName}: {newModifier}");
							item.m_itemData.m_shared.m_movementModifier = newModifier;
						}
					}
				}
				else if (wasChanged && originalMovementModifiers.TryGetValue(itemName, out float originalValue))
				{
					// Restore only if different
					if (currentModifier != originalValue)
					{
						log.Info($"Restoring original gear speed for {itemName}: {originalValue}");
						item.m_itemData.m_shared.m_movementModifier = originalValue;

						originalMovementModifiers.Remove(itemName);
					}
				}
			}
		}

		private static void LogGearSpeeds(ObjectDB objDB)
		{
			if (objDB == null || objDB.m_items == null)
			{
				log.Warn("Cannot log gear speeds because ObjectDB or its item list is null.");
				return;
			}

			log.Info("=== Gear Movement Modifiers ===");

			foreach (GameObject prefab in objDB.m_items)
			{
				ItemDrop item = prefab.GetComponent<ItemDrop>();
				if (item == null) continue;

				ItemDrop.ItemData itemData = item.m_itemData;
				ItemDrop.ItemData.ItemType itemType = itemData.m_shared.m_itemType;

				bool isRelevant =
					itemData.IsWeapon() ||
					itemType == ItemDrop.ItemData.ItemType.Helmet ||
					itemType == ItemDrop.ItemData.ItemType.Chest ||
					itemType == ItemDrop.ItemData.ItemType.Legs ||
					itemType == ItemDrop.ItemData.ItemType.Shield ||
					itemType == ItemDrop.ItemData.ItemType.Tool;

				if (!isRelevant) continue;

				string itemName = item.name;
				string displayName = itemData.m_shared.m_name;
				float vanillaModifier = itemData.m_shared.m_movementModifier;

				if (movementModifiers.TryGetValue(itemName, out float modifiedModifier))
				{
					log.Info($"{itemName} | {displayName} | Vanilla: {vanillaModifier} | Modified: {modifiedModifier}");
				}
				else
				{
					log.Info($"{itemName} | {displayName} | Vanilla: {vanillaModifier}");
				}
			}

			log.Info("=== End Gear Movement Modifiers ===");
		}
	}
}
