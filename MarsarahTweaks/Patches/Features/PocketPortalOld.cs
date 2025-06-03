using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using MarsarahTweaks.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TabHandler;

namespace MarsarahTweaks.Patches.Features
{
	/*public class CoroutineRunner : MonoBehaviour
	{
		private static CoroutineRunner _instance;
		public static CoroutineRunner Instance
		{
			get
			{
				if (_instance == null)
				{
					var go = new GameObject("CoroutineRunner");
					UnityEngine.Object.DontDestroyOnLoad(go);
					_instance = go.AddComponent<CoroutineRunner>();
				}
				return _instance;
			}
		}
	}*/

	class PocketPortalOld
	{
		/*private static bool initialized = false;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetScene_Awake_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null) return;

				if (!initialized)
				{
					initialized = true;
					CreatePocketPortal(__instance);
				}

				FieldInfo namedPrefabsField = typeof(ZNetScene).GetField("m_namedPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
				if (namedPrefabsField == null)
				{
					MarsarahTweaks.LogError("Could not find m_namedPrefabs field in ZNetScene!");
					return;
				}

				// Get the dictionary value
				var m_namedPrefabs = (Dictionary<int, GameObject>)namedPrefabsField.GetValue(ZNetScene.instance);
				if (m_namedPrefabs == null)
				{
					MarsarahTweaks.LogError("m_namedPrefabs is null!");
					return;
				}

				// Check if your prefab is registered by name (case-sensitive)
				bool isRegistered = m_namedPrefabs.Values.Any(prefab => prefab.name == "pocket_portal");
				MarsarahTweaks.LogInfo($"Prefab 'pocket_portal' in ZNetScene Awake Prefix? {isRegistered}");
			}
		}*/

		/*public static void Init()
		{
			//PrefabManager.OnVanillaPrefabsAvailable += CreatePocketPortal;
			PrefabManager.OnPrefabsRegistered += CreatePocketPortal;
		}*/

		/*private static void CreatePocketPortal(ZNetScene znetScene)
		{
			try
			{

				if (PrefabManager.Instance.GetPrefab("pocket_portal") != null)
				{
					MarsarahTweaks.LogInfo("Pocket Portal prefab already registered.");
					return;
				}

				GameObject originalPortal = PrefabManager.Instance.GetPrefab("portal_wood");
				if (originalPortal == null)
				{
					MarsarahTweaks.LogError("Original portal prefab not found!");
					return;
				}

				bool originalActive = originalPortal.activeSelf;
				originalPortal.SetActive(false);
				GameObject pocketPortal = UnityEngine.Object.Instantiate(originalPortal);
				pocketPortal.name = "pocket_portal";
				originalPortal.SetActive(originalActive);

				// Add missing components if needed (e.g. ZNetView, ZSyncTransform)
				if (!pocketPortal.GetComponent<ZNetView>())
				{
					MarsarahTweaks.LogInfo("Pocket Portal ZNetView missing. Adding new.");
					pocketPortal.AddComponent<ZNetView>();
				}
				ZNetView ppZNetView = pocketPortal.GetComponent<ZNetView>();
				ppZNetView.m_persistent = true;
				ppZNetView.m_distant = false;
				ppZNetView.m_type = ZDO.ObjectType.Solid;
				ppZNetView.m_syncInitialScale = false;

				if (!pocketPortal.GetComponent<WearNTear>())
				{
					MarsarahTweaks.LogInfo("Pocket Portal WearNTear missing. Adding new.");
					pocketPortal.AddComponent<WearNTear>();

				}
				WearNTear ppWearNTear = pocketPortal.GetComponent<WearNTear>();
				WearNTear originalWearNTear = originalPortal.GetComponent<WearNTear>();
				if (originalWearNTear == null)
				{
					//MarsarahTweaks.LogError("Original portal prefab is missing WearNTear!");
					//return;
					throw new Exception("Original portal prefab is missing WearNTear!");
				}

				// Add piece if missing and set up display info
				if (!pocketPortal.GetComponent<Piece>())
				{
					MarsarahTweaks.LogInfo("Pocket Portal Piece missing. Adding new.");
					pocketPortal.AddComponent<Piece>();
				}
				Piece ppPiece = pocketPortal.GetComponent<Piece>();
				Piece originalPiece = originalPortal.GetComponent<Piece>();
				if (originalPiece == null)
				{
					//MarsarahTweaks.LogError("Original portal prefab is missing Piece!");
					throw new Exception("Original portal prefab is missing Piece!");
					//return;
				}
				ppPiece.m_name = "Pocket Portal";
				ppPiece.m_description = "A custom pocket portal";
				ppPiece.m_craftingStation = null;

				if (!pocketPortal.GetComponent<TeleportWorld>())
				{
					pocketPortal.AddComponent<TeleportWorld>();
				}
				TeleportWorld ppTeleport = pocketPortal.GetComponent<TeleportWorld>();
				TeleportWorld originalTeleport = originalPortal.GetComponent<TeleportWorld>();
				if (originalTeleport == null)
				{
					//MarsarahTweaks.LogError("Original portal prefab is missing TeleportWorld!");
					//return;
					throw new Exception("Original portal prefab is missing TeleportWorld!");
				}

				// Wrap in CustomPrefab with fixReference to fix internal references automatically
				CustomPrefab customPrefab = new CustomPrefab(pocketPortal, fixReference: true);

				PrefabManager.Instance.AddPrefab(customPrefab);

				// Register to ZNetScene manually
				znetScene.m_prefabs.Add(pocketPortal);

				// Register to ZNetScene explicitly for networking
				PrefabManager.Instance.RegisterToZNetScene(pocketPortal);


				if (ZNetScene.instance.GetPrefab("pocket_portal") == null)
					MarsarahTweaks.LogError("ZNetScene does NOT recognize 'pocket_portal' after registration!");
				else
					MarsarahTweaks.LogInfo("ZNetScene prefab registration SUCCESS for pocket_portal");

				// Create a proper PieceConfig
				PieceConfig pieceConfig = new PieceConfig
				{
					PieceTable = "Hammer",
					Category = "Misc",
					Enabled = true,
					Requirements = new[]
					{
						new RequirementConfig("SurtlingCore", 1),
					}
				};

				// Add the piece to the PieceManager
				CustomPiece customPiece = new CustomPiece(pocketPortal, fixReference: true, pieceConfig);

				PieceManager.Instance.AddPiece(customPiece);

				pocketPortal.SetActive(true);

				MarsarahTweaks.LogInfo("Pocket Portal registered successfully.");
			}
			catch (Exception e)
			{
				MarsarahTweaks.LogError($"Exception during Pocket Portal registration:\n{e}");
			}
		}*/

		// ==================================== 
		// Very partially working version with asset bundle

		/*private static AssetBundle marsaBundle;

		public static void LoadAssetBundle()
		{
			var assembly = typeof(MarsarahTweaks).Assembly;

			// Find the exact resource name
			string resourceName = "marsabundle"; // Update below if needed

			// Try to match the full resource name
			foreach (string res in assembly.GetManifestResourceNames())
			{
				if (res.EndsWith(resourceName))
				{
					MarsarahTweaks.LogInfo($"🔍 Found embedded bundle resource: {res}");
					using (var stream = assembly.GetManifestResourceStream(res))
					{
						if (stream == null)
						{
							MarsarahTweaks.LogError("❌ Failed to open embedded resource stream.");
							return;
						}

						using (var memStream = new MemoryStream())
						{
							stream.CopyTo(memStream);
							marsaBundle = AssetBundle.LoadFromMemory(memStream.ToArray());
							MarsarahTweaks.LogInfo("✅ Loaded embedded marsabundle from memory.");
						}
					}
					return;
				}
			}

			MarsarahTweaks.LogError("❌ Could not find embedded resource ending in 'marsabundle'.");
		}

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetSceneAwakePatch
		{
			private static bool _initialized;

			private static void Postfix(ZNetScene __instance)
			{
				if (_initialized) return;
				_initialized = true;

				RegisterPortalPrefab();
			}
		}*/

