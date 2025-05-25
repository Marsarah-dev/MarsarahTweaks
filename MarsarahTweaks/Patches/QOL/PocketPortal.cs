/*using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.QOL
{
	[HarmonyPatch(typeof(ObjectDB), "Awake")]
	public static class PocketPortal_ObjectDB_Patch
	{
		private static void Postfix(ObjectDB __instance)
		{
			if (!ConfigManager.PocketPortalEnabled.Value) return;

			var portalPrefab = ZNetScene.instance?.GetPrefab("portal");
			if (portalPrefab == null) return;

			var piece = portalPrefab.GetComponent<Piece>();
			if (piece == null) return;

			// Set these EARLY so they are picked up by the hammer
			piece.m_name = "$piece_pocketportal";
			piece.m_description = "$piece_pocketportal_description";
			piece.m_category = Piece.PieceCategory.Misc; // Optional, but useful

			MarsarahTweaks.MLog("Pocket Portal localized name + category set.");
		}
	}


	[HarmonyPatch(typeof(Player), "Awake")]
	public static class PocketPortal_Hammer_Patch
	{
		private static void Postfix()
		{
			if (!ConfigManager.PocketPortalEnabled.Value) return;

			var hammer = ObjectDB.instance?.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>();
			if (hammer?.m_itemData?.m_shared?.m_buildPieces?.m_pieces == null) return;

			var pieceTable = hammer.m_itemData.m_shared.m_buildPieces;
			var portalPrefab = ZNetScene.instance?.GetPrefab("portal");
			if (portalPrefab == null) return;

			if (!pieceTable.m_pieces.Contains(portalPrefab))
			{
				pieceTable.m_pieces.Add(portalPrefab);
				MarsarahTweaks.MLog("Pocket Portal added to hammer menu.");
			}
		}
	}


	[HarmonyPatch(typeof(Localization), "SetupLanguage")]
	class LoadingTips_Patch
	{
		private static void Postfix(Localization __instance, string language)
		{
			if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

			if (__instance == null) return;

			if (!ConfigManager.PocketPortalEnabled.Value) return;

			if (language == "English")
			{
				var translationsField = typeof(Localization).GetField("m_translations", BindingFlags.Instance | BindingFlags.NonPublic);
				if (translationsField == null) return;

				var translations = translationsField.GetValue(__instance) as Dictionary<string, string>;
				if (translations == null) return;

				translations["piece_pocketportal"] = "Pocket Portal";
				translations["piece_pocketportal_description"] = "A compact dimensional gateway. One per player.";
			}
		}
	}
}*/



/*[HarmonyPatch(typeof(Player), "Awake")]
public static class PocketPortal_ShowOriginalPortal_Patch
{
	private static void Postfix(Player __instance)
	{
		if (!ConfigManager.PocketPortalEnabled.Value) return;

		var hammerPrefab = ObjectDB.instance?.GetItemPrefab("Hammer");
		if (hammerPrefab == null) return;

		var hammer = hammerPrefab.GetComponent<ItemDrop>();
		if (hammer == null || hammer.m_itemData.m_shared.m_buildPieces == null) return;

		var pieceTable = hammer.m_itemData.m_shared.m_buildPieces;
		if (pieceTable.m_pieces == null) return;

		var portalPrefab = ZNetScene.instance?.GetPrefab("portal");
		if (portalPrefab == null) return;

		MarsarahTweaks.MLog($"Portal category: {portalPrefab.GetComponent<Piece>().m_category}");
		//portalPrefab.GetComponent<Piece>().m_category = Piece.PieceCategory.Misc;

		if (!pieceTable.m_pieces.Contains(portalPrefab))
		{
			pieceTable.m_pieces.Add(portalPrefab);
			MarsarahTweaks.MLog("Added portal to hammer build pieces.");
		}
	}
}*/
