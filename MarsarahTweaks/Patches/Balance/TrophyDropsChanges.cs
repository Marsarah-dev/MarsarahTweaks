using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class TrophyDropsChanges
	{
		private static readonly LogManager log = new LogManager("Trophy Drops", LogManager.LogLevel.Warning);

		// Eikthyr - Rancid Remains (10% -> 20%), Ghost (10% -> 20%), Bear (10% -> 20%)
		// The Elder - Surtling (5% -> 10%), Draugr Elite (10% -> 20%), Wraith (5% -> 20%)
		// Bonemass - Cultist (10% -> 20%), Fenring (10% -> 20%), Stone Golem (5% -> 20%)
		// Moder - Deathsquito (5% -> 10%), Fuling Berserker (5% -> 10%), Vile (10% -> 20%)
		// Yagluth - Tick (5% -> 10%), Dverger (5% -> 10%), Seeker Soldier (5% -> 20%)
		// The Queen - Charred Warlock (5% -> 20%)

		// Dictionaries
		private static Dictionary<string, Dictionary<string, float>> originalTrophyDropRates = new Dictionary<string, Dictionary<string, float>>();
		private static readonly Dictionary<string, List<(string creature, string trophy, float rate)>> bossTrophyMappings = new Dictionary<string, List<(string, string, float)>>()
		{
			{ "Eikthyr", new List<(string, string, float)>
				{
					("Skeleton_Poison", "TrophySkeletonPoison", 0.2f),
					("Ghost", "TrophyGhost", 0.2f),
					("Bjorn", "TrophyBjorn", 0.2f)
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
					("GoblinBrute", "TrophyGoblinBrute", 0.1f),
					("Unbjorn", "TrophyBjornUndead", 0.2f)
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

		public static void UpdateTrophyDrops(ZNetScene instance)
		{
			if (instance == null) return;

			bool progHaltEnabled = ConfigManager.AutomaticProgressionHaltEnabled.Value;

			foreach (var boss in bossTrophyMappings)
			{
				bool canMakeChanges = !progHaltEnabled || (progHaltEnabled && GlobalKeyChecker.IsBossDefeated(boss.Key));
				if (!canMakeChanges)
				{
					log.Info($"Skipping trophy drops modifications for {boss.Key}");
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
							if (ConfigManager.BetterTrophyDropsEnabled.Value)
							{
								// Backup original drop rate
								if (!originalTrophyDropRates.ContainsKey(creature))
								{
									originalTrophyDropRates[creature] = new Dictionary<string, float>();
								}
								if (!originalTrophyDropRates[creature].ContainsKey(trophy))
								{
									originalTrophyDropRates[creature][trophy] = drop.m_chance; // Store original drop chance
									log.Info($"Backing up {trophy} drop rate ({drop.m_chance}) for {creature}");
								}

								log.Info($"Setting trophy: {trophy} drop rate: {rate} for creature: {creature}");
								drop.m_chance = rate;
							}
							else
							{
								// Restore backup
								if (originalTrophyDropRates.TryGetValue(creature, out var originalTrophies) && originalTrophies.TryGetValue(trophy, out float originalRate))
								{
									log.Info($"Restoring {trophy} drop rate ({originalRate}) for {creature}");
									drop.m_chance = originalRate;

									// Delete backup so we don't restore infinitely
									originalTrophies.Remove(trophy);
									if (originalTrophyDropRates[creature].Count == 0)
									{
										originalTrophyDropRates.Remove(creature);
										log.Info($"Cleared backup for {creature}");
									}
								}
							}
						}
					}
				}
			}
		}

		public static void RestoreTrophyDrops(ZNetScene instance)
		{
			bool progHaltEnabled = ConfigManager.AutomaticProgressionHaltEnabled.Value;

			foreach (var boss in bossTrophyMappings)
			{
				// Only restore backups for creatures whose boss has not yet been defeated
				bool canMakeChanges = progHaltEnabled && !GlobalKeyChecker.IsBossDefeated(boss.Key);
				if (!canMakeChanges)
				{
					log.Info($"[Restore Special] Skipping trophy drops restore for {boss.Key}");
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
								log.Info($"[Restore Special] Restoring {trophy} drop rate ({originalRate}) for {creature}");
								drop.m_chance = originalRate;

								// Delete backup so we don't restore infinitely
								originalTrophies.Remove(trophy);
								if (originalTrophyDropRates[creature].Count == 0)
								{
									originalTrophyDropRates.Remove(creature);
									log.Info($"[Restore Special] Cleared backup for {creature}");
								}
							}
						}
					}
				}
			}
		}

		private static bool trophiesLogged = false;

		public static void LogTrophies(ZNetScene instance)
		{
			if (instance == null)
			{
				log.Warn("Cannot log trophies because ZNetScene instance is null.");
				return;
			}

			if (trophiesLogged) return;

			log.Info("=== TROPHY DROP LOGGER START ===");

			foreach (GameObject prefab in instance.m_prefabs)
			{
				if (prefab == null) continue;

				CharacterDrop dropper = prefab.GetComponent<CharacterDrop>();
				if (dropper == null || dropper.m_drops == null || dropper.m_drops.Count == 0)
					continue;

				List<string> trophyDrops = new List<string>();
				foreach (CharacterDrop.Drop drop in dropper.m_drops)
				{
					if (drop?.m_prefab == null) continue;

					// Only care about trophy drops
					if (drop.m_prefab.name.ToLower().Contains("trophy"))
					{
						string entry = $"{drop.m_prefab.name}: {drop.m_chance * 100f:0.##}%";
						trophyDrops.Add(entry);
					}
				}

				if (trophyDrops.Count > 0)
				{
					log.Info($"Creature: {prefab.name} | Trophies: {string.Join(", ", trophyDrops)}");
				}
			}

			log.Info("=== TROPHY DROP LOGGER END ===");
			trophiesLogged = true;
		}
	}
}
