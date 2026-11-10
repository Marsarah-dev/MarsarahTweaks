
using HarmonyLib;
using MarsarahTweaks.Managers;
using UnityEngine;

namespace MarsarahTweaks.Patches.QOL
{
	internal static class ShieldGeneratorCapacity
	{
		private static readonly LogManager log = new LogManager("Shield Generator Capacity", LogManager.LogLevel.Warning);
		private const string ShieldGeneratorPrefab = "piece_shieldgenerator";

		[HarmonyPatch(typeof(ShieldGenerator), "Start")]
		private static class ShieldGenerator_StartPatch
		{
			private static void Prefix(ShieldGenerator __instance)
			{
				ApplyCapacity(__instance);
			}
		}

		public static void UpdateShieldGenerators()
		{
			foreach (ShieldGenerator generator in Object.FindObjectsByType<ShieldGenerator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
				ApplyCapacity(generator);
		}

		private static void ApplyCapacity(ShieldGenerator generator)
		{
			if (generator == null || ConfigManager.ShieldGeneratorCapacityConfig == null)
				return;

			if (Utils.GetPrefabName(generator.gameObject) != ShieldGeneratorPrefab)
				return;

			int capacity = ConfigManager.ShieldGeneratorCapacityConfig.Value;
			if (generator.m_maxFuel == capacity)
				return;

			log.Info($"Shield Generator fuel capacity: {generator.m_maxFuel} -> {capacity}");
			generator.m_maxFuel = capacity;
		}
	}
}
