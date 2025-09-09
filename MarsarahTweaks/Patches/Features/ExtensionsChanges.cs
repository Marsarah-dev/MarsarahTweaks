using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Features
{
	internal class ExtensionsChanges
	{
		private static readonly LogManager log = new LogManager("Extension Changes", LogManager.LogLevel.Warning);

		internal static bool lastExtensionSetting = ConfigManager.ExtensionsChangesEnabled.Value;

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class BuildPiecesModifications_Patch
		{
			static void Postfix(ref ZNetScene __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateExtensionsSpace(__instance);

				// Add watcher
				if (GameObject.FindFirstObjectByType<ExtensionWatcher>() == null)
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
			foreach (GameObject piece in znScene.m_prefabs)
			{
				Piece actualPiece = piece.GetComponent<Piece>();
				if (actualPiece == null) continue;

				string pieceName = actualPiece.m_name;

				if (ConfigManager.ExtensionsChangesEnabled.Value)
				{
					if (actualPiece.m_spaceRequirement >= 2)
					{
						// Backup
						if (!originalSpaceRequirements.TryGetValue(pieceName, out float originalSpaceRequirement))
						{
							log.Info($"Backing up space requirement for {pieceName}");
							originalSpaceRequirements[pieceName] = actualPiece.m_spaceRequirement;
						}

						// Apply new values
						actualPiece.m_spaceRequirement = 1;
					}
				}
				else if (originalSpaceRequirements.TryGetValue(pieceName, out float originalSpaceRequirement))
				{
					// Restore
					log.Info($"Restoring space requirement for {pieceName}");
					actualPiece.m_spaceRequirement = originalSpaceRequirement;

					originalSpaceRequirements.Remove(pieceName);
				}
			}
		}

		private static void UpdateExtensionsRange(StationExtension extension)
		{
			string extensioName = extension.name;

			if (ConfigManager.ExtensionsChangesEnabled.Value)
			{
				if (!originalStationDistance.ContainsKey(extensioName))
				{
					log.Info($"Backing up build distance for {extensioName}");
					originalStationDistance[extensioName] = extension.m_maxStationDistance;
				}

				extension.m_maxStationDistance = 7f;
			}
			else if (originalStationDistance.TryGetValue(extensioName, out float original))
			{
				log.Info($"Restoring build distance for {extensioName}");
				extension.m_maxStationDistance = original;

				originalStationDistance.Remove(extensioName);
			}
		}


		internal static void UpdateAllExtensions()
		{
			UpdateExtensionsSpace(ZNetScene.instance);

			foreach (StationExtension ext in GameObject.FindObjectsByType<StationExtension>(FindObjectsSortMode.None))
			{
				UpdateExtensionsRange(ext);
			}
		}
	}

	public class ExtensionWatcher : MonoBehaviour
	{
		void Update()
		{
			bool currentSetting = ConfigManager.ExtensionsChangesEnabled.Value;

			if (ExtensionsChanges.lastExtensionSetting != currentSetting)
			{
				ExtensionsChanges.lastExtensionSetting = currentSetting;
				ExtensionsChanges.UpdateAllExtensions();
			}
		}
	}
}