		/*public static void RegisterInit()
		{
			//PrefabManager.OnVanillaPrefabsAvailable += RegisterPortalPrefab;
			PrefabManager.OnPrefabsRegistered += RegisterPortalPrefab;
		}*/

		/*private static void RegisterPortalPrefab()
		{
			//GameObject portalPrefab = marsaBundle.LoadAsset<GameObject>("pocket_portal");
			GameObject portalPrefab = ZNetScene.instance.GetPrefab("portal_wood");
			if (portalPrefab == null)
			{
				MarsarahTweaks.LogError("❌ Could not load prefab 'pocket_portal' from bundle");
				return;
			}

			portalPrefab.SetActive(false);

			//foreach (var comp in portalPrefab.GetComponentsInChildren<MonoBehaviour>(true))
			//{
			//	MarsarahTweaks.LogInfo($"🔍 Component: {comp.GetType().Name}");
			//}

			//var teleportTrigger = portalPrefab.transform.Find("TELEPORT");
			//if (teleportTrigger == null)
			//{
			//	MarsarahTweaks.LogError("❌ TELEPORT object missing in prefab!");
			//}
			//else
			//{
			//	var boxCollider = teleportTrigger.GetComponent<BoxCollider>();
			//	if (boxCollider == null || !boxCollider.isTrigger)
			//	{
			//		MarsarahTweaks.LogWarn("⚠ TELEPORT BoxCollider missing or isTrigger not set!");
			//	}

			//	var teleportWorldTrigger = teleportTrigger.GetComponent<TeleportWorldTrigger>();
			//	if (teleportWorldTrigger == null)
			//	{
			//		MarsarahTweaks.LogWarn("⚠ TELEPORT missing TeleportWorldTrigger component!");
			//	}
			//}

			var netView = portalPrefab.GetComponent<ZNetView>();
			if (netView == null)
			{
				MarsarahTweaks.LogError("❌ portalPrefab missing ZNetView component!");
			}
			else
			{
				MarsarahTweaks.LogInfo($"ZNetView found, persistent: {netView.m_persistent}");
				netView.m_type = ZDO.ObjectType.Solid;
				netView.m_persistent = true;
			}

			foreach (var comp in portalPrefab.GetComponents<Component>())
			{
				Debug.Log($"[PocketPortal] Component: {comp.GetType().Name}");
			}

			Piece piece = portalPrefab.GetComponent<Piece>();
			if (piece != null)
			{
				piece.m_name = "Pocket Portal";
				piece.m_description = "A portable connection to another place.";
				piece.m_craftingStation = null;
			}
			else
			{
				MarsarahTweaks.LogWarn("⚠ Pocket Portal prefab is missing a Piece component.");
			}

			// Fix references just in case
			CustomPrefab customPrefab = new CustomPrefab(portalPrefab, fixReference: true);
			PrefabManager.Instance.AddPrefab(customPrefab);
			//MarsarahTweaks.LogInfo("Registering prefab to ZNetScene...");
			PrefabManager.Instance.RegisterToZNetScene(portalPrefab);
			//MarsarahTweaks.LogInfo("Done registering.");

			// Add as a placeable piece
			PieceConfig pieceConfig = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = "Misc",
				Requirements = new[]
				{
					new RequirementConfig("FineWood", 1),
					new RequirementConfig("GreydwarfEye", 1)
				}
			};

			CustomPiece customPiece = new CustomPiece(portalPrefab, fixReference: true, pieceConfig);
			PieceManager.Instance.AddPiece(customPiece);

			MarsarahTweaks.LogInfo("✅ Pocket portal registered and added to Hammer > Misc");


			// OCD logs
			//MarsarahTweaks.LogInfo($"Prefab in ZNetScene? {ZNetScene.instance.m_prefabs.Contains(portalPrefab)}");
			////MarsarahTweaks.LogInfo($"Prefab in ZNetScene? {ZNetScene.instance.m_namedPrefabs.ContainsKey("pocket_portal")}");

			//// Get the private field via reflection
			//FieldInfo namedPrefabsField = typeof(ZNetScene).GetField("m_namedPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
			//if (namedPrefabsField == null)
			//{
			//	MarsarahTweaks.LogError("Could not find m_namedPrefabs field in ZNetScene!");
			//	return;
			//}

			//// Get the dictionary value
			//var m_namedPrefabs = (Dictionary<int, GameObject>)namedPrefabsField.GetValue(ZNetScene.instance);
			//if (m_namedPrefabs == null)
			//{
			//	MarsarahTweaks.LogError("m_namedPrefabs is null!");
			//	return;
			//}

			//// Check if your prefab is registered by name (case-sensitive)
			//bool isRegistered = m_namedPrefabs.Values.Any(prefab => prefab.name == "pocket_portal");
			//MarsarahTweaks.LogInfo($"Prefab 'pocket_portal' in ZNetScene? {isRegistered}");

			portalPrefab.SetActive(true);
		}*/

		/*[HarmonyPatch(typeof(TeleportWorld), "UpdatePortal")]
		public static class Debug_UpdatePortal
		{
			static void Prefix(TeleportWorld __instance, ref ZNetView ___m_nview)
			{
				if (__instance.name.Contains("pocket_portal"))
				{
					var zdo = ___m_nview?.GetZDO();
					if (zdo == null) return;

					Debug.Log($"UpdatePortal - Tag: {zdo.GetString("tag")}, ConnectedTo: {zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal)}");
				}
			}
		}*/

		/*[HarmonyPatch(typeof(ZNetView), "Awake")]
		public class ZNetViewAwakeDebug
		{
			static void Postfix(ZNetView __instance)
			{
				ZLog.Log($"[ZNetView Debug] Awake on {__instance.name}, Has ZDO: {(__instance.GetZDO() != null)}");
			}
		}*/


		/*[HarmonyPatch(typeof(ZNetScene), "CreateObject")]
		class ZNetSceneCreateObjectPatch
		{
			static void Prefix(ZDO zdo)
			{
				//if (zdo.GetPrefab() == 853122569)
				//{
					MarsarahTweaks.LogInfo($"[ZNetScene] Creating object from ZDO: {zdo.m_uid}, prefab: {zdo.GetPrefab()}");
				//}
			}

			static void Postfix(GameObject __result)
			{
				if (__result != null)
				{
					//if (__result.name.Contains("pocket"))
					//{
						MarsarahTweaks.LogInfo($"[ZNetScene] Created: {__result.name}");
					//}
				}
				else
				{
					MarsarahTweaks.LogWarn($"[ZNetScene] CreateObject returned null");
				}
			}
		}*/


