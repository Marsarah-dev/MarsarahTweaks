using HarmonyLib;
using Jotunn.Configs;
using MarsarahTweaks.Managers;
using Splatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.BuildPieces
{
	internal class GlacialStonePortal
	{
		private static readonly LogManager log = new LogManager("Glacial Stone Portal", LogManager.LogLevel.Info);

		private static bool initialized = false;
		private static GameObject GlacialPortalPrefab;
		//private static GameObject MarblePortalPrefab;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || initialized)
					return;

				initialized = true;

				CreateGlacialPortal();
				//CreateMarblePortal();
			}
		}

		private static void CreateGlacialPortal()
		{
			if (MPrefabManager.GetPrefab("portal_glacial") != null) return;

			// Clone portal
			GlacialPortalPrefab = ClonePortalPrefab("portal", "portal_glacial");
			if (GlacialPortalPrefab == null) return;

			// Apply portal-specific data
			SetupGlacialPortalDefaults(GlacialPortalPrefab, "Glacial Stone Portal", "piece_stonecutter");

			// Add glacial portal to ZNetScene
			MPrefabManager.RegisterToZNetScene(GlacialPortalPrefab);

			// Configure the Piece data and add to buiild menu
			ConfigureGlacialPortalPieceData(GlacialPortalPrefab, "Stone", "FreezeGland", "SurtlingCore");

			// Toggle visibility
			TogglePortalVisibility();

			GlacialPortalPrefab.SetActive(true);
			log.Info("Glacial Portal registered and ready.");
		}

		/*private static void CreateMarblePortal()
		{
			if (MPrefabManager.GetPrefab("marble_portal") != null) return;

			// Clone portal
			MarblePortalPrefab = ClonePortalPrefab("portal", "marble_portal");
			if (MarblePortalPrefab == null) return;

			// Apply portal-specific data
			SetupGlacialPortalDefaults(MarblePortalPrefab, "Marble Portal", "piece_stonecutter");

			// Modify marble portal material
			ModifyPortalMaterial(MarblePortalPrefab, "blackmarble_column_1"); // blackmarble_floor, blackmarble_stair_corner 

			// Add glacial portal to ZNetScene
			MPrefabManager.RegisterToZNetScene(MarblePortalPrefab);

			// Configure the Piece data and add to buiild menu
			ConfigureGlacialPortalPieceData(MarblePortalPrefab, "BlackMarble", null, "SurtlingCore");

			// Toggle visibility
			TogglePortalVisibility();

			MarblePortalPrefab.SetActive(true);
			log.Info("Glacial Portal registered and ready.");
		}*/

		private static GameObject ClonePortalPrefab(string sourcePrefabName, string newPrefabName)
		{
			GameObject prefab = MPrefabManager.ClonePrefab(sourcePrefabName, newPrefabName);
			if (prefab == null)
			{
				log.Error($"Cloning of {sourcePrefabName} failed.");
			}

			return prefab;
		}

		private static void SetupGlacialPortalDefaults(GameObject prefab, string name, string craftingStation = null)
		{
			ZNetView znet = prefab.GetComponent<ZNetView>();
			if (znet != null)
			{
				znet.m_persistent = true;
				znet.m_distant = false;
				znet.m_type = ZDO.ObjectType.Solid;
				znet.m_syncInitialScale = false;
			}

			Piece piece = prefab.GetComponent<Piece>();
			if (piece != null)
			{
				piece.m_enabled = true;
				piece.m_name = name;
				piece.m_description = "";

				GameObject craftingStationPrefab = MPrefabManager.GetPrefab(craftingStation);
				if (craftingStationPrefab != null)
				{
					CraftingStation newCraftingStation = craftingStationPrefab.GetComponent<CraftingStation>();
					if (newCraftingStation != null)
					{
						piece.m_craftingStation = newCraftingStation;
						log.Info($"Crafting station set to {newCraftingStation.name} for piece {piece.name}");
					}
				}
			}
		}

		/*private static void ModifyPortalMaterial(GameObject targetPrefab, string sourcePrefabName)
		{
			// Get the mesh renderer of the target targetPrefab
			MeshRenderer meshRenderer = targetPrefab.GetComponentInChildren<MeshRenderer>();
			if (meshRenderer == null)
			{
				log.Error("Could not get MeshRenderer component.");
				return;
			}

			// Get the reference targetPrefab from ZNetScene
			GameObject sourcePrefab = MPrefabManager.GetPrefab(sourcePrefabName);
			if (sourcePrefab == null)
			{
				log.Error($"Could not find '{sourcePrefabName}' targetPrefab.");
				return;
			}

			// Get material from target targetPrefab
			MeshRenderer sourceRenderer = sourcePrefab.GetComponentInChildren<MeshRenderer>();
			if (sourceRenderer == null || sourceRenderer.sharedMaterial == null)
			{
				log.Error($"'{sourcePrefabName}' has no MeshRenderer or material.");
				return;
			}

			Material newMaterial = sourceRenderer.sharedMaterial;
			var materials = meshRenderer.materials;
			materials[0] = newMaterial;
			meshRenderer.materials = materials;

			log.Info($"Applied material from '{sourcePrefabName}' to '{targetPrefab.name}'.");
		}*/

		private static void ConfigureGlacialPortalPieceData(GameObject prefab, string resource1, string resource2, string resource3)
		{
			var reqs = new List<RequirementConfig>();

			if (!string.IsNullOrEmpty(resource1))
				reqs.Add(new RequirementConfig(resource1, 6, recover: true));

			if (!string.IsNullOrEmpty(resource2))
				reqs.Add(new RequirementConfig(resource2, 2, recover: true));

			if (!string.IsNullOrEmpty(resource3))
				reqs.Add(new RequirementConfig(resource3, 2, recover: true));

			var pieceConfig = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = "Misc",
				Requirements = reqs.ToArray()
			};

			MPrefabManager.AddToBuildMenu(prefab, pieceConfig);
		}

		public static void TogglePortalVisibility()
		{
			Piece glacialPortalPiece = GlacialPortalPrefab?.GetComponent<Piece>();
			//Piece marblePortalPiece = MarblePortalPrefab?.GetComponent<Piece>();
			if (glacialPortalPiece == null/* || marblePortalPiece == null*/)
			{
				log.Warn($"Piece component does not exist. No toggle made.");
				return;
			}

			if (ConfigManager.GlacialStonePortalEnabled.Value)
			{
				glacialPortalPiece.m_enabled = true;
				//marblePortalPiece.m_enabled = true;
			}
			else
			{
				glacialPortalPiece.m_enabled = false;
				//marblePortalPiece.m_enabled = false;
			}
		}

		// Adds the portal_glacial targetPrefab early to be picked up by ZDOMan
		[HarmonyPatch(typeof(Game), "Awake")]
		public static class EarlyPortalPrefabRegister
		{
			static void Prefix(Game __instance)
			{
				int hashGlacial = "portal_glacial".GetStableHashCode();
				if (!__instance.PortalPrefabHash.Contains(hashGlacial))
				{
					__instance.PortalPrefabHash.Add(hashGlacial);
					log.Info($"Registered portal_glacial targetPrefab hash early: {hashGlacial}");
				}

				/*int hashMarble = "marble_portal".GetStableHashCode();
				if (!__instance.PortalPrefabHash.Contains(hashMarble))
				{
					__instance.PortalPrefabHash.Add(hashMarble);
					log.Info($"Registered portal_glacial targetPrefab hash early: {hashMarble}");
				}*/
			}
		}

		// Adds the portal_glacial targetPrefab to the list of known portals
		[HarmonyPatch(typeof(Game), nameof(Game.ConnectPortals))]
		public static class Game_ConnectPortals_Patch
		{
			static void Prefix(Game __instance)
			{
				if (GlacialPortalPrefab == null)
				{
					log.Info("ConnectPortals patch: Glacial Portal targetPrefab not ready yet.");
					return;
				}

				if (!__instance.m_portalPrefabs.Contains(GlacialPortalPrefab))
				{
					__instance.m_portalPrefabs.Add(GlacialPortalPrefab);
					__instance.PortalPrefabHash.Add("portal_glacial".GetStableHashCode());
					log.Info("Registered 'portal_glacial' in Game.m_portalPrefabs via ConnectPortals.");
				}

				/*if (MarblePortalPrefab == null)
				{
					log.Info("ConnectPortals patch: Marble Portal targetPrefab not ready yet.");
					return;
				}

				if (!__instance.m_portalPrefabs.Contains(MarblePortalPrefab))
				{
					__instance.m_portalPrefabs.Add(MarblePortalPrefab);
					__instance.PortalPrefabHash.Add("marble_portal".GetStableHashCode());
					log.Info("Registered 'marble_portal' in Game.m_portalPrefabs via ConnectPortals.");
				}*/
			}
		}
	}
}
