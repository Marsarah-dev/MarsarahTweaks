using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class GearRecipeChanges
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class GearRecipeModifications_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				// Chearper Gear Recipes ===================================================================
				if (ConfigManager.cheaperGearEnabled.Value && __instance != null)
				{
					ApplyCheaperGearRecipeChanges(ref __instance);
				}

				// Alternate Gear Recipes ==================================================================
				if (ConfigManager.altGearRecipesEnabled.Value && __instance != null)
				{
					ApplyAlternateGearRecipeChanges(ref __instance);
				}
			}

			// Apply Recipe Changes =============================================
			private static void ApplyCheaperGearRecipeChanges(ref ObjectDB objDB)
			{
				foreach (Recipe recipe in objDB.m_recipes)
				{
					switch (recipe.name)
					{
						// == weapons, projeciles & shields ==
						// wood
						case "Recipe_Club":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 2;
										break;
									case "BoneFragments":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_SledgeStagbreaker":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "BoneFragments":
										req.m_amount = 2;
										req.m_amountPerLevel = 8;
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_Bow":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "LeatherScraps":
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ShieldWood":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 8;
										break;
									case "Resin":
										req.m_amount = 0;
										req.m_amountPerLevel = 0;
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ArrowWood":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ArrowFire":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
									case "Resin":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;

						// early tools
						case "Recipe_AxeStone":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;

						// flint
						case "Recipe_KnifeFlint":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SpearFlint":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Flint":
										req.m_amount = 8;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AxeFlint":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 1;
										break;
									case "LeatherScraps":
										req.m_amount = 1;
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowFlint":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;

						// tin

						// copper
						case "Recipe_KnifeCopper":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 1;
										break;
									case "Copper":
										req.m_amountPerLevel = 3;
										break;
									case "GreydwarfEye":
										req.m_amountPerLevel = 6;
										break;
									default:
										break;
								}
							}
							break;

						// bronze
						case "Recipe_MaceBronze":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
									case "LeatherScraps":
										req.m_amountPerLevel = 1;
										break;
									case "Bronze":
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordBronze":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Bronze":
										req.m_amountPerLevel = 3;
										break;
									case "LeatherScraps":
										req.m_amount = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AxeBronze":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 1;
										break;
									case "Bronze":
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AtgeirBronze":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 8;
										req.m_amountPerLevel = 2;
										break;
									case "LeatherScraps":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ShieldBronzeBuckler":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Bronze":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_PickaxeBronze":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Bronze":
										req.m_amount = 7;
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowBronze":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;

						// iron
						case "Recipe_MaceIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 2;
										break;
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "LeatherScraps":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SledgeIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 15;
										req.m_amountPerLevel = 7;
										break;
									case "YmirRemains":
										req.m_amount = 6;
										req.m_amountPerLevel = 0;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_Battleaxe":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "ElderBark":
										req.m_amount = 20;
										break;
									case "Iron":
										req.m_amount = 18;
										req.m_amountPerLevel = 7;
										break;
									case "LeatherScraps":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 2;
										break;
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AxeIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 2;
										break;
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "LeatherScraps":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AtgeirIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amountPerLevel = 3;
										break;
									case "Iron":
										req.m_amount = 15;
										req.m_amountPerLevel = 7;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_BowHuntsman":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ShieldIronBuckler":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 6;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_PickaxeIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;

						// silver & obsidian
						case "Recipe_KnifeSilver":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_amount = 7;
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_MaceSilver":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "ElderBark":
										req.m_amount = 15;
										req.m_amountPerLevel = 3;
										break;
									case "Silver":
										req.m_amount = 20;
										req.m_amountPerLevel = 7;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordSilver":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 15;
										req.m_amountPerLevel = 3;
										break;
									case "Silver":
										req.m_amount = 20;
										req.m_amountPerLevel = 7;
										break;
									case "LeatherScraps":
										req.m_amount = 5;
										break;
									case "Iron":
										req.m_amount = 3;
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_BowDraugrFang":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "Guck":
										req.m_amount = 7;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_Battleaxe_Crystal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_amount = 15;
										req.m_amountPerLevel = 7;
										break;
									case "Crystal":
										req.m_amount = 3;
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_FistFenrirClaw":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowObsidian":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									case "Obsidian":
										req.m_amount = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowFrost":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									case "Obsidian":
										req.m_amount = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowPoison":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									case "Obsidian":
										req.m_amount = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowSilver":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;

						// ocean
						case "Recipe_KnifeChitin":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amountPerLevel = 2;
										break;
									case "Chitin":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "LeatherScraps":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SpearChitin":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Chitin":
										req.m_amount = 15;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ShieldSerpentscale":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amount = 20;
										break;
									case "Iron":
										req.m_amount = 3;
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;

						// black metal & linen
						case "Recipe_MaceNeedle":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amount = 4;
										req.m_amountPerLevel = 2;
										break;
									case "Iron":
										req.m_amount = 10;
										break;
									case "LinenThread":
										req.m_amount = 5;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_KnifeBlackmetal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordBlackmetal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amount = 3;
										req.m_amountPerLevel = 2;
										break;
									case "BlackMetal":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AtgeirBlackmetal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amountPerLevel = 3;
										break;
									case "BlackMetal":
										req.m_amount = 15;
										req.m_amountPerLevel = 7;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AxeBlackMetal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amountPerLevel = 2;
										break;
									case "BlackMetal":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_PickaxeBlackMetal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "BlackMetal":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ShieldBlackmetal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Chain":
										req.m_amountPerLevel = 0;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ShieldBlackmetalTower":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Chain":
										req.m_amount = 6;
										req.m_amountPerLevel = 0;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowNeedle":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Needle":
										req.m_amount = 3;
										break;
									default:
										break;
								}
							}
							break;

						// mistlands
						case "Recipe_AxeJotunBane":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "YggdrasilWood":
										req.m_amountPerLevel = 1;
										break;
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "Eitr":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SpearCarapace":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "YggdrasilWood":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordMistwalker":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amount = 5;
										req.m_amountPerLevel = 1;
										break;
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "Eitr":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AtgeirHimminAfl":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "YggdrasilWood":
										req.m_amountPerLevel = 2;
										break;
									case "Eitr":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_KnifeSkollAndHati":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FineWood":
										req.m_amount = 5;
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SledgeDemolisher":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordKrom":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 15;
										req.m_amountPerLevel = 5;
										break;
									case "Bronze":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ShieldCarapace":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Eitr":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ShieldCarapaceBuckler":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Carapace":
										req.m_amount = 15;
										req.m_amountPerLevel = 7;
										break;
									case "Eitr":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_BowSpineSnap":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "BoneFragments":
										req.m_amount = 30;
										req.m_amountPerLevel = 15;
										break;
									case "Eitr":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowCarapace":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Carapace":
										req.m_amount = 2;
										break;
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_BoltBlackmetal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "BlackMetal":
										req.m_amount = 1;
										break;
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_BoltBone":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "BoneFragments":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_BoltCarapace":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_BoltIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_StaffSkeleton":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Eitr":
										req.m_amount = 15;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_StaffFireball":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "YggdrasilWood":
										req.m_amount = 15;
										break;
									case "Eitr":
										req.m_amount = 15;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_StaffIceShards":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "YggdrasilWood":
										req.m_amount = 15;
										break;
									case "Eitr":
										req.m_amount = 15;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_StaffShield":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "YggdrasilWood":
										req.m_amount = 15;
										break;
									case "Eitr":
										req.m_amount = 15;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;

						// ashlands
						case "Recipe_MaceEldner":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_MaceEldner_Blood":
						case "Recipe_MaceEldner_Lightning":
						case "Recipe_MaceEldner_Nature":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SpearSplitner":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "BonemawSerpentTooth":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SpearSplitner_Blood":
						case "Recipe_SpearSplitner_Lightning":
						case "Recipe_SpearSplitner_Nature":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordNiedhogg":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "CharredBone":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordNiedhogg_Blood":
						case "Recipe_SwordNiedhogg_Lightning":
						case "Recipe_SwordNiedhogg_Nature":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordSlayer":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 15;
										req.m_amountPerLevel = 7;
										break;
									case "AskHide":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_SwordSlayer_Blood":
						case "Recipe_SwordSlayer_Lightning":
						case "Recipe_SwordSlayer_Nature":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amountPerLevel = 7;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_AxeBerzerkr":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 15;
										req.m_amountPerLevel = 7;
										break;
									case "CharredBone":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_BowAshlands":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 8;
										req.m_amountPerLevel = 4;
										break;
									case "CharredBone":
										req.m_amount = 15;
										break;
									case "BonemawSerpentTooth":
										req.m_amount = 4;
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_CrossbowRipper_Blood":
						case "Recipe_CrossbowRipper_Lightning":
						case "Recipe_CrossbowRipper_Nature":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArrowCharred":
						case "Recipe_BoltCharred":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Blackwood":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_StaffGreenRoots":
						case "Recipe_StaffLightning":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "CelestialFeather":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_StaffRedTroll":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;

						// == armor ==
						// rag
						case "Recipe_ArmorRagsChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "LeatherScraps":
										req.m_amount = 6;
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorRagsLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "LeatherScraps":
										req.m_amount = 6;
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;

						// leather
						case "Recipe_HelmetLeather":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "DeerHide":
										req.m_amountPerLevel = 5;
										break;
									case "BoneFragments":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorLeatherChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "DeerHide":
										req.m_amountPerLevel = 5;
										break;
									case "BoneFragments":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorLeatherLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "DeerHide":
										req.m_amountPerLevel = 5;
										break;
									case "BoneFragments":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_CapeDeerHide":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "DeerHide":
										req.m_amountPerLevel = 3;
										break;
									case "BoneFragments":
										req.m_amount = 0;
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;

						// troll
						case "Recipe_HelmetTrollLeather":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "TrollHide":
										req.m_amountPerLevel = 3;
										break;
									case "BoneFragments":
										req.m_amount = 0;
										req.m_amountPerLevel = 0;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorTrollLeatherChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "TrollHide":
										req.m_amount = 6;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorTrollLeatherLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "TrollHide":
										req.m_amount = 6;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_CapeTrollHide":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "TrollHide":
										req.m_amount = 4;
										req.m_amountPerLevel = 3;
										break;
									case "BoneFragments":
										req.m_amount = 5;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;

						// root
						case "Recipe_HelmetRoot":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Root":
										req.m_amount = 5;
										break;
									case "ElderBark":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ArmorRootChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Root":
										req.m_amount = 5;
										break;
									case "ElderBark":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ArmorRootLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Root":
										req.m_amount = 5;
										break;
									case "ElderBark":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;

						// iron
						case "Recipe_HelmetIron":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorIronChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorIronLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;

						// fenris
						case "Recipe_HelmetFenrir":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "WolfHairBundle":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									case "WolfPelt":
										req.m_amount = 3;
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorFenrirChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "WolfHairBundle":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									case "WolfPelt":
										req.m_amount = 4;
										req.m_amountPerLevel = 2;
										break;
									case "LeatherScraps":
										req.m_amount = 7;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorFenrirLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "WolfHairBundle":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									case "WolfPelt":
										req.m_amount = 4;
										req.m_amountPerLevel = 2;
										break;
									case "LeatherScraps":
										req.m_amount = 7;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;

						// silver
						case "Recipe_HelmetDrake":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									case "WolfPelt":
										req.m_amountPerLevel = 1;
										break;
									case "TrophyHatchling":
										req.m_amount = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorWolfChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									case "WolfPelt":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorWolfLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_amount = 10;
										req.m_amountPerLevel = 4;
										break;
									case "WolfPelt":
										req.m_amountPerLevel = 1;
										break;
									case "WolfFang":
										req.m_amountPerLevel = 0;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_CapeWolf":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_amount = 2;
										req.m_amountPerLevel = 1;
										break;
									case "WolfPelt":
										req.m_amount = 5;
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;

						// linen
						case "Recipe_HelmetPadded":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 5;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorPaddedCuirass":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 5;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorPaddedGreaves":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 5;
										req.m_amountPerLevel = 3;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_CapeLinen":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "LinenThread":
										req.m_amount = 10;
										req.m_amountPerLevel = 2;
										break;
									default:
										break;
								}
							}
							break;

						// other armor
						case "Recipe_CapeLox":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "LoxPelt":
										req.m_amount = 5;
										break;
									default:
										break;
								}
							}
							break;

						// Mistlands Armor
						case "Recipe_HelmetMage":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "LinenThread":
										req.m_amount = 15;
										req.m_amountPerLevel = 5;
										break;
									case "Iron":
										req.m_amount = 0;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorMageChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Eitr":
										req.m_amount = 15;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorMageLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Eitr":
										req.m_amount = 15;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_HelmetCarapace":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Carapace":
										req.m_amount = 15;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;

						// other mistlands
						case "Recipe_CapeFeather":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "ScaleHide":
										req.m_amount = 7;
										req.m_amountPerLevel = 3;
										break;
									case "Eitr":
										req.m_amount = 10;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_Lantern":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Bronze":
										req.m_amount = 1;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_MechanicalSpring":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 1;
										break;
									default:
										break;
								}
							}
							break;

						// ashlands armor
						case "Recipe_HelmetMage_Ashlands":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "LinenThread":
										req.m_amount = 15;
										req.m_amountPerLevel = 5;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorMageChest_Ashlands":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Eitr":
										req.m_amount = 15;
										break;
									case "FlametalNew":
										req.m_amount = 0;
										req.m_amountPerLevel = 0;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorMageLegs_Ashlands":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Eitr":
										req.m_amount = 15;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_HelmetMedium_Ashlands":
						case "Recipe_ArmorMediumChest_Ashlands":
						case "Recipe_ArmorMediumLegs_Ashlands":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "AskHide":
										req.m_amountPerLevel = 4;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_HelmetFlametal":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "CharredBone":
										req.m_amount = 3;
										req.m_amountPerLevel = 1;
										break;
									case "Eitr":
										req.m_amountPerLevel = 0;
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorFlametalChest":
						case "Recipe_ArmorFlametalLegs":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 10;
										req.m_amountPerLevel = 5;
										break;
									case "CharredBone":
										req.m_amountPerLevel = 1;
										break;
									default:
										break;
								}
							}
							break;

						// other
						case "Recipe_CapeAsh":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 0;
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

			// Apply Recipe Changes =============================================
			private static void ApplyAlternateGearRecipeChanges(ref ObjectDB objDB)
			{
				foreach (Recipe recipe in objDB.m_recipes)
				{
					switch (recipe.name)
					{
						case "Recipe_Club":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "BoneFragments":
										req.m_resItem = objDB.GetItemPrefab("LeatherScraps").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_KnifeCopper":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "GreydwarfEye":
										req.m_amountPerLevel = 2;
										req.m_resItem = objDB.GetItemPrefab("LeatherScraps").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_KnifeSilver":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_resItem = objDB.GetItemPrefab("FineWood").GetComponent<ItemDrop>();
										break;
									case "Iron":
										req.m_resItem = objDB.GetItemPrefab("Obsidian").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_SwordSilver":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_amount = 10;
										req.m_amountPerLevel = 2;
										req.m_resItem = objDB.GetItemPrefab("FineWood").GetComponent<ItemDrop>();
										break;
									case "Iron":
										req.m_amount = 4;
										req.m_amountPerLevel = 2;
										req.m_resItem = objDB.GetItemPrefab("Obsidian").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ShieldSerpentscale":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_resItem = objDB.GetItemPrefab("Chitin").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_MaceNeedle":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_resItem = objDB.GetItemPrefab("BlackMetal").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ArmorWolfChest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Chain":
										req.m_amount = 3;
										req.m_resItem = objDB.GetItemPrefab("WolfFang").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_HelmetPadded":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_resItem = objDB.GetItemPrefab("BlackMetal").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ArmorPaddedCuirass":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_resItem = objDB.GetItemPrefab("BlackMetal").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_ArmorPaddedGreaves":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_resItem = objDB.GetItemPrefab("BlackMetal").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_CapeLinen":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_resItem = objDB.GetItemPrefab("BlackMetal").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_CapeLox":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Silver":
										req.m_resItem = objDB.GetItemPrefab("BlackMetal").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_CrossbowArbalest":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Wood":
										req.m_resItem = objDB.GetItemPrefab("FineWood").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;

						case "Recipe_HelmetMage":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "Iron":
										req.m_amount = 3;
										req.m_resItem = objDB.GetItemPrefab("ScaleHide").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_ArmorMageChest_Ashlands":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 5;
										req.m_amountPerLevel = 2;
										req.m_resItem = objDB.GetItemPrefab("SulfurStone").GetComponent<ItemDrop>();
										break;
									default:
										break;
								}
							}
							break;
						case "Recipe_CapeAsh":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "FlametalNew":
										req.m_amount = 5;
										req.m_amountPerLevel = 0;
										req.m_resItem = objDB.GetItemPrefab("SulfurStone").GetComponent<ItemDrop>();
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
	}
}
