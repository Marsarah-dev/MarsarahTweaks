using HarmonyLib;
using MarsarahTweaks.Managers;
using UnityEngine;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.QOL
{
	internal class PavingChanges
	{
		private static readonly LogManager log = new LogManager("Paving", LogManager.LogLevel.Info);

		private const string SourcePrefabName = "paved_road";
		private const string PavingPrefabName = "MarsarahTweaks_Paving";
		private const string HoePrefabName = "Hoe";

		private static readonly AccessTools.FieldRef<ZNetScene, Dictionary<int, GameObject>> namedPrefabsRef = AccessTools.FieldRefAccess<ZNetScene, Dictionary<int, GameObject>>("m_namedPrefabs");

		private static GameObject pavingPrefab;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		private static class PavingPrefab_Patch
		{
			private static void Postfix(ZNetScene __instance)
			{
				if (__instance == null) return;

				RegisterPavingPrefab(__instance);
				UpdatePavingAvailability(__instance);
			}
		}

		private static void RegisterPavingPrefab(ZNetScene znScene)
		{
			Dictionary<int, GameObject> namedPrefabs = namedPrefabsRef(znScene);
			int prefabHash = PavingPrefabName.GetStableHashCode();

			if (namedPrefabs.TryGetValue(prefabHash, out GameObject existingPrefab))
			{
				if (existingPrefab.name == PavingPrefabName)
				{
					pavingPrefab = existingPrefab;
					return;
				}

				log.Error($"Prefab hash collision for '{PavingPrefabName}'.");
				return;
			}

			GameObject sourcePrefab = znScene.GetPrefab(SourcePrefabName);
			if (sourcePrefab == null)
			{
				log.Error($"Could not find source prefab '{SourcePrefabName}'.");
				return;
			}

			log.Info($"Found source prefab '{SourcePrefabName}'.");

			GameObject inactiveParent = new GameObject("MarsarahTweaks_Paving_CloneContainer");
			inactiveParent.SetActive(false);

			GameObject clone = UnityEngine.Object.Instantiate(sourcePrefab, inactiveParent.transform);
			clone.name = PavingPrefabName;
			clone.SetActive(false);
			clone.transform.SetParent(null, false);

			UnityEngine.Object.Destroy(inactiveParent);

			log.Info($"Created inactive Paving clone from '{SourcePrefabName}'.");

			Piece piece = clone.GetComponent<Piece>();
			TerrainModifier terrainModifier = clone.GetComponent<TerrainModifier>();

			if (piece == null || terrainModifier == null)
			{
				log.Error($"Could not create '{PavingPrefabName}': source prefab is missing Piece or TerrainModifier.");
				UnityEngine.Object.Destroy(clone);
				return;
			}

			piece.m_name = "Paving";
			piece.m_description = "Pave the ground without leveling it.";

			terrainModifier.m_level = false;
			terrainModifier.m_smooth = false;
			terrainModifier.m_paintCleared = true;

			if (znScene.m_prefabs.Contains(sourcePrefab))
			{
				znScene.m_prefabs.Add(clone);
			}
			else
			{
				znScene.m_nonNetViewPrefabs.Add(clone);
			}

			namedPrefabs.Add(prefabHash, clone);

			pavingPrefab = clone;

			log.Info($"Paving registered successfully. Active: {clone.activeSelf}, TerrainModifier: {terrainModifier != null}, Level: {terrainModifier.m_level}, Smooth: {terrainModifier.m_smooth}, Paint: {terrainModifier.m_paintCleared}.");
		}

		public static void UpdatePavingAvailability(ZNetScene znScene)
		{
			if (znScene == null || pavingPrefab == null) return;

			GameObject hoePrefab = znScene.GetPrefab(HoePrefabName);
			if (hoePrefab == null)
			{
				log.Error("Could not find Hoe prefab.");
				return;
			}

			ItemDrop hoeItem = hoePrefab.GetComponent<ItemDrop>();
			PieceTable hoeTable = hoeItem?.m_itemData?.m_shared?.m_buildPieces;

			if (hoeTable == null)
			{
				log.Error("Could not find Hoe PieceTable.");
				return;
			}

			bool containsPaving = hoeTable.m_pieces.Contains(pavingPrefab);

			if (ConfigManager.PavingEnabled.Value && !containsPaving)
			{
				int pavedRoadIndex = hoeTable.m_pieces.FindIndex(prefab => prefab != null && prefab.name == SourcePrefabName);

				if (pavedRoadIndex >= 0)
					hoeTable.m_pieces.Insert(pavedRoadIndex + 1, pavingPrefab);
				else
					hoeTable.m_pieces.Add(pavingPrefab);

				log.Info("Added Paving to the Hoe build menu.");
			}
			else if (!ConfigManager.PavingEnabled.Value && containsPaving)
			{
				hoeTable.m_pieces.Remove(pavingPrefab);
				log.Info("Removed Paving from the Hoe build menu.");
			}
		}
	}
}