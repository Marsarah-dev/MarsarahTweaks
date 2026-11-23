
using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.Balance
{
	internal class DyrnwynDamageChanges
	{
		private static readonly LogManager log = new LogManager("Dyrnwyn Damage", LogManager.LogLevel.Warning);

		private const string DyrnwynPrefab = "SwordDyrnwyn";
		private const float NewFireDamage = 20f;
		private const float NewFireDamagePerLevel = 5f;

		// Backup original Fire damage and upgrade scaling
		private static readonly Dictionary<ItemDrop.ItemData.SharedData, (float Fire, float FirePerLevel)> originalDamages = new Dictionary<ItemDrop.ItemData.SharedData, (float Fire, float FirePerLevel)>();

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class DyrnwynDamageChanges_Patch
		{
			private static void Postfix(ObjectDB __instance)
			{
				if (__instance == null) return;

				UpdateDyrnwynDamage(__instance, false);
			}
		}

		public static void UpdateDyrnwynDamage(ObjectDB objDB, bool wasChanged)
		{
			if (objDB == null || objDB.m_items == null) return;

			bool enabled = ConfigManager.ImprovedDyrnwynDamageEnabled.Value;
			bool found = false;

			foreach (GameObject prefab in objDB.m_items)
			{
				if (prefab == null || prefab.name != DyrnwynPrefab) continue;

				ItemDrop item = prefab.GetComponent<ItemDrop>();
				if (item == null || item.m_itemData == null || item.m_itemData.m_shared == null) continue;

				var shared = item.m_itemData.m_shared;
				found = true;

				// Back up the original values only once
				if (!originalDamages.TryGetValue(shared, out var original))
				{
					original = (shared.m_damages.m_fire, shared.m_damagesPerLevel.m_fire);
					originalDamages.Add(shared, original);

					log.Info($"Backing up {prefab.name}: Fire {original.Fire}, Fire per level {original.FirePerLevel}");

					if (original.Fire != 10f || original.FirePerLevel != 0f)
					{
						log.Warn($"Unexpected vanilla Fire values for {prefab.name}: Fire {original.Fire}, Fire per level {original.FirePerLevel}");
					}
				}

				float targetFire = enabled ? NewFireDamage : original.Fire;
				float targetFirePerLevel = enabled ? NewFireDamagePerLevel : original.FirePerLevel;

				float currentFire = shared.m_damages.m_fire;
				float currentFirePerLevel = shared.m_damagesPerLevel.m_fire;

				if (currentFire == targetFire && currentFirePerLevel == targetFirePerLevel)
				{
					log.Info($"{prefab.name}: Fire damage already matches {(enabled ? "modified" : "vanilla")} values.");
					break;
				}

				// DamageTypes is a struct, so assign the modified values back
				HitData.DamageTypes damages = shared.m_damages;
				damages.m_fire = targetFire;
				shared.m_damages = damages;

				HitData.DamageTypes damagesPerLevel = shared.m_damagesPerLevel;
				damagesPerLevel.m_fire = targetFirePerLevel;
				shared.m_damagesPerLevel = damagesPerLevel;

				if (enabled)
				{
					log.Info($"Modified {prefab.name}: Fire {currentFire} -> {targetFire}, Fire per level {currentFirePerLevel} -> {targetFirePerLevel}");
				}
				else
				{
					log.Info($"Restored {prefab.name}: Fire {currentFire} -> {original.Fire}, Fire per level {currentFirePerLevel} -> {original.FirePerLevel}");
				}

				log.Info($"{prefab.name}: Slash {shared.m_damages.m_slash}, Slash per level {shared.m_damagesPerLevel.m_slash} (unchanged)");

				break;
			}

			if (!found && ZNet.instance != null)
			{
				log.Warn($"Could not find Dyrnwyn prefab: {DyrnwynPrefab}");
			}
			else if (found && wasChanged)
			{
				log.Info($"Improved Dyrnwyn Damage {(enabled ? "enabled" : "disabled")}.");
			}
		}
	}
}
