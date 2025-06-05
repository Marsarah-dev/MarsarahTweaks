using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class PortalsPerPlayer
	{
		private static GameObject PortalPrefab = MPrefabManager.GetPrefab("portal_wood");
		private static GameObject PortalStonePrefab = MPrefabManager.GetPrefab("portal_stone");

		// Limit Portals per player
		[HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
		public static class Portal_LimitPlacement
		{
			static bool Prefix(Player __instance, Piece piece, ref bool __result)
			{
				if (piece.name == PortalPrefab.name && PlayerReachedPortalLimit(PortalPrefab) || piece.name == PortalStonePrefab.name && PlayerReachedPortalLimit(PortalStonePrefab))
				{
					__instance.Message(MessageHud.MessageType.Center, "You reached the maximum number of allowed portals.");
					__result = false;
					return false;
				}

				return true;
			}
		}

		private static bool PlayerReachedPortalLimit(GameObject portalPrefab)
		{
			if (ZNet.instance == null || ZDOMan.instance == null || Player.m_localPlayer == null) return false;
			if (portalPrefab == null)	return false;

			string localPlayerName = Player.m_localPlayer.GetPlayerName();
			int portalHash = portalPrefab.name.GetStableHashCode();

			var zdoDictField = typeof(ZDOMan).GetField("m_objectsByID", BindingFlags.NonPublic | BindingFlags.Instance);
			if (zdoDictField == null)
				return false;

			Dictionary<ZDOID, ZDO> zdoDict = zdoDictField.GetValue(ZDOMan.instance) as Dictionary<ZDOID, ZDO>;
			if (zdoDict == null)
				return false;

			int count = 0;
			foreach (var zdo in zdoDict.Values)
			{
				if (zdo == null) continue;
				if (zdo.GetPrefab() != portalHash) continue;
				if (zdo.GetString(ZDOVars.s_creatorName) == localPlayerName) count++;
			}

			if (ConfigManager.MaxPortalsPerPlayer.Value < 0)
			{
				return false;
			}
			if (count >= ConfigManager.MaxPortalsPerPlayer.Value)
			{
				return true;
			}
			return false;
		}

		/*[HarmonyPatch(typeof(Player), "Update")]
		public class Player_Update_DebugPortalCount
		{
			static void Postfix(Player __instance)
			{
				if (__instance != Player.m_localPlayer) return;

				if (Input.GetKeyDown(KeyCode.F3))
				{
					LogPortalCount();
				}
			}
		}

		public static void LogPortalCount()
		{
			if (ZNet.instance == null || ZDOMan.instance == null || Player.m_localPlayer == null)
			{
				MarsarahTweaks.LogWarn("Required instances are missing.");
				return;
			}

			string localPlayerName = Player.m_localPlayer.GetPlayerName();
			int portalWoodHash = MPrefabManager.GetPrefab("portal_wood").name.GetStableHashCode();
			int portalStoneHash = MPrefabManager.GetPrefab("portal_stone").name.GetStableHashCode();
			int portalPocketHash = MPrefabManager.GetPrefab("pocket_portal").name.GetStableHashCode();
			int portalUnusedHash = MPrefabManager.GetPrefab("portal").name.GetStableHashCode();

			var zdoDictField = typeof(ZDOMan).GetField("m_objectsByID", BindingFlags.NonPublic | BindingFlags.Instance);
			if (zdoDictField == null)
			{
				MarsarahTweaks.LogError("Could not access m_objectsByID field.");
				return;
			}

			var zdoDict = zdoDictField.GetValue(ZDOMan.instance) as Dictionary<ZDOID, ZDO>;
			if (zdoDict == null)
			{
				MarsarahTweaks.LogError("m_objectsByID is null or invalid.");
				return;
			}

			int countW = 0;
			foreach (var zdo in zdoDict.Values)
			{
				if (zdo == null) continue;
				if (zdo.GetPrefab() != portalWoodHash) continue;
				if (zdo.GetString(ZDOVars.s_creatorName) == localPlayerName) countW++;
			}

			int countS = 0;
			foreach (var zdo in zdoDict.Values)
			{
				if (zdo == null) continue;
				if (zdo.GetPrefab() != portalStoneHash) continue;
				if (zdo.GetString(ZDOVars.s_creatorName) == localPlayerName) countS++;
			}

			int countP = 0;
			foreach (var zdo in zdoDict.Values)
			{
				if (zdo == null) continue;
				if (zdo.GetPrefab() != portalPocketHash) continue;
				if (zdo.GetString(ZDOVars.s_creatorName) == localPlayerName) countP++;
			}

			int countU = 0;
			foreach (var zdo in zdoDict.Values)
			{
				if (zdo == null) continue;
				if (zdo.GetPrefab() != portalUnusedHash) continue;
				if (zdo.GetString(ZDOVars.s_creatorName) == localPlayerName) countU++;
			}

			MarsarahTweaks.LogInfo($"Normal Portals built by this player: {countW}");
			MarsarahTweaks.LogInfo($"Stone Portals built by this player: {countS}");
			MarsarahTweaks.LogInfo($"Pocket Portals built by this player: {countP}");
			MarsarahTweaks.LogInfo($"Unused Portals built by this player: {countU}");
		}*/
	}
}
