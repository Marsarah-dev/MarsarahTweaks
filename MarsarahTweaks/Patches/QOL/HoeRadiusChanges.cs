
using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.QOL
{
	internal static class HoeRadiusChanges
	{
		private static readonly LogManager log = new LogManager("Hoe Radius", LogManager.LogLevel.Warning);

		private const float RadiusStep = 1f;
		private const float MinRadius = 1f;
		private const float PathenMinRadius = 2f;
		private const float MaxRadius = 7f;

		private const int RadiusWireMagic = 0x4D545248;
		private const int RadiusWireVersion = 1;
		private static readonly HashSet<int> SupportedTerrainHashes = new HashSet<int>
		{
			"mud_road_v2".GetStableHashCode(),
			"path_v2".GetStableHashCode(),
			"paved_road_v2".GetStableHashCode(),
			"MarsarahTweaks_Paving".GetStableHashCode(),
			"cultivate_v2".GetStableHashCode(),
			"replant_v2".GetStableHashCode()
		};

		private static TerrainOp activeOperation;
		private static float originalLevel;
		private static float originalRaise;
		private static float originalSmooth;
		private static float originalPaint;
		private static float defaultRadius;
		private static float currentRadius;

		private static Transform previewTransform;
		private static Vector3 originalPreviewScale;
		private static Transform pathenParticleTransform;
		private static Vector3 originalPathenParticleScale;
		private static bool interceptPlacementScroll;
		private static bool suppressCameraZoom;

		private static readonly AccessTools.FieldRef<Player, PieceTable> buildPiecesRef = AccessTools.FieldRefAccess<Player, PieceTable>("m_buildPieces");
		private static readonly Dictionary<string, float> savedRadii = new Dictionary<string, float>();

		[HarmonyPatch(typeof(Player), "UpdatePlacement")]
		private static class Player_UpdatePlacement_Patch
		{
			private static void Prefix(Player __instance, bool takeInput, PieceTable ___m_buildPieces, GameObject ___m_placementGhost)
			{
				interceptPlacementScroll = false;

				if (!ConfigManager.AdjustableHoeRadiusEnabled.Value || !TryGetSupportedOperation(__instance, ___m_buildPieces, out TerrainOp operation))
				{
					RestoreOriginalSettings();
					return;
				}

				if (activeOperation != operation)
					SelectOperation(operation);

				if (activeOperation == null)
					return;

				UpdatePreview(___m_placementGhost);

				if (!takeInput || Hud.IsPieceSelectionVisible())
					return;

				float scroll = ZInput.GetMouseScrollWheel();

				if (Mathf.Abs(scroll) < 0.001f)
					return;

				// Consume the scroll only for our six supported operations.
				interceptPlacementScroll = true;

				float newRadius = Mathf.Clamp(currentRadius + Mathf.Sign(scroll) * RadiusStep, GetMinimumRadius(activeOperation), MaxRadius);

				if (Mathf.Approximately(newRadius, currentRadius))
					return;

				currentRadius = newRadius;
				savedRadii[activeOperation.gameObject.name] = currentRadius;

				ApplyRadius(activeOperation.m_settings, currentRadius / defaultRadius);

				UpdatePreview(___m_placementGhost);

				log.Info($"Adjusted '{activeOperation.name}' radius to {currentRadius:0.##}m.");
			}

			private static void Postfix()
			{
				interceptPlacementScroll = false;
			}

			private static Exception Finalizer(Exception __exception)
			{
				interceptPlacementScroll = false;
				return __exception;
			}
		}

		[HarmonyPatch(typeof(ZInput), nameof(ZInput.GetMouseScrollWheel))]
		private static class ZInput_GetMouseScrollWheel_Patch
		{
			private static void Postfix(ref float __result)
			{
				if (interceptPlacementScroll || suppressCameraZoom)
					__result = 0f;
			}
		}

		[HarmonyPatch(typeof(Player), "Update")]
		private static class Player_Update_Patch
		{
			private static void Postfix(Player __instance)
			{
				if (__instance != Player.m_localPlayer)
					return;

				if (!ConfigManager.AdjustableHoeRadiusEnabled.Value || !__instance.InPlaceMode())
					RestoreOriginalSettings();
			}
		}

		[HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
		private static class GameCamera_UpdateCamera_Patch
		{
			private static void Prefix()
			{
				suppressCameraZoom = false;

				Player player = Player.m_localPlayer;

				if (player == null || !ConfigManager.AdjustableHoeRadiusEnabled.Value || !player.InPlaceMode() || Hud.IsPieceSelectionVisible())
					return;

				suppressCameraZoom = TryGetSupportedOperation(player, buildPiecesRef(player), out _);
			}

			private static Exception Finalizer(Exception __exception)
			{
				suppressCameraZoom = false;
				return __exception;
			}
		}

		[HarmonyPatch(typeof(TerrainOp.Settings), "Serialize")]
		private static class TerrainSettings_Serialize_Patch
		{
			private static void Postfix(ZPackage __0, GameObject __1)
			{
				if (!ConfigManager.AdjustableHoeRadiusEnabled.Value || activeOperation == null || __0 == null || __1 == null || Player.m_localPlayer == null)
					return;
				string prefabName = __1.name;
				if (prefabName.EndsWith("(Clone)", StringComparison.Ordinal))
					prefabName = prefabName.Substring(0, prefabName.Length - "(Clone)".Length);
				if (prefabName != activeOperation.name)
					return;
				int prefabHash = prefabName.GetStableHashCode();
				if (!SupportedTerrainHashes.Contains(prefabHash))
					return;
				__0.Write(RadiusWireMagic);
				__0.Write(RadiusWireVersion);
				__0.Write(prefabHash);
				__0.Write(currentRadius);
				log.Info($"Sent terrain radius: Piece={prefabName}, Radius={currentRadius:0.##}m, Hash={prefabHash}.");
			}
		}

		[HarmonyPatch(typeof(TerrainOp.Settings), "Deserialize")]
		private static class TerrainSettings_Deserialize_Patch
		{
			private static void Postfix(ZPackage __0, ref TerrainOp.Settings __result)
			{
				if (!ConfigManager.AdjustableHoeRadiusEnabled.Value || __0 == null || __result == null)
					return;
				int start = __0.GetPos();
				if (start < sizeof(int) || __0.Size() - start < sizeof(int))
					return;
				if (__0.ReadInt() != RadiusWireMagic)
				{
					__0.SetPos(start);
					return;
				}
				if (__0.Size() - __0.GetPos() < 12)
				{
					log.Warn("Received incomplete terrain radius data. Using vanilla settings.");
					__0.SetPos(start);
					return;
				}
				int version = __0.ReadInt();
				int prefabHash = __0.ReadInt();
				float radius = __0.ReadSingle();
				int end = __0.GetPos();
				__0.SetPos(start - sizeof(int));
				int serializedHash = __0.ReadInt();
				__0.SetPos(end);
				float minimum = prefabHash == "path_v2".GetStableHashCode() ? PathenMinRadius : MinRadius;
				if (version != RadiusWireVersion || serializedHash != prefabHash || !SupportedTerrainHashes.Contains(prefabHash) || float.IsNaN(radius) || float.IsInfinity(radius) || radius < minimum || radius > MaxRadius)
				{
					log.Warn("Rejected invalid terrain radius data. Using vanilla settings.");
					__0.SetPos(start);
					return;
				}
				if (__result.m_raise || (!__result.m_level && !__result.m_smooth && !__result.m_paintCleared))
				{
					log.Warn("Received radius data for an unexpected terrain operation. Using vanilla settings.");
					return;
				}
				float originalRadius = GetEffectiveRadius(__result);
				if (originalRadius <= 0f)
				{
					log.Warn("Received terrain operation has no valid radius. Using vanilla settings.");
					return;
				}
				TerrainOp.Settings settings = CloneSettings(__result);
				float scale = radius / originalRadius;
				if (settings.m_level)
					settings.m_levelRadius *= scale;
				if (settings.m_raise)
					settings.m_raiseRadius *= scale;
				if (settings.m_smooth)
					settings.m_smoothRadius *= scale;
				if (settings.m_paintCleared)
					settings.m_paintRadius *= scale;
				__result = settings;
				log.Info($"Applied network terrain radius: Hash={prefabHash}, Radius={radius:0.##}m, Level={settings.m_levelRadius:0.##}, Smooth={settings.m_smoothRadius:0.##}, Paint={settings.m_paintRadius:0.##}.");
			}
		}

		private static bool TryGetSupportedOperation(Player player, PieceTable pieceTable, out TerrainOp operation)
		{
			operation = null;

			if (player != Player.m_localPlayer || pieceTable == null || ObjectDB.instance == null)
				return false;

			GameObject hoe = ObjectDB.instance.GetItemPrefab("Hoe");
			GameObject cultivator = ObjectDB.instance.GetItemPrefab("Cultivator");

			PieceTable hoeTable = hoe?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces;
			PieceTable cultivatorTable = cultivator?.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces;

			bool isHoe = hoeTable != null && pieceTable == hoeTable;
			bool isCultivator = cultivatorTable != null && pieceTable == cultivatorTable;

			if (!isHoe && !isCultivator)
				return false;

			GameObject prefab = pieceTable.GetSelectedPrefab();
			if (prefab == null)
				return false;

			string name = prefab.name;

			bool supported = isHoe
				? name == "mud_road_v2" || name == "path_v2" || name == "paved_road_v2" || name == "MarsarahTweaks_Paving"
				: name == "cultivate_v2" || name == "replant_v2";

			if (!supported)
				return false;

			operation = prefab.GetComponent<TerrainOp>();
			return operation != null && operation.m_settings != null;
		}

		private static void SelectOperation(TerrainOp operation)
		{
			RestoreOriginalSettings();

			activeOperation = operation;
			TerrainOp.Settings settings = operation.m_settings;

			originalLevel = settings.m_levelRadius;
			originalRaise = settings.m_raiseRadius;
			originalSmooth = settings.m_smoothRadius;
			originalPaint = settings.m_paintRadius;

			defaultRadius = 0f;

			if (settings.m_level)
				defaultRadius = Mathf.Max(defaultRadius, originalLevel);
			if (settings.m_raise)
				defaultRadius = Mathf.Max(defaultRadius, originalRaise);
			if (settings.m_smooth)
				defaultRadius = Mathf.Max(defaultRadius, originalSmooth);
			if (settings.m_paintCleared)
				defaultRadius = Mathf.Max(defaultRadius, originalPaint);

			if (defaultRadius <= 0f)
			{
				log.Warn($"No active terrain radius found for '{operation.name}'.");
				activeOperation = null;
				return;
			}

			if (savedRadii.TryGetValue(operation.gameObject.name, out float savedRadius))
			{
				currentRadius = Mathf.Clamp(savedRadius, GetMinimumRadius(operation), MaxRadius);
				ApplyRadius(settings, currentRadius / defaultRadius);
			}
			else
			{
				currentRadius = defaultRadius;
			}

			log.Info($"Selected '{operation.name}' with radius {currentRadius:0.##}m.");
		}

		private static void ApplyRadius(TerrainOp.Settings settings, float scale)
		{
			if (settings == null)
				return;

			settings.m_levelRadius = settings.m_level ? originalLevel * scale : originalLevel;
			settings.m_raiseRadius = settings.m_raise ? originalRaise * scale : originalRaise;
			settings.m_smoothRadius = settings.m_smooth ? originalSmooth * scale : originalSmooth;
			settings.m_paintRadius = settings.m_paintCleared ? originalPaint * scale : originalPaint;
		}

		private static void UpdatePreview(GameObject ghost)
		{
			if (ghost == null || activeOperation == null || defaultRadius <= 0f)
				return;

			float scale = currentRadius / defaultRadius;

			TerrainOp ghostOperation = ghost.GetComponent<TerrainOp>();
			if (ghostOperation != null)
				ApplyRadius(ghostOperation.m_settings, scale);

			Transform indicator = ghost.transform.Find("_GhostOnly");

			if (indicator == null)
				indicator = ghost.transform.Find("GhostOnly");

			if (indicator != previewTransform)
			{
				RestorePreviewScale();

				previewTransform = indicator;

				if (previewTransform != null)
				{
					originalPreviewScale = previewTransform.localScale;

					// Pathen's nested particle uses Local scaling and ignores its parent's scale.
					if (activeOperation.name == "path_v2")
					{
						ParticleSystem particle = previewTransform.GetComponentInChildren<ParticleSystem>(true);

						if (particle != null && particle.transform != previewTransform && particle.main.scalingMode == ParticleSystemScalingMode.Local)
						{
							pathenParticleTransform = particle.transform;
							originalPathenParticleScale = pathenParticleTransform.localScale;

							log.Info("Detected Pathen's local-scaled ghost particle.");
						}
						else
						{
							log.Warn("Could not find Pathen's expected local-scaled ghost particle.");
						}
					}
				}
			}

			if (previewTransform != null)
				previewTransform.localScale = originalPreviewScale * scale;

			// Scale Pathen's nested particle directly because it ignores the parent scale.
			if (pathenParticleTransform != null)
				pathenParticleTransform.localScale = originalPathenParticleScale * scale;
		}

		private static void RestorePreviewScale()
		{
			if (pathenParticleTransform != null)
				pathenParticleTransform.localScale = originalPathenParticleScale;

			pathenParticleTransform = null;

			if (previewTransform != null)
				previewTransform.localScale = originalPreviewScale;

			previewTransform = null;
		}

		private static void RestoreOriginalSettings()
		{
			if (activeOperation != null)
			{
				TerrainOp.Settings settings = activeOperation.m_settings;

				settings.m_levelRadius = originalLevel;
				settings.m_raiseRadius = originalRaise;
				settings.m_smoothRadius = originalSmooth;
				settings.m_paintRadius = originalPaint;
			}

			RestorePreviewScale();

			activeOperation = null;
			defaultRadius = 0f;
			currentRadius = 0f;
		}

		private static float GetMinimumRadius(TerrainOp operation)
		{
			return operation != null && operation.name == "path_v2" ? PathenMinRadius : MinRadius;
		}

		private static float GetEffectiveRadius(TerrainOp.Settings settings)
		{
			float radius = 0f;
			if (settings.m_level)
				radius = Mathf.Max(radius, settings.m_levelRadius);
			if (settings.m_raise)
				radius = Mathf.Max(radius, settings.m_raiseRadius);
			if (settings.m_smooth)
				radius = Mathf.Max(radius, settings.m_smoothRadius);
			if (settings.m_paintCleared)
				radius = Mathf.Max(radius, settings.m_paintRadius);
			return radius;
		}
		private static TerrainOp.Settings CloneSettings(TerrainOp.Settings source)
		{
			return new TerrainOp.Settings
			{
				m_levelOffset = source.m_levelOffset,
				m_level = source.m_level,
				m_levelRadius = source.m_levelRadius,
				m_square = source.m_square,
				m_raise = source.m_raise,
				m_raiseRadius = source.m_raiseRadius,
				m_raisePower = source.m_raisePower,
				m_raiseDelta = source.m_raiseDelta,
				m_smooth = source.m_smooth,
				m_smoothRadius = source.m_smoothRadius,
				m_smoothPower = source.m_smoothPower,
				m_paintCleared = source.m_paintCleared,
				m_paintHeightCheck = source.m_paintHeightCheck,
				m_paintType = source.m_paintType,
				m_paintRadius = source.m_paintRadius,
				m_paintStrength = source.m_paintStrength,
				m_paintExp = source.m_paintExp,
				m_paintCurve = source.m_paintCurve,
				m_rotation = source.m_rotation,
				m_sides = source.m_sides,
				m_addMedianMax = source.m_addMedianMax,
				m_centerMultiplicationFactor = source.m_centerMultiplicationFactor,
				m_pointMultiplicationFactor = source.m_pointMultiplicationFactor,
				m_halfOffset = source.m_halfOffset
			};
		}
	}
}
