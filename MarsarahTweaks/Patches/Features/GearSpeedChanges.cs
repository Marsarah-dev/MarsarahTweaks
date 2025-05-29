using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class GearSpeedChanges
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class GearSpeedChanges_Patch
		{
			private static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

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
			{ "ArmorRootChest", 0.01f },

			// Legs
			{ "ArmorBronzeLegs", 0f }, { "ArmorIronLegs", 0f }, { "ArmorWolfLegs", 0f },
			{ "ArmorPaddedGreaves", 0f }, { "ArmorCarapaceLegs", 0f }, { "ArmorFlametalLegs", 0f },
			{ "ArmorMageLegs", 0f }, { "ArmorMageLegs_Ashlands", 0f },
			{ "ArmorTrollLeatherLegs", 0.02f }, { "ArmorAshlandsMediumlegs", 0.02f },
			{ "ArmorRootLegs", 0.01f },

			// Two-handed weapons
			{ "Battleaxe", -0.05f }, { "BattleaxeCrystal", -0.05f },

			// Shields
			{ "ShieldFlametalTower", -0.10f }, { "ShieldBlackmetalTower", -0.10f },
			{ "ShieldBoneTower", -0.10f }, { "ShieldIronTower", -0.10f }, { "ShieldWoodTower", -0.10f },
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
							//MarsarahTweaks.LogInfo($"Backing up gear speed for {itemName}: {currentModifier}");
						}

						// Apply modification if different
						if (currentModifier != newModifier)
						{
							//MarsarahTweaks.LogInfo($"Applying gear speed modification for {itemName}: {newModifier}");
							item.m_itemData.m_shared.m_movementModifier = newModifier;
						}
					}
				}
				else if (wasChanged && originalMovementModifiers.TryGetValue(itemName, out float originalValue))
				{
					// Restore only if different
					if (currentModifier != originalValue)
					{
						//MarsarahTweaks.LogInfo($"Restoring original gear speed for {itemName}: {originalValue}");
						item.m_itemData.m_shared.m_movementModifier = originalValue;

						originalMovementModifiers.Remove(itemName);
					}
				}
			}
		}
	}
}
