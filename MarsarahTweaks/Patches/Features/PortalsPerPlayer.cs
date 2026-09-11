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
		private static readonly LogManager log = new LogManager("Portals Per Player", LogManager.LogLevel.Warning);

		//private static GameObject PortalPrefab = MPrefabManager.GetPrefab("portal_wood");
		//private static GameObject PortalStonePrefab = MPrefabManager.GetPrefab("portal_stone");
		private const string PortalPrefabName = "portal_wood";
		private const string PortalStonePrefabName = "portal_stone";
		private const string PortalGlacialPrefabName = "portal_glacial";
		//private static GameObject PortalGlacialPrefab = MPrefabManager.GetPrefab("portal_glacial");

		// Limit Portals per player
		[HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
		public static class Portal_LimitPlacement
		{
			static bool Prefix(Player __instance, Piece piece, ref bool __result)
			{
				if ((piece.name == PortalPrefabName && PlayerReachedPortalLimit(__instance, PortalPrefabName)) ||
					(piece.name == PortalStonePrefabName && PlayerReachedPortalLimit(__instance, PortalStonePrefabName)) ||
					(piece.name == PortalGlacialPrefabName && PlayerReachedPortalLimit(__instance, PortalGlacialPrefabName)))
				{
					log.Info($"Blocking portal placement for {__instance.GetPlayerName()} ({__instance.GetPlayerID()}): portal limit reached");
					__instance.Message(MessageHud.MessageType.Center, "You reached the maximum number of allowed portals.");
					__result = false;
					return false;
				}

				return true;
			}
		}

		private static bool PlayerReachedPortalLimit(Player player, string portalPrefabName)
		{
			if (ZNet.instance == null || ZDOMan.instance == null || Player.m_localPlayer == null) return false;

			if (ConfigManager.MaxPortalsPerPlayer.Value < 0)
			{
				log.Info("No portal limit set");
				return false;
			}

			long playerId = player.GetPlayerID();
			int portalHash = portalPrefabName.GetStableHashCode();

			var zdoDictField = typeof(ZDOMan).GetField("m_objectsByID", BindingFlags.NonPublic | BindingFlags.Instance);
			if (zdoDictField == null)
			{
				log.Error("Zdo dictionary field is null");
				return false;
			}

			Dictionary<ZDOID, ZDO> zdoDict = zdoDictField.GetValue(ZDOMan.instance) as Dictionary<ZDOID, ZDO>;
			if (zdoDict == null)
			{
				log.Error("Zdo dictionary is null");
				return false;
			}

			int count = 0;
			foreach (var zdo in zdoDict.Values)
			{
				if (zdo == null) continue;
				if (zdo.GetPrefab() != portalHash) continue;

				long creatorId = zdo.GetLong(ZDOVars.s_creator, 0L);
				if (creatorId == playerId) count++;
			}

			log.Info($"Player {player.GetPlayerName()} ({playerId}) has {count}/{ConfigManager.MaxPortalsPerPlayer.Value} {portalPrefabName} portals");

			if (count >= ConfigManager.MaxPortalsPerPlayer.Value)
			{
				log.Info($"Player {player.GetPlayerName()} reached portal limit");
				return true;
			}

			return false;
		}
	}
}
