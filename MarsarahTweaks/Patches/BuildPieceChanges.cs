using HarmonyLib;
using JetBrains.Annotations;
using MarsarahTweaks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches
{
	internal class BuildPieceChanges
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
						MarsarahTweaks.MLog($"ZNetScene Awake: Updating {ConfigManager.Configs.BuildPieceAmountsModifications.Name}...");

						if (!ConfigManager.buildPieceAmountsEnabled.Value && !ConfigManager.buildPieceMaterialsEnabled.Value) return;

						UpdateBuildPieces(__instance, false, false);
					}
					else
					{
						MarsarahTweaks.MLog($"ZNetScene Awake: I am a server. No changes made to {ConfigManager.Configs.BuildPieceAmountsModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ZNetScene Awake: Too early to do anything. No changes made to {ConfigManager.Configs.BuildPieceAmountsModifications.Name}...");
				}
			}
		}

		// Dictionaries
		private static readonly Dictionary<string, Dictionary<string, (string originalResItem, string newResItem, int amount)>> defaultBuildPieceRequirements
			= new Dictionary<string, Dictionary<string, (string originalResItem, string newResItem, int amount)>>();

		private static Dictionary<string, Dictionary<string, int>> newPieceAmounts = new Dictionary<string, Dictionary<string, int>>()
		{
			{ "$piece_preptable", new Dictionary<string, int>
				{
					{ "FineWood", 10 }, // 20
					{ "LeatherScraps", 10 } // 15
				}
			},
			{ "$piece_cookingstation_iron", new Dictionary<string, int>
				{
					{ "Iron", 2 }, // 3
					{ "Chain", 2 } // 3
				}
			},
			{ "$piece_itemstand", new Dictionary<string, int>
				{
					{ "FineWood", 2 } // 4
				}
			},
			{ "$piece_blastfurnace", new Dictionary<string, int>
				{
					{ "Iron", 5 } // 10
				}
			},
			{ "$piece_oven", new Dictionary<string, int>
				{
					{ "Iron", 5 } // 15
				}
			},
			{ "$piece_forge_ext3", new Dictionary<string, int> // grinding wheel
				{
					{ "Wood", 15 } // 25
				}
			},
			{ "$piece_forge_ext4", new Dictionary<string, int> // smith's anvil
				{
					{ "Iron", 7 } // 20
				}
			},
			{ "$piece_forge_ext5", new Dictionary<string, int> // forge cooler
				{
					{ "FineWood", 10 } // 25
				}
			},
			{ "$piece_forge_ext6", new Dictionary<string, int> // forge toolrack
				{
					{ "Iron", 5 } // 15
				}
			},
			{ "$piece_woodwindowshutter", new Dictionary<string, int>
				{
					{ "Wood", 2 } // 4
				}
			},
			{ "$piece_darkwoodgate", new Dictionary<string, int>
				{
					{ "Iron", 3 } // 4
				}
			},
			{ "$piece_irongate", new Dictionary<string, int>
				{
					{ "Iron", 3 } // 4
				}
			},
			{ "$piece_chest", new Dictionary<string, int>
				{
					{ "Iron", 1 } // 2
				}
			},
			{ "$piece_chestprivate", new Dictionary<string, int>
				{
					{ "Iron", 4 } // 8
				}
			},
			{ "$piece_chestblackmetal", new Dictionary<string, int>
				{
					{ "BlackMetal", 4 } // 6
				}
			},
			{ "$piece_brazierceiling01", new Dictionary<string, int>
				{
					{ "Bronze", 3 } // 5
				}
			},
			{ "$piece_sconce", new Dictionary<string, int>
				{
					{ "Copper", 1 } // 2
				}
			},
			{ "$piece_groundtorchwood", new Dictionary<string, int>
				{
					{ "Wood", 1 } // 2
				}
			},
			{ "$piece_groundtorch", new Dictionary<string, int>
				{
					{ "Iron", 1 } // 2
				}
			},
			{ "$piece_groundtorchgreen", new Dictionary<string, int>
				{
					{ "Iron", 1 } // 2
				}
			},
			{ "$piece_groundtorchblue", new Dictionary<string, int>
				{
					{ "Iron", 1 } // 2
				}
			},
			{ "$piece_portal", new Dictionary<string, int>
				{
					{ "FineWood", 10 } // 20
				}
			},
			{ "$piece_portal_stone", new Dictionary<string, int>
				{
					{ "Grausten", 10 } // 30
				}
			},
			{ "$piece_rug_lox", new Dictionary<string, int>
				{
					{ "LoxPelt", 3 } // 4
				}
			},
			{ "$piece_rug_wolf", new Dictionary<string, int>
				{
					{ "WolfPelt", 3 } // 4
				}
			},
			{ "$piece_rug_deer", new Dictionary<string, int>
				{
					{ "DeerHide", 3 } // 4
				}
			},
			{ "$piece_banner01", new Dictionary<string, int>
				{
					{ "LeatherScraps", 5 } // 6
				}
			},
			{ "$piece_fermenter", new Dictionary<string, int>
				{
					{ "FineWood", 15 } // 30
				}
			},
			{ "$piece_bathtub", new Dictionary<string, int>
				{
					{ "Iron", 5 } // 10
				}
			},
			{ "$piece_crystalwall1x1", new Dictionary<string, int>
				{
					{ "Crystal", 1 } // 2
				}
			},
			{ "$piece_incinerator", new Dictionary<string, int>
				{
					{ "Iron", 5 } // 8
				}
			},
			{ "$piece_stonewall1x1", new Dictionary<string, int>
				{
					{ "Stone", 2 } // 3
				}
			},
			{ "$piece_stonewall2x1", new Dictionary<string, int>
				{
					{ "Stone", 3 } // 4
				}
			},
			{ "$piece_stonepillar", new Dictionary<string, int>
				{
					{ "Stone", 3 } // 5
				}
			},
			{ "$piece_stonearch", new Dictionary<string, int>
				{
					{ "Stone", 3 } // 4
				}
			},
			{ "$piece_stonefloor2x2", new Dictionary<string, int>
				{
					{ "Stone", 4 } // 6
				}
			},
			{ "$piece_stonestair", new Dictionary<string, int>
				{
					{ "Stone", 3 } // 8
				}
			},
			{ "$piece_blackmarble2x1x1", new Dictionary<string, int>
				{
					{ "BlackMarble", 3 } // 4
				}
			},
			{ "$piece_blackmarble_stair", new Dictionary<string, int>
				{
					{ "BlackMarble", 3 } // 8
				}
			},
			{ "$piece_blackmarble_base1", new Dictionary<string, int>
				{
					{ "BlackMarble", 4 } // 5
				}
			},
			{ "$piece_blackmarble_basecorner", new Dictionary<string, int>
				{
					{ "BlackMarble", 5 } // 6
				}
			},
			{ "$piece_blackmarble_out1", new Dictionary<string, int>
				{
					{ "BlackMarble", 4 } // 5
				}
			},
			{ "$piece_blackmarble_outcorner", new Dictionary<string, int>
				{
					{ "BlackMarble", 5 } // 6
				}
			},
			{ "$piece_blackmarble_arch", new Dictionary<string, int>
				{
					{ "BlackMarble", 4 } // 5
				}
			},
			{ "$piece_dvergr_stake_wall", new Dictionary<string, int>
				{
					{ "YggdrasilWood", 4 }, // 8
					{ "Iron", 2 } // 8
				}
			},
			{ "$piece_sharpstakes", new Dictionary<string, int>
				{
					{ "Wood", 4 }, // 6
					{ "RoundLog", 2 } // 4
				}
			},
			{ "$piece_dvergr_sharpstakes", new Dictionary<string, int>
				{
					{ "YggdrasilWood", 4 }, // 5
					{ "Iron", 1 } // 2
				}
			},
			{ "$piece_trap", new Dictionary<string, int>
				{
					{ "BlackMetal", 3 }, // 5
					{ "BronzeNails", 5 } // 10
				}
			},
			{ "$piece_turret", new Dictionary<string, int>
				{
					{ "BlackMetal", 7 }, // 10
					{ "YggdrasilWood", 7 }, // 10
					{ "MechanicalSpring", 2 } // 3
				}
			},
			{ "$piece_eitrrefinery", new Dictionary<string, int>
				{
					{ "BlackMarble", 10 } // 20
				}
			},
			{ "$piece_blackforge_ext2", new Dictionary<string, int>
				{
					{ "Copper", 5 } // 8
				}
			},
			{ "$piece_sapcollector", new Dictionary<string, int>
				{
					{ "YggdrasilWood", 5 }, // 10
					{ "BlackMetal", 3 } // 5
				}
			},
			{ "$piece_magetable", new Dictionary<string, int>
				{
					{ "YggdrasilWood", 10 }, // 20
					{ "BlackMetal", 5 } // 10
				}
			},
			{ "$piece_magetable_ext", new Dictionary<string, int> // Rune Table
				{
					{ "BlackMarble", 5 }, // 10
					{ "Eitr", 5 } // 10
				}
			},
			{ "$piece_magetable_ext2", new Dictionary<string, int> // Unfading Candles
				{
					{ "BlackMarble", 5 }, // 10
					{ "Eitr", 5 } // 10
				}
			},
			{ "$piece_magetable_ext3", new Dictionary<string, int> // Feathery Wreath
				{
					{ "Eitr", 5 } // 10
				}
			},
			{ "$piece_hexagonalgate", new Dictionary<string, int>
				{
					{ "Copper", 4 } // 8
				}
			},
			{ "$piece_dvergr_spiralstair", new Dictionary<string, int>
				{
					{ "YggdrasilWood", 3 }, // 5
					{ "Copper", 1 } // 2
				}
			},
			{ "$piece_dvergr_spiralstair_right", new Dictionary<string, int>
				{
					{ "YggdrasilWood", 3 }, // 5
					{ "Copper", 1 } // 2
				}
			},
			{ "$piece_blackmarble_bench", new Dictionary<string, int>
				{
					{ "BlackMarble", 5 }, // 6
					{ "Copper", 2 } // 3
				}
			},
			{ "$piece_table_round", new Dictionary<string, int>
				{
					{ "IronNails", 10 } // 20
				}
			},
			{ "$piece_blackmarble_table", new Dictionary<string, int>
				{
					{ "BlackMarble", 5 }, // 6
					{ "Copper", 2 } // 6
				}
			},
			{ "$piece_brazierfloor01", new Dictionary<string, int>
				{
					{ "Bronze", 3 } // 5
				}
			},
			{ "$piece_brazierfloor02", new Dictionary<string, int>
				{
					{ "Bronze", 3 }, // 5
					{ "GreydwarfEye", 2 } // 5
				}
			},
			{ "$piece_jute_carpet", new Dictionary<string, int>
				{
					{ "JuteRed", 3 } // 4
				}
			},
			{ "$piece_juteblue_carpet", new Dictionary<string, int>
				{
					{ "JuteBlue", 3 } // 4
				}
			},
			{ "$piece_rug_hare", new Dictionary<string, int>
				{
					{ "ScaleHide", 2 } // 4
				}
			},
			{ "$piece_dvergr_lantern", new Dictionary<string, int>
				{
					{ "Copper", 1 } // 2
				}
			},
			{ "$piece_dvergr_lantern_pole", new Dictionary<string, int>
				{
					{ "Copper", 2 } // 3
				}
			},
			{ "$piece_clothdoor", new Dictionary<string, int> // Red Jute Curtain
				{
					{ "JuteRed", 3 } // 4 
				}
			},
			{ "$piece_hanging_cloth_blue1", new Dictionary<string, int> // Blue Jute Drapes
				{
					{ "JuteBlue", 3 } // 4
				}
			},
			{ "$piece_hanging_cloth_blue2", new Dictionary<string, int> // Blue Jute Curtain
				{
					{ "JuteBlue", 3 } // 4
				}
			},
			{ "$piece_ashwood_archedwall", new Dictionary<string, int>
				{
					{ "Blackwood", 1 } // 2
				}
			},
			{ "$piece_ashwood_floor_2x2", new Dictionary<string, int>
				{
					{ "Blackwood", 2 } // 4
				}
			},
			{ "$piece_ashwood_floor_1x1", new Dictionary<string, int>
				{
					{ "Blackwood", 1 } // 2
				}
			},
			{ "$piece_ashwood_floor_deco", new Dictionary<string, int>
				{
					{ "Blackwood", 2 } // 4
				}
			},
			{ "$piece_ashwood_beam_1m", new Dictionary<string, int>
				{
					{ "Blackwood", 1 } // 2
				}
			},
			{ "$piece_ashwood_beam_2m", new Dictionary<string, int>
				{
					{ "Blackwood", 2 } // 4
				}
			},
			{ "$piece_ashwood_pole_1m", new Dictionary<string, int>
				{
					{ "Blackwood", 1 } // 2
				}
			},
			{ "$piece_ashwood_pole_2m", new Dictionary<string, int>
				{
					{ "Blackwood", 2 } // 4
				}
			},
			{ "$piece_ashwoodstair", new Dictionary<string, int>
				{
					{ "Blackwood", 1 } // 2
				}
			},
			{ "$piece_grausten_stoneladder", new Dictionary<string, int>
				{
					{ "Grausten", 3 } // 5
				}
			},
			{ "$piece_grausten_stair", new Dictionary<string, int>
				{
					{ "Grausten", 3 } // 8
				}
			},
			{ "$piece_grausten_floor1x1", new Dictionary<string, int>
				{
					{ "Grausten", 1 } // 2
				}
			},
			{ "$piece_grausten_pillarmedium", new Dictionary<string, int>
				{
					{ "Grausten", 2 } // 3
				}
			},
			{ "$piece_grausten_pillartapered", new Dictionary<string, int>
				{
					{ "Grausten", 4 } // 5
				}
			},
			{ "$piece_grausten_pillartaperedinverted", new Dictionary<string, int>
				{
					{ "Grausten", 4 } // 5
				}
			},
			{ "$piece_grausten_beammedium", new Dictionary<string, int>
				{
					{ "Grausten", 2 } // 3
				}
			},
			{ "$piece_grausten_wall1x2", new Dictionary<string, int>
				{
					{ "Grausten", 2 } // 4
				}
			},
			{ "$piece_grausten_wall2x2", new Dictionary<string, int>
				{
					{ "Grausten", 4 } // 6
				}
			},
			{ "$piece_grausten_wall4x2", new Dictionary<string, int>
				{
					{ "Grausten", 8 } // 12
				}
			},
			{ "$piece_grausten_window4x2", new Dictionary<string, int>
				{
					{ "Grausten", 8 } // 10
				}
			},
			{ "$piece_grausten_roof45_corner", new Dictionary<string, int>
				{
					{ "Grausten", 4 } // 5
				}
			},
			{ "$piece_grausten_roof45_corner2", new Dictionary<string, int>
				{
					{ "Grausten", 4 } // 5
				}
			},
			{ "$piece_grausten_roof45_archcorner", new Dictionary<string, int>
				{
					{ "Grausten", 4 } // 5
				}
			},
			{ "$piece_grausten_roof45_archcorner2", new Dictionary<string, int>
				{
					{ "Grausten", 4 } // 5
				}
			},
			{ "$piece_flametalgate", new Dictionary<string, int>
				{
					{ "FlametalNew", 8 } // 16
				}
			},
			{ "$piece_rug_asksvin", new Dictionary<string, int>
				{
					{ "AskHide", 3 } // 4
				}
			}
		};

		private static Dictionary<string, Dictionary<string, string>> newPieceMaterials = new Dictionary<string, Dictionary<string, string>>()
		{
			{
				"$piece_workbench_ext4", new Dictionary<string, string> // tool rack
				{
					{ "Obsidian", "Coal" }
				}
			},
			{
				"$piece_darkwoodgate", new Dictionary<string, string>
				{
					{ "Iron", "BlackMetal" }
				}
			},
			{
				"$piece_bathtub", new Dictionary<string, string>
				{
					{ "Iron", "BlackMetal" }
				}
			}
		};

		// Modify Build Pieces =========================================================================
		public static void UpdateBuildPieces(ZNetScene znScene, bool amountsWasChanged, bool materialsWasChanged)
		{
			foreach (GameObject piece in znScene.m_prefabs)
			{
				Piece actualPiece = piece.GetComponent<Piece>();
				if (actualPiece == null) continue;

				Piece.Requirement[] requirements = actualPiece.m_resources;
				string pieceName = actualPiece.m_name;

				bool hasPieceAmountsChange = ConfigManager.buildPieceAmountsEnabled.Value && newPieceAmounts.ContainsKey(pieceName);
				bool hasPieceMaterialsChange = ConfigManager.buildPieceMaterialsEnabled.Value && newPieceMaterials.ContainsKey(pieceName);

				foreach (Piece.Requirement req in requirements)
				{
					// Apply piece amounts modifications
					if (hasPieceAmountsChange && newPieceAmounts[pieceName].TryGetValue(req.m_resItem.name, out var amountValue))
					{
						CreateBackup(pieceName, req, null);

						//MarsarahTweaks.MLog($"(Piece Amounts) Applying amounts for Piece {pieceName} - Resource {req.m_resItem.name}: {req.m_amount} -> {amountValue}");
						ApplyChanges(req, (null, amountValue), modifyResItem: false);
					}

					// Apply piece materials modifications
					if (hasPieceMaterialsChange && newPieceMaterials[pieceName].TryGetValue(req.m_resItem.name, out var materialValue))
					{
						CreateBackup(pieceName, req, materialValue);

						//MarsarahTweaks.MLog($"(Piece Materials) Applying material for Piece {pieceName} - Resource {req.m_resItem.name} -> {materialValue}");
						ApplyChanges(req, (materialValue, null), modifyResItem: true);
					}

					// Restore backups when disabling features
					if (!ConfigManager.buildPieceAmountsEnabled.Value && newPieceAmounts.ContainsKey(pieceName) && amountsWasChanged)
					{
						//MarsarahTweaks.MLog($"(Gear Amounts) Was changed: {amountsWasChanged}");
						if (RestoreBackup(pieceName, req, false))
						{
							// Remove backup unless materials modification still needs it
							if (!hasPieceMaterialsChange || !newPieceMaterials[pieceName].ContainsKey(req.m_resItem.name))
							{
								//MarsarahTweaks.MLog($"(Piece Amounts) Removing backup for: {pieceName} - {req.m_resItem.name}");
								defaultBuildPieceRequirements[pieceName].Remove(req.m_resItem.name);
							}
						}
					}

					if (!ConfigManager.buildPieceMaterialsEnabled.Value && newPieceMaterials.ContainsKey(pieceName) && materialsWasChanged)
					{
						//MarsarahTweaks.MLog($"(Gear Materials) Was changed: {materialsWasChanged}");
						if (RestoreBackup(pieceName, req, true))
						{
							if (hasPieceAmountsChange && newPieceAmounts[pieceName].ContainsKey(req.m_resItem.name))
							{
								// Apply gear amounts modifications again after restoring
								if (newPieceAmounts[pieceName].TryGetValue(req.m_resItem.name, out var restoredValue))
								{
									//MarsarahTweaks.MLog($"(Piece Materials - Amounts) Re-applying changes for: {pieceName} - {req.m_resItem.name}");
									ApplyChanges(req, (null, restoredValue), false);
								}
							}
							else if (!hasPieceAmountsChange || !newPieceAmounts[pieceName].ContainsKey(req.m_resItem.name))
							{
								//MarsarahTweaks.MLog($"(Piece Materials) Removing backup for: {pieceName} - {req.m_resItem.name}");
								defaultBuildPieceRequirements[pieceName].Remove(req.m_resItem.name);
							}
						}
					}
				}

				// Remove entire backup entry if empty
				if (defaultBuildPieceRequirements.ContainsKey(pieceName) && defaultBuildPieceRequirements[pieceName].Count == 0)
				{
					//MarsarahTweaks.MLog($"(Cleanup) Removing backup for: {pieceName}");
					defaultBuildPieceRequirements.Remove(pieceName);
				}
			}
		}

		// Apply Changes ================================================================================
		private static void ApplyChanges(Piece.Requirement req, (string newResItem, int? amount) values, bool modifyResItem = false)
		{
			if (values.amount.HasValue && req.m_amount != values.amount.Value)
			{
				//MarsarahTweaks.MLog($"Applying amounts for Resource {req.m_resItem.name}: {req.m_amount} -> {values.amount.Value}");
				req.m_amount = values.amount.Value;
			}
			if (modifyResItem && !string.IsNullOrEmpty(values.newResItem) && req.m_resItem.name != values.newResItem)
			{
				//MarsarahTweaks.MLog($"Applying material for Resource {req.m_resItem.name} -> {values.newResItem}");
				req.m_resItem = ZNetScene.instance.GetPrefab(values.newResItem).GetComponent<ItemDrop>();
			}
		}

		// Create Backup ================================================================================
		private static void CreateBackup(string pieceName, Piece.Requirement req, string newResItem)
		{
			// Check if we haven't backed up this piece yet
			if (!defaultBuildPieceRequirements.TryGetValue(pieceName, out var pieceBackup))
			{
				pieceBackup = new Dictionary<string, (string originalResItem, string newResItem, int amount)>();
				defaultBuildPieceRequirements[pieceName] = pieceBackup;
			}

			string currentResItem = req.m_resItem.name;

			// If there's already a backup for this resource, don't overwrite it unless newResItem is missing
			if (pieceBackup.TryGetValue(currentResItem, out var existingBackup))
			{
				if (newResItem != null && existingBackup.newResItem == null)
				{
					//MarsarahTweaks.MLog($"Updating backup for {pieceName} - oldResItem: {currentResItem} with newResItem: {newResItem}");
					pieceBackup[currentResItem] = (existingBackup.originalResItem, newResItem, existingBackup.amount);
				}
			}
			else
			{
				// Create a new backup for this resource without affecting existing ones
				//MarsarahTweaks.MLog($"Creating new backup for {pieceName} - {currentResItem}");
				pieceBackup[currentResItem] = (currentResItem, newResItem, req.m_amount);
			}
		}

		// Restore Build Pieces Amounts =================================================================
		private static bool RestoreBackup(string pieceName, Piece.Requirement req, bool restoreMaterials)
		{
			if (!defaultBuildPieceRequirements.TryGetValue(pieceName, out var recipeBackup))
			{
				//MarsarahTweaks.MLog("Restore backup first check.");
				return false;
			}

			// Restoring original materials if any
			//MarsarahTweaks.MLog($"Restore backup - Recipe name: {pieceName}, Given requirement: {req.m_resItem.name}");
			foreach (var kvp in recipeBackup)
			{
				var (originalMaterial, newMaterial, amount) = kvp.Value;
				{
					//MarsarahTweaks.MLog($"Restore backup - values: {originalMaterial}, {newMaterial}, {restoreMaterials}");

					if (req.m_resItem.name == newMaterial && restoreMaterials)
					{
						//MarsarahTweaks.MLog($"Restoring original material for {pieceName} from {req.m_resItem.name} to {originalMaterial}");
						req.m_resItem = ZNetScene.instance.GetPrefab(originalMaterial).GetComponent<ItemDrop>();
						break;
					}
				}
			}

			if (recipeBackup.TryGetValue(req.m_resItem.name, out var originalValues))
			{
				//MarsarahTweaks.MLog($"Restoring backup for: {pieceName} - {req.m_resItem.name}");

				// Restore original values
				req.m_amount = originalValues.amount;

				return true;
			}

			//MarsarahTweaks.MLog("Restore backup - we got to the end.");
			return false; // No backup found
		}
	}
}
