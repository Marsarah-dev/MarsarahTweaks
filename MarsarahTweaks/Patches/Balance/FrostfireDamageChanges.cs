
using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.Balance
{
	internal class FrostfireDamageChanges
	{
		private static readonly LogManager log = new LogManager("Frostfire Damage", LogManager.LogLevel.Warning);

		// New base elemental damage
		private const float NewFireDamage = 40f;
		private const float NewFrostDamage = 80f;

		// Backup original damage by shared item data reference
		private static readonly Dictionary<ItemDrop.ItemData.SharedData, (float Fire, float Frost)> originalDamages = new Dictionary<ItemDrop.ItemData.SharedData, (float Fire, float Frost)>();

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class FrostfireDamageChanges_Patch
		{
			private static void Postfix(ObjectDB __instance)
			{
				if (__instance == null) return;

				UpdateFrostfireDamage(__instance, false);
			}
		}

		public static void UpdateFrostfireDamage(ObjectDB objDB, bool wasChanged)
		{
			if (objDB == null || objDB.m_items == null) return;

			bool enabled = ConfigManager.ImprovedFrostfireDamageEnabled.Value;
			int weaponsFound = 0;
			int weaponsChanged = 0;

			foreach (GameObject prefab in objDB.m_items)
			{
				if (prefab == null) continue;
				if (!prefab.name.EndsWith("_FrostFire", StringComparison.Ordinal)) continue;

				ItemDrop item = prefab.GetComponent<ItemDrop>();
				if (item == null || item.m_itemData == null || item.m_itemData.m_shared == null) continue;

				var shared = item.m_itemData.m_shared;
				weaponsFound++;

				// Back up vanilla values only once per shared data instance
				if (!originalDamages.TryGetValue(shared, out var original))
				{
					original = (shared.m_damages.m_fire, shared.m_damages.m_frost);
					originalDamages.Add(shared, original);

					log.Info($"Backing up {prefab.name}: Fire {original.Fire}, Frost {original.Frost}");

					// Verify the expected vanilla values during testing
					if (original.Frost != 88f || (original.Fire != 12f && original.Fire != 10f))
					{
						log.Warn($"Unexpected vanilla elemental damage for {prefab.name}: Fire {original.Fire}, Frost {original.Frost}");
					}
				}

				float targetFire = enabled ? NewFireDamage : original.Fire;
				float targetFrost = enabled ? NewFrostDamage : original.Frost;

				float currentFire = shared.m_damages.m_fire;
				float currentFrost = shared.m_damages.m_frost;

				if (currentFire == targetFire && currentFrost == targetFrost) continue;

				// DamageTypes is a struct, so assign it back after modification
				HitData.DamageTypes damages = shared.m_damages;
				damages.m_fire = targetFire;
				damages.m_frost = targetFrost;
				shared.m_damages = damages;

				weaponsChanged++;

				if (enabled)
				{
					log.Info($"Modified {prefab.name}: Fire {currentFire} -> {targetFire}, Frost {currentFrost} -> {targetFrost} | Upgrades: +{shared.m_damagesPerLevel.m_fire} Fire, +{shared.m_damagesPerLevel.m_frost} Frost");
				}
				else
				{
					log.Info($"Restored {prefab.name}: Fire {currentFire} -> {original.Fire}, Frost {currentFrost} -> {original.Frost}");
				}
			}

			if (wasChanged || enabled)
			{
				log.Info($"Frostfire Damage {(enabled ? "enabled" : "disabled")}: Found {weaponsFound} weapons, updated {weaponsChanged}.");
			}
		}
	}
}
