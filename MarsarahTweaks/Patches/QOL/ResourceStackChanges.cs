using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.QOL
{
	internal class ResourceStackChanges
	{
		private static readonly LogManager log = new LogManager("Resource Stack Changes", LogManager.LogLevel.Warning);

		// Dictionaries
		private static readonly Dictionary<ItemDrop.ItemData.SharedData, int> originalStacks = new Dictionary<ItemDrop.ItemData.SharedData, int>();

		private static readonly Dictionary<string, int> resourceStackChanges = new Dictionary<string, int>()
		{
			// Valuables
			{ "Amber", 50 },
			{ "AmberPearl", 50 },
			{ "Ruby", 50 },
			{ "SilverNecklace", 50 },
			{ "AncientGemstoneBlack", 50 }, // Draumyx
			{ "AncientGemstonePurple", 50 }, // Veydris
			{ "AncientGemstoneOrange", 50 }, // Solryth
			{ "AncientGemstoneGreen", 50 }, // Grimvarn

			// Cores
			{ "SurtlingCore", 20 },

			// Weapon Moulds
			{ "MoldAtgeir", 30 },
			{ "MoldAxe", 30 },
			{ "MoldAxe2H", 30 },
			{ "MoldBow", 30 },
			{ "MoldCrossbow", 30 },
			{ "MoldFistweapon", 30 },
			{ "MoldKnife", 30 },
			{ "MoldMace", 30 },
			{ "MoldMace2H", 30 },
			{ "MoldSpear", 30 },
			{ "MoldSword", 30 },
			{ "MoldSword2H", 30 },

			// Shield Moulds
			{ "MoldShieldBuckler", 30 },
			{ "MoldShieldRound", 30 },
			{ "MoldShieldTower", 30 },

			// Staff Moulds
			{ "MoldStaffOrbofAhri", 30 },
			{ "MoldStafffrostorbs", 30 },
			{ "MoldStaffspiritcaller", 30 },
			{ "MoldStaffthunderblood", 30 },

			// Heavy Armor Moulds
			{ "MoldArmorGoldHelmet", 30 },
			{ "MoldArmorGoldChest", 30 },
			{ "MoldArmorGoldLegs", 30 },

			// Medium Armor Moulds
			{ "MoldArmorMediumHelmet", 30 },
			{ "MoldArmormediumChest", 30 },
			{ "MoldArmorMediumLegs", 30 },

			// Mage Armor Moulds
			{ "MoldArmorMageHelmet", 30 },
			{ "MoldArmorMageChest", 30 },
			{ "MoldArmorMageLegs", 30 },

			// Key Moulds
			{ "MoldKeys", 30 }
		};

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		private static class ObjectDB_AwakePatch
		{
			private static void Postfix(ObjectDB __instance)
			{
				// Must run on both clients and dedicated servers.
				UpdateResourceStacks(__instance);
			}
		}

		[HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
		private static class ObjectDB_CopyOtherDBPatch
		{
			private static void Postfix(ObjectDB __instance)
			{
				UpdateResourceStacks(__instance);
			}
		}

		public static void UpdateResourceStacks(ObjectDB objDB)
		{
			if (objDB == null || objDB.m_items == null) return;

			bool enabled = ConfigManager.LargerResourceStacksEnabled.Value;
			int modifiedCount = 0;
			int restoredCount = 0;
			int foundCount = 0;

			foreach (var entry in resourceStackChanges)
			{
				var item = objDB.m_items.Find(i => i != null && i.name == entry.Key);
				if (item == null) continue;

				ItemDrop itemDrop = item.GetComponent<ItemDrop>();
				var sharedData = itemDrop?.m_itemData?.m_shared;
				if (sharedData == null) continue;

				foundCount++;

				if (enabled)
				{
					int currentStack = sharedData.m_maxStackSize;
					int targetStack = entry.Value;

					// Never reduce existing maximum stack sizes.
					if (currentStack >= targetStack) continue;

					// Backup original stack size.
					if (!originalStacks.ContainsKey(sharedData))
					{
						originalStacks[sharedData] = currentStack;
					}

					// Apply new stack size.
					sharedData.m_maxStackSize = targetStack;
					modifiedCount++;

					log.Info($"{entry.Key}: Stack size {currentStack} -> {targetStack}");
				}
				else
				{
					// Restore original stack size.
					if (!originalStacks.TryGetValue(sharedData, out int originalStack)) continue;

					log.Info($"{entry.Key}: Restoring stack size {sharedData.m_maxStackSize} -> {originalStack}");

					sharedData.m_maxStackSize = originalStack;
					originalStacks.Remove(sharedData);
					restoredCount++;
				}
			}

			log.Info($"Resource stacks | Enabled: {enabled} | Found: {foundCount}/{resourceStackChanges.Count} | Modified: {modifiedCount} | Restored: {restoredCount}");
		}
	}
}