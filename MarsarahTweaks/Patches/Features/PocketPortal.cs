using HarmonyLib;
using Jotunn;
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
		private static Recipe PortalCoreRecipe;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || initialized)
					return;

				initialized = true;

				Init();
			}
		}

		private static void Init() 
		{
			CreatePocketPortal();
			CreatePortalCore();
		}

		private static void CreatePocketPortal()
		{
			if (MPrefabManager.GetPrefab("pocket_portal") != null) return;

			// Clone Pocket Portal
			ClonePocketPortalPrefab();
			if (PocketPortalPrefab == null)	return;

			// Apply portal-specific data
			SetupPocketPortalDefaults();

			// Register new portal effects
			GameObject pocketPortalEffectsPrefab = RegisterPocketPortalEffects();

			// Apply new portal effects
			ApplyPocketPortalVisuals(pocketPortalEffectsPrefab);

			// Add pocket portal to ZNetScene
			MPrefabManager.RegisterToZNetScene(PocketPortalPrefab);

			// Configure the Piece data and add to buiild menu
			ConfigurePocketPortalPieceData();			

			PocketPortalPrefab.SetActive(true);
			//MarsarahTweaks.LogInfo("[PocketPortal] Pocket Portal registered and ready.");
		}

		private static void ClonePocketPortalPrefab()
		{
			PocketPortalPrefab = MPrefabManager.ClonePrefab("portal_wood", "pocket_portal");
			if (PocketPortalPrefab == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Cloning of portal_wood failed.");
			}
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
				piece.m_enabled = true;
				piece.m_name = "Pocket Portal";
				piece.m_description = "A portal meant to be easy to carry and make exploration more convenient";
				piece.m_craftingStation = null;
			}
		}

		private static void ConfigurePocketPortalPieceData()
		{
			var pieceConfig = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = "Misc",
				Requirements = new[]
				{
					new RequirementConfig("PortalCore", 1, recover: true)
				}
			};

			MPrefabManager.AddToBuildMenu(PocketPortalPrefab, pieceConfig);
			TogglePocketPortalVisibility();
		}

		private static GameObject RegisterPocketPortalEffects()
		{
			GameObject pocketPortalEffectsPrefab = MPrefabManager.ClonePrefab("fx_portal_connected", "fx_pocket_portal_connected");
			if (pocketPortalEffectsPrefab == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Failed to clone portal effect prefab!");
				return null;
			}

			foreach (var ps in pocketPortalEffectsPrefab.GetComponentsInChildren<ParticleSystem>())
			{
				var main = ps.main;
				main.startColor = new ParticleSystem.MinMaxGradient(
					new Color(0f, 1f, 3f) // Cyan-blue
				);
			}

			var blueFlames = pocketPortalEffectsPrefab.transform.Find("blue flames")?.GetComponent<ParticleSystem>();
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

			MPrefabManager.RegisterToZNetScene(pocketPortalEffectsPrefab);

			return pocketPortalEffectsPrefab;
		}

		private static void ApplyPocketPortalVisuals(GameObject pocketPortalEffectsPrefab)
		{
			GameObject vanillaUnusedPortalPrefab = MPrefabManager.GetPrefab("portal");
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

					if (pocketPortalEffectsPrefab == null)
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
								m_prefab = pocketPortalEffectsPrefab,
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

		private static void CreatePortalCore()
		{
			// Clone the Portal Core
			GameObject portalCorePrefab = ClonePortalCorePrefab();
			if (portalCorePrefab == null) return;

			// Modify colors
			ModifyPortalCoreColors(portalCorePrefab);

			// Modify icon
			ItemDrop portalCoreItemDrop = portalCorePrefab.GetComponent<ItemDrop>();
			Sprite portalCoreNewIcon = ModifyPortalCoreIcon(portalCoreItemDrop);

			// Configure Portal Core data
			portalCoreItemDrop.m_itemData.m_shared.m_icons = new Sprite[] { portalCoreNewIcon };
			portalCoreItemDrop.m_itemData.m_shared.m_name = "Portal Core";
			portalCoreItemDrop.m_itemData.m_shared.m_description = "The core of an easy to carry portal";
			portalCoreItemDrop.m_itemData.m_shared.m_maxStackSize = 1;
			portalCoreItemDrop.m_itemData.m_shared.m_weight = 10f;

			// Add Portal Core to ZnetScene
			MPrefabManager.RegisterToZNetScene(portalCorePrefab);

			// Add Portal Core to Items
			MPrefabManager.RegisterItem(portalCorePrefab);

			CreatePortalCoreRecipe();
		}

		private static GameObject ClonePortalCorePrefab()
		{
			GameObject portalCorePrefab = MPrefabManager.ClonePrefab("SurtlingCore", "PortalCore");
			if (portalCorePrefab == null)
			{
				MarsarahTweaks.LogError("[PortalCore] Cloning of SurtlingCore failed.");
				return null;
			}

			return portalCorePrefab;
		}

		private static void ModifyPortalCoreColors(GameObject portalCorePrefab)
		{
			// Find the core child object (SurtlingCore -> attach -> core)
			Transform coreTransform = portalCorePrefab.transform.Find("attach/core");
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
				}
				else
				{
					MarsarahTweaks.LogWarn("[PortalCore] No Renderer found on 'attach/core' object");
				}
			}
			else
			{
				MarsarahTweaks.LogWarn("[PortalCore] 'attach/core' child not found");
			}

			// Find the Point Light child (SurtlingCore -> attach -> Point Light)
			Transform pointLightTransform = portalCorePrefab.transform.Find("attach/Point light");
			if (pointLightTransform != null)
			{
				Light pointLight = pointLightTransform.GetComponent<Light>();
				if (pointLight != null)
				{
					// Set color (cyan-blue) and intensity
					pointLight.color = new Color(0f, 0.5f, 1f); // RGB (0-1)
					pointLight.intensity = 2.5f; // Brightness multiplier
					pointLight.range = 3f; // Light radius
				}
				else
				{
					MarsarahTweaks.LogWarn("[PortalCore] No Light component found on 'attach/Point Light'");
				}
			}
			else
			{
				MarsarahTweaks.LogWarn("[PortalCore] 'attach/Point Light' child not found");
			}
		}

		private static Sprite ModifyPortalCoreIcon(ItemDrop portalCoreItemDrop)
		{
			Sprite originalIcon = MPrefabManager.GetPrefab("SurtlingCore").GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0];
			if (originalIcon == null)
			{
				MarsarahTweaks.LogWarn($"[PortalCore] Could not get icon for Surtling Core");
				return null;
			}
			if (originalIcon.texture == null)
			{
				MarsarahTweaks.LogWarn("[PortalCore] Original icon texture is null.");
				return null;
			}

			Rect rect = new Rect(360, 1340, 64, 64); // This is the location of the SurtlingCore icon in the game's atlas

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

			return newIcon;
		}

		private static void CreatePortalCoreRecipe()
		{
			RecipeConfig recipeConfig = new RecipeConfig
			{
				Item = "PortalCore",
				Amount = 1,
				CraftingStation = "Workbench",
				MinStationLevel = 4,
				Requirements = new[]
				{
					new RequirementConfig("SurtlingCore", 5),
					new RequirementConfig("FineWood", 20),
					new RequirementConfig("FreezeGland", 5),
					new RequirementConfig("Obsidian", 20)
				}
			};

			PortalCoreRecipe = MPrefabManager.RegisterRecipe(recipeConfig);
			TogglePortalCoreVisibility();
		}

		public static void TogglePocketPortalVisibility()
		{
			Piece pocketPortalPiece = PocketPortalPrefab.GetComponent<Piece>();
			if (pocketPortalPiece == null)
			{
				MarsarahTweaks.LogWarn($"[PocketPortal] Piece component does not exist. No toggle made.");
				return;
			}

			if (ConfigManager.PocketPortalEnabled.Value)
			{
				pocketPortalPiece.m_enabled = true;
			}
			else
			{
				pocketPortalPiece.m_enabled = false;
			}
		}

		public static void TogglePortalCoreVisibility()
		{
			if (PortalCoreRecipe == null)
			{
				MarsarahTweaks.LogWarn($"[PortalCore] Recipe does not exist. No toggle made.");
				return;
			}

			if (ConfigManager.PocketPortalEnabled.Value)
			{
				PortalCoreRecipe.m_enabled = true;
			}
			else
			{
				PortalCoreRecipe.m_enabled = false;
			}
		}

		// Adds the pocket_portal prefab early to be picked up by ZDOMan
		[HarmonyPatch(typeof(Game), "Awake")]
		public static class EarlyPortalPrefabRegister
		{
			static void Prefix(Game __instance)
			{
				int hash = "pocket_portal".GetStableHashCode();
				if (!__instance.PortalPrefabHash.Contains(hash))
				{
					__instance.PortalPrefabHash.Add(hash);
					MarsarahTweaks.LogInfo($"Registered pocket_portal prefab hash early: {hash}");
				}
			}
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
					//MarsarahTweaks.LogWarn("[PocketPortal] ConnectPortals patch: prefab not ready yet.");
					return;
				}

				if (!__instance.m_portalPrefabs.Contains(portal))
				{
					__instance.m_portalPrefabs.Add(portal);
					__instance.PortalPrefabHash.Add("pocket_portal".GetStableHashCode());
					//MarsarahTweaks.LogInfo("[PocketPortal] Registered 'pocket_portal' in Game.m_portalPrefabs via ConnectPortals.");
				}
			}
		}

		// I need this patch to rezolve the issue with PlatformUserID parsing
		[HarmonyPatch(typeof(TeleportWorld), "Awake")]
		public static class TeleportWorld_Awake_AuthorFix
		{
			static void Postfix(TeleportWorld __instance)
			{
				//if (!__instance.name.Contains("pocket_portal")) return;

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

					//MarsarahTweaks.LogInfo($"[PocketPortal] Auto-assigned missing tagauthor: {authorId}");
				}
			}
		}

		// Limit Pocket Portals per player
		[HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
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

		private static bool PlayerHasPocketPortal()
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
		}

		[HarmonyPatch(typeof(Tutorial), "Awake")]
		public static class Tutorial_Awake_Patch
		{
			static void Postfix(Tutorial __instance)
			{
				if (__instance == null)	return;

				AddCustomTutorial();
			}
		}

		private static void AddCustomTutorial()
		{
			if (Tutorial.instance == null) return;

			var customTutorial = new Tutorial.TutorialText
			{
				m_name = "portal_core_intro",
				m_topic = "Pocket Portal",
				m_label = "Portal Core",
				m_text = "You've crafted a <color=yellow>Portal Core</color>.\nUse it to build a <color=yellow>Pocket Portal</color> which should make your exploration journeys easier.\nBut keep in mind that only one of these portals can be built in the world.\nHowever, you can always destroy it and place it somewhere else as needed.",
				m_isMunin = false
			};

			Tutorial.instance.m_texts.Add(customTutorial);

			//MarsarahTweaks.LogInfo("Added custom tutorial text.");
		}

		[HarmonyPatch(typeof(Player), "OnInventoryChanged")]
		public static class Player_OnInventoryChanged_Patch
		{
			static void Postfix(Player __instance)
			{
				if (__instance == null || !__instance.IsOwner()) return;
				if (__instance.HaveSeenTutorial("portal_core_intro")) return;

				foreach (var item in __instance.GetInventory().GetAllItems())
				{
					if (item.m_shared.m_name == "Portal Core")
					{
						__instance.ShowTutorial("portal_core_intro");
						//MarsarahTweaks.LogInfo("Triggered Portal Core tutorial.");
						break;
					}
				}
			}
		}
	}
}
