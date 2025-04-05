using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class ExtensionsChanges
	{
		internal static bool lastExtensionSetting = ConfigManager.extensionsChangesEnabled.Value;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class BuildPiecesModifications_Patch
		{
			static void Postfix(ref ZNetScene __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateExtensionsSpace(__instance);

				// Add watcher
				if (GameObject.FindObjectOfType<ExtensionWatcher>() == null)
				{
					GameObject watcher = new GameObject("ExtensionWatcher");
					GameObject.DontDestroyOnLoad(watcher);
					watcher.AddComponent<ExtensionWatcher>();
				}
			}
		}

		[HarmonyPatch(typeof(StationExtension), "Awake")]
		class ExtensionChangesDistance_Patch
		{
			private static void Postfix(StationExtension __instance/*, ref float ___m_maxStationDistance*/)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateExtensionsRange(__instance);
			}
		}

		private static readonly Dictionary<string, float> originalSpaceRequirements = new Dictionary<string, float>();
		private static readonly Dictionary<string, float> originalStationDistance = new Dictionary<string, float>();

		private static void UpdateExtensionsSpace(ZNetScene znScene)
		{
			//int modifiedPieces = 0;
			//int restoredPieces = 0;

			foreach (GameObject piece in znScene.m_prefabs)
			{
				Piece actualPiece = piece.GetComponent<Piece>();
				if (actualPiece == null) continue;

				string pieceName = actualPiece.m_name;

				if (ConfigManager.extensionsChangesEnabled.Value)
				{
					if (actualPiece.m_spaceRequirement >= 2)
					{
						// Backup
						if (!originalSpaceRequirements.TryGetValue(pieceName, out float originalSpaceRequirement))
						{
							//MarsarahTweaks.MLog($"Backing up space requirement for {pieceName}");
							originalSpaceRequirements[pieceName] = actualPiece.m_spaceRequirement;
						}

						// Apply new values
						actualPiece.m_spaceRequirement = 1;
						//modifiedPieces++;
					}
				}
				else if (originalSpaceRequirements.TryGetValue(pieceName, out float originalSpaceRequirement))
				{
					// Restore
					//MarsarahTweaks.MLog($"Restoring space requirement for {pieceName}");
					actualPiece.m_spaceRequirement = originalSpaceRequirement;

					originalSpaceRequirements.Remove(pieceName);
					//restoredPieces++;
				}
			}

			/*if (modifiedPieces > 0)
			{
				MarsarahTweaks.MLog($"Updated {modifiedPieces} build pieces to require less space.");
			}
			if (restoredPieces > 0)
			{
				MarsarahTweaks.MLog($"Restored {restoredPieces} build pieces to original space requirements.");
			}*/
		}

		private static void UpdateExtensionsRange(StationExtension extension)
		{
			// string extensioName = extension.name ?? extension.GetInstanceID().ToString();
			string extensioName = extension.name;

			if (ConfigManager.extensionsChangesEnabled.Value)
			{
				if (!originalStationDistance.ContainsKey(extensioName))
				{
					//MarsarahTweaks.MLog($"Backing up build distance for {extensioName}");
					originalStationDistance[extensioName] = extension.m_maxStationDistance;
				}

				extension.m_maxStationDistance = 7f;
			}
			else if (originalStationDistance.TryGetValue(extensioName, out float original))
			{
				//MarsarahTweaks.MLog($"Restoring build distance for {extensioName}");
				extension.m_maxStationDistance = original;

				originalStationDistance.Remove(extensioName);
			}
		}


		internal static void UpdateAllExtensions()
		{
			UpdateExtensionsSpace(ZNetScene.instance);

			foreach (StationExtension ext in GameObject.FindObjectsOfType<StationExtension>())
			{
				UpdateExtensionsRange(ext);
			}
		}
	}

	public class ExtensionWatcher : MonoBehaviour
	{
		void Update()
		{
			bool currentSetting = ConfigManager.extensionsChangesEnabled.Value;

			if (ExtensionsChanges.lastExtensionSetting != currentSetting)
			{
				ExtensionsChanges.lastExtensionSetting = currentSetting;
				ExtensionsChanges.UpdateAllExtensions();
			}
		}
	}
}