		/*[HarmonyPatch(typeof(ZNetView), "Awake")]
		class ZNetViewAwakeLogger
		{
			static void Prefix(ZNetView __instance)
			{
				if (__instance.gameObject.name.Contains("portal"))
				{
					MarsarahTweaks.LogInfo($"[ZNetView] Awake on: {__instance.gameObject.name}, init ZDO? {ZNetView.m_initZDO != null}");

					// ======================================

					ZDO zdo = __instance.GetZDO();
					ZDOID id = zdo?.m_uid ?? default;
					//ZDO maybe = ZDOMan.instance.m_objectsByID.TryGetValue(id, out var real) ? real : null;

					// 1. Get the private field via reflection
					FieldInfo objectsByIdField = typeof(ZDOMan).GetField("m_objectsByID", BindingFlags.NonPublic | BindingFlags.Instance);
					if (objectsByIdField == null)
					{
						MarsarahTweaks.LogError("Could not find m_objectsByID field in ZDOMan!");
						return;
					}

					// 2. Extract the dictionary
					var m_objectsByID = (Dictionary<ZDOID, ZDO>)objectsByIdField.GetValue(ZDOMan.instance);
					if (m_objectsByID == null)
					{
						MarsarahTweaks.LogError("m_objectsByID is null!");
						return;
					}

					ZDO maybe = m_objectsByID.TryGetValue(id, out var real) ? real : null;

					Debug.Log($"[ZNetViewDebug] {__instance.name}: ZDO? {zdo != null}, ZDOID: {id}, Exists in ZDOMan? {maybe != null}");

					var prefab = ZNetScene.instance.GetPrefab("pocket_portal");
					var view = prefab?.GetComponent<ZNetView>();
					ZLog.Log($"[DEBUG] prefab has ZNetView? {view != null}, persistent? {view?.m_persistent}, valid? {view?.IsValid()}");
				}
			}

			static void Postfix(ZNetView __instance)
			{
				if (__instance.gameObject.name.Contains("portal"))
				{
					MarsarahTweaks.LogInfo($"[ZNetView] Post-Awake ZDO: {__instance.GetZDO()?.m_uid}");
				}
			}
		}



		[HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
		public static class Player_PlacePiece_Patch
		{
			public static void Postfix(Piece piece)
			{
				if (piece == null) return;

				GameObject placedGO = piece.gameObject;

				//if (placedGO.name.Contains("pocket_portal") || piece.m_name == "Pocket Portal")
				if (placedGO.name.Contains("portal"))
				{
					MarsarahTweaks.LogInfo("🚀 Pocket Portal placed!");

					var view = piece.GetComponent<ZNetView>();
					var zdo = view?.GetZDO();
					MarsarahTweaks.LogInfo($"[PocketPortal] Placed portal: view valid? {view?.IsValid() ?? false}, ZDO? {(zdo != null ? zdo.m_uid.ToString() : "null")}");

					MarsarahTweaks.LogInfo($"[DEBUG] placed portal view valid? {view.IsValid()}, ZDOID: {view.GetZDO()?.m_uid}");

					//CoroutineRunner.Instance.StartCoroutine(LogPortalConnectionsDelayed(placedGO));
				}
			}
		}

		[HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.CreateNewZDO), new Type[] { typeof(Vector3), typeof(int) })]
		static class Patch_ZDOMan_CreateZDO
		{
			static void Postfix(Vector3 position, int prefabHash, ZDO __result)
			{
				if (__result != null)
				{
					MarsarahTweaks.LogInfo($"[ZDOMan] Created ZDO at {position}, prefab hash: {prefabHash}, ZDO ID: {__result.m_uid}");
				}
			}
		}

		[HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.Load))]
		static class Patch_ZDOMan_Load
		{
			static void Prefix()
			{
				MarsarahTweaks.LogInfo("[ZDOMan] Load() called — prefab state before load:");
				FieldInfo namedPrefabsField = typeof(ZNetScene).GetField("m_namedPrefabs", BindingFlags.NonPublic | BindingFlags.Instance);
				if (namedPrefabsField == null)
				{
					MarsarahTweaks.LogError("Could not find m_namedPrefabs field in ZNetScene!");
					return;
				}

				// Get the dictionary value
				var m_namedPrefabs = (Dictionary<int, GameObject>)namedPrefabsField.GetValue(ZNetScene.instance);
				if (m_namedPrefabs == null)
				{
					MarsarahTweaks.LogError("m_namedPrefabs is null!");
					return;
				}

				foreach (var kvp in m_namedPrefabs)
				{
					MarsarahTweaks.LogInfo($"Prefab: {kvp.Key}");
				}
			}
		}*/


		/*public static IEnumerator LogPortalConnectionsDelayed(GameObject portal)
		{
			yield return new WaitForSeconds(2f); // wait 

			var netView = portal.GetComponent<ZNetView>();
			if (netView == null)
			{
				MarsarahTweaks.LogError("Portal missing ZNetView!");
				yield break;
			}

			ZDO zdo = netView.GetZDO();
			if (zdo == null)
			{
				MarsarahTweaks.LogError("Portal's ZDO is still null after delay!");
				yield break;
			}

			MarsarahTweaks.LogInfo($"Portal {portal.name} ZDOID: {zdo.m_uid}");
			MarsarahTweaks.LogInfo($"Owner: {zdo.GetOwner()}");
			MarsarahTweaks.LogInfo($"Connection ZDOID: {zdo.GetConnection()}");
		}*/

		/*[HarmonyPatch(typeof(TeleportWorld), "Awake")]
		public static class Debug_TeleportWorld_Awake
		{
			static void Postfix(TeleportWorld __instance)
			{
				//var zdo = __instance.GetComponent<ZNetView>()?.GetZDO();
				//var tag = zdo?.GetString("tag") ?? "(null)";
				//MarsarahTweaks.LogInfo($"[PortalDebug] TeleportWorld.Awake called on {__instance.name} | Tag: {tag}");

				if (__instance.name.StartsWith("pocket_portal"))
				{
					var zdo = __instance.GetComponent<ZNetView>()?.GetZDO();
					var tag = zdo?.GetString("tag") ?? "(null)";

					MarsarahTweaks.LogInfo($"[PortalDebug] Calling SetText for portal {__instance.name} with Tag: {tag}");
					__instance.SetText(tag);

					// Force an UpdatePortal check after a short delay (to ensure ZDO is ready)
					//__instance.Invoke("UpdatePortal", 0.5f);
				}
			}
		}*/

		/*[HarmonyPatch(typeof(TeleportWorld), "UpdatePortal")]
		public static class Debug_TeleportWorld_UpdatePortal
		{
			static void Postfix(TeleportWorld __instance, ref ZNetView ___m_nview, ref bool ___m_hadTarget)
			{
				if (__instance.name.StartsWith("pocket_portal"))
				{
					if (___m_nview.IsValid() && !(__instance.m_proximityRoot == null))
					{
						Player closestPlayer = Player.GetClosestPlayer(__instance.m_proximityRoot.position, __instance.m_activationRange);
						bool flag = HaveTarget();
						if (flag && !___m_hadTarget)
						{
							__instance.m_connected.Create(base.transform.position, base.transform.rotation);
						}
						___m_hadTarget = flag;
						bool flag2 = false;
						if ((bool)closestPlayer)
						{
							flag2 = closestPlayer.IsTeleportable() || __instance.m_allowAllItems;
						}
						__instance.m_target_found.SetActive(flag2 && TargetFound());
					}
				}
			}
		}*/

		/*[HarmonyPatch(typeof(TeleportWorld), "SetText")]
		public static class Debug_TeleportWorld_SetText
		{
			static void Postfix(TeleportWorld __instance, string text)
			{
				if (__instance.name.StartsWith("pocket_portal"))
				{
					MarsarahTweaks.LogInfo($"[PortalDebug] RPC_SetTag theoretically called for portal {__instance.name} with Tag: {text}");
				}
			}
		}*/

