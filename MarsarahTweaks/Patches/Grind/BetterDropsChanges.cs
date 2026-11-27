using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CharacterDrop;
using HarmonyLib;

namespace MarsarahTweaks.Patches.Grind
{
	internal class BetterDropsChanges
	{
		private static readonly LogManager log = new LogManager("Better Drops", LogManager.LogLevel.Warning);

		// Eykthir
		// - Ghost - Add Necklace: 1 (100% chance) - scrapped since update
		// The Elder
		// - Writhan - Writhan Roots 2x
		// Bonemass
		// - Fenring - Fenris Hair: between 1 - 2
		//			 - Fenris Claw (remove Wolf Fang): 1 (100% chance)
		// Bonemass
		// - Bat - Add Blood Bag: 50%
		// Yagluth
		// - Dvergr Rogue, Mage - Soft Tissue: 100% chance (from 25%)
		// Fader
		// - Barka - Frozen Branch 2x
		// - Moose - Moose Hide 3-5 (from 2)
		// - Hexen - Nornathread 2-4 (from 1-3)
		//         - Armor mould drop chances increased from 5% to 25%
		// - Krigen - Armor mould drop chances increased from 3% to 15%
		//          - Memorial Coal drop chance increased from 20% to 30%
		// - Fallen Warrior - Frostfire and Thunderblood Essence drop chances increased from 50% to 75%
		// - Armor mould star scaling disabled, original drop amounts preserved

		// Dictionaries
		private static Dictionary<string, Dictionary<string, (int min, int max, float chance, bool levelMultiplier, bool removed, bool newlyAdded)>> originalDrops = new Dictionary<string, Dictionary<string, (int min, int max, float chance, bool levelMultiplier, bool removed, bool newlyAdded)>>();
		private static readonly Dictionary<string, Dictionary<string, List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>>> bossDropMappings = new Dictionary<string, Dictionary<string, List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>>>()
		{
			// Valheim uses an exclusive upper bound for randomized drop amounts.
			// For an intended range of 2-4, use min: 2 and max: 5.
			// For fixed drop amounts, min and max can remain identical.
			// Null min/max preserves the original drop amount.
			// Null levelMultiplier preserves the original star scaling.
			// existingOnly prevents creating missing drop entries.

			/*["Eikthyr"] = new Dictionary<string, List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>>()
			{
				["Ghost"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("SilverNecklace", 1, 1, 1f, false, null, false)
				}
			},*/

			["The Elder"] = new Dictionary<string, List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>>()
			{
				["Writhan"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("WrithanRoots", 2, 2, 1f, false, null, false)
				}
			},

			["Bonemass"] = new Dictionary<string, List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>>()
			{
				["Fenring"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("WolfHairBundle", 1, 3, 1f, false, null, false),
					("WolfClaw", 1, 1, 1f, false, null, false),
					("WolfFang", 1, 1, 1f, true, null, false)
				},

				["Bat"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("Bloodbag", 1, 1, 0.5f, false, null, false)
				}
			},

			["Yagluth"] = new Dictionary<string, List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>>()
			{
				["Dverger"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("Softtissue", 1, 3, 1f, false, null, false)
				},
				["DvergerMage"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("Softtissue", 1, 3, 1f, false, null, false)
				}
			},

			["The Queen"] = new Dictionary<string, List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>>()
			{
				["DvergerAshlands"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("Softtissue", 1, 4, 1f, false, null, false)
				}
			},

			["Fader"] = new Dictionary<string, List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>>()
			{
				["Barka"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("BarkaBranch", 2, 2, 1f, false, null, false)
				},

				["JotunWitch"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("NornThread", 2, 5, 1f, false, null, false),

					// Mage armor moulds
					("MoldArmorMageHelmet", null, null, 0.25f, false, false, true),
					("MoldArmorMageChest", null, null, 0.25f, false, false, true),
					("MoldArmorMageLegs", null, null, 0.25f, false, false, true)
				},

				["Moose"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("MooseHide", 3, 6, 1f, false, null, false)
				},

				["JotunWarrior"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					// Heavy armor moulds
					("MoldArmorGoldHelmet", null, null, 0.15f, false, false, true),
					("MoldArmorGoldChest", null, null, 0.15f, false, false, true),
					("MoldArmorGoldLegs", null, null, 0.15f, false, false, true),

					// Medium armor moulds
					("MoldArmorMediumHelmet", null, null, 0.15f, false, false, true),
					("MoldArmormediumChest", null, null, 0.15f, false, false, true), // it's capitalized like this
					("MoldArmorMediumLegs", null, null, 0.15f, false, false, true),

					// Memorial Coal
					("MemorialCoal", null, null, 0.30f, false, null, true)
				},

				["JotunWarriorDualWield"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					// Heavy armor moulds
					("MoldArmorGoldHelmet", null, null, 0.15f, false, false, true),
					("MoldArmorGoldChest", null, null, 0.15f, false, false, true),
					("MoldArmorGoldLegs", null, null, 0.15f, false, false, true),

					// Medium armor moulds
					("MoldArmorMediumHelmet", null, null, 0.15f, false, false, true),
					("MoldArmormediumChest", null, null, 0.15f, false, false, true), // it's capitalized like this
					("MoldArmorMediumLegs", null, null, 0.15f, false, false, true),

					// Memorial Coal
					("MemorialCoal", null, null, 0.30f, false, null, true)
				},

				["FallenWarrior"] = new List<(string item, int? min, int? max, float chance, bool remove, bool? levelMultiplier, bool existingOnly)>()
				{
					("OrbFrostFire", null, null, 0.75f, false, null, true),
					("OrbThunderBlood", null, null, 0.75f, false, null, true)
				}
			}
		};

