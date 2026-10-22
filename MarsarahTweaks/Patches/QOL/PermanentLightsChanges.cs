using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsarahTweaks.Patches.QOL
{
	internal class PermanentLightsChanges
	{
		private static readonly LogManager log = new LogManager("Permanent Lights", LogManager.LogLevel.Warning);

		private const int CampfirePermanentFuel = 5;
		private const int HearthPermanentFuel = 10;

		// Fireplace is shared by decorative lights and open fires. Only the latter
		// are affected by the Lights & Fires (and All Supported Pieces) scopes.
		private static bool IsFire(Fireplace fireplace, string prefabName)
		{
			string pieceName = fireplace.GetComponent<Piece>()?.m_name;
			if (pieceName == "$piece_firepit" || pieceName == "$piece_firepit_iron" ||
				pieceName == "$piece_hearth" || pieceName == "$piece_bonfire")
				return true;

			return prefabName.IndexOf("fire_pit", StringComparison.OrdinalIgnoreCase) >= 0 ||
				prefabName.IndexOf("firepit", StringComparison.OrdinalIgnoreCase) >= 0 ||
				prefabName.IndexOf("hearth", StringComparison.OrdinalIgnoreCase) >= 0 ||
				prefabName.IndexOf("bonfire", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static bool CoversFireplaces => ConfigManager.PermanentLightsMode.Value != ConfigManager.PermanentLightsOptions.Off;

		private static bool CoversFires =>
			ConfigManager.PermanentLightsMode.Value == ConfigManager.PermanentLightsOptions.LightsAndFires ||
			ConfigManager.PermanentLightsMode.Value == ConfigManager.PermanentLightsOptions.AllSupportedPieces;

		private static bool CoversUtilities =>
			ConfigManager.PermanentLightsMode.Value == ConfigManager.PermanentLightsOptions.AllSupportedPieces;

		private static void Refill(ZNetView nview, float maxFuel)
		{
			if (nview == null || !nview.IsOwner() || maxFuel <= 0f)
				return;

			ZDO zdo = nview.GetZDO();
			if (zdo == null || zdo.GetFloat("fuel", 0f) >= maxFuel)
				return;

			zdo.Set("fuel", maxFuel);
		}

		[HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
		private static class Fireplace_UpdateFireplace_Patch
		{
			// This makes fuel burn faster. Meant for testing.
			/*private static bool burnSpeedLogged;

			private static void Prefix(Fireplace __instance)
			{
				__instance.m_secPerFuel = 3f;

				if (!burnSpeedLogged)
				{
					log.Info("Temporary testing: Fireplace fuel burn time set to 3 seconds per fuel.");
					burnSpeedLogged = true;
				}
			}*/

			private static void Postfix(Fireplace __instance, ZNetView ___m_nview)
			{
				string prefabName = Utils.GetPrefabName(__instance.gameObject);
				bool isFire = IsFire(__instance, prefabName);

				// Preserve the existing reduced capacities, but only for selected fires.
				if (prefabName == "fire_pit")
					__instance.m_maxFuel = CoversFires ? CampfirePermanentFuel : 10f;
				else if (prefabName == "hearth")
					__instance.m_maxFuel = CoversFires ? HearthPermanentFuel : 20f;

				if (!CoversFireplaces || (isFire && !CoversFires))
					return;

				Refill(___m_nview, __instance.m_maxFuel);
			}
		}

		// Stone Oven: cooking station that consumes its own wood fuel.
		[HarmonyPatch(typeof(CookingStation), "UpdateCooking")]
		private static class CookingStation_UpdateCooking_Patch
		{
			// This makes Stone Oven fuel burn faster. Meant for testing.
			/*private static bool ovenBurnSpeedLogged;

			private static void Prefix(CookingStation __instance)
			{
				if (Utils.GetPrefabName(__instance.gameObject) != "piece_oven")
					return;

				__instance.m_secPerFuel = 3;

				if (!ovenBurnSpeedLogged)
				{
					log.Info("Temporary testing: Stone Oven fuel burn time set to 3 seconds.");
					ovenBurnSpeedLogged = true;
				}
			}*/

			private static void Postfix(CookingStation __instance, ZNetView ___m_nview)
			{
				if (!CoversUtilities || !__instance.m_useFuel || Utils.GetPrefabName(__instance.gameObject) != "piece_oven")
					return;

				Refill(___m_nview, __instance.m_maxFuel);
			}
		}

		// Hot Tub: internally a Smelter, not a Fireplace.
		[HarmonyPatch(typeof(Smelter), "UpdateSmelter")]
		private static class Smelter_UpdateSmelter_Patch
		{
			// This makes Hot Tub fuel burn faster. Meant for testing.
			/*private static bool hotTubBurnSpeedLogged;

			private static void Prefix(Smelter __instance)
			{
				if (Utils.GetPrefabName(__instance.gameObject) != "piece_bathtub")
					return;

				__instance.m_secPerProduct = 3f;

				if (!hotTubBurnSpeedLogged)
				{
					log.Info("Temporary testing: Hot Tub fuel burn time set to 3 seconds.");
					hotTubBurnSpeedLogged = true;
				}
			}*/

			private static void Postfix(Smelter __instance, ZNetView ___m_nview)
			{
				if (!CoversUtilities || Utils.GetPrefabName(__instance.gameObject) != "piece_bathtub")
					return;

				Refill(___m_nview, __instance.m_maxFuel);
			}
		}

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		private static class PermanentLightsBuildPieces_Patch
		{
			private static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || (ZNet.instance != null && ZNet.instance.IsDedicated()))
					return;

				UpdateLightBuildPiecesAmounts(__instance, false);
			}
		}

		// Keep existing cost adjustments, but restore costs when a scope excludes a piece.
		private static readonly Dictionary<string, Piece.Requirement[]> originalLightPieceCosts = new Dictionary<string, Piece.Requirement[]>();

		public static readonly Dictionary<string, Dictionary<string, int>> lightPieceCostChanges = new Dictionary<string, Dictionary<string, int>>()
		{
			{ "$piece_firepit",             new Dictionary<string, int>() { { "Wood", CampfirePermanentFuel } } },
			{ "$piece_hearth",              new Dictionary<string, int>() { { "Wood", HearthPermanentFuel } } },
			{ "$piece_sconce",              new Dictionary<string, int>() { { "Resin", 6 } } },
			{ "$piece_groundtorch",         new Dictionary<string, int>() { { "Resin", 6 } } },
			{ "$piece_groundtorchwood",     new Dictionary<string, int>() { { "Resin", 4 } } },
			{ "$piece_groundtorchgreen",    new Dictionary<string, int>() { { "Guck", 6 } } },
			{ "$piece_groundtorchblue",     new Dictionary<string, int>() { { "GreydwarfEye", 6 } } },
			{ "$piece_brazierfloor01",      new Dictionary<string, int>() { { "Coal", 5 } } },
			{ "$piece_brazierfloor02",      new Dictionary<string, int>() { { "GreydwarfEye", 5 } } },
			{ "$piece_brazierceiling01",    new Dictionary<string, int>() { { "Coal", 5 } } }
		};

		private static Piece.Requirement CopyRequirement(Piece.Requirement req)
		{
			return new Piece.Requirement
			{
				m_resItem = req.m_resItem,
				m_amount = req.m_amount,
				m_amountPerLevel = req.m_amountPerLevel,
				m_recover = req.m_recover
			};
		}

		private static GameObject FindPiecePrefab(ZNetScene scene, string pieceName)
		{
			if (pieceName == "$piece_firepit")
				return scene.m_prefabs.Find(p => p.name == "fire_pit" && p.GetComponent<Piece>()?.m_name == pieceName);

			return scene.m_prefabs.Find(p => p.GetComponent<Piece>()?.m_name == pieceName);
		}

		public static void UpdateLightBuildPiecesAmounts(ZNetScene znScene, bool wasChanged)
		{
			if (znScene == null)
				return;

			foreach (var change in lightPieceCostChanges)
			{
				string pieceName = change.Key;
				bool fireCost = pieceName == "$piece_firepit" || pieceName == "$piece_hearth";
				bool shouldModify = fireCost ? CoversFires : CoversFireplaces;
				bool hasBackup = originalLightPieceCosts.TryGetValue(pieceName, out Piece.Requirement[] backup);

				if (!shouldModify && !hasBackup)
					continue;

				GameObject prefab = FindPiecePrefab(znScene, pieceName);
				Piece piece = prefab?.GetComponent<Piece>();
				if (piece == null)
				{
					if (shouldModify)
						log.Warn($"{pieceName} prefab not found in ZNetScene.");
					continue;
				}

				if (!shouldModify)
				{
					piece.m_resources = backup.Select(CopyRequirement).ToArray();
					originalLightPieceCosts.Remove(pieceName);
					log.Info($"Restored build costs for {pieceName}.");
					continue;
				}

				if (!hasBackup)
					originalLightPieceCosts[pieceName] = piece.m_resources.Select(CopyRequirement).ToArray();

				foreach (Piece.Requirement req in piece.m_resources)
				{
					if (req.m_resItem != null && change.Value.TryGetValue(req.m_resItem.name, out int newAmount))
					{
						req.m_amount = newAmount;
						if (req.m_resItem.name == "Wood")
							req.m_recover = true;
					}
				}

				if (pieceName == "$piece_hearth" && !piece.m_resources.Any(req => req.m_resItem != null && req.m_resItem.name == "Wood"))
				{
					GameObject woodPrefab = znScene.GetPrefab("Wood");
					ItemDrop wood = woodPrefab?.GetComponent<ItemDrop>();
					if (wood != null)
					{
						piece.m_resources = piece.m_resources.Append(new Piece.Requirement
						{
							m_amount = HearthPermanentFuel,
							m_amountPerLevel = 0,
							m_resItem = wood,
							m_recover = true
						}).ToArray();
						log.Info("Added the Hearth's wood build requirement.");
					}
					else
						log.Warn("Could not find Wood prefab for the Hearth build requirement.");
				}
			}
		}
	}
}