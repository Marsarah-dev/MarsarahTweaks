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
		//private static GameObject CustomFxPrefab;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null || initialized)
					return;

				initialized = true;
				RegisterPocketPortal(__instance);
			}
		}

		private static void RegisterPocketPortal(ZNetScene znetScene)
		{
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

			/*if (!PocketPortalPrefab.GetComponent<WearNTear>()) PocketPortalPrefab.AddComponent<WearNTear>();
			if (!PocketPortalPrefab.GetComponent<Piece>()) PocketPortalPrefab.AddComponent<Piece>();
			if (!PocketPortalPrefab.GetComponent<TeleportWorld>()) PocketPortalPrefab.AddComponent<TeleportWorld>();*/
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

				if (tp != null)
				{
					tp.m_target_found = effectPocketPortal.GetComponent<EffectFade>();
					tp.m_colorTargetfound = new Color(1f, 4f, 6f, 1f); // glowing cyan-blue
					MarsarahTweaks.LogInfo("[PocketPortal] 🔄 Replaced target_found_red with target_found and applied custom color.");

					tp.m_connected = vanillaPortalPrefab.GetComponent<TeleportWorld>().m_connected;
					//tp.m_connected = new EffectList(); // disables red flash
					MarsarahTweaks.LogInfo("[PocketPortal] 🔇 Disabled fx_portal_connected on link.");
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

			// Change effects list
			/*GameObject originalFx = PrefabManager.Instance.GetPrefab("fx_portal_connected");
			if (originalFx == null)
			{
				MarsarahTweaks.LogError("[PocketPortal] Could not find fx_portal_connected prefab!");
				return;
			}

			//originalFx.SetActive(false);
			CustomFxPrefab = UnityEngine.Object.Instantiate(originalFx);
			//originalFx.SetActive(true);
			CustomFxPrefab.name = "fx_pocket_portal_connected";
			CustomFxPrefab.SetActive(false);

			// Modify particle system color
			foreach (var ps in CustomFxPrefab.GetComponentsInChildren<ParticleSystem>())
			{
				var main = ps.main;
				main.startColor = new ParticleSystem.MinMaxGradient(new Color(0f, 1f, 3f)); // cyan-ish
			}

			UnityEngine.Object.DontDestroyOnLoad(CustomFxPrefab);

			CustomFxPrefab.SetActive(true);

			// Add directly to ZNetScene
			PrefabManager.Instance.RegisterToZNetScene(CustomFxPrefab);

			List<GameObject> prefabs = ZNetScene.instance.m_prefabs;
			if (!prefabs.Contains(CustomFxPrefab))
			{
				prefabs.Add(CustomFxPrefab);
				MarsarahTweaks.LogInfo("[PocketPortal] Added fx_pocket_portal_connected to ZNetScene.m_prefabs manually");
			}

			var namedPrefabsField = typeof(ZNetScene).GetField("m_namedPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
			var namedPrefabs = (Dictionary<int, GameObject>)namedPrefabsField.GetValue(ZNetScene.instance);

			int hash = CustomFxPrefab.name.GetStableHashCode();
			if (!namedPrefabs.ContainsKey(hash))
			{
				namedPrefabs.Add(hash, CustomFxPrefab);
				MarsarahTweaks.LogInfo("[PocketPortal] Manually added fx_pocket_portal_connected to ZNetScene.m_namedPrefabs");
			}

			EffectList newEffectList = new EffectList();
			EffectData effectData = new EffectData()
			{
				m_prefab = CustomFxPrefab,
				m_enabled = true,
				m_variant = -1,
				m_attach = false,
				m_follow = false,
				m_inheritParentRotation = false,
				m_inheritParentScale = false,
				m_multiplyParentVisualScale = false,
				m_randomRotation = false,
				m_scale = false,
				m_childTransform = ""
			};

			newEffectList.m_effectPrefabs = new EffectData[] { effectData };

			if (tp != null)
			{
				tp.m_connected = newEffectList;
				MarsarahTweaks.LogInfo("[PocketPortal] Successfully assigned custom effect list to portal");
			}
			else
			{
				MarsarahTweaks.LogWarn("[PocketPortal] TeleportWorld not found while assigning new target_found VFX.");
			}*/

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
					new RequirementConfig("SurtlingCore", 1),
				}
			};

			var customPiece = new CustomPiece(PocketPortalPrefab, fixReference: true, pieceConfig);
			PieceManager.Instance.AddPiece(customPiece);

			PocketPortalPrefab.SetActive(true);
			MarsarahTweaks.LogInfo("[PocketPortal] ✅ Pocket Portal registered and ready.");
		}

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

		/*[HarmonyPatch(typeof(TeleportWorld), "UpdatePortal")]
		public static class TeleportWorld_UpdatePortal_Patch
		{
			private static void Prefix(TeleportWorld __instance)
			{
				if (!__instance.name.Contains("pocket_portal")) return;

				if (__instance.m_connected == null)
				{
					MarsarahTweaks.LogError($"[PocketPortal] m_connected is null for {__instance.name}");
				}
				else if (__instance.m_connected.m_effectPrefabs == null)
				{
					MarsarahTweaks.LogError($"[PocketPortal] m_effectPrefabs is null for {__instance.name}");
				}
				else if (__instance.m_connected.m_effectPrefabs.Length == 0)
				{
					MarsarahTweaks.LogError($"[PocketPortal] m_effectPrefabs is empty for {__instance.name}");
				}
				else if (__instance.m_connected.m_effectPrefabs[0].m_prefab == null)
				{
					//__instance.m_connected.m_effectPrefabs[0].m_prefab = CustomFxPrefab;

					//MarsarahTweaks.LogWarn($"[PocketPortal] 🔄 Reassigned EffectList on Load for for {__instance.name}");

					MarsarahTweaks.LogError($"[PocketPortal] prefab is null for {__instance.name}");
				}
				else 
				{
					MarsarahTweaks.LogInfo($"[PocketPortal] ✅ Effect prefab ready: {__instance.m_connected.m_effectPrefabs[0].m_prefab.name}");
				}
			}
		}*/

		/*[HarmonyPatch(typeof(EffectList), nameof(EffectList.Create))]
		public static class EffectList_Create_Patch
		{
			private static void Prefix(EffectList __instance)
			{
				if (__instance.m_effectPrefabs == null || __instance.m_effectPrefabs.Length == 0) return;

				// Fix null prefabs in the EffectList
				foreach (var effectData in __instance.m_effectPrefabs)
				{
					if (effectData.m_prefab == null)
					{
						GameObject newFXPrefab = ZNetScene.instance.GetPrefab("fx_pocket_portal_connected");
						if (newFXPrefab == null)
						{
							MarsarahTweaks.LogWarn("[PocketPortal] Could not retrieve fx_pocket_portal_connected from ZNetScene. Using static variable.");
							newFXPrefab = CustomFxPrefab;
						}

						effectData.m_prefab = newFXPrefab;
						MarsarahTweaks.LogInfo("[PocketPortal] 🔄 Fixed null prefab in EffectList");
					}
				}
			}
		}*/
	}
}