		// Apply drop modifications to altar-summoned Fallen Warriors
		[HarmonyPatch(typeof(CharacterDrop), "Start")]
		class FallenWarriorDrops_Patch
		{
			private static void Postfix(CharacterDrop __instance)
			{
				if (__instance == null) return;
				if (Utils.GetPrefabName(__instance.gameObject) != "FallenWarrior") return;
				if (!ConfigManager.BetterDropsEnabled.Value) return;

				if (ConfigManager.AutomaticProgressionHaltEnabled.Value && !GlobalKeyChecker.IsBossDefeated("Fader")) return;

				foreach (var dropInfo in bossDropMappings["Fader"]["FallenWarrior"])
				{
					CharacterDrop.Drop existingDrop = __instance.m_drops.Find(d => d.m_prefab != null && d.m_prefab.name == dropInfo.item);

					if (existingDrop == null)
					{
						log.Warn($"Expected drop {dropInfo.item} not found on summoned Fallen Warrior.");
						continue;
					}

					log.Info($"Updating summoned Fallen Warrior drop {dropInfo.item}: Chance {existingDrop.m_chance * 100f:0.##}% -> {dropInfo.chance * 100f:0.##}%");

					existingDrop.m_chance = dropInfo.chance;
				}
			}
		}

		// Helper to backup a drop outside of UpdateBetterDrops
		public static void BackupDrop(string creatureName, string item, CharacterDrop.Drop drop, bool removed = false, bool newlyAdded = false)
		{
			if (!originalDrops.ContainsKey(creatureName))
				originalDrops[creatureName] = new Dictionary<string, (int, int, float, bool, bool, bool)>();

			if (!originalDrops[creatureName].ContainsKey(item))
			{
				originalDrops[creatureName][item] = (drop.m_amountMin, drop.m_amountMax, drop.m_chance, drop.m_levelMultiplier, removed, newlyAdded);
				log.Info($"Backing up drop {item} for {creatureName} (removed:{removed}, newlyAdded:{newlyAdded}, levelMultiplier:{drop.m_levelMultiplier})");
			}
		}

