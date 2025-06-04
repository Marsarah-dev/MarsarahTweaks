using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Managers
{
	// MPrefabManager: MarsarahTweaks prefab cloning and registration system
	internal class MPrefabManager
	{
		public static GameObject ClonePrefab(string nameOfOriginal, string nameOfClone)
		{
			if (nameOfOriginal.IsNullOrWhiteSpace() || nameOfClone.IsNullOrWhiteSpace())
			{
				MarsarahTweaks.LogWarn("[MPrefabManager] Given strings for cloning are null or empty. Cannot clone prefab.");
				return null;
			}

			if (ZNetScene.instance.GetPrefab(nameOfClone) != null)
			{
				MarsarahTweaks.LogWarn($"[MPrefabManager] A prefab named {nameOfClone} already exists in ZNetScene. Skipping clone.");
				return null;
			}

			GameObject originalPrefab = ZNetScene.instance.GetPrefab(nameOfOriginal);
			if (originalPrefab == null)
			{
				MarsarahTweaks.LogError($"[MPrefabManager] Original prefab {nameOfOriginal} not found.");
				return null;
			}

			return ClonePrefab(originalPrefab, nameOfClone);
		}

		public static GameObject ClonePrefab(GameObject originalPrefab, string nameOfClone)
		{
			if (originalPrefab == null || nameOfClone.IsNullOrWhiteSpace())
			{
				MarsarahTweaks.LogWarn("[MPrefabManager] Null original or empty clone name.");
				return null;
			}

			if (ZNetScene.instance.GetPrefab(nameOfClone) != null)
			{
				MarsarahTweaks.LogWarn($"[MPrefabManager] A prefab named {nameOfClone} already exists in ZNetScene. Skipping clone.");
				return null;
			}

			originalPrefab.SetActive(false);
			GameObject clonedPrefab = UnityEngine.Object.Instantiate(originalPrefab);
			originalPrefab.SetActive(true);

			clonedPrefab.SetActive(false);
			clonedPrefab.name = nameOfClone;			

			return clonedPrefab;
		}

		public static bool ValidatePrefab(GameObject prefab, bool hasNetView = false, bool hasTeleport = false, bool hasTransform = false, bool hasPiece = false, bool hasWearNTear = false)
		{
			if (prefab == null)
			{
				MarsarahTweaks.LogWarn("[MPrefabManager] Cannot validate null prefab.");
				return false;
			}

			if (hasNetView && prefab.GetComponent<ZNetView>() == null)
			{
				MarsarahTweaks.LogWarn($"[MPrefabManager] {prefab.name} is missing ZNetView. Adding.");
				prefab.AddComponent<ZNetView>();
			}

			if (hasTeleport && prefab.GetComponent<TeleportWorld>() == null)
			{
				MarsarahTweaks.LogWarn($"[MPrefabManager] {prefab.name} is marked as portal but missing TeleportWorld. Adding.");
				prefab.AddComponent<TeleportWorld>();
			}

			if (hasTransform && prefab.GetComponent<ZSyncTransform>() == null)
			{
				MarsarahTweaks.LogWarn($"[MPrefabManager] {prefab.name} missing ZSyncTransform. Adding.");
				prefab.AddComponent<ZSyncTransform>();
			}

			if (hasPiece && prefab.GetComponent<Piece>() == null)
			{
				MarsarahTweaks.LogWarn($"[MPrefabManager] {prefab.name} is marked as piece but missing Piece. Adding.");
				prefab.AddComponent<Piece>();
			}

			if (hasWearNTear && prefab.GetComponent<WearNTear>() == null)
			{
				MarsarahTweaks.LogWarn($"[MPrefabManager] {prefab.name} is marked as destructible but missing WearNTear. Adding.");
				prefab.AddComponent<WearNTear>();
			}

			return true;
		}

		public static void RegisterToZNetScene(GameObject prefab)
		{
			if (prefab == null)
			{
				MarsarahTweaks.LogError("[MPrefabManager] Tried to register null prefab.");
				return;
			}

			var znetScene = ZNetScene.instance;
			if (znetScene == null)
			{
				MarsarahTweaks.LogError("[MPrefabManager] ZNetScene.instance is null. Cannot register prefab.");
				return;
			}

			string name = prefab.name;
			int hash = name.GetStableHashCode();

			// Reflect to access m_namedPrefabs
			var namedPrefabsField = typeof(ZNetScene).GetField("m_namedPrefabs", BindingFlags.Instance | BindingFlags.NonPublic);
			var namedPrefabs = namedPrefabsField?.GetValue(znetScene) as Dictionary<int, GameObject>;
			if (namedPrefabs == null)
			{
				MarsarahTweaks.LogError("[MPrefabManager] Failed to reflect ZNetScene.m_namedPrefabs.");
				return;
			}

			if (namedPrefabs.ContainsKey(hash))
			{
				MarsarahTweaks.LogWarn($"[MPrefabManager] Prefab '{name}' already registered in ZNetScene.");
				return;
			}

			if (prefab.GetComponent<ZNetView>() != null)
			{
				znetScene.m_prefabs.Add(prefab);
			}
			else
			{
				znetScene.m_nonNetViewPrefabs.Add(prefab);
			}

			namedPrefabs.Add(hash, prefab);

			if (ZNetScene.instance.GetPrefab(prefab.name) == null)
			{
				MarsarahTweaks.LogError($"[MPrefabManager] Failed to register prefab '{name}'!");
			}
			else
			{
				MarsarahTweaks.LogInfo($"[MPrefabManager] Registered prefab '{name}' to ZNetScene.");
			}
		}
	}
}
