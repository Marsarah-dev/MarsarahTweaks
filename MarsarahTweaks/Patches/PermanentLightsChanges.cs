using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace MarsarahTweaks.Patches
{
	internal class PermanentLightsChanges
	{
		[HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
		class PermanentLights_Patch
		{
			static void Postfix(Fireplace __instance, ref ZNetView ___m_nview)
			{
				// Ensure this only runs on the server (or the owner of the fireplace)
				if (___m_nview.IsOwner())
				{
					if (ConfigManager.permanentLightsEnabled.Value)
					{
						//MarsarahTweaks.MLog($"Setting max fuel for {__instance.m_name}");
						___m_nview.GetZDO().Set("fuel", __instance.m_maxFuel);
					}
					/*else
					{
						// Reset fuel to behave normally when the config is disabled
						if (__instance.m_maxFuel > 0)
						{
							float currentFuel = ___m_nview.GetZDO().GetFloat("fuel", __instance.m_maxFuel);
							___m_nview.GetZDO().Set("fuel", Mathf.Min(currentFuel, __instance.m_maxFuel));
						}
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

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ZNetScene Awake: Updating {ConfigManager.Configs.PermanentLightsModifications.Name}...");

						UpdateLightBuildPiecesAmounts(__instance, false);
					}
					else
					{
						MarsarahTweaks.MLog($"ZNetScene Awake: I am a server. No changes made to {ConfigManager.Configs.PermanentLightsModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ZNetScene Awake: Too early to do anything. No changes made to {ConfigManager.Configs.PermanentLightsModifications.Name}...");
				}
			}
		}

		private static readonly Dictionary<string, Dictionary<string, int>> originalLightPieceCosts = new Dictionary<string, Dictionary<string, int>>();
		public static readonly Dictionary<string, Dictionary<string, int>> lightPieceCostChanges = new Dictionary<string, Dictionary<string, int>>()
		{
			{ "$piece_firepit",				new Dictionary<string, int>() { { "Wood", 10 } } },
			{ "$piece_hearth",				new Dictionary<string, int>() { { "Wood", 20 } } },
			{ "$piece_sconce",				new Dictionary<string, int>() { { "Resin", 6 } } },
			{ "$piece_groundtorch",			new Dictionary<string, int>() { { "Resin", 6 } } },
			{ "$piece_groundtorchwood",		new Dictionary<string, int>() { { "Resin", 4 } } },
			{ "$piece_groundtorchgreen",	new Dictionary<string, int>() { { "Guck", 6 } } },
			{ "$piece_groundtorchblue",		new Dictionary<string, int>() { { "GreydwarfEye", 6 } } },
			{ "$piece_brazierfloor01",		new Dictionary<string, int>() { { "Coal", 5 } } },
			{ "$piece_brazierfloor02",		new Dictionary<string, int>() { { "GreydwarfEye", 5 } } },
			{ "$piece_brazierceiling01",	new Dictionary<string, int>() { { "Coal", 5 } } }
		};

		public static void UpdateLightBuildPiecesAmounts(ZNetScene znScene, bool wasChanged)
		{
			if (ConfigManager.permanentLightsEnabled.Value)
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
						//MarsarahTweaks.MLog($"{pieceName} prefab not found in znScene!");
						continue;
					}

					//MarsarahTweaks.MLog($"{pieceName} - Found prefab: {prefab.name}");

					Piece component = prefab.GetComponent<Piece>();
					if (component == null)
					{
						//MarsarahTweaks.MLog($"{pieceName} - Found prefab, but no Piece component!");
						continue;
					}

					// Log original resources before modifying
					/*foreach (Piece.Requirement req in component.m_resources)
					{
						MarsarahTweaks.MLog($"{pieceName} - Original: {req.m_resItem.name} x {req.m_amount}");
					}*/

					// Backup original costs only if they haven't been backed up yet
					if (!originalLightPieceCosts.ContainsKey(pieceName))
					{
						//MarsarahTweaks.MLog($"Backing up {pieceName}");
						originalLightPieceCosts[pieceName] = component.m_resources
							.ToDictionary(req => req.m_resItem.name, req => req.m_amount);
					}

					// Apply cost modifications
					foreach (Piece.Requirement req in component.m_resources)
					{
						if (lightPieceCostChanges[pieceName].TryGetValue(req.m_resItem.name, out int newAmount))
						{
							//MarsarahTweaks.MLog($"Applying new amount for {pieceName} - {req.m_resItem.name}");
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
							//MarsarahTweaks.MLog($"Added Wood requirement for {pieceName}");
						}
					}

					// Log final resources to confirm changes
					/*foreach (Piece.Requirement req in component.m_resources)
					{
						MarsarahTweaks.MLog($"{pieceName} - After change: {req.m_resItem.name} x {req.m_amount}");
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
								//MarsarahTweaks.MLog($"Restoring backup for {pieceName}");
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
						//MarsarahTweaks.MLog($"Removed Wood requirement for {pieceName}");
					}
				}

				// Clear backup data after restore
				originalLightPieceCosts.Clear();
			}
		}
	}
}
