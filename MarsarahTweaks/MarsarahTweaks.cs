using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System.IO;
using UnityEngine;

namespace MarsarahTweaks
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahTweaks : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahTweaks";
		internal const string ModVersion = "0.2.0";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		// Config file stuff
		private static string ConfigFileName => MarsarahTweaks.ModGUID + ".cfg";
		private static string ConfigFileFullPath => Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

		private readonly Harmony harmony = new Harmony(ModGUID);

		private static readonly ConfigSync configSync = new ConfigSync(ModGUID)
		{
			DisplayName = ModName,
			CurrentVersion = ModVersion,
			MinimumRequiredVersion = ModVersion
		};

		// Config entries
		public static ConfigEntry<bool> serverConfigLocked;
		public static ConfigEntry<bool> testJumpEnabled;

		void Awake()
		{
			//ConfigManager.Init(Config);

			serverConfigLocked = config("1 - Main", "Lock Configuration", true, "If on, only server admins can change the configuration.");
			_ = configSync.AddLockingConfigEntry(serverConfigLocked);
			
			testJumpEnabled = config("1 - Main", "Test Jump", true, "Test Jump Patch");

			harmony.PatchAll();
			SetupWatcher();
		}

		private void OnDestroy()
		{
			Config.Save();
		}

		private void SetupWatcher()
		{
			FileSystemWatcher watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName)
			{
				IncludeSubdirectories = true,
				SynchronizingObject = ThreadingHelper.SynchronizingObject,
				EnableRaisingEvents = true
			};

			watcher.Changed += ReadConfigValues;
			watcher.Created += ReadConfigValues;
			watcher.Renamed += ReadConfigValues;
		}

		private void ReadConfigValues(object sender, FileSystemEventArgs e)
		{
			MarsarahTweaks.MLog($"ReadConfigValues triggered by file change: {e.FullPath}");

			if (!File.Exists(ConfigFileFullPath))
				return;

			try
			{
				MarsarahTweaks.MLog("ReadConfigValues called");
				Config.Reload();
			}
			catch
			{
				MarsarahTweaks.MLog("There was an issue loading " + ConfigFileName);
			}
		}

		ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
		{
			ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			return configEntry;
		}

		ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);


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
