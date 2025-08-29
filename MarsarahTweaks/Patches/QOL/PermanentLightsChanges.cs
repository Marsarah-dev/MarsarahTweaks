using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal class PermanentLightsChanges
	{
		private static readonly LogManager log = new LogManager("Permanent Lights", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
		class PermanentLights_Patch
		{
			private static void Postfix(Fireplace __instance, ref ZNetView ___m_nview)
			{
				// Ensure this only runs on the server (or the owner of the fireplace)
				if (___m_nview.IsOwner())
				{
					if (ConfigManager.PermanentLightsEnabled.Value)
					{
						log.Info($"Setting max fuel for {__instance.m_name}");
						___m_nview.GetZDO().Set("fuel", __instance.m_maxFuel);
					}
					/*else
					{
						__instance.m_secPerFuel = 2f; // This makes fuel being consumed every 2s
						//log.Info($"No longer setting max fuel for {__instance.m_name}");
					}*/
				}
			}
		}


		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class PermanentLightsBuildPieces_Patch
		{
			static void Postfix(ZNetScene __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateLightBuildPiecesAmounts(__instance, false);
			}
		}

		private static readonly Dictionary<string, Dictionary<string, int>> originalLightPieceCosts = new Dictionary<string, Dictionary<string, int>>();
		public static readonly Dictionary<string, Dictionary<string, int>> lightPieceCostChanges = new Dictionary<string, Dictionary<string, int>>()
		{
			{ "$piece_firepit",             new Dictionary<string, int>() { { "Wood", 10 } } },
			{ "$piece_hearth",              new Dictionary<string, int>() { { "Wood", 20 } } },
			{ "$piece_sconce",              new Dictionary<string, int>() { { "Resin", 6 } } },
			{ "$piece_groundtorch",         new Dictionary<string, int>() { { "Resin", 6 } } },
			{ "$piece_groundtorchwood",     new Dictionary<string, int>() { { "Resin", 4 } } },
			{ "$piece_groundtorchgreen",    new Dictionary<string, int>() { { "Guck", 6 } } },
			{ "$piece_groundtorchblue",     new Dictionary<string, int>() { { "GreydwarfEye", 6 } } },
			{ "$piece_brazierfloor01",      new Dictionary<string, int>() { { "Coal", 5 } } },
			{ "$piece_brazierfloor02",      new Dictionary<string, int>() { { "GreydwarfEye", 5 } } },
			{ "$piece_brazierceiling01",    new Dictionary<string, int>() { { "Coal", 5 } } }
		};

		public static void UpdateLightBuildPiecesAmounts(ZNetScene znScene, bool wasChanged)
		{
			if (ConfigManager.PermanentLightsEnabled.Value)
			{
				foreach (var pieceName in lightPieceCostChanges.Keys)
				{
					GameObject prefab = null;

					if (pieceName == "$piece_firepit")
					{
						prefab = znScene.m_prefabs.Find(p =>
						{
							Piece piece = p.GetComponent<Piece>();
							return piece != null
								&& !string.IsNullOrEmpty(piece.m_name)
								&& piece.m_name.Equals(pieceName, StringComparison.OrdinalIgnoreCase)
								&& p.name == "fire_pit"; // Ensures we get the correct prefab (it returns the BogWitch_Fire_Pit otherwise)
						});
					}
					else
					{
						prefab = znScene.m_prefabs.Find(p => p.GetComponent<Piece>()?.m_name == pieceName);
					}

					if (prefab == null)
					{
						log.Warn($"{pieceName} prefab not found in znScene!");
						continue;
					}

					Piece component = prefab.GetComponent<Piece>();
					if (component == null)
					{
						log.Warn($"{pieceName} - Found prefab, but no Piece component!");
						continue;
					}

					// Backup original costs only if they haven't been backed up yet
					if (!originalLightPieceCosts.ContainsKey(pieceName))
					{
						log.Info($"Backing up {pieceName}");
						originalLightPieceCosts[pieceName] = component.m_resources.ToDictionary(req => req.m_resItem.name, req => req.m_amount);
					}

					// Apply cost modifications
					foreach (Piece.Requirement req in component.m_resources)
					{
						if (lightPieceCostChanges[pieceName].TryGetValue(req.m_resItem.name, out int newAmount))
						{
							log.Info($"Applying new amount for {pieceName} - {req.m_resItem.name}");
							req.m_amount = newAmount;
							if (req.m_resItem.name == "Wood") req.m_recover = true;
						}
					}

					// Special case: Hearth requires extra wood requirement
					if (pieceName == "$piece_hearth")
					{
						Piece.Requirement woodReq = new Piece.Requirement()
						{
							m_amount = 20,
							m_amountPerLevel = 0,
							m_resItem = znScene.GetPrefab("Wood").GetComponent<ItemDrop>(),
							m_recover = true
						};

						// Ensure the Hearth only gets the extra Wood requirement once
						if (!component.m_resources.Any(req => req.m_resItem == woodReq.m_resItem && req.m_amount == woodReq.m_amount))
						{
							component.m_resources = component.m_resources.Append(woodReq).ToArray();
							log.Info($"Added Wood requirement for {pieceName}");
						}
					}

					// LogInfo final resources to confirm changes
					/*foreach (Piece.Requirement req in component.m_resources)
					{
						log.Info($"{pieceName} - After change: {req.m_resItem.name} x {req.m_amount}");
					}*/
				}
			}
			else if (wasChanged)
			{
				// Restore only the modified pieces
				foreach (var pieceName in originalLightPieceCosts.Keys)
				{
					GameObject prefab = null;

					if (pieceName == "$piece_firepit")
					{
						prefab = znScene.m_prefabs.Find(p =>
						{
							Piece piece = p.GetComponent<Piece>();
							return piece != null
								&& !string.IsNullOrEmpty(piece.m_name)
								&& piece.m_name.Equals(pieceName, StringComparison.OrdinalIgnoreCase)
								&& p.name == "fire_pit"; // Ensures we get the correct prefab (it returns the BogWitch_Fire_Pit otherwise)
						});
					}
					else
					{
						prefab = znScene.m_prefabs.Find(p => p.GetComponent<Piece>()?.m_name == pieceName);
					}

					if (prefab == null) continue;

					Piece component = prefab.GetComponent<Piece>();
					if (component == null) continue;

					if (originalLightPieceCosts.TryGetValue(pieceName, out var originalCosts))
					{
						foreach (Piece.Requirement req in component.m_resources)
						{
							if (originalCosts.TryGetValue(req.m_resItem.name, out int originalAmount))
							{
								log.Info($"Restoring backup for {pieceName}");
								req.m_amount = originalAmount;
								if (req.m_resItem.name == "Wood")
								{
									req.m_recover = false;
								}
							}
						}
					}

					// Special case: Remove extra Hearth wood requirement
					if (pieceName == "$piece_hearth")
					{
						component.m_resources = component.m_resources
							.Where(req => !(req.m_resItem.name == "Wood" && req.m_amount == 20))
							.ToArray();
						log.Info($"Removed Wood requirement for {pieceName}");
					}
				}

				// Clear backup data after restore
				originalLightPieceCosts.Clear();
			}
		}
	}
}
