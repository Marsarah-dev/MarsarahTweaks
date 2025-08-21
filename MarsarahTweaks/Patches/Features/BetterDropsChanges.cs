using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CharacterDrop;

namespace MarsarahTweaks.Patches.Features
{
	internal class BetterDropsChanges
	{
		// Eykthir - Ghost
		// - Add Necklace: 1 (100% chance)
		// Bonemass - Fenring
		// - Fenris Hair: between 1 - 2
		// - Fenris Claw (remove Wolf Fang): 1 (100% chance)
		// Bonemass - Bat
		// - Add Blood Bag: 50%
		// Yagluth - Dvergr Rogue, Mage
		// - Soft Tissue: 100% chance (from 25%)

		// Dictionaries
		private static Dictionary<string, Dictionary<string, (int min, int max, float chance, bool removed, bool newlyAdded)>> originalDrops = new Dictionary<string, Dictionary<string, (int min, int max, float chance, bool removed, bool newlyAdded)>>();
		private static readonly Dictionary<string, Dictionary<string, List<(string item, int min, int max, float chance, bool remove)>>> bossDropMappings = new Dictionary<string, Dictionary<string, List<(string item, int min, int max, float chance, bool remove)>>>()
		{
			["Eikthyr"] = new Dictionary<string, List<(string item, int min, int max, float chance, bool remove)>>()
			{
				["Ghost"] = new List<(string item, int min, int max, float chance, bool remove)>()
				{
					("SilverNecklace", 1, 1, 1f, false)
				}
			},

			["Bonemass"] = new Dictionary<string, List<(string item, int min, int max, float chance, bool remove)>>()
			{
				["Fenring"] = new List<(string item, int min, int max, float chance, bool remove)>()
				{
					("WolfHairBundle", 1, 2, 1f, false),
					("WolfClaw", 1, 1, 1f, false),
					("WolfFang", 1, 1, 1f, true)
				},

				["Bat"] = new List<(string item, int min, int max, float chance, bool remove)>()
				{
					("Bloodbag", 1, 1, 0.5f, false)
				}
			},

			["Yagluth"] = new Dictionary<string, List<(string item, int min, int max, float chance, bool remove)>>()
			{
				["Dverger"] = new List<(string item, int min, int max, float chance, bool remove)>()
				{
					("Softtissue", 1, 2, 1f, false)
				},
				["DvergerMage"] = new List<(string item, int min, int max, float chance, bool remove)>()
				{
					("Softtissue", 1, 2, 1f, false)
				}
			},

			["The Queen"] = new Dictionary<string, List<(string item, int min, int max, float chance, bool remove)>>()
			{
				["DvergerAshlands"] = new List<(string item, int min, int max, float chance, bool remove)>()
				{
					("Softtissue", 1, 3, 1f, false)
				}
			}
		};

		// Helper to backup a drop outside of UpdateBetterDrops
		public static void BackupDrop(string creatureName, string item, CharacterDrop.Drop drop, bool removed = false, bool newlyAdded = false)
		{
			if (!originalDrops.ContainsKey(creatureName))
				originalDrops[creatureName] = new Dictionary<string, (int, int, float, bool, bool)>();
			if (!originalDrops[creatureName].ContainsKey(item))
			{
				originalDrops[creatureName][item] = (drop.m_amountMin, drop.m_amountMax, drop.m_chance, removed, newlyAdded);
				//MarsarahTweaks.LogInfo($"[Better Drops] Backing up drop {item} for {creatureName} (removed:{removed}, newlyAdded:{newlyAdded})");
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
					//MarsarahTweaks.LogInfo($"[Better Drops] Skipping drops modifications for {boss.Key}");
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
									//MarsarahTweaks.LogInfo($"[Better Drops] Removing drop {dropInfo.item} from {creatureName}");
									creatureDrop.m_drops.Remove(existingDrop);
								}
								continue; // Skip adding or modifying this drop since it's marked for removal
							}

							// If drop doesn't exist, create it
							if (existingDrop == null)
							{
								GameObject dropPrefab = instance.GetPrefab(dropInfo.item);
								if (dropPrefab == null) continue;

								existingDrop = new CharacterDrop.Drop
								{
									m_prefab = dropPrefab,
									m_amountMin = dropInfo.min,
									m_amountMax = dropInfo.max,
									m_chance = dropInfo.chance,
									m_dontScale = false
								};
								creatureDrop.m_drops.Add(existingDrop);
								//MarsarahTweaks.LogInfo($"[Better Drops] Adding new drop {dropInfo.item} to {creatureName} (min:{dropInfo.min}, max:{dropInfo.max}, chance:{dropInfo.chance})");

								// Backup newly added drop
								BackupDrop(creatureName, dropInfo.item, existingDrop, newlyAdded: true);
								continue;
							}

							// If drop exists, apply new values
							if (existingDrop != null)
							{
								BackupDrop(creatureName, dropInfo.item, existingDrop);
								//MarsarahTweaks.LogInfo($"[Better Drops] Updating drop {dropInfo.item} for {creatureName} (min:{dropInfo.min}, max:{dropInfo.max}, chance:{dropInfo.chance})");

								existingDrop.m_amountMin = dropInfo.min;
								existingDrop.m_amountMax = dropInfo.max;
								existingDrop.m_chance = dropInfo.chance;
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
										//MarsarahTweaks.LogInfo($"[Better Drops] Removed newly added drop {dropInfo.item} for {creatureName}");
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
												m_dontScale = false
											};
											creatureDrop.m_drops.Add(existingDrop);
											//MarsarahTweaks.LogInfo($"[Better Drops] Restored previously removed drop {dropInfo.item} for {creatureName}");
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
										//MarsarahTweaks.LogInfo($"[Better Drops] Restored drop {dropInfo.item} for {creatureName} (min:{original.min}, max:{original.max}, chance:{original.chance})");
									}
								}

								// Remove from backup
								backup.Remove(dropInfo.item);
								if (backup.Count == 0)
								{
									originalDrops.Remove(creatureName);
									//MarsarahTweaks.LogInfo($"[Better Drops] Cleared backup for {creatureName}");
								}
							}
						}
					}
				}
			}
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
					//MarsarahTweaks.LogInfo($"[Better Drops Restore] Skipping drops restore for {boss.Key}");
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
									//MarsarahTweaks.LogInfo($"[Better Drops Restore] Removed newly added drop {dropInfo.item} for {creatureName}");
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
											m_dontScale = false
										};
										creatureDrop.m_drops.Add(existingDrop);
										//MarsarahTweaks.LogInfo($"[Better Drops Restore] Restored previously removed drop {dropInfo.item} for {creatureName}");
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
									//MarsarahTweaks.LogInfo($"[Better Drops Restore] Restored drop {dropInfo.item} for {creatureName} (min:{original.min}, max:{original.max}, chance:{original.chance})");
								}
							}

							// Remove from backup
							creatureBackup.Remove(dropInfo.item);
							if (creatureBackup.Count == 0)
							{
								originalDrops.Remove(creatureName);
								//MarsarahTweaks.LogInfo($"[Better Drops Restore] Cleared backup for {creatureName}");
							}
						}
					}
				}
			}
		}
	}
}