		/*[HarmonyPatch(typeof(TeleportWorld), "RPC_SetTag")]
		public static class Debug_TeleportWorld_RPC_SetTag
		{
			static void Postfix(TeleportWorld __instance, ref ZNetView ___m_nview, long sender, string tag, string authorId)
			{
				if (__instance.name.StartsWith("pocket_portal"))
				{
					//MarsarahTweaks.LogInfo($"We have a SetTag with Pocket Portal: {__instance.name} with tag: {tag} and authorId: {authorId}");

					if (___m_nview.IsValid() && ___m_nview.IsOwner())
					{
						//GetTagSignature(out var tagRaw, out var authorId2);
						string tagRaw = "";
						string authorId2 = "";

						var getTagSignatureMethod = typeof(TeleportWorld).GetMethod("GetTagSignature", BindingFlags.NonPublic | BindingFlags.Instance);

						if (getTagSignatureMethod != null)
						{
							MarsarahTweaks.LogInfo("We called private method GetTagSignature");

							object[] parameters = new object[] { null, null }; // string tagRaw, string authorId2
							getTagSignatureMethod.Invoke(__instance, parameters);

							tagRaw = (string)parameters[0];
							authorId2 = (string)parameters[1];

							MarsarahTweaks.LogInfo($"Tags received: tagRaw: {tagRaw}, authorId2: {authorId2}");
						}
						else
						{
							MarsarahTweaks.LogWarn("[PocketPortal] Failed to find GetTagSignature method.");
						}

						//if (!(tagRaw == tag) || !(authorId2 == authorId))
						//{
							MarsarahTweaks.LogInfo("We are performing connection shenanigans");

							ZDO zDO = ___m_nview.GetZDO();
							zDO.UpdateConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
							ZDOID connectionZDOID = zDO.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
							MarsarahTweaks.LogInfo($"Connection ZDOID: {connectionZDOID}");
							//SetConnectedPortal(connectionZDOID);
							Traverse.Create(__instance).Method("SetConnectedPortal", new object[] { connectionZDOID }).GetValue();
							MarsarahTweaks.LogInfo("We called private method SetConnectedPortal");

							zDO.Set(ZDOVars.s_tag, tag);
							zDO.Set(ZDOVars.s_tagauthor, authorId);

							//MarsarahTweaks.LogInfo("END", false, true);
						//}
					}
				}
			}
		}*/

		/*[HarmonyPatch(typeof(TeleportWorld), "SetConnectedPortal")]
		public static class Debug_TeleportWorld_SetConnectedPortal
		{
			static void Postfix(TeleportWorld __instance, ref ZNetView ___m_nview, ZDOID targetID)
			{
				if (__instance.name.StartsWith("pocket_portal"))
				{
					MarsarahTweaks.LogInfo($"Target ID: {targetID}");

					ZDO zDO = ZDOMan.instance.GetZDO(targetID);
					if (zDO != null)
					{
						long owner = zDO.GetOwner();
						MarsarahTweaks.LogInfo($"Owner: {owner}");
						if (owner == 0L)
						{
							MarsarahTweaks.LogInfo($"Owner: {owner}");
							//zDO.SetOwner(ZDOMan.GetSessionID());
							//zDO.SetConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
						}
						else
						{
							MarsarahTweaks.LogInfo($"[PortalDebug] RPC_SetConnected theoretically called for portal {__instance.name}");
							//___m_nview.InvokeRPC(owner, "RPC_SetConnected", targetID);
						}
					}
					else
					{
						MarsarahTweaks.LogWarn($"ZDO for {targetID} is null.");
					}
				}
			}
		}*/

		/*[HarmonyPatch(typeof(ZDOMan), "AddToSector")]
		public static class Debug_PortalConnections
		{
			static void Postfix(ZDO zdo)
			{
				if (zdo.GetPrefab() == 853122569) // Replace with your prefab name
				{
					Debug.Log($"Portal ZDO: {zdo.m_uid}, Tag: {zdo.GetString("tag")}");
					Debug.Log($"Connected to: {zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal)}");
				}
			}
		}*/


		/*[HarmonyPatch(typeof(TeleportWorld), "Awake")]
		public class Debug_TeleportWorldAwake
		{
			static void Postfix(TeleportWorld __instance)
			{
				ZNetView nview = __instance.GetComponent<ZNetView>();
				if (!nview || !nview.IsValid()) return;

				ZDO zdo = nview.GetZDO();
				if (zdo == null) return;

				string tag = zdo.GetString(ZDOVars.s_tag);
				string author = zdo.GetString(ZDOVars.s_tagauthor);

				Debug.Log($"[TeleportWorld.Awake] tag: '{tag}', author: '{author}', prefab: {zdo.GetPrefab()}");
			}
		}*/


		/*[HarmonyPatch(typeof(TeleportWorld), "Awake")]
		public class PocketPortal_StartPatch
		{
			static void Postfix(TeleportWorld __instance)
			{
				var nview = __instance.GetComponent<ZNetView>();
				if (!nview || !nview.IsValid()) return;

				var zdo = nview.GetZDO();
				if (zdo == null || zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal) != ZDOID.None) return;

				string tag = zdo.GetString(ZDOVars.s_tag);
				string authorId = zdo.GetString(ZDOVars.s_tagauthor);

				if (string.IsNullOrEmpty(tag)) // empty tag: connect to another empty-tag portal
				{
					foreach (ZNetView potential in GameObject.FindObjectsOfType<ZNetView>())
					{
						if (potential == nview) continue;
						if (!potential.IsValid()) continue;

						ZDO otherZDO = potential.GetZDO();
						if (otherZDO == null) continue;

						if (!TeleportWorld_PortalLike(potential.gameObject)) continue;

						if (!string.IsNullOrEmpty(otherZDO.GetString(ZDOVars.s_tag))) continue; // must also be empty
						if (otherZDO.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal) != ZDOID.None) continue;

						// Match by author too?
						if (otherZDO.GetString(ZDOVars.s_tagauthor) != authorId) continue;

						zdo.SetConnection(ZDOExtraData.ConnectionType.Portal, otherZDO.m_uid);
						otherZDO.SetConnection(ZDOExtraData.ConnectionType.Portal, zdo.m_uid);
						break;
					}
				}
			}
		}


		[HarmonyPatch(typeof(TeleportWorld), "RPC_SetTag")]
		public class PocketPortal_RPCTagPatch
		{
			static void Postfix(TeleportWorld __instance)
			{
				ZNetView nview = __instance.GetComponent<ZNetView>();
				if (!nview || !nview.IsValid()) return;

				ZDO zdo = nview.GetZDO();
				if (zdo == null) return;

				// Disconnect old connection
				ZDOID oldTarget = zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
				if (oldTarget != ZDOID.None)
				{
					ZDO oldZdo = ZDOMan.instance.GetZDO(oldTarget);
					if (oldZdo != null)
					{
						oldZdo.SetConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
					}
					zdo.SetConnection(ZDOExtraData.ConnectionType.Portal, ZDOID.None);
				}

				string tag = zdo.GetString(ZDOVars.s_tag);
				string authorId = zdo.GetString(ZDOVars.s_tagauthor);

				// Skip if already connected
				if (zdo.GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal) != ZDOID.None)
					return;

				// Manually search for match
				foreach (ZNetView potential in GameObject.FindObjectsOfType<ZNetView>())
				{
					if (potential == nview) continue;
					if (!potential.IsValid()) continue;

					ZDO otherZDO = potential.GetZDO();
					if (otherZDO == null) continue;

					// Match by tag + author
					//if (otherZDO.GetPrefab() != zdo.GetPrefab()) continue; // optional: match only your prefab
					if (!TeleportWorld_PortalLike(potential.gameObject)) continue; // allow matching other portals
					if (otherZDO.GetString(ZDOVars.s_tag) != tag) continue;
					if (otherZDO.GetString(ZDOVars.s_tagauthor) != authorId) continue; // ???

					// Connect both ends
					zdo.SetConnection(ZDOExtraData.ConnectionType.Portal, otherZDO.m_uid);
					otherZDO.SetConnection(ZDOExtraData.ConnectionType.Portal, zdo.m_uid);
					break;
				}
			}
		}

		private static bool TeleportWorld_PortalLike(GameObject go)
		{
			return go.GetComponent<TeleportWorld>() != null;
		}*/









