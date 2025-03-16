using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
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

		private static readonly ConfigSync configSync = new ServerSync.ConfigSync(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
		private static ConfigEntry<bool> serverConfigLocked;
		public static ConfigEntry<bool> testJumpEnabled;

		void Awake()
		{
			serverConfigLocked = config("1 - Main Mod", "Lock Configuration", true, "If on, only server admins can change the configuration.");
			_ = configSync.AddLockingConfigEntry<bool>(serverConfigLocked);
			testJumpEnabled = config("2 - Test", "Test Jump", true, "Test stuff");

			harmony.PatchAll();
			SetupWatcher();
		}

		private void OnDestroy()
		{
			((BaseUnityPlugin)this).Config.Save();
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
				MLog("ReadConfigValues called");
				((BaseUnityPlugin)this).Config.Reload();
			}
			catch
			{
				MLog("There was an issue loading " + ConfigFileName);
			}
		}

		// Config Template =========================================================================
		ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
		{
			ConfigDescription extendedDescription = new ConfigDescription(description.Description + (synchronizedSetting ? " [Synced with Server] " : " [Not Synced with Server] "), description.AcceptableValues, description.Tags);

			ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			return configEntry;
		}

		ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

		// Test Jump ===============================================================================
		[HarmonyPatch(typeof(Character), nameof(Character.Jump))]
		class TestJump_Patch
		{
			static void Prefix (ref float ___m_jumpForce)
			{
				if (testJumpEnabled.Value)
				{
					MLog("Initial jump force: " + ___m_jumpForce);
					___m_jumpForce = 15;
					MLog("Modified jump force: " + ___m_jumpForce);
				}
				else
				{
					___m_jumpForce = 8;
					MLog("Default jump force: " + ___m_jumpForce);
				}
			}
		}

		// Marsarah Tweaks Log =====================================================================
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
