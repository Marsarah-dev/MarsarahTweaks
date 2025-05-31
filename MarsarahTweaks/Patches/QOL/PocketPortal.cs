using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	class PocketPortal
	{
		/*private CustomPiece pocketPortalPiece;

		public void Init()
		{
			PrefabManager.OnVanillaPrefabsAvailable += RegisterPocketPortal;
		}

		private void RegisterPocketPortal()
		{
			// 1. Clone the vanilla portal prefab
			GameObject original = PrefabManager.Instance.GetPrefab("portal_wood");
			if (original == null)
			{
				Jotunn.Logger.LogError("Could not find portal_wood prefab.");
				return;
			}

			GameObject clone = UnityEngine.Object.Instantiate(original);
			clone.name = "pocket_portal";

			// 2. Wrap it in a CustomPiece with a PieceConfig
			var config = new PieceConfig
			{
				PieceTable = "Hammer",
				Category = "Misc",
				ExtendStation = true,
				CraftingStation = "piece_workbench"
			};

			pocketPortalPiece = new CustomPiece(clone, false, config);

			// 3. Register the prefab and piece with Jotunn
			PrefabManager.Instance.AddPrefab(clone);
			PieceManager.Instance.AddPiece(pocketPortalPiece);

			Jotunn.Logger.LogInfo("Pocket Portal registered successfully.");
		}*/

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
