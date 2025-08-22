using HarmonyLib;
using Jotunn;
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
	internal class SilverSconce
	{
		private static bool initialized = false;
		private static GameObject SilverSconcePrefab;
		private static GameObject SilverSconcePrefabBlue;
		private static GameObject SilverSconcePrefabGreen;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || initialized)
					return;

				initialized = true;

				CreateSilverSconce();
			}
		}

		private static void CreateSilverSconce()
		{
			if (MPrefabManager.GetPrefab("piece_walltorch_silver") != null && MPrefabManager.GetPrefab("piece_walltorch_silver_blue") != null && MPrefabManager.GetPrefab("piece_walltorch_silver_green") != null) return;

			// Clone Bronze Sconce
			SilverSconcePrefab = CloneBronzeSconcePrefab("piece_walltorch", "piece_walltorch_silver");
			if (SilverSconcePrefab == null) return;

			SilverSconcePrefabBlue = CloneBronzeSconcePrefab("piece_walltorch", "piece_walltorch_silver_blue");
			if (SilverSconcePrefabBlue == null) return;

			SilverSconcePrefabGreen = CloneBronzeSconcePrefab("piece_walltorch", "piece_walltorch_silver_green");
			if (SilverSconcePrefabGreen == null) return;

			// Apply sconce-specific data
			SetupSilverSconceDefaults(SilverSconcePrefab, "Silver Sconce", "forge");
			SetupSilverSconceDefaults(SilverSconcePrefabBlue, "Blue-burning Silver Sconce", "forge");
			SetupSilverSconceDefaults(SilverSconcePrefabGreen, "Green-burning Silver Sconce", "forge");

			// Increase silver sconce size
			ScaleSilverSconce(SilverSconcePrefab, Vector3.one * 1.1f);
			ScaleSilverSconce(SilverSconcePrefabBlue, Vector3.one * 1.1f);
			ScaleSilverSconce(SilverSconcePrefabGreen, Vector3.one * 1.1f);

			// Modify Silver Sconce material
			ModifySilverSconceMaterial(SilverSconcePrefab, "SilverNecklace", "yggashoot_log");
			ModifySilverSconceMaterial(SilverSconcePrefabBlue, "SilverNecklace", "yggashoot_log");
			ModifySilverSconceMaterial(SilverSconcePrefabGreen, "SilverNecklace", "yggashoot_log");

			// Modify the sconce fuel
			ModifyFireplaceFuel(SilverSconcePrefabBlue, "GreydwarfEye");
			ModifyFireplaceFuel(SilverSconcePrefabGreen, "Guck");

			// Modify silver sconce blue and green lights
			CopyLightSettings("piece_groundtorch_blue", SilverSconcePrefabBlue);
			CopyLightSettings("piece_groundtorch_green", SilverSconcePrefabGreen);

			// Add silver sconce to ZNetScene
			MPrefabManager.RegisterToZNetScene(SilverSconcePrefab);
			MPrefabManager.RegisterToZNetScene(SilverSconcePrefabBlue);
			MPrefabManager.RegisterToZNetScene(SilverSconcePrefabGreen);

			// Configure the Piece data and add to buiild menu
			ConfigureSilverSconcePieceData(SilverSconcePrefab, "ElderBark", "Silver", "Resin");
			ConfigureSilverSconcePieceData(SilverSconcePrefabBlue, "ElderBark", "Silver", "GreydwarfEye");
			ConfigureSilverSconcePieceData(SilverSconcePrefabGreen, "ElderBark", "Silver", "Guck");

			// Toggle visibility
			ToggleSilverSconceVisibility();

			SilverSconcePrefab.SetActive(true);
			SilverSconcePrefabBlue.SetActive(true);
			SilverSconcePrefabGreen.SetActive(true);
			MarsarahTweaks.LogInfo("[SilverSconce] Silver Sconces registered and ready.");
		}

		private static GameObject CloneBronzeSconcePrefab(string sourcePrefabName, string newPrefabName)
		{
			GameObject prefab = MPrefabManager.ClonePrefab(sourcePrefabName, newPrefabName);
			if (prefab == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] Cloning of {sourcePrefabName} failed.");
			}

			return prefab;
		}

		private static void SetupSilverSconceDefaults(GameObject prefab, string name, string craftingStation = null)
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
					}
				}
			}

			Fireplace fireplace = prefab.GetComponent<Fireplace>();
			if (fireplace != null)
			{
				fireplace.m_name = name;
			}
		}

		private static void ScaleSilverSconce(GameObject prefab, Vector3 scale)
		{
			// Set the local scale of the prefab's root
			prefab.transform.localScale = scale;
		}

		private static void ModifySilverSconceMaterial(GameObject prefab, string metalPrefabName, string woodPrefabName)
		{
			// Get the mesh renderer of the sconce
			MeshRenderer meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
			if (meshRenderer == null)
			{
				MarsarahTweaks.LogWarn("[SilverSconce] Could not get MeshRenderer component.");
				return;
			}

			// Get the silver necklace prefab
			GameObject metalPrefab = MPrefabManager.GetPrefab(metalPrefabName); // SilverNecklace
			if (metalPrefab == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] Could not find '{metalPrefabName}' prefab.");
				return;
			}

			// Get the material from its MeshRenderer
			MeshRenderer metalRenderer = metalPrefab.GetComponentInChildren<MeshRenderer>();
			if (metalRenderer == null || metalRenderer.sharedMaterial == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] '{metalPrefabName}' has no MeshRenderer or material.");
				return;
			}

			// Get the prefab for the wood part
			GameObject woodPrefab = MPrefabManager.GetPrefab(woodPrefabName); // yggashoot_log, StaffShield
			if (woodPrefab == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] Could not find '{woodPrefabName}' prefab.");
				return;
			}

			// Get the material from its MeshRenderer
			MeshRenderer woodRenderer = woodPrefab.GetComponentInChildren<MeshRenderer>();
			if (woodRenderer == null || woodRenderer.sharedMaterial == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] '{woodPrefabName}' has no MeshRenderer or material.");
				return;
			}

			// Replace the sconce's material
			Material silverMaterial = metalRenderer.sharedMaterial;
			Material woodMaterial = woodRenderer.sharedMaterial;
			Material[] materials = meshRenderer.materials;

			if (materials.Length >= 2)
			{
				materials[0] = woodMaterial;
				materials[1] = silverMaterial;
			}
			else if (materials.Length == 1)
			{
				materials[0] = woodMaterial; // fallback
			}
			meshRenderer.materials = materials;
		}

		private static void ModifyFireplaceFuel(GameObject prefab, string fuelItemName)
		{
			Fireplace fireplace = prefab.GetComponent<Fireplace>();
			if (fireplace == null)
			{
				MarsarahTweaks.LogWarn($"[SilverSconce] No Fireplace component found on {prefab.name}");
				return;
			}

			GameObject fuelItem = MPrefabManager.GetPrefab(fuelItemName);
			if (fuelItem == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] Could not find fuel item '{fuelItemName}' for {prefab.name}");
				return;
			}

			fireplace.m_fuelItem = fuelItem.GetComponent<ItemDrop>();
			MarsarahTweaks.LogInfo($"[SilverSconce] Set fuel for {prefab.name} to {fuelItemName}");
		}

		private static void CopyLightSettings(string sourcePrefabName, GameObject targetPrefab)
		{
			GameObject sourcePrefab = MPrefabManager.GetPrefab(sourcePrefabName);
			if (sourcePrefab == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] Source prefab '{sourcePrefabName}' not found.");
				return;
			}

			Transform sourceEnabled = sourcePrefab.transform.Find("_enabled");
			if (sourceEnabled == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] Could not find '_enabled' child on {sourcePrefabName}.");
				return;
			}

			// Destroy old enabled
			Transform existingEnabled = targetPrefab.transform.Find("_enabled");
			if (existingEnabled != null)
			{
				UnityEngine.Object.DestroyImmediate(existingEnabled.gameObject);
			}

			// Deep clone
			GameObject newEnabled = UnityEngine.Object.Instantiate(sourceEnabled.gameObject, targetPrefab.transform);
			newEnabled.name = "_enabled";
			newEnabled.transform.localPosition = new Vector3(0.11f, 0.5f, 0f);
			newEnabled.transform.localRotation = Quaternion.identity;
			newEnabled.transform.localScale = Vector3.one;

			// Make sure it's visible and rebind it to Fireplace
			newEnabled.SetActive(true);

			Fireplace fp = targetPrefab.GetComponent<Fireplace>();
			if (fp != null)
			{
				fp.m_enabledObject = newEnabled;
				//MarsarahTweaks.LogInfo($"[SilverSconce] Rebound Fireplace.m_enabledObject for {targetPrefab.name}");
			}
			else
			{
				MarsarahTweaks.LogWarn($"[SilverSconce] No Fireplace component found on {targetPrefab.name}");
			}

			//MarsarahTweaks.LogInfo($"[SilverSconce] Replaced '_enabled' from {sourcePrefabName} to {targetPrefab.name}");
		}

		private static void ConfigureSilverSconcePieceData(GameObject prefab, string resourceWood, string resourceMetal, string resourceFuel)
		{
			var pieceConfig = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = "Furniture",
				Requirements = new[]
				{
					new RequirementConfig(resourceWood, 2, recover: true), // ElderBark
					new RequirementConfig(resourceMetal, ConfigManager.BuildPieceAmountsEnabled.Value? 1 : 2, recover: true), // Silver
					new RequirementConfig(resourceFuel, ConfigManager.PermanentLightsEnabled.Value ? 6 : 2, recover: true) // Resin
				}
			};

			MPrefabManager.AddToBuildMenu(prefab, pieceConfig);
		}

		public static void ToggleSilverSconceVisibility()
		{
			Piece silverSconcePiece = SilverSconcePrefab?.GetComponent<Piece>();
			Piece silverSconcePieceBlue = SilverSconcePrefabBlue?.GetComponent<Piece>();
			if (silverSconcePiece == null || silverSconcePieceBlue == null)
			{
				MarsarahTweaks.LogWarn($"[SilverSconce] Piece component does not exist. No toggle made.");
				return;
			}

			if (ConfigManager.SilverSconceEnabled.Value)
			{
				silverSconcePiece.m_enabled = true;
				silverSconcePieceBlue.m_enabled = true;
			}
			else
			{
				silverSconcePiece.m_enabled = false;
				silverSconcePieceBlue.m_enabled = false;
			}
		}

		public static void RefreshSilverSconceRequirements()
		{
			RefreshSilverSconceRequirements(SilverSconcePrefab, "ElderBark", "Silver", "Resin");
			RefreshSilverSconceRequirements(SilverSconcePrefabBlue, "ElderBark", "Silver", "GreydwarfEye");
			RefreshSilverSconceRequirements(SilverSconcePrefabGreen, "ElderBark", "Silver", "Guck");
		}

		private static void RefreshSilverSconceRequirements(GameObject silverSconce, string resourceWood, string resourceMetal, string resourceFuel)
		{
			if (silverSconce == null) return;

			var piece = silverSconce.GetComponent<Piece>();
			if (piece == null) return;

			// Clear & rebuild requirements
			var wood = MPrefabManager.GetPrefab(resourceWood)?.GetComponent<ItemDrop>();
			var metal = MPrefabManager.GetPrefab(resourceMetal)?.GetComponent<ItemDrop>();
			var fuel = MPrefabManager.GetPrefab(resourceFuel)?.GetComponent<ItemDrop>();

			if (wood == null || metal == null || fuel == null)
			{
				MarsarahTweaks.LogError($"[SilverSconce] Missing one or more resource prefabs ({resourceWood}, {resourceMetal}, {resourceFuel}).");
				return;
			}

			piece.m_resources = new[]
			{
				new Piece.Requirement { m_resItem = wood, m_amount = 2, m_recover = true },
				new Piece.Requirement { m_resItem = metal, m_amount = ConfigManager.BuildPieceAmountsEnabled.Value? 1 : 2, m_recover = true },
				new Piece.Requirement { m_resItem = fuel, m_amount = ConfigManager.PermanentLightsEnabled.Value ? 6 : 2, m_recover = true }
			};

			MarsarahTweaks.LogInfo($"[SilverSconce] Refreshed build requirements: Metal={piece.m_resources[1].m_amount}");
			MarsarahTweaks.LogInfo($"[SilverSconce] Refreshed build requirements: Fuel={piece.m_resources[2].m_amount}");
		}
	}
}