		// ======================= Partially working version with dynamic clone

		//private static bool initialized = false;

		//public static void Init()
		//{
		//	//PrefabManager.OnVanillaPrefabsAvailable += CreatePocketPortal;
		//	PrefabManager.OnPrefabsRegistered += CreatePocketPortal;
		//}

		//private static void CreatePocketPortal()
		//{
		//	try
		//	{
		//		if (initialized)
		//		{
		//			return;
		//		}
		//		else
		//		{
		//			initialized = true;
		//		}

		//		if (PrefabManager.Instance.GetPrefab("pocket_portal") != null)
		//		{
		//			MarsarahTweaks.LogInfo("Pocket Portal prefab already registered.");
		//			return;
		//		}

		//		GameObject originalPortal = PrefabManager.Instance.GetPrefab("portal_wood");
		//		if (originalPortal == null)
		//		{
		//			MarsarahTweaks.LogError("Original portal prefab not found!");
		//			return;
		//		}

		//		bool originalActive = originalPortal.activeSelf;
		//		originalPortal.SetActive(false);
		//		GameObject pocketPortal = UnityEngine.Object.Instantiate(originalPortal);
		//		pocketPortal.name = "pocket_portal";

		//		originalPortal.SetActive(originalActive);

		//		// Add missing components if needed (e.g. ZNetView, ZSyncTransform)
		//		if (!pocketPortal.GetComponent<ZNetView>())
		//		{
		//			MarsarahTweaks.LogInfo("Pocket Portal ZNetView missing. Adding new.");
		//			pocketPortal.AddComponent<ZNetView>();
		//		}
		//		ZNetView ppZNetView = pocketPortal.GetComponent<ZNetView>();
		//		ppZNetView.m_persistent = true;
		//		/*ppZNetView.m_distant = false;
		//		ppZNetView.m_type = ZDO.ObjectType.Solid;
		//		ppZNetView.m_syncInitialScale = false;*/

		//		if (!pocketPortal.GetComponent<WearNTear>())
		//		{
		//			MarsarahTweaks.LogInfo("Pocket Portal WearNTear missing. Adding new.");
		//			pocketPortal.AddComponent<WearNTear>();

		//		}
		//		WearNTear ppWearNTear = pocketPortal.GetComponent<WearNTear>();
		//		WearNTear originalWearNTear = originalPortal.GetComponent<WearNTear>();
		//		if (originalWearNTear == null)
		//		{
		//			//MarsarahTweaks.LogError("Original portal prefab is missing WearNTear!");
		//			//return;
		//			throw new Exception("Original portal prefab is missing WearNTear!");
		//		}
		//		/*ppWearNTear.m_health = originalWearNTear.m_health;
		//		ppWearNTear.m_noRoofWear = originalWearNTear.m_noRoofWear;
		//		ppWearNTear.m_supports = originalWearNTear.m_supports;
		//		ppWearNTear.m_ashDamageImmune = originalWearNTear.m_ashDamageImmune;
		//		ppWearNTear.m_ashDamageResist = originalWearNTear.m_ashDamageResist;
		//		ppWearNTear.m_burnable = originalWearNTear.m_burnable;
		//		ppWearNTear.m_materialType = originalWearNTear.m_materialType;
		//		ppWearNTear.m_staticPosition = originalWearNTear.m_staticPosition;
		//		ppWearNTear.m_damages = originalWearNTear.m_damages;
		//		ppWearNTear.m_minToolTier = originalWearNTear.m_minToolTier;
		//		ppWearNTear.m_hitNoise = originalWearNTear.m_hitNoise;
		//		ppWearNTear.m_destroyNoise = originalWearNTear.m_destroyNoise;
		//		ppWearNTear.m_triggerPrivateArea = originalWearNTear.m_triggerPrivateArea;
		//		ppWearNTear.m_destroyedEffect = originalWearNTear.m_destroyedEffect;
		//		ppWearNTear.m_hitEffect = originalWearNTear.m_hitEffect;
		//		ppWearNTear.m_switchEffect = originalWearNTear.m_switchEffect;
		//		ppWearNTear.m_autoCreateFragments = originalWearNTear.m_autoCreateFragments;
		//		ppWearNTear.m_fragmentRoots = originalWearNTear.m_fragmentRoots;*/

