using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MarsarahTweaks.Patches;
using MarsarahTweaks.Patches.UI;
using ServerSync;
using System.IO;
using UnityEngine;

namespace MarsarahTweaks
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahTweaks : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahTweaks";
		internal const string ModVersion = "0.4.1";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		private readonly Harmony harmony = new Harmony(ModGUID);

		void Awake()
		{
			ConfigManager.Init(Config);
			CustomConsoleCommandHandler.Init(); // Register new console commands
			UISmartBiome.UpdateBiomeWeights(); // Set the correct biome weight dictionary at startup

			harmony.PatchAll();
		}

		void Update()
		{
			// Hide/display UI
			UIController.UpdateUIDisplay();
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

			Debug.Log($"[Marsarah Tweaks] : {log}");

			if (footer)
			{
				Debug.Log("===================================================");
			}
		}
	}
}
