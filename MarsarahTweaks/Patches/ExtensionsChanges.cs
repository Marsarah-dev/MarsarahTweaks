using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches
{
	internal class ExtensionsChanges
	{
		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class BuildPiecesModifications_Patch
		{
			static void Postfix(ref ZNetScene __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ZNetScene Awake: Updating {ConfigManager.Configs.ExtensionsModifications.Name}...");

						UpdateExtensions(__instance);
					}
					else
					{
						MarsarahTweaks.MLog($"ZNetScene Awake: I am a server. No changes made to {ConfigManager.Configs.ExtensionsModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ZNetScene Awake: Too early to do anything. No changes made to {ConfigManager.Configs.ExtensionsModifications.Name}...");
				}
			}
		}

		[HarmonyPatch(typeof(StationExtension), "Awake")]
		class ExtensionChangesDistance_Patch
		{
			private static void Postfix(ref float ___m_maxStationDistance)
			{
				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						if (ConfigManager.extensionsChangesEnabled.Value)
						{
							//MarsarahTweaks.MLog($"StationExtension Awake: Updating {ConfigManager.Configs.ExtensionsModifications.Name}...");

							___m_maxStationDistance = 7f;
						}
					}
					else
					{
						MarsarahTweaks.MLog($"StationExtension Awake: I am a server. No changes made to {ConfigManager.Configs.ExtensionsModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"StationExtension Awake: Too early to do anything. No changes made to {ConfigManager.Configs.ExtensionsModifications.Name}...");
				}
			}
		}

		private static Dictionary <string, float> originalSpaceRequirements = new Dictionary<string, float>();
		private static Dictionary <string, float> originalStationDistance = new Dictionary<string, float>();

		public static void UpdateExtensions(ZNetScene znScene)
		{
			int modifiedPieces = 0;
			int restoredPieces = 0;

			foreach (GameObject piece in znScene.m_prefabs)
			{
				Piece actualPiece = piece.GetComponent<Piece>();
				if (actualPiece == null) continue;

				string pieceKey = actualPiece.m_name;

				if (ConfigManager.extensionsChangesEnabled.Value)
				{
					if (actualPiece.m_spaceRequirement >= 2)
					{
						// Backup
						if (!originalSpaceRequirements.TryGetValue(pieceKey, out float originalSpaceRequirement))
						{
							originalSpaceRequirements[pieceKey] = actualPiece.m_spaceRequirement;
						}

						// Apply new values
						actualPiece.m_spaceRequirement = 1;
						modifiedPieces++;
					}
				}
				else if (originalSpaceRequirements.TryGetValue(pieceKey, out float originalSpaceRequirement))
				{
					// Restore
					actualPiece.m_spaceRequirement = originalSpaceRequirement;
					restoredPieces++;
				}
			}

			if (modifiedPieces > 0)
			{
				//MarsarahTweaks.MLog($"Updated {modifiedPieces} build pieces to require less space.");
			}
			if (restoredPieces > 0)
			{
				//MarsarahTweaks.MLog($"Restored {restoredPieces} build pieces to original space requirements.");
			}
		}
	}
}