		//		// Add piece if missing and set up display info
		//		if (!pocketPortal.GetComponent<Piece>())
		//		{
		//			MarsarahTweaks.LogInfo("Pocket Portal Piece missing. Adding new.");
		//			pocketPortal.AddComponent<Piece>();
		//		}
		//		Piece ppPiece = pocketPortal.GetComponent<Piece>();
		//		Piece originalPiece = originalPortal.GetComponent<Piece>();
		//		if (originalPiece == null)
		//		{
		//			//MarsarahTweaks.LogError("Original portal prefab is missing Piece!");
		//			throw new Exception("Original portal prefab is missing Piece!");
		//			//return;
		//		}
		//		ppPiece.m_name = "Pocket Portal";
		//		ppPiece.m_description = "A custom pocket portal";
		//		/*ppPiece.m_icon = originalPiece.m_icon;
		//		ppPiece.enabled = originalPiece.enabled;
		//		ppPiece.m_category = originalPiece.m_category;*/
		//		//ppPiece.m_craftingStation = originalPiece.m_craftingStation;
		//		ppPiece.m_craftingStation = null;
		//		/*ppPiece.m_resources = originalPiece.m_resources;
		//		ppPiece.m_destroyedLootPrefab = originalPiece.m_destroyedLootPrefab;
		//		ppPiece.m_returnResourceHeightOffset = originalPiece.m_returnResourceHeightOffset;
		//		ppPiece.m_primaryTarget = originalPiece.m_primaryTarget;
		//		ppPiece.m_randomTarget = originalPiece.m_randomTarget;
		//		ppPiece.m_targetNonPlayerBuilt = originalPiece.m_targetNonPlayerBuilt;
		//		ppPiece.m_isUpgrade = originalPiece.m_isUpgrade;
		//		ppPiece.m_comfort = originalPiece.m_comfort;
		//		ppPiece.m_comfortGroup = originalPiece.m_comfortGroup;
		//		ppPiece.m_comfortObject = originalPiece.m_comfortObject;
		//		ppPiece.m_groundPiece = originalPiece.m_groundPiece;
		//		ppPiece.m_allowAltGroundPlacement = originalPiece.m_allowAltGroundPlacement;
		//		ppPiece.m_groundOnly = originalPiece.m_groundOnly;
		//		ppPiece.m_cultivatedGroundOnly = originalPiece.m_cultivatedGroundOnly;
		//		ppPiece.m_waterPiece = originalPiece.m_waterPiece;
		//		ppPiece.m_clipGround = originalPiece.m_clipGround;
		//		ppPiece.m_clipEverything = originalPiece.m_clipEverything;
		//		ppPiece.m_noInWater = originalPiece.m_noInWater;
		//		ppPiece.m_notOnWood = originalPiece.m_notOnWood;
		//		ppPiece.m_notOnTiltingSurface = originalPiece.m_notOnTiltingSurface;
		//		ppPiece.m_inCeilingOnly = originalPiece.m_inCeilingOnly;
		//		ppPiece.m_notOnFloor = originalPiece.m_notOnFloor;
		//		ppPiece.m_noClipping = originalPiece.m_noClipping;
		//		ppPiece.m_onlyInTeleportArea = originalPiece.m_onlyInTeleportArea;
		//		ppPiece.m_allowedInDungeons = originalPiece.m_allowedInDungeons;
		//		ppPiece.m_spaceRequirement = originalPiece.m_spaceRequirement;
		//		ppPiece.m_repairPiece = originalPiece.m_repairPiece;
		//		ppPiece.m_removePiece = originalPiece.m_removePiece;
		//		ppPiece.m_canRotate = originalPiece.m_canRotate;
		//		ppPiece.m_randomInitBuildRotation = originalPiece.m_randomInitBuildRotation;
		//		ppPiece.m_canBeRemoved = originalPiece.m_canBeRemoved;
		//		ppPiece.m_allowRotatedOverlap = originalPiece.m_allowRotatedOverlap;
		//		ppPiece.m_vegetationGroundOnly = originalPiece.m_vegetationGroundOnly;
		//		ppPiece.m_blockingPieces = originalPiece.m_blockingPieces;
		//		ppPiece.m_blockRadius = originalPiece.m_blockRadius;
		//		ppPiece.m_mustConnectTo = originalPiece.m_mustConnectTo;
		//		ppPiece.m_mustBeAboveConnected = originalPiece.m_mustBeAboveConnected;
		//		ppPiece.m_noVines = originalPiece.m_noVines;
		//		ppPiece.m_extraPlacementDistance = originalPiece.m_extraPlacementDistance;
		//		ppPiece.m_onlyInBiome = originalPiece.m_onlyInBiome;
		//		ppPiece.m_harvest = originalPiece.m_harvest;
		//		ppPiece.m_harvestRadius = originalPiece.m_harvestRadius;
		//		ppPiece.m_harvestRadiusMaxLevel = originalPiece.m_harvestRadiusMaxLevel;
		//		ppPiece.m_placeEffect = originalPiece.m_placeEffect;
		//		ppPiece.m_dlc = originalPiece.m_dlc;
		//		ppPiece.m_dlc = originalPiece.m_dlc;*/

		//		if (!pocketPortal.GetComponent<TeleportWorld>())
		//		{
		//			pocketPortal.AddComponent<TeleportWorld>();
		//		}
		//		TeleportWorld ppTeleport = pocketPortal.GetComponent<TeleportWorld>();
		//		TeleportWorld originalTeleport = originalPortal.GetComponent<TeleportWorld>();
		//		if (originalTeleport == null)
		//		{
		//			//MarsarahTweaks.LogError("Original portal prefab is missing TeleportWorld!");
		//			//return;
		//			throw new Exception("Original portal prefab is missing TeleportWorld!");
		//		}
		//		/*ppTeleport.m_activationRange = originalTeleport.m_activationRange;
		//		ppTeleport.m_exitDistance = originalTeleport.m_exitDistance;
		//		ppTeleport.m_proximityRoot = originalTeleport.m_proximityRoot;
		//		ppTeleport.m_colorUnconnected = originalTeleport.m_colorUnconnected;
		//		ppTeleport.m_colorTargetfound = originalTeleport.m_colorTargetfound;
		//		//ppTeleport.m_target_found = originalTeleport.m_target_found;
		//		ppTeleport.m_model = originalTeleport.m_model;
		//		ppTeleport.m_connected = originalTeleport.m_connected;
		//		ppTeleport.m_allowAllItems = originalTeleport.m_allowAllItems;

		//		// Clone target_found properly
		//		var newEffectFade = pocketPortal.GetComponentInChildren<EffectFade>();
		//		if (newEffectFade != null)
		//		{
		//			//MarsarahTweaks.LogWarn("Waking and assigning new EffectFade in cloned portal.");
		//			Traverse.Create(newEffectFade).Method("Awake").GetValue(); // ensure initialized
		//			ppTeleport.m_target_found = newEffectFade;
		//		}
		//		else
		//		{
		//			MarsarahTweaks.LogWarn("No EffectFade found in cloned portal. Falling back to original.");
		//			ppTeleport.m_target_found = originalTeleport.m_target_found;
		//		}*/


		//		// pocketPortal.SetActive(true); // this causes NREs

		//		// Wrap in CustomPrefab with fixReference to fix internal references automatically
		//		CustomPrefab customPrefab = new CustomPrefab(pocketPortal, fixReference: true);

		//		PrefabManager.Instance.AddPrefab(customPrefab);

		//		// Register to ZNetScene explicitly for networking
		//		PrefabManager.Instance.RegisterToZNetScene(pocketPortal);


		//		if (ZNetScene.instance.GetPrefab("pocket_portal") == null)
		//			MarsarahTweaks.LogError("ZNetScene does NOT recognize 'pocket_portal' after registration!");
		//		else
		//			MarsarahTweaks.LogInfo("ZNetScene prefab registration SUCCESS for pocket_portal");

		//		// Create a proper PieceConfig
		//		PieceConfig pieceConfig = new PieceConfig
		//		{
		//			PieceTable = "Hammer",
		//			Category = "Misc",
		//			Enabled = true,
		//			Requirements = new[]
		//			{
		//				new RequirementConfig("SurtlingCore", 1),
		//			}
		//		};

		//		// Add the piece to the PieceManager
		//		CustomPiece customPiece = new CustomPiece(pocketPortal, fixReference: true, pieceConfig);

		//		PieceManager.Instance.AddPiece(customPiece);

		//		pocketPortal.SetActive(true);

		//		MarsarahTweaks.LogInfo("Pocket Portal registered successfully.");
		//	}
		//	catch (Exception e)
		//	{
		//		MarsarahTweaks.LogError($"Exception during Pocket Portal registration:\n{e}");
		//	}
		//}


		// ===============================================================

