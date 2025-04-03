using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class TrophyDropsChanges
	{
		// Eikthyr - Rancid Remains (10% -> 20%)
		// The Elder - Surtling (5% -> 10%), Draugr Elite (10% -> 20%), Wraith (5% -> 20%)
		// Bonemass - Cultist (10% -> 20%), Fenring (10% -> 20%), Stone Golem (5% -> 20%)
		// Moder - Deathsquito (5% -> 10%), Fuling Berserker (5% -> 10%)
		// Yagluth - Tick (5% -> 10%), Dverger (5% -> 10%), Seeker Soldier (5% -> 20%)
		// The Queen - Charred Warlock (5% -> 20%)

		/*
		    { "Skeleton_Poison", ("TrophySkeletonPoison", 0.2f) }, // Eikthyr
			{ "Surtling", ("TrophySurtling", 0.1f) }, // The Elder
			{ "Draugr_Elite", ("TrophyDraugrElite", 0.2f) }, // The Elder
			{ "Wraith", ("TrophyWraith", 0.2f) }, //  The Elder
			{ "Fenring", ("TrophyFenring", 0.2f) }, // Bonemass
			{ "Fenring_Cultist", ("TrophyCultist", 0.2f) }, // Bonemass
			{ "StoneGolem", ("TrophySGolem", 0.2f) }, // Bonemass
			{ "Deathsquito", ("TrophyDeathsquito", 0.1f) }, // Moder
			{ "GoblinBrute", ("TrophyGoblinBrute", 0.1f) }, // Moder
			{ "Tick", ("TrophyTick", 0.1f) }, // Yagluth
			{ "Dverger", ("TrophyDvergr", 0.1f) }, // Yagluth
			{ "DvergerMage", ("TrophyDvergr", 0.1f) }, // Yagluth
			{ "SeekerBrute", ("TrophySeekerBrute", 0.2f) }, // Yagluth
			{ "DvergerAshlands", ("TrophyDvergr", 0.1f) }, // The Queen
			{ "Charred_Mage", ("TrophyCharredMage", 0.2f) } // The Queen
		 */

		// Dictionaries
		//private static Dictionary<string, (string trophy, float originalRate)> originalTrophyDropRates = new Dictionary<string, (string trophy, float originalRate)>();
		private static Dictionary<string, Dictionary<string, float>> originalTrophyDropRates = new Dictionary<string, Dictionary<string, float>>();
		/*private static readonly Dictionary<string, (string trophy, float rate)> newTrophyDropRates = new Dictionary<string, (string trophy, float rate)>()
		{
			{ "Skeleton_Poison", ("TrophySkeletonPoison", 0.2f) }, // 0.1f
			{ "Surtling", ("TrophySurtling", 0.1f) }, // 0.05f
			{ "Draugr_Elite", ("TrophyDraugrElite", 0.2f) }, // 0.1f
			{ "Wraith", ("TrophyWraith", 0.2f) }, // 0.05f
			{ "Fenring", ("TrophyFenring", 0.2f) }, // 0.1f
			{ "Fenring_Cultist", ("TrophyCultist", 0.2f) }, // 0.1f
			{ "StoneGolem", ("TrophySGolem", 0.2f) }, // 0.05f
			{ "Deathsquito", ("TrophyDeathsquito", 0.1f) }, // 0.05f
			{ "GoblinBrute", ("TrophyGoblinBrute", 0.1f) }, // 0.05f
			{ "Tick", ("TrophyTick", 0.1f) }, // 0.05f
			{ "Dverger", ("TrophyDvergr", 0.1f) }, // 0.05f
			{ "DvergerMage", ("TrophyDvergr", 0.1f) }, // 0.05f
			{ "DvergerAshlands", ("TrophyDvergr", 0.1f) }, // 0.05f
			{ "SeekerBrute", ("TrophySeekerBrute", 0.2f) }, // 0.05f
			{ "Charred_Mage", ("TrophyCharredMage", 0.2f) } // 0.05f
		};*/

		private static readonly Dictionary<string, List<(string creature, string trophy, float rate)>> bossTrophyMappings = new Dictionary<string, List<(string, string, float)>>()
		{
			{ "Eikthyr", new List<(string, string, float)>
				{
					("Skeleton_Poison", "TrophySkeletonPoison", 0.2f)
				}
			},
			{ "The Elder", new List<(string, string, float)>
				{
					("Surtling", "TrophySurtling", 0.1f),
					("Draugr_Elite", "TrophyDraugrElite", 0.2f),
					("Wraith", "TrophyWraith", 0.2f)
				}
			},
			{ "Bonemass", new List<(string, string, float)>
				{
					("Fenring", "TrophyFenring", 0.2f),
					("Fenring_Cultist", "TrophyCultist", 0.2f),
					("StoneGolem", "TrophySGolem", 0.2f)
				}
			},
			{ "Moder", new List<(string, string, float)>
				{
					("Deathsquito", "TrophyDeathsquito", 0.1f),
					("GoblinBrute", "TrophyGoblinBrute", 0.1f)
				}
			},
			{ "Yagluth", new List<(string, string, float)>
				{
					("Tick", "TrophyTick", 0.1f),
					("Dverger", "TrophyDvergr", 0.1f),
					("DvergerMage", "TrophyDvergr", 0.1f),
					("SeekerBrute", "TrophySeekerBrute", 0.2f)
				}
			},
			{ "The Queen", new List<(string, string, float)>
				{
					("DvergerAshlands", "TrophyDvergr", 0.1f),
					("Charred_Mage", "TrophyCharredMage", 0.2f)
				}
			}
		};

		public static void updateTrophyDrops(ZNetScene instance)
		{
			if (instance == null) return;

			bool progHaltEnabled = ConfigManager.automaticProgressionHaltEnabled.Value;

			foreach (var boss in bossTrophyMappings)
			{
				bool canMakeChanges = !progHaltEnabled || (progHaltEnabled && GlobalKeyChecker.isBossDefeated(boss.Key));

				if (!canMakeChanges)
				{
					//MarsarahTweaks.MLog($"Skipping trophy drops modifications for {boss.Key}");
					continue; 
				}

				foreach (var (creature, trophy, rate) in boss.Value)
				{
					GameObject prefab = instance.GetPrefab(creature);
					if (prefab == null) continue;

					CharacterDrop creatureDrop = prefab.GetComponent<CharacterDrop>();
					if (creatureDrop == null) continue;

					foreach (CharacterDrop.Drop drop in creatureDrop.m_drops)
					{
						if (drop.m_prefab.name == trophy)
						{
							if (ConfigManager.betterTrophyDropsEnabled.Value)
							{
								// Backup original drop rate
								if (!originalTrophyDropRates.ContainsKey(creature))
								{
									originalTrophyDropRates[creature] = new Dictionary<string, float>();
								}
								if (!originalTrophyDropRates[creature].ContainsKey(trophy))
								{
									originalTrophyDropRates[creature][trophy] = drop.m_chance; // Store original drop chance
									//MarsarahTweaks.MLog($"[Trophy Drops] Backing up {trophy} drop rate ({drop.m_chance}) for {creature}");
								}

								//MarsarahTweaks.MLog($"[Trophy Drops] Setting trophy: {trophy} drop rate: {rate} for creature: {creature}");
								drop.m_chance = rate;
							}
							else
							{
								// Restore backup
								if (originalTrophyDropRates.TryGetValue(creature, out var originalTrophies) && originalTrophies.TryGetValue(trophy, out float originalRate))
								{
									//MarsarahTweaks.MLog($"[Trophy Drops] Restoring {trophy} drop rate ({originalRate}) for {creature}");
									drop.m_chance = originalRate;

									// Delete backup so we don't restore infinitely
									originalTrophies.Remove(trophy);
									if (originalTrophyDropRates[creature].Count == 0)
									{
										originalTrophyDropRates.Remove(creature);
										//MarsarahTweaks.MLog($"[Trophy Drops] Cleared backup for {creature}");
									}
								}
							}
						}
					}
				}
			}
		}

		public static void restoreTrophyDrops(ZNetScene instance)
		{
			bool progHaltEnabled = ConfigManager.automaticProgressionHaltEnabled.Value;

			foreach (var boss in bossTrophyMappings)
			{
				// Only restore backups for creatures whose boss has not yet been defeated
				bool canMakeChanges = progHaltEnabled && !GlobalKeyChecker.isBossDefeated(boss.Key);

				if (!canMakeChanges)
				{
					//MarsarahTweaks.MLog($"[Trophy Drops Restore Special] Skipping trophy drops restore for {boss.Key}");
					continue;
				}

				foreach (var (creature, trophy, rate) in boss.Value)
				{
					GameObject prefab = instance.GetPrefab(creature);
					if (prefab == null) continue;

					CharacterDrop creatureDrop = prefab.GetComponent<CharacterDrop>();
					if (creatureDrop == null) continue;

					foreach (CharacterDrop.Drop drop in creatureDrop.m_drops)
					{
						if (drop.m_prefab.name == trophy)
						{
							if (originalTrophyDropRates.TryGetValue(creature, out var originalTrophies) && originalTrophies.TryGetValue(trophy, out float originalRate))
							{
								//MarsarahTweaks.MLog($"[Trophy Drops Restore Special] Restoring {trophy} drop rate ({originalRate}) for {creature}");
								drop.m_chance = originalRate;

								// Delete backup so we don't restore infinitely
								originalTrophies.Remove(trophy);
								if (originalTrophyDropRates[creature].Count == 0)
								{
									originalTrophyDropRates.Remove(creature);
									//MarsarahTweaks.MLog($"[Trophy Drops Restore Special] Cleared backup for {creature}");
								}
							}
						}
					}
				}
			}
		}


		/*[HarmonyPatch(typeof(ZNetScene), "Update")]
		class TrophyDrops_Patch
		{
			static void Postfix(ref ZNetScene __instance)
			{
				if (__instance == null) return;

				//updateTrophyDrops(__instance);
			}
		}*/

		/*public static void updateTrophyDrops(ZNetScene znScene)
		{
			foreach (var kvp in newTrophyDropRates)
			{
				string creature = kvp.Key;
				string trophy = kvp.Value.trophy;
				float rate = kvp.Value.rate;

				GameObject creaturePrefab = znScene.GetPrefab(creature);

				if (creaturePrefab == null)
				{
					MarsarahTweaks.MLog($"[Trophy Drops] Could not get prefab for {creature}");
					continue;
				}

				CharacterDrop creatureDrop = creaturePrefab.GetComponent<CharacterDrop>();

				if (creatureDrop == null)
				{
					MarsarahTweaks.MLog($"[Trophy Drops] Could not get CharacterDrop for {creature}");
					continue;
				}

				foreach (CharacterDrop.Drop drop in creatureDrop.m_drops)
				{
					if (drop.m_prefab.name == trophy)
					{
						if (ConfigManager.betterTrophyDropsEnabled.Value)
							{
							// Backup original drop rate
							if (!originalTrophyDropRates.ContainsKey(creature))
							{
								originalTrophyDropRates[creature] = new Dictionary<string, float>();
							}
							if (!originalTrophyDropRates[creature].ContainsKey(trophy))
							{
								originalTrophyDropRates[creature][trophy] = drop.m_chance; // Store original drop chance
								MarsarahTweaks.MLog($"[Trophy Drops] Backing up {trophy} drop rate ({drop.m_chance}) for {creature}");
							}

							MarsarahTweaks.MLog($"[Trophy Drops] Setting trophy: {trophy} drop rate: {rate} for creature: {creature}");
							drop.m_chance = rate;
						}
						else
						{
							// Restore backup
							if (originalTrophyDropRates.TryGetValue(creature, out var originalTrophies) && originalTrophies.TryGetValue(trophy, out float originalRate))
							{
								MarsarahTweaks.MLog($"[Trophy Drops] Restoring {trophy} drop rate ({originalRate}) for {creature}");
								drop.m_chance = originalRate;
							}
						}
					}
					break;
				}
			}
		}*/
	}
}