		public static void UpdateBetterDrops(ZNetScene instance)
		{
			if (instance == null) return;

			bool progHaltEnabled = ConfigManager.AutomaticProgressionHaltEnabled.Value;

			foreach (var boss in bossDropMappings)
			{
				// Only proceed if progression halt allows it
				bool canMakeChanges = !progHaltEnabled || (progHaltEnabled && GlobalKeyChecker.IsBossDefeated(boss.Key));
				if (!canMakeChanges)
				{
					log.Info($"Skipping drops modifications for {boss.Key}");
					continue;
				}

				foreach (var creatureEntry in boss.Value)
				{
					string creatureName = creatureEntry.Key;
					GameObject prefab = instance.GetPrefab(creatureName);
					if (prefab == null) continue;

					CharacterDrop creatureDrop = prefab.GetComponent<CharacterDrop>();
					if (creatureDrop == null) continue;

					foreach (var dropInfo in creatureEntry.Value)
					{
						CharacterDrop.Drop existingDrop = creatureDrop.m_drops.Find(d => d.m_prefab.name == dropInfo.item);

						if (ConfigManager.BetterDropsEnabled.Value)
						{
							// Handle removal of drop
							if (dropInfo.remove)
							{
								if (existingDrop != null)
								{
									BackupDrop(creatureName, dropInfo.item, existingDrop, removed: true);
									log.Info($"Removing drop {dropInfo.item} from {creatureName}");
									creatureDrop.m_drops.Remove(existingDrop);
								}
								continue; // Skip adding or modifying this drop since it's marked for removal
							}

							// If drop doesn't exist, create it unless marked as existing only
							if (existingDrop == null)
							{
								if (dropInfo.existingOnly)
								{
									log.Warn($"Expected drop {dropInfo.item} not found for {creatureName}. Skipping modification.");
									continue;
								}

								if (!dropInfo.min.HasValue || !dropInfo.max.HasValue)
								{
									log.Warn($"Cannot create drop {dropInfo.item} for {creatureName} without valid drop amounts.");
									continue;
								}

								GameObject dropPrefab = instance.GetPrefab(dropInfo.item);

								if (dropPrefab == null) continue;

								existingDrop = new CharacterDrop.Drop
								{
									m_prefab = dropPrefab,
									m_amountMin = dropInfo.min.Value,
									m_amountMax = dropInfo.max.Value,
									m_chance = dropInfo.chance,
									m_dontScale = false
								};

								if (dropInfo.levelMultiplier.HasValue)
									existingDrop.m_levelMultiplier = dropInfo.levelMultiplier.Value;

								creatureDrop.m_drops.Add(existingDrop);
								log.Info($"Adding new drop {dropInfo.item} to {creatureName} (min:{dropInfo.min}, max:{dropInfo.max}, chance:{dropInfo.chance})");

								// Backup newly added drop
								BackupDrop(creatureName, dropInfo.item, existingDrop, newlyAdded: true);
								continue;
							}

							// If drop exists, apply new values
							if (existingDrop != null)
							{
								BackupDrop(creatureName, dropInfo.item, existingDrop);

								int targetMin = dropInfo.min ?? existingDrop.m_amountMin;
								int targetMax = dropInfo.max ?? existingDrop.m_amountMax;
								bool targetLevelMultiplier = dropInfo.levelMultiplier ?? existingDrop.m_levelMultiplier;

								log.Info($"Updating drop {dropInfo.item} for {creatureName}: Amount {existingDrop.m_amountMin}-{existingDrop.m_amountMax} -> {targetMin}-{targetMax}, Chance {existingDrop.m_chance * 100f:0.##}% -> {dropInfo.chance * 100f:0.##}%, LevelMultiplier {existingDrop.m_levelMultiplier} -> {targetLevelMultiplier}");

								existingDrop.m_amountMin = targetMin;
								existingDrop.m_amountMax = targetMax;
								existingDrop.m_chance = dropInfo.chance;
								existingDrop.m_levelMultiplier = targetLevelMultiplier;
							}
						}
						else
						{
							// Restore backup if config is OFF
							if (originalDrops.TryGetValue(creatureName, out var backup) && backup.TryGetValue(dropInfo.item, out var original))
							{
								if (original.newlyAdded) // This drop was added by Better Drops
								{
									if (existingDrop != null)
									{
										creatureDrop.m_drops.Remove(existingDrop);
										log.Info($"Removed newly added drop {dropInfo.item} for {creatureName}");
									}
								}
								else if (original.removed) // This drop was removed by Better Drops
								{
									if (existingDrop == null)
									{
										GameObject dropPrefab = instance.GetPrefab(dropInfo.item);
										if (dropPrefab != null)
										{
											existingDrop = new CharacterDrop.Drop
											{
												m_prefab = dropPrefab,
												m_amountMin = original.min,
												m_amountMax = original.max,
												m_chance = original.chance,
												m_levelMultiplier = original.levelMultiplier,
												m_dontScale = false
											};
											creatureDrop.m_drops.Add(existingDrop);
											log.Info($"Restored previously removed drop {dropInfo.item} for {creatureName}");
										}
									}
								}
								else // This drop existed before and was modified
								{
									if (existingDrop != null)
									{
										existingDrop.m_amountMin = original.min;
										existingDrop.m_amountMax = original.max;
										existingDrop.m_chance = original.chance;
										existingDrop.m_levelMultiplier = original.levelMultiplier;
										log.Info($"Restored drop {dropInfo.item} for {creatureName} (min:{original.min}, max:{original.max}, chance:{original.chance}, levelMultiplier:{original.levelMultiplier})");
									}
								}

								// Remove from backup
								backup.Remove(dropInfo.item);
								if (backup.Count == 0)
								{
									originalDrops.Remove(creatureName);
									log.Info($"Cleared backup for {creatureName}");
								}
							}
						}
					}
				}
			}

			// DEBUG - Verify creature drop tables after applying or restoring modifications
			//LogCreatureDrops(instance);
		}

