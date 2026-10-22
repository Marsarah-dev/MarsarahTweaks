
using HarmonyLib;
using MarsarahTweaks.Managers;
using UnityEngine;

namespace MarsarahTweaks.Patches.QOL
{
	internal static class HoeRadiusChanges
	{
		private static readonly LogManager log = new LogManager("Hoe Radius", LogManager.LogLevel.Info);

		[HarmonyPatch(typeof(Player), "UpdatePlacement")]
		private static class Player_UpdatePlacement_Patch
		{
			private static void Prefix(Player __instance, bool takeInput, PieceTable ___m_buildPieces)
			{
				if (!ConfigManager.AdjustableHoeRadiusEnabled.Value || !takeInput || __instance != Player.m_localPlayer || ___m_buildPieces == null || ObjectDB.instance == null)
					return;

				GameObject hoePrefab = ObjectDB.instance.GetItemPrefab("Hoe");
				PieceTable hoeTable = hoePrefab?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces;

				if (hoeTable == null || ___m_buildPieces != hoeTable)
					return;

				float scroll = ZInput.GetMouseScrollWheel();

				if (Mathf.Approximately(scroll, 0f))
					return;

				GameObject selectedPrefab = ___m_buildPieces.GetSelectedPrefab();
				TerrainOp terrainOp = selectedPrefab?.GetComponent<TerrainOp>();

				if (terrainOp == null)
					return;

				TerrainOp.Settings settings = terrainOp.m_settings;

				log.Info($"Scroll={scroll}, Piece={selectedPrefab.name}, Level={settings.m_levelRadius}, Smooth={settings.m_smoothRadius}, Paint={settings.m_paintRadius}, Raise={settings.m_raiseRadius}");
			}
		}
	}
}
