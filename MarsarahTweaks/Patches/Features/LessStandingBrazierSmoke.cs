
using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class LessStandingBrazierSmoke
	{
		private static readonly LogManager log = new LogManager("Less Standing Brazier Smoke", LogManager.LogLevel.Info);

		private const float SmokeIntervalMultiplier = 4f;

		private static readonly string[] brazierPrefabs =
		{
			"piece_brazierfloor01",
			"piece_brazierfloor02",
			"piece_brazierceiling01"
		};

		private static readonly Dictionary<string, float> originalIntervals = new Dictionary<string, float>();

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		private static class LessStandingBrazierSmoke_Patch
		{
			private static void Postfix(ZNetScene __instance)
			{
				if (__instance == null) return;
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				UpdateSmoke(__instance);
			}
		}

		public static void UpdateSmoke(ZNetScene znScene)
		{
			if (znScene == null) return;
			if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

			bool enabled = ConfigManager.LessStandingBrazierSmokeEnabled.Value;

			// Update prefabs and preserve their original smoke intervals.
			foreach (string prefabName in brazierPrefabs)
			{
				GameObject prefab = znScene.GetPrefab(prefabName);

				if (prefab == null)
				{
					log.Warn($"Could not find brazier prefab: {prefabName}");
					continue;
				}

				SmokeSpawner[] spawners = prefab.GetComponentsInChildren<SmokeSpawner>(true);

				if (spawners.Length == 0)
				{
					log.Warn($"No SmokeSpawner found on {prefabName}");
					continue;
				}

				foreach (SmokeSpawner spawner in spawners)
				{
					if (!originalIntervals.ContainsKey(prefabName))
					{
						originalIntervals[prefabName] = spawner.m_interval;
						log.Info($"Backed up smoke interval for {prefabName}: {spawner.m_interval}s");
					}

					ApplySmokeInterval(spawner, prefabName, enabled);
				}
			}

			// Also update standing braziers already placed in the world.
			foreach (SmokeSpawner spawner in Object.FindObjectsByType<SmokeSpawner>(FindObjectsSortMode.None))
			{
				Piece piece = spawner.GetComponentInParent<Piece>();
				if (piece == null) continue;

				string prefabName = null;

				if (piece.m_name == "$piece_brazierfloor01")
					prefabName = "piece_brazierfloor01";
				else if (piece.m_name == "$piece_brazierfloor02")
					prefabName = "piece_brazierfloor02";
				else if (piece.m_name == "$piece_brazierceiling01")
					prefabName = "piece_brazierceiling01";

				if (prefabName == null || !originalIntervals.ContainsKey(prefabName)) continue;

				ApplySmokeInterval(spawner, prefabName, enabled);
			}
		}

		private static void ApplySmokeInterval(SmokeSpawner spawner, string prefabName, bool enabled)
		{
			float originalInterval = originalIntervals[prefabName];
			float targetInterval = enabled ? originalInterval * SmokeIntervalMultiplier : originalInterval;

			if (Mathf.Approximately(spawner.m_interval, targetInterval)) return;

			log.Info($"{prefabName}: Smoke interval {spawner.m_interval:0.##}s -> {targetInterval:0.##}s");

			spawner.m_interval = targetInterval;
		}
	}
}
