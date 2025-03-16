using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public static string ConfigFileName = ModGUID + ".cfg";
		public static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

		private readonly Harmony harmony = new Harmony(ModGUID);

		void Awake()
		{
			ConfigManager.ReadConfigFile();

			harmony.PatchAll();
			SetupWatcher();
		}
		private void SetupWatcher()
		{
			FileSystemWatcher watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
			watcher.Changed += ReadConfigValues;
			watcher.Created += ReadConfigValues;
			watcher.Renamed += ReadConfigValues;
			watcher.IncludeSubdirectories = true;
			watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
			watcher.EnableRaisingEvents = true;
		}

		private void ReadConfigValues(object sender, FileSystemEventArgs e)
		{
			if (!File.Exists(ConfigFileFullPath))
			{
				return;
			}
			try
			{
				MTLog("ReadConfigValues called");
				((BaseUnityPlugin)this).Config.Reload();
			}
			catch
			{
				MTLog("There was an issue loading " + ConfigFileName);
			}
		}

		// Marsarah Tweaks Log =====================================================================
		public static void MTLog(string log, bool header = false, bool footer = false)
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
