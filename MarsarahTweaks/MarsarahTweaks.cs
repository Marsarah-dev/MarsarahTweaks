
using BepInEx;
using HarmonyLib;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahTweaks : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahTweaks";
		internal const string ModVersion = "1.7.4";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		private readonly Harmony harmony = new Harmony(ModGUID);

		private void Awake()
		{
			LogManager.SetGlobalLogLevel(LogManager.LogLevel.Warning); // None, Error, Warning, Info
			ConfigManager.Init(Config);
			//CustomConsoleCommandHandler.Init(); // Register new console commands

			harmony.PatchAll();
		}

		private void Start()
		{
			CompatibilityManager.Initialize();
			CompatibilityManager.UpdateIncompatibilities();
			//CompatibilityManager.DumpAllLoadedMods();
		}

		private void OnDestroy()
		{
			Config.Save();
		}
	}
}
