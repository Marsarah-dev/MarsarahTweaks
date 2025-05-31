using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Managers
{
	internal class AssetManager
	{
		/*private readonly Dictionary<string, UnityEngine.Object> assets = new Dictionary<string, UnityEngine.Object>();

		public void RegisterAsset(string name, UnityEngine.Object asset)
		{
			if (!assets.ContainsKey(name))
			{
				assets[name] = asset;
			}
		}

		public T GetAsset<T>(string name) where T : UnityEngine.Object
		{
			return assets.TryGetValue(name, out var asset) ? asset as T : null;
		}

		public GameObject ClonePrefab(GameObject original, string newName)
		{
			GameObject clone = UnityEngine.Object.Instantiate(original);
			clone.name = newName;
			RegisterAsset(newName, clone);
			return clone;
		}*/

		// ===================================================

		/*private static AssetBundle _bundle;

		public static void Load(string bundlePath)
		{
			_bundle = AssetBundle.LoadFromFile(bundlePath);
		}

		public static GameObject GetPrefab(string name)
		{
			return _bundle?.LoadAsset<GameObject>(name);
		}*/

		// ===========================================================

		/*public static GameObject pocketPortal = null;

		public static void InitializeCustomAssets()
		{
			var assetBundle = GetAssetBundleFromResource("marsabundle");
			pocketPortal = assetBundle.LoadAsset<GameObject>("Assets/MarsaCustomAssets/pocket_portal.prefab");
		}

		private static AssetBundle GetAssetBundleFromResource(string fileName)
		{
			var execAssembly = Assembly.GetExecutingAssembly();

			var resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

			using (var stream = execAssembly.GetManifestResourceStream(resourceName))
			{
				return AssetBundle.LoadFromStream(stream);
			}
		}*/

		/*[HarmonyPatch(typeof(ZNetScene), "Awake")]
		static class ZNetSceneAwake_Patch
		{
			static void Prefix (ZNetScene __instance)
			{
				if (__instance == null) return;

				__instance.m_prefabs.Add(pocketPortal);
			}
		}

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		static class ObjectDBAwake_Patch
		{
			static void Postfix()
			{
				AddMarsaPrefabsToObjDB();
			}
		}

		[HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
		static class ObjectDBCopy_Patch
		{
			static void Postfix()
			{
				AddMarsaPrefabsToObjDB();
			}
		}*/

		/*private static void AddMarsaPrefabsToObjDB()
		{
			if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0) return;

			ItemDrop itemDrop = pocketPortal.GetComponent<ItemDrop>();

			if (itemDrop != null)
			{
				if (ObjectDB.instance.GetItemPrefab(pocketPortal.name.GetStableHashCode()) == null)
				{
					ObjectDB.instance.m_items.Add(pocketPortal);
				}
			}
		}*/
	}
}
