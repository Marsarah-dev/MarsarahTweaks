/*using HarmonyLib;
using Jotunn.Configs;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.BuildPieces
{
	internal class SilverTableTorch
	{
		private static readonly LogManager log = new LogManager("Silver Table Torch", LogManager.LogLevel.Info);

		private static bool initialized = false;
		private static GameObject SilverTableTorchPrefab;
		private static GameObject SilverTableTorchPrefabBlue;
		private static GameObject SilverTableTorchPrefabGreen;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || initialized)
					return;

				initialized = true;

				CreateSilverTableTorch();
			}
		}

		private static void CreateSilverTableTorch()
		{
			if (MPrefabManager.GetPrefab("piece_tabletorch_silver") != null && MPrefabManager.GetPrefab("piece_tabletorch_silver_blue") != null && MPrefabManager.GetPrefab("piece_tabletorch_silver_green") != null) return;

			// Clone standing torch
			SilverTableTorchPrefab = CloneStandingTorchPrefab("piece_groundtorch", "piece_tabletorch_silver");
			if (SilverTableTorchPrefab == null) return;
			SilverTableTorchPrefabBlue = CloneStandingTorchPrefab("piece_groundtorch_blue", "piece_tabletorch_silver_blue");
			if (SilverTableTorchPrefabBlue == null) return;
			SilverTableTorchPrefabGreen = CloneStandingTorchPrefab("piece_groundtorch_green", "piece_tabletorch_silver_green");
			if (SilverTableTorchPrefabGreen == null) return;

			// Apply torch-specific data
			SetupSilverTorchDefaults(SilverTableTorchPrefab, "Silver Table Torch", "forge");
			SetupSilverTorchDefaults(SilverTableTorchPrefabBlue, "Blue-burning Silver Table Torch", "forge");
			SetupSilverTorchDefaults(SilverTableTorchPrefabGreen, "Green-burning Silver Table Torch", "forge");

			// Decrease silver torch size
			ScaleSilverTorch(SilverTableTorchPrefab);
			ScaleSilverTorch(SilverTableTorchPrefabBlue);
			ScaleSilverTorch(SilverTableTorchPrefabGreen);

			// Modify silver torch material
			ModifySilverTorchMaterial(SilverTableTorchPrefab, "SilverNecklace");
			ModifySilverTorchMaterial(SilverTableTorchPrefabBlue, "SilverNecklace");
			ModifySilverTorchMaterial(SilverTableTorchPrefabGreen, "SilverNecklace");

			// Add silver sconce to ZNetScene
			MPrefabManager.RegisterToZNetScene(SilverTableTorchPrefab);
			MPrefabManager.RegisterToZNetScene(SilverTableTorchPrefabBlue);
			MPrefabManager.RegisterToZNetScene(SilverTableTorchPrefabGreen);

			// Configure the Piece data and add to buiild menu
			ConfigureSilverTorchPieceData(SilverTableTorchPrefab, "Silver", "Resin");
			ConfigureSilverTorchPieceData(SilverTableTorchPrefabBlue, "Silver", "GreydwarfEye");
			ConfigureSilverTorchPieceData(SilverTableTorchPrefabGreen,"Silver", "Guck");

			// Toggle visibility
			ToggleSilverTorchVisibility();

			SilverTableTorchPrefab.SetActive(true);
			SilverTableTorchPrefabBlue.SetActive(true);
			SilverTableTorchPrefabGreen.SetActive(true);
			log.Info("Silver Torches registered and ready.");
		}

		private static GameObject CloneStandingTorchPrefab(string sourcePrefabName, string newPrefabName)
		{
			GameObject prefab = MPrefabManager.ClonePrefab(sourcePrefabName, newPrefabName);
			if (prefab == null)
			{
				log.Error($"Cloning of {sourcePrefabName} failed.");
			}

			return prefab;
		}

		private static void SetupSilverTorchDefaults(GameObject prefab, string name, string craftingStation = null)
		{
			ZNetView znet = prefab.GetComponent<ZNetView>();
			if (znet != null)
			{
				znet.m_persistent = true;
				znet.m_distant = false;
				znet.m_type = ZDO.ObjectType.Default;
				znet.m_syncInitialScale = true;
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

			Fireplace fireplace = prefab.GetComponent<Fireplace>();
			if (fireplace != null)
			{
				fireplace.m_name = name;
				log.Info($"Name set for Fireplace component");
			}
		}

		private static void ScaleSilverTorch(GameObject prefab)
		{
			// Set the local scale of the prefab's root
			Vector3 scale = Vector3.one * 0.4f;
			prefab.transform.localScale = scale;
		}

		private static void ModifySilverTorchMaterial(GameObject prefab, string metalPrefabName)
		{
			// Get the mesh renderer of the sconce
			MeshRenderer meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
			if (meshRenderer == null)
			{
				log.Error("Could not get MeshRenderer component.");
				return;
			}

			// Get the silver necklace prefab
			GameObject metalPrefab = MPrefabManager.GetPrefab(metalPrefabName);
			if (metalPrefab == null)
			{
				log.Error($"Could not find '{metalPrefabName}' prefab.");
				return;
			}

			// Get the material from its MeshRenderer
			MeshRenderer metalRenderer = metalPrefab.GetComponentInChildren<MeshRenderer>();
			if (metalRenderer == null || metalRenderer.sharedMaterial == null)
			{
				log.Error($"'{metalPrefabName}' has no MeshRenderer or material.");
				return;
			}

			// Replace the sconce's material
			Material silverMaterial = metalRenderer.sharedMaterial;
			Material[] materials = meshRenderer.materials;

			materials[0] = silverMaterial;
			meshRenderer.materials = materials;
		}

		private static void ConfigureSilverTorchPieceData(GameObject prefab, string resourceMetal, string resourceFuel)
		{
			var pieceConfig = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = "Furniture",
				Requirements = new[]
				{
					new RequirementConfig(resourceMetal, 1, recover: true),
					new RequirementConfig(resourceFuel, ConfigManager.PermanentLightsEnabled.Value ? 6 : 2, recover: false)
				}
			};

			MPrefabManager.AddToBuildMenu(prefab, pieceConfig);
		}

		public static void ToggleSilverTorchVisibility()
		{
			Piece silverTorchPiece = SilverTableTorchPrefab?.GetComponent<Piece>();
			Piece silverTorchPieceBlue = SilverTableTorchPrefabBlue?.GetComponent<Piece>();
			Piece silverTorchPieceGreen = SilverTableTorchPrefabGreen?.GetComponent<Piece>();
			if (silverTorchPiece == null || silverTorchPieceBlue == null || silverTorchPieceGreen == null)
			{
				log.Warn($"Piece component does not exist. No toggle made.");
				return;
			}

			if (ConfigManager.BuildPiecesLightingEnabled.Value)
			{
				silverTorchPiece.m_enabled = true;
				silverTorchPieceBlue.m_enabled = true;
				silverTorchPieceGreen.m_enabled = true;
			}
			else
			{
				silverTorchPiece.m_enabled = false;
				silverTorchPieceBlue.m_enabled = false;
				silverTorchPieceGreen.m_enabled = false;
			}
		}

		public static void RefreshSilverTorchRequirements()
		{
			RefreshSilverTorchRequirements(SilverTableTorchPrefab, "Silver", "Resin");
			RefreshSilverTorchRequirements(SilverTableTorchPrefabBlue, "Silver", "GreydwarfEye");
			RefreshSilverTorchRequirements(SilverTableTorchPrefabGreen, "Silver", "Guck");
		}

		private static void RefreshSilverTorchRequirements(GameObject silverSconce, string resourceMetal, string resourceFuel)
		{
			if (silverSconce == null) return;

			var piece = silverSconce.GetComponent<Piece>();
			if (piece == null) return;

			// Clear & rebuild requirements
			var metal = MPrefabManager.GetPrefab(resourceMetal)?.GetComponent<ItemDrop>();
			var fuel = MPrefabManager.GetPrefab(resourceFuel)?.GetComponent<ItemDrop>();

			if (metal == null || fuel == null)
			{
				log.Error($"Missing one or more resource prefabs ({resourceMetal}, {resourceFuel}).");
				return;
			}

			piece.m_resources = new[]
			{
				new Piece.Requirement { m_resItem = metal, m_amount = 1, m_recover = true },
				new Piece.Requirement { m_resItem = fuel, m_amount = ConfigManager.PermanentLightsEnabled.Value ? 6 : 2, m_recover = false }
			};

			log.Info($"Refreshed build requirements: Fuel={piece.m_resources[1].m_amount}");
		}
	}
}*/
