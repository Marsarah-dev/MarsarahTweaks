using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace MarsarahTweaks.Managers
{
	internal static class IconManager
	{
		private static readonly LogManager log = new LogManager("Asset Manager", LogManager.LogLevel.Warning);

		private static readonly Dictionary<string, Sprite> _icons = new Dictionary<string, Sprite>();

		public static Sprite LoadEmbeddedIcon(string resourceName)
		{
			if (string.IsNullOrEmpty(resourceName)) return null;
			if (_icons.TryGetValue(resourceName, out var cached)) return cached;

			var asm = Assembly.GetExecutingAssembly();
			using Stream stream = asm.GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				log.Warn($"Icon resource '{resourceName}' not found in assembly.");
				return null;
			}

			byte[] data;
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				data = ms.ToArray();
			}

			// Create small texture; LoadImage will replace size.
			Texture2D tex = new Texture2D(2, 2);
			bool ok = TryLoadImageBytes(tex, data);

			if (!ok)
			{
				log.Error($"Failed to decode image bytes for '{resourceName}'.");
				UnityEngine.Object.Destroy(tex);
				return null;
			}

			tex.wrapMode = TextureWrapMode.Clamp;
			tex.filterMode = FilterMode.Bilinear;

			var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
			_icons[resourceName] = sprite;
			return sprite;
		}

		// Attempts several reflective ways to call LoadImage so we never need a compile-time dependency on ImageConversion.
		private static bool TryLoadImageBytes(Texture2D tex, byte[] data)
		{
			if (tex == null || data == null) return false;

			try
			{
				// 1) Try instance method Texture2D.LoadImage(byte[]) or LoadImage(byte[], bool)
				var texType = typeof(Texture2D);

				MethodInfo inst = texType.GetMethod("LoadImage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(byte[]) }, null);
				if (inst != null)
				{
					var result = inst.Invoke(tex, new object[] { data });
					if (inst.ReturnType == typeof(bool)) return (bool)result;
					return true;
				}

				inst = texType.GetMethod("LoadImage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(byte[]), typeof(bool) }, null);
				if (inst != null)
				{
					var result = inst.Invoke(tex, new object[] { data, false });
					if (inst.ReturnType == typeof(bool)) return (bool)result;
					return true;
				}
			}
			catch (Exception ex)
			{
				log.Warn($"Texture2D.LoadImage reflection failed: {ex.Message}");
			}

			try
			{
				// 2) Try static UnityEngine.ImageConversion.LoadImage(Texture2D, byte[]) or with bool
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					var type = asm.GetType("UnityEngine.ImageConversion");
					if (type == null) continue;

					MethodInfo m = type.GetMethod("LoadImage", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(Texture2D), typeof(byte[]) }, null);
					if (m != null)
					{
						var result = m.Invoke(null, new object[] { tex, data });
						if (m.ReturnType == typeof(bool)) return (bool)result;
						return true;
					}

					m = type.GetMethod("LoadImage", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);
					if (m != null)
					{
						var result = m.Invoke(null, new object[] { tex, data, false });
						if (m.ReturnType == typeof(bool)) return (bool)result;
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				log.Warn($"ImageConversion.LoadImage reflection failed: {ex.Message}");
			}

			return false;
		}
	}

	/*internal class AssetManager
	{
		// ========================================================================
		// The below section is for custom asset bundles exported with Unity Editor

		public static GameObject pocketPortal = null;

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
		}

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
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
		}

		private static void AddMarsaPrefabsToObjDB()
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
		}
	}*/
}
