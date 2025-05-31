using HarmonyLib;
using MarsarahTweaks.Patches.QOL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarsarahTweaks.Managers
{
	internal class PrefabManager
	{
		/*private readonly Dictionary<string, GameObject> registeredPrefabs = new Dictionary<string, GameObject>();

		public void RegisterPrefab(GameObject prefab)
		{
			if (prefab != null && !registeredPrefabs.ContainsKey(prefab.name))
			{
				registeredPrefabs[prefab.name] = prefab;
				ZNetScene.instance.m_prefabs.Add(prefab);
				//ZNetScene.instance.m_namedPrefabs[prefab.name.GetStableHashCode()] = prefab;
				var field = typeof(ZNetScene).GetField("m_namedPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
				var dict = field?.GetValue(ZNetScene.instance) as Dictionary<string, GameObject>;
				if (dict != null && !dict.ContainsKey(prefab.name))
					dict[prefab.name] = prefab;
			}
		}

		public GameObject GetRegisteredPrefab(string name)
		{
			return registeredPrefabs.TryGetValue(name, out var prefab) ? prefab : null;
		}*/

		// ==================================================================

		/*private static readonly List<GameObject> _prefabs = new List<GameObject>();
		private static readonly Dictionary<string, GameObject> _namedPrefabs = new Dictionary<string, GameObject>();

		public static GameObject ClonePrefab(string originalName, string newName)
		{
			var existing = ZNetScene.instance?.GetPrefab(newName);
			if (existing != null)
			{
				MarsarahTweaks.LogWarn($"[PrefabManager: ClonePrefab] Tried to clone prefab '{newName}' but it already exists.");
				return existing;
			}

			var original = ZNetScene.instance.GetPrefab(originalName);
			if (original == null)
			{
				MarsarahTweaks.LogError($"[PrefabManager: ClonePrefab] Original prefab '{originalName}' not found.");
				return null;
			}

			bool wasActive = original.activeSelf;
			original.SetActive(false);

			var clone = UnityEngine.Object.Instantiate(original);
			clone.name = newName;

			original.SetActive(wasActive);

			if (!clone.GetComponent<ZNetView>())
			{
				MarsarahTweaks.LogError($"[PrefabManager: ClonePrefab] Cloned prefab '{newName}' is missing ZNetView!");
				return null;
			}

			return clone;
		}

		public static void RegisterPrefab(GameObject prefab)
		{
			if (prefab == null) return;

			_prefabs.Add(prefab);
			_namedPrefabs[prefab.name] = prefab;
		}

		public static void InjectToZNetScene(ZNetScene scene)
		{
			foreach (var prefab in _prefabs)
			{
				if (!scene.m_prefabs.Contains(prefab))
					scene.m_prefabs.Add(prefab);

				var field = typeof(ZNetScene).GetField("m_namedPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
				var dict = field?.GetValue(scene) as Dictionary<string, GameObject>;
				if (dict != null && !dict.ContainsKey(prefab.name))
					dict[prefab.name] = prefab;
			}
		}

		public static void InjectToObjectDB()
		{
			if (ObjectDB.instance == null) return;

			foreach (var prefab in _prefabs)
			{
				if (!prefab.TryGetComponent<ItemDrop>(out var _)) continue;

				if (!ObjectDB.instance.m_items.Contains(prefab))
				{
					ObjectDB.instance.m_items.Add(prefab);
					MarsarahTweaks.LogInfo($"[PrefabManager: InjectToObjectDB] Added '{prefab.name}' to ObjectDB.");
				}
			}
		}*/

		// ================================================================


		/*[HarmonyPatch(typeof(ObjectDB), "Awake")]
		static class ObjectDBAwake_Patch
		{
			static void Postfix()
			{
				AddToHammerBuildMenu(PocketPortal.pocketPortalPrefab);
			}
		}*/

		/*[HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
		static class ObjectDBCopy_Patch
		{
			static void Postfix()
			{
				AddToHammerBuildMenu(PocketPortal.pocketPortalPrefab);
			}
		}*/

		/*public static GameObject ClonePrefab(string originalName, string newName)
		{
			var existing = ZNetScene.instance?.GetPrefab(newName);
			if (existing != null)
			{
				MarsarahTweaks.LogWarn($"Tried to clone prefab '{newName}' but it already exists!");
				return existing;
			}

			var original = ZNetScene.instance.GetPrefab(originalName);
			if (original == null)
			{
				MarsarahTweaks.LogWarn($"Original prefab '{originalName}' not found.");
				return null;
			}

			// 1. Make sure clone is inactive BEFORE Instantiate
			bool wasActive = original.activeSelf;
			original.SetActive(false);

			// 2. Clone while inactive
			var clone = UnityEngine.Object.Instantiate(original);
			clone.name = newName;

			// 3. Leave clone inactive to prevent any Awake/Start calls until registered
			// (You can SetActive(true) later only if you really need to)

			// 4. Restore original's active state
			original.SetActive(wasActive);

			// 5. (Optional) Validate clone
			if (!clone.GetComponent<ZNetView>())
			{
				MarsarahTweaks.LogError($"Cloned prefab '{newName}' is missing ZNetView!");
				return null;
			}

			return clone;
		}*/

		/*public static void RegisterPrefab(ZNetScene scene, GameObject prefab)
		{
			if (prefab == null)
			{
				MarsarahTweaks.LogError("Tried to register null prefab.");
				return;
			}

			// Add to m_prefabs list
			if (!scene.m_prefabs.Contains(prefab))
			{
				scene.m_prefabs.Add(prefab);
				MarsarahTweaks.LogInfo($"Added {prefab.name} to ZNetScene.m_prefabs.");
			}

			// Add to m_namedPrefabs dictionary via reflection
			var namedPrefabsField = typeof(ZNetScene).GetField("m_namedPrefabs", BindingFlags.Instance | BindingFlags.NonPublic);
			if (namedPrefabsField != null)
			{
				var namedPrefabs = namedPrefabsField.GetValue(scene) as Dictionary<string, GameObject>;
				if (namedPrefabs != null && !namedPrefabs.ContainsKey(prefab.name))
				{
					namedPrefabs[prefab.name] = prefab;
					MarsarahTweaks.LogInfo($"Added {prefab.name} to ZNetScene.m_namedPrefabs.");
				}
			}

			// Validate ZNetView
			var znv = prefab.GetComponent<ZNetView>();
			if (znv == null)
			{
				MarsarahTweaks.LogError($"{prefab.name} is missing ZNetView — this will break networking.");
			}

			// Patch TeleportWorld safely
			var tele = prefab.GetComponent<TeleportWorld>();
			if (tele != null)
			{
				if (tele.m_model == null)
				{
					var renderer = prefab.GetComponentInChildren<MeshRenderer>();
					if (renderer != null) tele.m_model = renderer;
				}

				if (tele.m_proximityRoot == null)
				{
					tele.m_proximityRoot = prefab.transform;
				}
			}

			MarsarahTweaks.LogInfo($"ZNetScene has {scene.m_prefabs.Count} prefabs after registering {prefab.name}.");
		}*/

		/*public static void AddToHammerBuildMenu(GameObject prefab)
		{
			if (prefab == null)
			{
				MarsarahTweaks.LogError("[PrefabManager: AddToHammer] Tried to add null prefab.");
				return;
			}

			if (!prefab.activeSelf)
			{
				MarsarahTweaks.LogWarn($"[PrefabManager: AddToHammer] Prefab '{prefab.name}' is inactive. Skipping.");
				return;
			}

			var piece = prefab.GetComponent<Piece>();
			if (piece == null)
			{
				MarsarahTweaks.LogError($"[PrefabManager: AddToHammer] Prefab '{prefab.name}' is missing Piece component!");
				return;
			}

			var hammerPrefab = ObjectDB.instance?.GetItemPrefab("Hammer");
			if (hammerPrefab == null) return;

			var hammer = hammerPrefab.GetComponent<ItemDrop>();
			if (hammer == null) return;

			var table = hammer.m_itemData.m_shared.m_buildPieces;
			if (table == null) return;

			if (table.m_pieces.Any(p => p != null && p.name == prefab.name))
			{
				MarsarahTweaks.LogInfo($"[PrefabManager: AddToHammer] Pocket portal '{prefab.name}' already exists in build pieces.");
				return;
			}

			table.m_pieces.Add(prefab);
			MarsarahTweaks.LogInfo($"[PrefabManager: AddToHammer] Added '{prefab.name}' to build pieces.");
		}*/
	}
}
