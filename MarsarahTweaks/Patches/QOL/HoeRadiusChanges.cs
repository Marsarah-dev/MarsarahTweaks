
using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace MarsarahTweaks.Patches.QOL
{
	internal static class HoeRadiusChanges
	{
		private static readonly LogManager log = new LogManager("Hoe Radius", LogManager.LogLevel.Info);

		private const float RadiusStep = 1f;
		private const float MinRadius = 1f;
		private const float MaxRadius = 7f;

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

				float newRadius = Mathf.Clamp(currentRadius + Mathf.Sign(scroll) * RadiusStep, MinRadius, MaxRadius);

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
				currentRadius = Mathf.Clamp(savedRadius, MinRadius, MaxRadius);
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
	}
}
