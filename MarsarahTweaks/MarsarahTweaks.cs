using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MarsarahTweaks
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahTweaks : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahTweaks";
		internal const string ModVersion = "0.1.0";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		private readonly Harmony harmony = new Harmony(ModGUID);

		void Awake()
		{
			ConfigManager.Init(Config);
			harmony.PatchAll();
		}

		private void OnDestroy()
		{
			Config.Save();
		}

		// Logger =====================================================================
		public static void MLog(string log, bool header = false, bool footer = false)
		{
			if (header)
			{
				Debug.Log("===================================================");
			}

			Debug.Log($"[Marsarah Tweaks] : " + log);

			if (footer)
			{
				Debug.Log("===================================================");
			}
		}
	}
}