		/*public static GameObject pocketPortal;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		public static class ZNetSceneAwakePatch
		{
			private static bool _initialized;

			private static void Postfix(ZNetScene __instance)
			{
				if (_initialized) return;
				_initialized = true;

				var pocketPortal = PrefabManager.ClonePrefab("portal_wood", "pocket_portal");
				if (pocketPortal == null)
				{
					MarsarahTweaks.LogError("Failed to clone pocket portal.");
					return;
				}

				PocketPortal.Create(pocketPortal);

				MarsarahTweaks.LogInfo("[PocketPortal: Awake] Pocket portal initialized.");
			}
		}

		[HarmonyPatch(typeof(ZNetScene), "Update")]
		public static class ZNetScene_DelayedInjection
		{
			private static bool injected = false;

			private static void Postfix(ZNetScene __instance)
			{
				if (injected || pocketPortal == null) return;

				MarsarahTweaks.LogInfo("[Pocket Portal: ZNetScene.Update] Injecting Pocket Portal after ZNetScene fully initialized");

				// Inject into m_prefabs and m_namedPrefabs
				PrefabManager.RegisterPrefab(pocketPortal);
				PrefabManager.InjectToZNetScene(__instance);

				if (!pocketPortal.activeSelf)
					pocketPortal.SetActive(true);

				PrefabManager.InjectToObjectDB();
				PrefabManager.AddToHammerBuildMenu(pocketPortal);

				injected = true;
			}
		}

		public static void Create(GameObject clone)
		{
			pocketPortal = clone;

			// Add missing components
			if (!pocketPortal.GetComponent<ZSyncTransform>())
			{
				pocketPortal.AddComponent<ZSyncTransform>();
				MarsarahTweaks.LogInfo("[Pocket Portal: Create] Added missing ZSyncTransform.");
			}

			var znv = pocketPortal.GetComponent<ZNetView>();
			if (znv == null)
			{
				MarsarahTweaks.LogError("[Pocket Portal: Create] Pocket portal is missing ZNetView.");
				return;
			}

			if (znv.m_syncInitialScale)
			{
				MarsarahTweaks.LogInfo("[Pocket Portal: Create] ZNetView is syncing scale.");
			}

			if (!znv.m_persistent)
			{
				MarsarahTweaks.LogWarn("[Pocket Portal: Create] ZNetView is non-persistent. Is that intentional?");
			}

			// Customize piece info
			var piece = pocketPortal.GetComponent<Piece>();
			if (piece != null)
			{
				piece.m_name = "$piece_portal";
				piece.m_description = "$piece_portal_description";
				piece.m_category = Piece.PieceCategory.Misc;
				piece.m_craftingStation = null;
			}

			// Customize teleport visuals
			var tele = pocketPortal.GetComponent<TeleportWorld>();
			if (tele != null)
			{
				if (tele.m_model == null)
				{
					var renderer = pocketPortal.GetComponentInChildren<MeshRenderer>();
					if (renderer != null)
					{
						tele.m_model = renderer;
						MarsarahTweaks.LogInfo("[Pocket Portal: Create] Assigned fallback renderer to tele.m_model.");
					}
					else
					{
						MarsarahTweaks.LogWarn("[Pocket Portal: Create] Could not find fallback renderer.");
					}
				}

				if (tele.m_proximityRoot == null)
				{
					tele.m_proximityRoot = pocketPortal.transform;
					MarsarahTweaks.LogInfo("[Pocket Portal: Create] Assigned fallback proximityRoot.");
				}

				tele.m_colorUnconnected = Color.red;
				tele.m_colorTargetfound = Color.cyan;
			}

			MarsarahTweaks.LogInfo($"[Pocket Portal: Create] Customized pocket portal prefab at {Time.time}.");
		}*/


		// ====================================================================


		/*public static GameObject pocketPortalPrefab;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		static class ZNetSceneAwake_Patch
		{
			static void Postfix(ZNetScene __instance, ref Dictionary<ZDO, ZNetView> ___m_instances)
			{
				CreatePocketPortal();

				PrefabManager.RegisterPrefab(__instance, PocketPortal.pocketPortalPrefab);

				if (pocketPortalPrefab != null && !pocketPortalPrefab.activeSelf)
				{
					pocketPortalPrefab.SetActive(true);
					MarsarahTweaks.LogInfo("Activated pocket_portal prefab after registration.");
				}
			}
		}*/

		/*private static bool _initialized = false;

		public static void CreatePocketPortal()
		{
			if (_initialized)
			{
				MarsarahTweaks.LogInfo("Pocket portal already initialized, skipping.");
				return;
			}

			_initialized = true;

			pocketPortalPrefab = PrefabManager.ClonePrefab("portal_wood", "pocket_portal");
			if (pocketPortalPrefab == null) return;

			if (pocketPortalPrefab.GetComponent<ZSyncTransform>() == null)
			{
				pocketPortalPrefab.AddComponent<ZSyncTransform>();
				MarsarahTweaks.LogInfo("Added missing ZSyncTransform to pocket_portal.");
			}

			ZNetView znv = pocketPortalPrefab.GetComponent<ZNetView>();
			if (znv == null)
			{
				MarsarahTweaks.LogError($"{pocketPortalPrefab.name} is missing ZNetView — this will break networking.");
				return;
			}

			if (znv.m_syncInitialScale)
			{
				MarsarahTweaks.LogInfo("ZNetView is syncing scale.");
			}

			if (!znv.m_persistent)
			{
				MarsarahTweaks.LogWarn($"{pocketPortalPrefab.name} has non-persistent ZNetView — is that intended?");
			}

			// Update the Piece data
			Piece piece = pocketPortalPrefab.GetComponent<Piece>();
			if (piece != null)
			{
				piece.m_name = "$piece_portal"; // Use localization for now
				piece.m_description = "$piece_portal_description";
				piece.m_category = Piece.PieceCategory.Misc;
				piece.m_craftingStation = null; // Optional: limit to no station
			}

			// Update teleport visual colors
			TeleportWorld tele = pocketPortalPrefab.GetComponent<TeleportWorld>();
			if (tele != null)
			{
				if (tele.m_model == null)
				{
					var renderer = pocketPortalPrefab.GetComponentInChildren<MeshRenderer>();
					if (renderer != null)
					{
						tele.m_model = renderer;
						MarsarahTweaks.LogInfo("Assigned fallback renderer to tele.m_model.");
					}
					else
					{
						MarsarahTweaks.LogWarn("TeleportWorld.m_model is null and no MeshRenderer found.");
					}
				}

				if (tele.m_proximityRoot == null)
				{
					tele.m_proximityRoot = pocketPortalPrefab.transform;
					MarsarahTweaks.LogInfo("Assigned fallback proximityRoot.");
				}

				tele.m_colorUnconnected = Color.red;
				tele.m_colorTargetfound = Color.cyan;
			}

			MarsarahTweaks.LogInfo($"Cloned pocket portal from portal_wood at: {Time.time}");
		}*/


		/*[HarmonyPatch(typeof(ZNetScene), "RemoveObjects")]
		class ZNetScene_RemoveObjects_Patch
		{
			static bool Prefix(ZNetScene __instance, List<ZDO> currentNearObjects, List<ZDO> currentDistantObjects, ref List<ZNetView> ___m_tempRemoved, ref Dictionary<ZDO, ZNetView> ___m_instances)
			{
				try
				{
					byte b = (byte)(Time.frameCount & 0xFF);
					foreach (ZDO zdo in currentNearObjects)
					{
						if (zdo == null)
						{
							MarsarahTweaks.LogWarn("RemoveObjects: null ZDO in currentNearObjects");
							continue;
						}
						zdo.TempRemoveEarmark = b;
					}
					foreach (ZDO zdo in currentDistantObjects)
					{
						if (zdo == null)
						{
							MarsarahTweaks.LogWarn("RemoveObjects: null ZDO in currentDistantObjects");
							continue;
						}
						zdo.TempRemoveEarmark = b;
					}

					___m_tempRemoved.Clear();

					MarsarahTweaks.LogInfo($"m_instances count: {___m_instances.Count}");
					int nullKeyCount = 0, nullValueCount = 0;
					foreach (var kvp in ___m_instances)
					{
						if (kvp.Key == null) nullKeyCount++;
						if (kvp.Value == null)
						{
							nullValueCount++;
							MarsarahTweaks.LogInfo($"Key whose value is null: {kvp.Key}");
						}
					}
					MarsarahTweaks.LogInfo($"m_instances null keys: {nullKeyCount}, null values: {nullValueCount}");

					foreach (var znv in ___m_instances.Values)
					{
						if (znv == null)
						{
							MarsarahTweaks.LogWarn("RemoveObjects: null ZNetView in m_instances, skipping...");
							continue;
						}
						var zdo = znv.GetZDO();
						if (zdo == null)
						{
							MarsarahTweaks.LogWarn($"RemoveObjects: ZNetView {znv.name} has null ZDO");
							continue;
						}
						if (zdo.TempRemoveEarmark != b)
						{
							___m_tempRemoved.Add(znv);
						}
					}

					for (int i = 0; i < ___m_tempRemoved.Count; i++)
					{
						var znv = ___m_tempRemoved[i];
						if (znv == null)
						{
							MarsarahTweaks.LogWarn("RemoveObjects: null ZNetView in m_tempRemoved");
							continue;
						}
						var zdo = znv.GetZDO();
						if (zdo == null)
						{
							MarsarahTweaks.LogWarn($"RemoveObjects: ZNetView {znv.name} in m_tempRemoved has null ZDO");
							continue;
						}
						znv.ResetZDO();
						UnityEngine.Object.Destroy(znv.gameObject);
						if (!zdo.Persistent && zdo.IsOwner())
						{
							ZDOMan.instance.DestroyZDO(zdo);
						}
						___m_instances.Remove(zdo);
					}
				}
				catch (Exception e)
				{
					MarsarahTweaks.LogError($"Exception in RemoveObjects: {e}");
				}

				// Skip original RemoveObjects to avoid crashing until issue fixed
				return false;
			}
		}*/