		public static void RestoreBetterDrops(ZNetScene instance)
		{
			if (instance == null) return;

			bool progHaltEnabled = ConfigManager.AutomaticProgressionHaltEnabled.Value;

			foreach (var boss in bossDropMappings)
			{
				// Only restore for bosses whose progression halt is enabled and not defeated yet
				bool canRestore = progHaltEnabled && !GlobalKeyChecker.IsBossDefeated(boss.Key);
				if (!canRestore)
				{
					log.Info($"Skipping drops restore for {boss.Key}");
					continue;
				}

				foreach (var creatureEntry in boss.Value)
				{
					string creatureName = creatureEntry.Key;
					GameObject prefab = instance.GetPrefab(creatureName);
					if (prefab == null) continue;

					CharacterDrop creatureDrop = prefab.GetComponent<CharacterDrop>();
					if (creatureDrop == null) continue;

					foreach (var dropInfo in creatureEntry.Value)
					{
						CharacterDrop.Drop existingDrop = creatureDrop.m_drops.Find(d => d.m_prefab.name == dropInfo.item);

						if (originalDrops.TryGetValue(creatureName, out var creatureBackup) && creatureBackup.TryGetValue(dropInfo.item, out var original))
						{
							if (original.newlyAdded) // remove drops added by Better Drops
							{
								if (existingDrop != null)
								{
									creatureDrop.m_drops.Remove(existingDrop);
									log.Info($"Removed newly added drop {dropInfo.item} for {creatureName}");
								}
							}
							else if (original.removed) // restore drops removed by Better Drops
							{
								if (existingDrop == null)
								{
									GameObject dropPrefab = instance.GetPrefab(dropInfo.item);
									if (dropPrefab != null)
									{
										existingDrop = new CharacterDrop.Drop
										{
											m_prefab = dropPrefab,
											m_amountMin = original.min,
											m_amountMax = original.max,
											m_chance = original.chance,
											m_levelMultiplier = original.levelMultiplier,
											m_dontScale = false
										};
										creatureDrop.m_drops.Add(existingDrop);
										log.Info($"Restored previously removed drop {dropInfo.item} for {creatureName}");
									}
								}
							}
							else // restore modified drops
							{
								if (existingDrop != null)
								{
									existingDrop.m_amountMin = original.min;
									existingDrop.m_amountMax = original.max;
									existingDrop.m_chance = original.chance;
									existingDrop.m_levelMultiplier = original.levelMultiplier;
									log.Info($"Restored drop {dropInfo.item} for {creatureName} (min:{original.min}, max:{original.max}, chance:{original.chance}, levelMultiplier:{original.levelMultiplier})");
								}
							}

							// Remove from backup
							creatureBackup.Remove(dropInfo.item);
							if (creatureBackup.Count == 0)
							{
								originalDrops.Remove(creatureName);
								log.Info($"Cleared backup for {creatureName}");
							}
						}
					}
				}
			}

			// DEBUG - Verify creature drop tables after restoring modifications for Progression Halt
			//LogCreatureDrops(instance);
		}

		private static void LogCreatureDrops(ZNetScene instance)
		{
			if (instance == null) return;

			string[] creatureNames =
			{
				"JotunWarrior",
				"JotunWarriorDualWield",
				"JotunWitch",
				"Moose",
				"FallenWarrior"
			};

			log.Info($"Creature Drop Tables | Better Drops: {ConfigManager.BetterDropsEnabled.Value} | Progression Halt: {ConfigManager.AutomaticProgressionHaltEnabled.Value} | Fader Defeated: {GlobalKeyChecker.FaderDefeated}", header: true);

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
	}
}
