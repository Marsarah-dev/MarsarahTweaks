
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class RootForestDrops
	{
		private static readonly LogManager log = new LogManager("Root Forest Drops", LogManager.LogLevel.Info);

		private const string RootPrefabName = "TentaRoot_wild";

		private static GameObject cachedPrefab;
		private static CharacterDrop cachedDrop;
		private static List<CharacterDrop.Drop> originalDrops;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class RootForestDrops_Patch
		{
			private static void Postfix(ref ZNetScene __instance)
			{
				if (__instance == null) return;

				UpdateRootForestDrops(__instance, false);
			}
		}

		public static void UpdateRootForestDrops(ZNetScene instance, bool wasChanged)
		{
			if (instance == null) return;

			GameObject prefab = instance.GetPrefab(RootPrefabName);
			if (prefab == null)
			{
				log.Warn($"Could not find prefab: {RootPrefabName}");
				return;
			}

			// Cache the original drop configuration for each new prefab
			if (cachedPrefab != prefab)
			{
				cachedPrefab = prefab;
				cachedDrop = prefab.GetComponent<CharacterDrop>();

				if (cachedDrop == null)
				{
					cachedDrop = prefab.AddComponent<CharacterDrop>();
					log.Info($"Added CharacterDrop component to {RootPrefabName}");
				}

				originalDrops = cachedDrop.m_drops != null ? new List<CharacterDrop.Drop>(cachedDrop.m_drops) : new List<CharacterDrop.Drop>();

				log.Info($"Backed up {originalDrops.Count} original drops for {RootPrefabName}");
			}

			if (ConfigManager.RootForestDropsEnabled.Value)
			{
				GameObject wood = instance.GetPrefab("Wood");
				GameObject coreWood = instance.GetPrefab("RoundLog");
				GameObject resin = instance.GetPrefab("Resin");

				if (wood == null || coreWood == null || resin == null)
				{
					log.Warn("Could not find one or more resources for Root Forest Drops.");
					return;
				}

				// Rebuild from original drops to prevent duplicates
				List<CharacterDrop.Drop> modifiedDrops = new List<CharacterDrop.Drop>(originalDrops)
				{
					CreateDrop(wood, 0.5f),
					CreateDrop(coreWood, 0.5f),
					CreateDrop(resin, 0.2f)
				};

				cachedDrop.m_drops = modifiedDrops;

				log.Info($"Applied drops to {RootPrefabName}: Wood 50%, Core Wood 50%, Resin 20%");
			}
			else if (wasChanged)
			{
				cachedDrop.m_drops = new List<CharacterDrop.Drop>(originalDrops);

				log.Info($"Restored original drops for {RootPrefabName}");
			}
		}

		private static CharacterDrop.Drop CreateDrop(GameObject prefab, float chance)
		{
			return new CharacterDrop.Drop
			{
				m_prefab = prefab,
				m_amountMin = 1,
				m_amountMax = 1,
				m_chance = chance,
				m_onePerPlayer = false,
				m_levelMultiplier = false
			};
		}
	}
}
