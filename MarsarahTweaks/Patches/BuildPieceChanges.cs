using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahMod
{
	internal class BuildPieceChanges
	{
		// Build Piece Modifications ===============================================================
		[HarmonyPatch(typeof(PieceTable), nameof(PieceTable.UpdateAvailable))]
		class BuildPiecesModifications_Patch
		{
			static void Postfix(ref List<GameObject> ___m_pieces)
			{
				foreach (GameObject piece in ___m_pieces)
				{
					Piece component = piece.GetComponent<Piece>();
					Piece.Requirement[] requirements = component.m_resources;
					string pieceName = component.m_name;

					if (MarsarahMod.cheaperBuildPiecesEnabled.Value)
					{
						updateBuildPiecesAmounts(ref requirements, ref pieceName);
					}
					if (MarsarahMod.altBuildPiecesMaterialsEnabled.Value && MMShared.itemDropSuccess)
					{
						updateBuildPiecesMaterials(ref requirements, ref pieceName);
					}
				}
			}
		}

		// Build Piece Return ======================================================================
		[HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
		class BuildPieceReturn_Patch
		{
			static void Prefix([NotNull] ref Piece.Requirement[] ___m_resources, [NotNull] ref string ___m_name)
			{
				if (MarsarahMod.cheaperBuildPiecesEnabled.Value)
				{
					updateBuildPiecesAmounts(ref ___m_resources, ref ___m_name);
				}

				if (MarsarahMod.altBuildPiecesMaterialsEnabled.Value && MMShared.itemDropSuccess)
				{
					updateBuildPiecesMaterials(ref ___m_resources, ref ___m_name);
				}
			}
		}

		// Update Build Pieces Amounts =================================================================
		private static void updateBuildPiecesAmounts(ref Piece.Requirement[] requirements, ref string name)
		{
			switch (name)
			{
				case "$piece_preptable":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "FineWood":
								req.m_amount = 10; // 20
								break;
							case "LeatherScraps":
								req.m_amount = 10; // 15
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_cookingstation_iron":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 2; // 3
								break;
							case "Chain":
								req.m_amount = 2; // 3
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_itemstand":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "FineWood":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blastfurnace":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 5; // 10
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_oven":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 5; // 15
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_forge_ext3": // grinding wheel
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Wood":
								req.m_amount = 15; // 25
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_forge_ext4": // smith's anvil
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 7; // 20
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_forge_ext5": // forge cooler
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "FineWood":
								req.m_amount = 10; // 25
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_forge_ext6": // forge toolrack
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 5; // 15
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_woodwindowshutter":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Wood":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_darkwoodgate":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_irongate":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_chest":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_chestprivate":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 4; // 8
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_chestblackmetal":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMetal":
								req.m_amount = 4; // 6
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_brazierceiling01":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Bronze":
								req.m_amount = 3; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_sconce":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Copper":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_groundtorchwood":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Wood":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_groundtorch":
				case "$piece_groundtorchgreen":
				case "$piece_groundtorchblue":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_portal":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "FineWood":
								req.m_amount = 10; // 20
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_portal_stone":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 10; // 30
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_rug_lox":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "LoxPelt":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_rug_wolf":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "WolfPelt":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_rug_deer":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "DeerHide":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_banner01":
				case "$piece_banner02":
				case "$piece_banner03":
				case "$piece_banner04":
				case "$piece_banner05":
				case "$piece_banner06":
				case "$piece_banner07":
				case "$piece_banner08":
				case "$piece_banner09":
				case "$piece_banner10":
				case "$piece_banner11":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "LeatherScraps":
								req.m_amount = 5; // 6
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_fermenter":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "FineWood":
								req.m_amount = 15; // 30
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_bathtub":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 5; // 10
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_crystalwall1x1":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Crystal":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_incinerator":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_amount = 5; // 8
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_stonewall1x1":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Stone":
								req.m_amount = 2; // 3
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_stonewall2x1":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Stone":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_stonepillar":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Stone":
								req.m_amount = 3; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_stonearch":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Stone":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_stonefloor2x2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Stone":
								req.m_amount = 4; // 6
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_stonestair":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Stone":
								req.m_amount = 3; // 8
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble2x1x1":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble_stair":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 3; // 8
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble_base1": // Plinth
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble_basecorner": // Plinth Corner
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 5; // 6
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble_out1": // Cornice
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble_outcorner": // Cornice Corner
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 5; // 6
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble_arch":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_dvergr_stake_wall":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "YggdrasilWood":
								req.m_amount = 4; // 8
								break;
							case "Iron":
								req.m_amount = 2; // 8
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_sharpstakes":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Wood":
								req.m_amount = 4; // 6
								break;
							case "RoundLog":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_dvergr_sharpstakes":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "YggdrasilWood":
								req.m_amount = 4; // 5
								break;
							case "Iron":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_trap":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMetal":
								req.m_amount = 3; // 5
								break;
							case "BronzeNails":
								req.m_amount = 5; // 10
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_turret":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMetal":
								req.m_amount = 7; // 10
								break;
							case "YggdrasilWood":
								req.m_amount = 7; // 10
								break;
							case "MechanicalSpring":
								req.m_amount = 2; // 3
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_eitrrefinery":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 10; // 20
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackforge_ext2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Copper":
								req.m_amount = 5; // 8
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_sapcollector":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "YggdrasilWood":
								req.m_amount = 5; // 10
								break;
							case "BlackMetal":
								req.m_amount = 3; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_magetable":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "YggdrasilWood":
								req.m_amount = 10; // 20
								break;
							case "BlackMetal":
								req.m_amount = 5; // 10
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_magetable_ext": // Rune Table
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 5; // 10
								break;
							case "Eitr":
								req.m_amount = 5; // 10
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_magetable_ext2": // Unfading Candles
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 5; // 10
								break;
							case "Eitr":
								req.m_amount = 5; // 10
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_magetable_ext3": // Feathery Wreath
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Eitr":
								req.m_amount = 5; // 10
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_hexagonalgate":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Copper":
								req.m_amount = 4; // 8
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_dvergr_spiralstair":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "YggdrasilWood":
								req.m_amount = 3; // 5
								break;
							case "Copper":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_dvergr_spiralstair_right":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "YggdrasilWood":
								req.m_amount = 3; // 5
								break;
							case "Copper":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble_bench":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 5; // 6
								break;
							case "Copper":
								req.m_amount = 2; // 3
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_table_round":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "IronNails":
								req.m_amount = 10; // 20
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_blackmarble_table":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "BlackMarble":
								req.m_amount = 5; // 6
								break;
							case "Copper":
								req.m_amount = 2; // 6
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_brazierfloor01":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Bronze":
								req.m_amount = 3; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_brazierfloor02":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Bronze":
								req.m_amount = 3; // 5
								break;
							case "GreydwarfEye":
								req.m_amount = 2; // 5 // TODO: check with permanent lights (currently not working)
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_jute_carpet":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "JuteRed":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_juteblue_carpet":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "JuteBlue":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_rug_hare":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "ScaleHide":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_dvergr_lantern":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Copper":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_dvergr_lantern_pole":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Copper":
								req.m_amount = 2; // 3
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_clothdoor": // Red Jute Curtain
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "JuteRed":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_hanging_cloth_blue1": // Blue Jute Drapes
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "JuteBlue":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_hanging_cloth_blue2": // Blue Jute Curtain
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "JuteBlue":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwood_archedwall":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwood_floor_2x2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwood_floor_1x1":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwood_floor_deco":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwood_beam_1m":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwood_beam_2m":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwood_pole_1m":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwood_pole_2m":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_ashwoodstair":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Blackwood":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_stoneladder":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 3; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_stair":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 3; // 8
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_floor1x1":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 1; // 2
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_pillarmedium":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 2; // 3
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_pillartapered":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_pillartaperedinverted":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_beammedium":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 2; // 3
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_wall1x2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 2; // 4
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_wall2x2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 4; // 6
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_wall4x2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 8; // 12
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_window4x2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 8; // 10
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_roof45_corner":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_roof45_corner2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_roof45_archcorner":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_grausten_roof45_archcorner2":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Grausten":
								req.m_amount = 4; // 5
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_flametalgate":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "FlametalNew":
								req.m_amount = 8; // 16
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_rug_asksvin":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "AskHide":
								req.m_amount = 3; // 4
								break;
							default:
								break;
						}
					}
					break;
				default:
					break;
			}
		}

		// Update Build Pieces Materials ==========================================================
		private static void updateBuildPiecesMaterials(ref Piece.Requirement[] requirements, ref string name)
		{
			switch (name)
			{
				case "$piece_workbench_ext4": // tool rack
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Obsidian":
								req.m_resItem = MMShared.coalID;
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_darkwoodgate":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_resItem = MMShared.blackMetalID;
								break;
							default:
								break;
						}
					}
					break;
				case "$piece_bathtub":
					foreach (Piece.Requirement req in requirements)
					{
						switch (req.m_resItem.name)
						{
							case "Iron":
								req.m_resItem = MMShared.blackMetalID;
								break;
							default:
								break;
						}
					}
					break;
				default:
					break;
			}
		}
	}
}
