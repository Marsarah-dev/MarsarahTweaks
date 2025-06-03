using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Splatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static EffectList;
using static UnityEngine.UI.Image;
using static ZDOExtraData;

namespace MarsarahTweaks.Patches.Features
{
	public static class PocketPortal
	{
		private static bool initialized = false;
		private static GameObject PocketPortalPrefab;
		private static GameObject PortalCorePrefab;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || initialized)
					return;

				initialized = true;
				CreatePocketPortal(__instance);
			}
		}

		private static void CreatePocketPortal(ZNetScene znetScene)
		{
			ModifyPortalEffects();
			CreatePortalCore();
			CreatePortalCoreRecipe();

			if (PrefabManager.Instance.GetPrefab("pocket_portal") != null)
			{
				return;
			}

			GameObject vanillaUnusedPortalPrefab = PrefabManager.Instance.GetPrefab("portal");
			if (vanillaUnusedPortalPrefab == null)
			{
				MarsarahTweaks.LogError("Original portal not found");
				return;
			}

			GameObject vanillaPortalPrefab = PrefabManager.Instance.GetPrefab("portal_wood");
			if (vanillaPortalPrefab == null)
			{
				MarsarahTweaks.LogError("Original portal_wood not found");
				return;
			}

			vanillaPortalPrefab.SetActive(false);
			PocketPortalPrefab = UnityEngine.Object.Instantiate(vanillaPortalPrefab);
			vanillaPortalPrefab.SetActive(true);

			PocketPortalPrefab.name = "pocket_portal";

			ZNetView znet = PocketPortalPrefab.GetComponent<ZNetView>() ?? PocketPortalPrefab.AddComponent<ZNetView>();
			znet.m_persistent = true;
			znet.m_distant = false;
			znet.m_type = ZDO.ObjectType.Solid;
			znet.m_syncInitialScale = false;

			TeleportWorld tp = PocketPortalPrefab.GetComponent<TeleportWorld>();

			// Replace red _target_found with blue one from "portal"
			Transform oldEffect = PocketPortalPrefab.transform.Find("_target_found_red");
			if (oldEffect != null)
			{
				UnityEngine.Object.DestroyImmediate(oldEffect.gameObject);
				MarsarahTweaks.LogInfo("[PocketPortal] 🔇 Destroyed _target_found_red on clone portal.");
			}

			Transform newEffect = vanillaUnusedPortalPrefab.transform.Find("_target_found");
			if (newEffect != null)
			{
				GameObject effectPocketPortal = UnityEngine.Object.Instantiate(newEffect.gameObject, PocketPortalPrefab.transform);
				effectPocketPortal.name = "_target_found";

				Vector3 newPosition = effectPocketPortal.transform.localPosition;
				newPosition.y -= 0.3f; // Negative Y value moves it downward
				effectPocketPortal.transform.localPosition = newPosition;

				if (tp != null)
				{
					tp.m_target_found = effectPocketPortal.GetComponent<EffectFade>();
					tp.m_colorTargetfound = new Color(1f, 4f, 6f, 1f); // glowing cyan-blue
					MarsarahTweaks.LogInfo("[PocketPortal] 🔄 Replaced target_found_red with target_found and applied custom color.");

					ReplaceConnectedEffect(tp);
				}
				else
				{
					MarsarahTweaks.LogWarn("[PocketPortal] TeleportWorld not found while assigning new target_found VFX.");
				}
			}
			else
			{
				MarsarahTweaks.LogWarn("[PocketPortal] Could not find _target_found in vanilla portal.");
			}

			Piece piece = PocketPortalPrefab.GetComponent<Piece>();
			piece.m_name = "Pocket Portal";
			piece.m_description = "A custom pocket portal";
			piece.m_craftingStation = null;

			var customPrefab = new CustomPrefab(PocketPortalPrefab, fixReference: true);
			PrefabManager.Instance.AddPrefab(customPrefab);
			PrefabManager.Instance.RegisterToZNetScene(PocketPortalPrefab);

			if (!znetScene.m_prefabs.Contains(PocketPortalPrefab))
			{
				MarsarahTweaks.LogInfo("[PocketPortal] Adding clone to ZNetScenePrefabs manually.");
				znetScene.m_prefabs.Add(PocketPortalPrefab);
			}

			var pieceConfig = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = "Misc",
				Requirements = new[]
				{
					new RequirementConfig("PortalCore", 1),
				}
			};

			var customPiece = new CustomPiece(PocketPortalPrefab, fixReference: true, pieceConfig);
			PieceManager.Instance.AddPiece(customPiece);

			PocketPortalPrefab.SetActive(true);
			MarsarahTweaks.LogInfo("[PocketPortal] ✅ Pocket Portal registered and ready.");
		}

		// Clone and register the fx_portal_connected effect (unchanged)
		private static void ModifyPortalEffects()
		{
			GameObject originalFx = PrefabManager.Instance.GetPrefab("fx_portal_connected");
			if (originalFx == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Could not find fx_portal_connected prefab!");
				return;
			}

			GameObject fxClone = PrefabManager.Instance.CreateClonedPrefab("fx_pocket_portal_connected", originalFx);

			if (fxClone == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Failed to clone effect prefab!");
				return;
			}

			foreach (var ps in fxClone.GetComponentsInChildren<ParticleSystem>())
			{
				var main = ps.main;
				main.startColor = new ParticleSystem.MinMaxGradient(
					new Color(0f, 1f, 3f) // Cyan-blue
				);
				MarsarahTweaks.LogInfo($"[PocketPortal] Modified particle system: {ps.name}");
			}

			var blueFlames = fxClone.transform.Find("blue flames")?.GetComponent<ParticleSystem>();
			if (blueFlames != null)
			{
				var renderer = blueFlames.GetComponent<ParticleSystemRenderer>();
				if (renderer != null && renderer.material != null)
				{
					renderer.material.color = new Color(0f, 1f, 3f); // Apply tint directly
					MarsarahTweaks.LogInfo("[PocketPortal] Set material color on 'blue flames' renderer.");
				}
			}
			else
			{
				MarsarahTweaks.LogWarn("[PocketPortal] Could not find 'blue flames' particle system.");
			}

			var customFx = new CustomPrefab(fxClone, fixReference: true);
			PrefabManager.Instance.AddPrefab(customFx);
			PrefabManager.Instance.RegisterToZNetScene(fxClone);

			if (ZNetScene.instance.GetPrefab("fx_pocket_portal_connected") == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] ❌ Failed to register effect prefab!");
			}
			else
			{
				MarsarahTweaks.LogInfo("[PocketPortal] ✅ Successfully registered effect prefab");
			}
		}

		// Swap the effectlist of the portal
		private static void ReplaceConnectedEffect(TeleportWorld tp)
		{
			if (tp == null)
			{
				MarsarahTweaks.LogWarn("[PocketPortal] ReplaceConnectedEffect: TeleportWorld was null.");
				return;
			}

			GameObject customFx = PrefabManager.Instance.GetPrefab("fx_pocket_portal_connected");
			if (customFx == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] ReplaceConnectedEffect: Custom effect prefab not found.");
				return;
			}

			tp.m_connected = new EffectList
			{
				m_effectPrefabs = new[]
				{
					new EffectData
					{
						m_prefab = customFx,
						m_enabled = true
					}
				}
			};

			MarsarahTweaks.LogInfo("[PocketPortal] 🎨 Custom portal connect effect assigned.");
		}

		// Clone and register the Portal Core
		private static void CreatePortalCore()
		{
			GameObject surtlingCore = PrefabManager.Instance.GetPrefab("SurtlingCore");
			if (surtlingCore == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Could not find SurtlingCore prefab!");
				return;
			}

			PortalCorePrefab = PrefabManager.Instance.CreateClonedPrefab("PortalCore", surtlingCore);

			if (PortalCorePrefab == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Failed to clone SurtlingCore prefab!");
				return;
			}

			/*var particleSystem = PortalCorePrefab.GetComponentInChildren<ParticleSystem>();
			if (particleSystem != null)
			{
				var main = particleSystem.main;
				main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.5f, 0.75f, 1f)); // Light blue
			}
			else
			{
				MarsarahTweaks.LogWarn("[PortalCore] ParticleSystem not found to modify startColor");
			}*/

			// Find the core child object (SurtlingCore -> attach -> core)
			Transform coreTransform = PortalCorePrefab.transform.Find("attach/core");
			if (coreTransform != null)
			{
				Renderer coreRenderer = coreTransform.GetComponent<Renderer>();
				if (coreRenderer != null)
				{
					//MarsarahTweaks.LogInfo($"Core found: {coreTransform != null}, Renderer: {coreRenderer != null}");
					//MarsarahTweaks.LogInfo($"R: {coreMaterial.GetColor("_EmissionColor").r} - G: {coreMaterial.GetColor("_EmissionColor").g} - B: {coreMaterial.GetColor("_EmissionColor").b} - A: {coreMaterial.GetColor("_EmissionColor").a}");
					// R: 1.429 - G: 0.8574001 - B: 0 - A: 1

					// Create a new material instance to avoid affecting other objects
					Material coreMaterial = new Material(coreRenderer.sharedMaterial);

					//coreMaterial.SetColor("_Color", new Color(0.1f, 0.3f, 1f)); // Deep blue base

					// Enable emission and set color (bright cyan-blue)
					coreMaterial.EnableKeyword("_EMISSION");					
					coreMaterial.SetColor("_EmissionColor", new Color(0f, 1f, 5f) * 3f); // HDR intensity

					// 3. Force glow intensity (Valheim-specific)
					//coreMaterial.SetFloat("_Glow", 1f); // Full glow intensity
					//coreMaterial.SetFloat("_GlowStrength", 3f); // Additional boost

					// Apply the material
					coreRenderer.sharedMaterial = coreMaterial;

					MarsarahTweaks.LogInfo("[PortalCore] Modified core emission color");
				}
				else
				{
					MarsarahTweaks.LogWarn("[PortalCore] No Renderer found on core object");
				}
			}
			else
			{
				MarsarahTweaks.LogWarn("[PortalCore] 'attach/core' child not found");
			}

			// Find the Point Light child
			Transform pointLightTransform = PortalCorePrefab.transform.Find("attach/Point light");
			if (pointLightTransform != null)
			{
				Light pointLight = pointLightTransform.GetComponent<Light>();
				if (pointLight != null)
				{
					// Set color (cyan-blue) and intensity
					pointLight.color = new Color(0f, 0.5f, 1f); // RGB (0-1)
					pointLight.intensity = 3f; // Brightness multiplier
					pointLight.range = 3f; // Light radius

					MarsarahTweaks.LogInfo("[PortalCore] Modified Point Light color");
				}
				else
				{
					MarsarahTweaks.LogWarn("[PortalCore] No Light component found on Point Light");
				}
			}
			else
			{
				MarsarahTweaks.LogWarn("[PortalCore] 'attach/Point Light' child not found");
			}

			ItemDrop itemDrop = PortalCorePrefab.GetComponent<ItemDrop>();
			itemDrop.m_itemData.m_shared.m_name = "Portal Core";
			itemDrop.m_itemData.m_shared.m_description = "The core of an easy to carry portal";
			itemDrop.m_itemData.m_shared.m_maxStackSize = 1;
			itemDrop.m_itemData.m_shared.m_weight = 10f;
			//itemDrop.m_itemData.m_shared.m_icons = new[] { /* assign your custom sprite here */ };

			var customPortalCore = new CustomPrefab(PortalCorePrefab, fixReference: true);
			PrefabManager.Instance.AddPrefab(customPortalCore);
			PrefabManager.Instance.RegisterToZNetScene(PortalCorePrefab);

			var customPortalcoreItem = new CustomItem(PortalCorePrefab, fixReference: true);
			ItemManager.Instance.AddItem(customPortalcoreItem);

			if (ZNetScene.instance.GetPrefab("PortalCore") == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] ❌ Failed to register PortalCore prefab!");
			}
			else
			{
				MarsarahTweaks.LogInfo("[PocketPortal] ✅ Successfully registered PortalCore prefab");
			}
		}

		// Create the Portal Core recipe
		private static void CreatePortalCoreRecipe()
		{
			var recipeConfig = new RecipeConfig
			{
				Item = "PortalCore",
				Amount = 1,
				CraftingStation = "piece_workbench",
				Requirements = new[]
				{
					new RequirementConfig("SurtlingCore", 5),
					new RequirementConfig("FineWood", 20),
					new RequirementConfig("GreydwarfEye", 20),
					new RequirementConfig("Wood", 20)
				}
			};

			var customRecipe = new CustomRecipe(recipeConfig);
			ItemManager.Instance.AddRecipe(customRecipe);
		}

		// Adds the pocket_portal prefab to the list of known portals
		[HarmonyPatch(typeof(Game), nameof(Game.ConnectPortals))]
		public static class Game_ConnectPortals_Patch
		{
			static void Prefix(Game __instance)
			{
				GameObject portal = PocketPortalPrefab;
				if (portal == null)
				{
					MarsarahTweaks.LogWarn("[PocketPortal] ConnectPortals patch: prefab not ready yet.");
					return;
				}

				if (!__instance.m_portalPrefabs.Contains(portal))
				{
					__instance.m_portalPrefabs.Add(portal);
					__instance.PortalPrefabHash.Add("pocket_portal".GetStableHashCode());
					MarsarahTweaks.LogInfo("[PocketPortal] ✅ Registered 'pocket_portal' in Game.m_portalPrefabs via ConnectPortals.");
				}
			}
		}

		// I need this patch to rezolve the issue with PlatformUserID parsing
		[HarmonyPatch(typeof(TeleportWorld), "Awake")]
		public static class TeleportWorld_Awake_AuthorFix
		{
			static void Postfix(TeleportWorld __instance)
			{
				if (!__instance.name.Contains("pocket_portal")) return;

				var nview = __instance.GetComponent<ZNetView>();
				if (nview == null || !nview.IsValid()) return;

				var zdo = nview.GetZDO();
				if (zdo == null) return;

				// Only set author if it's missing
				string author = zdo.GetString(ZDOVars.s_tagauthor);
				if (string.IsNullOrEmpty(author))
				{
					string authorId = PlatformManager.DistributionPlatform?.LocalUser?.PlatformUserID.ToString() ?? "";
					zdo.Set(ZDOVars.s_tagauthor, authorId);

					MarsarahTweaks.LogInfo($"[PocketPortal] Auto-assigned missing tagauthor: {authorId}");
				}
			}
		}
	}
}
