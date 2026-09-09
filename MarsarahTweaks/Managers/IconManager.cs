using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace MarsarahTweaks.Managers
{
	internal static class IconManager
	{
		private static readonly LogManager log = new LogManager("Icon Manager", LogManager.LogLevel.Warning);

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

		private static bool TryLoadImageBytes(Texture2D tex, byte[] data)
		{
			if (tex == null || data == null) return false;

			try
			{
				Assembly imageConversionAssembly = Assembly.Load("UnityEngine.ImageConversionModule");
				Type imageConversionType = imageConversionAssembly.GetType("UnityEngine.ImageConversion");

				if (imageConversionType == null)
				{
					log.Error("Could not find UnityEngine.ImageConversion type.");
					return false;
				}

				MethodInfo loadImage = imageConversionType.GetMethod("LoadImage", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);

				if (loadImage == null)
				{
					log.Error("Could not find ImageConversion.LoadImage method.");
					return false;
				}

				var result = loadImage.Invoke(null, new object[] { tex, data, false });

				return result is bool success && success;
			}
			catch (Exception ex)
			{
				log.Error($"Failed to load image bytes: {ex}");
				return false;
			}
		}
	}
}
