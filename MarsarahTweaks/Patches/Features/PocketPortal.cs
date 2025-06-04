using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using MarsarahTweaks.Managers;
using Splatform;
using System;
using System.Collections;
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
		private static GameObject PocketPortalEffectsPrefab;
		private static GameObject PortalCorePrefab;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || initialized)
					return;

				initialized = true;

				Init();

				//CreatePocketPortal(__instance);
			}
		}

		private static void Init() 
		{
			/*
			* Create Pocket Portal
			* Create Portal Core
			* Create Portal Core Recipe
			*/

			CreatePocketPortal();
		}

		private static void CreatePocketPortal()
		{
			if (ZNetScene.instance.GetPrefab("pocket_portal") != null) return;

			// Clone Pocket Portal
			ClonePocketPortalPrefab();
			if (PocketPortalPrefab == null)	return;

			// Validate pocket portal prefab
			if (!ValidatePocketPortalPrefab()) return;

			// Apply portal-specific data
			SetupPocketPortalDefaults();

			// Register new portal effects
			RegisterPocketPortalEffects();

			// Apply new portal effects
			ApplyPocketPortalVisuals();

			// Add pocket portal to ZNetScene
			MPrefabManager.RegisterToZNetScene(PocketPortalPrefab);

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
			MarsarahTweaks.LogInfo("[PocketPortal] Pocket Portal registered and ready.");
		}

		private static void ClonePocketPortalPrefab()
		{
			PocketPortalPrefab = MPrefabManager.ClonePrefab("portal_wood", "pocket_portal");
			if (PocketPortalPrefab == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Cloning of portal_wood failed.");
			}
		}

		private static bool ValidatePocketPortalPrefab()
		{
			bool pocketPortalValidated = MPrefabManager.ValidatePrefab(PocketPortalPrefab, hasNetView: true, hasTeleport: true, hasPiece: true, hasWearNTear: true);
			if (!pocketPortalValidated)
			{
				MarsarahTweaks.LogError($"[PocketPortal] Prefab validation failed.");
				return false;
			}
			return true;
		}

		private static void SetupPocketPortalDefaults()
		{
			ZNetView znet = PocketPortalPrefab.GetComponent<ZNetView>();
			if (znet != null)
			{
				znet.m_persistent = true;
				znet.m_distant = false;
				znet.m_type = ZDO.ObjectType.Solid;
				znet.m_syncInitialScale = false;
			}

			Piece piece = PocketPortalPrefab.GetComponent<Piece>();
			if (piece != null)
			{
				piece.m_name = "Pocket Portal";
				piece.m_description = "A portal meant to be easy to carry and make exploration more convenient";
				piece.m_craftingStation = null;
			}			
		}

		private static void RegisterPocketPortalEffects()
		{
			PocketPortalEffectsPrefab = MPrefabManager.ClonePrefab("fx_portal_connected", "fx_pocket_portal_connected");
			if (PocketPortalEffectsPrefab == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Failed to clone portal effect prefab!");
				return;
			}

			foreach (var ps in PocketPortalEffectsPrefab.GetComponentsInChildren<ParticleSystem>())
			{
				var main = ps.main;
				main.startColor = new ParticleSystem.MinMaxGradient(
					new Color(0f, 1f, 3f) // Cyan-blue
				);
			}

			var blueFlames = PocketPortalEffectsPrefab.transform.Find("blue flames")?.GetComponent<ParticleSystem>();
			if (blueFlames != null)
			{
				var renderer = blueFlames.GetComponent<ParticleSystemRenderer>();
				if (renderer != null && renderer.material != null)
				{
					renderer.material.color = new Color(0f, 1f, 3f);
				}
			}
			else
			{
				MarsarahTweaks.LogWarn("[PocketPortal] Could not find 'blue flames' particle system.");
			}

			MPrefabManager.RegisterToZNetScene(PocketPortalEffectsPrefab);
		}

		private static void ApplyPocketPortalVisuals()
		{
			GameObject vanillaUnusedPortalPrefab = ZNetScene.instance.GetPrefab("portal");
			if (vanillaUnusedPortalPrefab == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Unused portal prefab not found");
				return;
			}

			// Destroy old target_found effect from the pocket portal
			Transform oldEffect = PocketPortalPrefab.transform.Find("_target_found_red");
			if (oldEffect != null)
			{
				UnityEngine.Object.DestroyImmediate(oldEffect.gameObject);
			}

			// Add new effect taken from the unused portal
			Transform newEffect = vanillaUnusedPortalPrefab.transform.Find("_target_found");
			if (newEffect != null)
			{
				// Create the new effect
				GameObject effectPocketPortal = UnityEngine.Object.Instantiate(newEffect.gameObject, PocketPortalPrefab.transform);
				effectPocketPortal.name = "_target_found_blue";

				// Change its position relative to the portal
				Vector3 newPosition = effectPocketPortal.transform.localPosition;
				newPosition.y -= 0.3f;
				effectPocketPortal.transform.localPosition = newPosition;

				// Apply new effect to the TeleportWorld component
				TeleportWorld tp = PocketPortalPrefab.GetComponent<TeleportWorld>();
				if (tp != null)
				{
					tp.m_target_found = effectPocketPortal.GetComponent<EffectFade>();
					tp.m_colorTargetfound = new Color(1f, 4f, 6f, 1f); // glowing cyan-blue

					if (PocketPortalEffectsPrefab == null)
					{
						MarsarahTweaks.LogError("[PocketPortal] Custom effect prefab not found.");
						return;
					}

					tp.m_connected = new EffectList
					{
						m_effectPrefabs = new[]
						{
							new EffectData
							{
								m_prefab = PocketPortalEffectsPrefab,
								m_enabled = true
							}
						}
					};
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
		}

		/*private static void CreatePocketPortal(ZNetScene znetScene)
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

					ReplaceConnectedEffect(tp); // This needs the new effect added to prefabs before used
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
			piece.m_description = "A portal meant to be easy to carry and make exploration more convenient";
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

			// Hide from build menu if disabled
			if (!ConfigManager.PocketPortalEnabled.Value)
			{
				PocketPortalPrefab.GetComponent<Piece>().m_enabled = false;
				MarsarahTweaks.LogInfo("[PocketPortal] 🔒 Portal hidden from build menu due to config.");
			}

			PocketPortalPrefab.SetActive(true);
			MarsarahTweaks.LogInfo("[PocketPortal] ✅ Pocket Portal registered and ready.");
		}*/

		// Swap the effectlist of the portal
		/*private static void ReplaceConnectedEffect(TeleportWorld tp)
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
		}*/

		// Clone and register the fx_portal_connected effect
		/*private static void ModifyPortalEffects()
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
		}*/

		// Clone and register the Portal Core
		/*private static void CreatePortalCore()
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

			// Find the core child object (SurtlingCore -> attach -> core)
			Transform coreTransform = PortalCorePrefab.transform.Find("attach/core");
			if (coreTransform != null)
			{
				Renderer coreRenderer = coreTransform.GetComponent<Renderer>();
				if (coreRenderer != null)
				{
					// Create a new material instance to avoid affecting other objects
					Material coreMaterial = new Material(coreRenderer.sharedMaterial);

					// Enable emission and set color (bright cyan-blue)
					coreMaterial.EnableKeyword("_EMISSION");					
					coreMaterial.SetColor("_EmissionColor", new Color(0f, 1f, 5f) * 3f); // HDR intensity

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
					pointLight.intensity = 2.5f; // Brightness multiplier
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
			Sprite originalIcon = PrefabManager.Instance.GetPrefab("SurtlingCore").GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
			
			Rect rect = new Rect(360, 1340, 64, 64);

			// Copy only the icon's sub-region from the large texture
			Texture2D croppedTex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
			RenderTexture rt = RenderTexture.GetTemporary(originalIcon.texture.width, originalIcon.texture.height, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(originalIcon.texture, rt);
			RenderTexture.active = rt;

			croppedTex.ReadPixels(
				new Rect(rect.x, rect.y, rect.width, rect.height),
				0, 0
			);
			croppedTex.Apply();

			Color[] pixels = croppedTex.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				Color c = pixels[i];

				// Detect red-glow pixels
				if (c.r > 0.5f && c.r > c.g + 0.1f && c.r > c.b + 0.1f)
				{
					// Convert red glow to cyan
					pixels[i] = new Color(0.0f, c.r * 0.8f, c.r * 1.1f, c.a); // From red -> cyan (G & B dominate)
				}
				else
				{
					// Leave other pixels mostly intact
					pixels[i] = c;
				}
			}
			croppedTex.SetPixels(pixels);
			croppedTex.Apply();

			RenderTexture.ReleaseTemporary(rt);
			RenderTexture.active = null;

			// Create new Sprite from cropped region
			Sprite newIcon = Sprite.Create(
				croppedTex,
				new Rect(0, 0, croppedTex.width, croppedTex.height),
				new Vector2(0.5f, 0.5f),
				originalIcon.pixelsPerUnit
			);

			itemDrop.m_itemData.m_shared.m_icons = new Sprite[] { newIcon };
			itemDrop.m_itemData.m_shared.m_name = "Portal Core";
			itemDrop.m_itemData.m_shared.m_description = "The core of an easy to carry portal";
			itemDrop.m_itemData.m_shared.m_maxStackSize = 1;
			itemDrop.m_itemData.m_shared.m_weight = 10f;

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
		}*/

		// Create the Portal Core recipe
		/*private static void CreatePortalCoreRecipe()
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

			if (!ConfigManager.PocketPortalEnabled.Value)
			{
				customRecipe.Recipe.m_enabled = false;
				MarsarahTweaks.LogInfo("[PortalCore] 🔒 Recipe hidden from workbench due to config.");
			}

			ItemManager.Instance.AddRecipe(customRecipe);
		}*/

		// Adds the pocket_portal prefab to the list of known portals
		/*[HarmonyPatch(typeof(Game), nameof(Game.ConnectPortals))]
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
		}*/

		// Limit Pocket Portals per player
		/*[HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
		public static class PocketPortal_LimitPlacement
		{
			static bool Prefix(Player __instance, Piece piece, ref bool __result)
			{
				if (piece.name == PocketPortalPrefab.name && PlayerHasPocketPortal())
				{
					__instance.Message(MessageHud.MessageType.Center, "You can only place one Pocket Portal.");
					__result = false; // Prevent further execution
					return false;     // Skip original method
				}

				return true; // Let placement continue normally
			}
		}

		public static bool PlayerHasPocketPortal()
		{
			if (ZNet.instance == null || ZDOMan.instance == null || Player.m_localPlayer == null)
				return false;

			string localPlayerName = Player.m_localPlayer.GetPlayerName();
			int pocketPortalHash = PocketPortalPrefab.name.GetStableHashCode();

			var zdoDictField = typeof(ZDOMan).GetField("m_objectsByID", BindingFlags.NonPublic | BindingFlags.Instance);
			if (zdoDictField == null)
				return false;

			Dictionary<ZDOID, ZDO> zdoDict = zdoDictField.GetValue(ZDOMan.instance) as Dictionary<ZDOID, ZDO>;
			if (zdoDict == null)
				return false;

			foreach (var zdo in zdoDict.Values)
			{
				if (zdo == null) continue;
				if (zdo.GetPrefab() != pocketPortalHash) continue;
				if (zdo.GetString(ZDOVars.s_creatorName) == localPlayerName)
					return true;
			}

			return false;
		}*/

		/*[HarmonyPatch(typeof(Player), "Update")]
		public class Player_Update_DebugPortalCount
		{
			static void Postfix(Player __instance)
			{
				if (__instance != Player.m_localPlayer) return;

				if (Input.GetKeyDown(KeyCode.F8))
				{
					LogPocketPortalCount();
				}
			}
		}

		public static void LogPocketPortalCount()
		{
			if (ZNet.instance == null || ZDOMan.instance == null || Player.m_localPlayer == null)
			{
				MarsarahTweaks.LogWarn("Required instances are missing.");
				return;
			}

			//long localPlayerUID = ZNet.GetUID();
			string localPlayerName = Player.m_localPlayer.GetPlayerName();
			//int pocketPortalHash = 853122569;
			int pocketPortalHash = PocketPortalPrefab.name.GetStableHashCode();

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

			int count = 0;
			foreach (var zdo in zdoDict.Values)
			{
				if (zdo == null)
					continue;

				if (zdo.GetPrefab() != pocketPortalHash)
					continue;

				if (zdo.GetString(ZDOVars.s_creatorName) == localPlayerName)
					count++;
			}

			MarsarahTweaks.LogInfo($"Pocket Portals built by this player: {count}");
		}*/
	}
}