		/*[HarmonyPatch(typeof(Player), "Awake")]
		public static class PocketPortal_ShowOriginalPortal_Patch
		{
			private static void Postfix(Player __instance)
			{
				if (__instance == null) return;
				if (ZNetScene.instance == null || ObjectDB.instance == null) return;
				if (!ConfigManager.PocketPortalEnabled.Value) return;

				var hammerPrefab = ObjectDB.instance?.GetItemPrefab("Hammer");
				if (hammerPrefab == null) return;

				var hammer = hammerPrefab.GetComponent<ItemDrop>();
				if (hammer == null || hammer.m_itemData.m_shared.m_buildPieces == null) return;

				var pieceTable = hammer.m_itemData.m_shared.m_buildPieces;
				if (pieceTable.m_pieces == null) return;

				var portalPrefab = ZNetScene.instance?.GetPrefab("pocket_portal");
				if (portalPrefab == null)
				{
					MarsarahTweaks.LogWarn("Could not get prefab for pocket portal.");
					return;
				}

				// Check for all portal components
				Transform pocketPortalTransform = portalPrefab.GetComponent<Transform>();
				if (pocketPortalTransform == null)
				{
					MarsarahTweaks.LogWarn("Could not get Transform component for pocket portal.");
					return;
				}

				Piece pocketPortalPiece = portalPrefab.GetComponent<Piece>();
				if (pocketPortalPiece == null)
				{
					MarsarahTweaks.LogWarn("Could not get Piece component for pocket portal.");
					return;
				}
				pocketPortalPiece.m_craftingStation = null;
				
				ZNetView pocketPortalZNetView = portalPrefab.GetComponent<ZNetView>();
				if (pocketPortalZNetView == null)
				{
					MarsarahTweaks.LogWarn("Could not get ZNetView component for pocket portal.");
					return;
				}

				//MarsarahTweaks.LogInfo($"ZNetView is valid: {pocketPortalZNetView.IsValid()}");

				WearNTear pocketPortalWearNTear = portalPrefab.GetComponent<WearNTear>();
				if ( pocketPortalWearNTear == null)
				{
					MarsarahTweaks.LogWarn("Could not get WearNTear component for pocket portal.");
					return;
				}
				
				TeleportWorld pocketPortalTeleport = portalPrefab.GetComponent<TeleportWorld>();
				if (pocketPortalTeleport == null)
				{
					MarsarahTweaks.LogWarn("Could not get TeleportWorld component for pocket portal.");
					return;
				}

				//MarsarahTweaks.LogInfo($"m_model: {pocketPortalTeleport.m_model}");
				//MarsarahTweaks.LogInfo($"m_proximityRoot: {pocketPortalTeleport.m_proximityRoot}");
				//MarsarahTweaks.LogInfo($"m_connected: {pocketPortalTeleport.m_connected.m_effectPrefabs.Length}");

				if (!pieceTable.m_pieces.Contains(portalPrefab))
				{
					pieceTable.m_pieces.Add(portalPrefab);
					MarsarahTweaks.LogInfo("Added pocket portal to hammer build pieces.");
				}
			}
		}*/


		// =======================================================

		/*[HarmonyPatch(typeof(Player), "Awake")]
		public static class PocketPortal_ShowOriginalPortal_Patch
		{
			private static void Postfix(Player __instance)
			{
				if (!ConfigManager.PocketPortalEnabled.Value) return;

				var hammerPrefab = ObjectDB.instance?.GetItemPrefab("Hammer");
				if (hammerPrefab == null) return;

				var hammer = hammerPrefab.GetComponent<ItemDrop>();
				if (hammer == null || hammer.m_itemData.m_shared.m_buildPieces == null) return;

				var pieceTable = hammer.m_itemData.m_shared.m_buildPieces;
				if (pieceTable.m_pieces == null) return;

				var portalPrefab = ZNetScene.instance?.GetPrefab("portal");
				if (portalPrefab == null) return;

				Piece portalPiece = portalPrefab.GetComponent<Piece>();
				//AddLocalizationWord("piece_pocket_portal", "Pocket Portal");
				//AddLocalizationWord("piece_pocket_portal_desc", "A magical portal that can be used for emergency cases");
				//portalPiece.m_name = "$piece_pocket_portal";
				//portalPiece.m_description = "$piece_pocket_portal_desc";
				portalPiece.m_category = Piece.PieceCategory.Misc;

				// Set the worstation to forge for testing
				GameObject forgePrefab = ZNetScene.instance?.GetPrefab("forge");
				if (forgePrefab != null)
				{
					//MarsarahTweaks.LogInfo("Forge prefab found");
					CraftingStation station = forgePrefab.GetComponent<CraftingStation>();
					if (station != null)
					{
						//MarsarahTweaks.LogInfo("Station component found");
						portalPiece.m_craftingStation = station;
					}
				}

				// Set resource costs
				portalPiece.m_resources = new Piece.Requirement[]
				{
					new Piece.Requirement
					{
						m_resItem = ObjectDB.instance.GetItemPrefab("GreydwarfEye").GetComponent<ItemDrop>(),
						m_amount = 10,
						m_recover = true
					},
					new Piece.Requirement
					{
						m_resItem = ObjectDB.instance.GetItemPrefab("Stone").GetComponent<ItemDrop>(),
						m_amount = 10,
						m_recover = true
					},
					new Piece.Requirement
					{
						m_resItem = ObjectDB.instance.GetItemPrefab("SurtlingCore").GetComponent<ItemDrop>(),
						m_amount = 2,
						m_recover = true
					}
				};

				if (!pieceTable.m_pieces.Contains(portalPrefab))
				{
					pieceTable.m_pieces.Add(portalPrefab);
					MarsarahTweaks.LogInfo("Added portal to hammer build pieces.");
				}
			}
		}

		private static void AddLocalizationWord(string key, string value)
		{
			var loc = Localization.instance;
			if (loc == null) return;

			var method = typeof(Localization).GetMethod("AddWord", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(loc, new object[] { key, value });
			}
			else
			{
				MarsarahTweaks.LogWarn("Could not find AddWord method via reflection.");
			}
		}*/
	}
}
