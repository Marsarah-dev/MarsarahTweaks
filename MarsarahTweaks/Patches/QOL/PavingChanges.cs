using HarmonyLib;
using MarsarahTweaks.Managers;
using UnityEngine;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.QOL
{
	internal class PavingChanges
	{
		private static readonly LogManager log = new LogManager("Paving", LogManager.LogLevel.Warning);

		private const string SourcePrefabName = "paved_road_v2";
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

		[HarmonyPatch(typeof(ObjectDB), "UpdateRegisters")]
		private static class PavingTerrainOpRegistry_Patch
		{
			private static void Postfix(ObjectDB __instance)
			{
				RegisterPavingTerrainOp(__instance);
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
					RegisterPavingTerrainOp(ObjectDB.instance);
					return;
				}

				log.Error($"Prefab hash collision for '{PavingPrefabName}'.");
				return;
			}

			GameObject hoePrefab = znScene.GetPrefab(HoePrefabName);
			ItemDrop hoeItem = hoePrefab != null ? hoePrefab.GetComponent<ItemDrop>() : null;
			PieceTable hoeTable = hoeItem?.m_itemData?.m_shared?.m_buildPieces;

			if (hoeTable == null)
			{
				log.Error("Could not find the Hoe PieceTable.");
				return;
			}

			GameObject sourcePrefab = hoeTable.m_pieces.Find(prefab => prefab != null && prefab.name == SourcePrefabName);

			if (sourcePrefab == null)
			{
				log.Error($"Could not find '{SourcePrefabName}' in the Hoe PieceTable.");

				foreach (GameObject prefab in hoeTable.m_pieces)
				{
					if (prefab != null && prefab.name.Contains("paved"))
						log.Info($"Available paved-road candidate: '{prefab.name}', TerrainOp={prefab.GetComponent<TerrainOp>() != null}.");
				}

				return;
			}

			log.Info($"Found active Hoe prefab '{sourcePrefab.name}'.");

			GameObject inactiveParent = new GameObject("MarsarahTweaks_Paving_CloneContainer");
			inactiveParent.transform.SetParent(znScene.transform, false);
			inactiveParent.SetActive(false);

			GameObject clone = UnityEngine.Object.Instantiate(sourcePrefab, inactiveParent.transform);
			clone.name = PavingPrefabName;
			clone.SetActive(true);

			log.Info($"Created Paving prefab from '{SourcePrefabName}'. ActiveSelf={clone.activeSelf}, ActiveInHierarchy={clone.activeInHierarchy}.");

			Piece piece = clone.GetComponent<Piece>();
			TerrainOp terrainOp = clone.GetComponent<TerrainOp>();

			if (piece == null || terrainOp == null)
			{
				log.Error($"Could not create '{PavingPrefabName}': source prefab is missing Piece or TerrainOp.");
				UnityEngine.Object.Destroy(inactiveParent);
				return;
			}

			if (terrainOp.m_settings.m_paintRadius <= 0f)
			{
				log.Error($"Could not create '{PavingPrefabName}': source prefab has no valid paint radius.");
				UnityEngine.Object.Destroy(inactiveParent);
				return;
			}

			piece.m_name = "Paving Without Leveling";
			piece.m_description = "Pave the ground without leveling it.";

			terrainOp.m_settings.m_level = false;
			terrainOp.m_settings.m_raise = false;
			terrainOp.m_settings.m_smooth = false;
			terrainOp.m_settings.m_paintCleared = true;

			log.Info($"Paving terrain operation: Level={terrainOp.m_settings.m_level}, Raise={terrainOp.m_settings.m_raise}, Smooth={terrainOp.m_settings.m_smooth}, Paint={terrainOp.m_settings.m_paintCleared}, PaintType={terrainOp.m_settings.m_paintType}, PaintRadius={terrainOp.m_settings.m_paintRadius}.");

			if (clone.GetComponent<ZNetView>() != null)
			{
				znScene.m_prefabs.Add(clone);
			}
			else
			{
				znScene.m_nonNetViewPrefabs.Add(clone);
			}

			namedPrefabs.Add(prefabHash, clone);

			pavingPrefab = clone;

			RegisterPavingTerrainOp(ObjectDB.instance);

			log.Info($"Paving registered successfully. ActiveSelf={clone.activeSelf}, ActiveInHierarchy={clone.activeInHierarchy}, TerrainOp={terrainOp != null}.");
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

		private static void RegisterPavingTerrainOp(ObjectDB objectDB)
		{
			if (objectDB == null || pavingPrefab == null) return;

			TerrainOp terrainOp = pavingPrefab.GetComponent<TerrainOp>();

			if (terrainOp == null)
			{
				log.Error("Cannot register Paving TerrainOp: component is missing.");
				return;
			}

			int prefabHash = objectDB.GetPrefabHash(pavingPrefab);

			if (objectDB.m_terrainOpsByHash.TryGetValue(prefabHash, out TerrainOp existingOp))
			{
				if (existingOp != terrainOp)
				{
					log.Error($"TerrainOp hash collision for '{PavingPrefabName}' ({prefabHash}).");
					return;
				}
			}
			else
			{
				objectDB.m_terrainOpsByHash.Add(prefabHash, terrainOp);
				log.Info($"Registered Paving TerrainOp with hash {prefabHash}.");
			}

			if (!objectDB.m_terrainOps.Contains(terrainOp))
				objectDB.m_terrainOps.Add(terrainOp);
		}
	}
}