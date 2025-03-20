using HarmonyLib;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace MarsarahTweaks.Patches
{
	internal class GearRecipeChanges
	{
		private static Dictionary<string, Dictionary<string, (string originalResItem, string newResItem, int amount, int amountPerLevel)>> defaultGearRecipeValues 
			= new Dictionary<string, Dictionary<string, (string originalResItem, string newResItem, int amount, int amountPerLevel)>>();
		
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class GearRecipeModifications_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating GearRecipeModifications...");

						if (!ConfigManager.gearRecipeAmountsEnabled.Value && !ConfigManager.gearRecipeMaterialsEnabled.Value) return;

						ModifyGearRecipes(__instance, false, false);
					}
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to Gear Recipes...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to Gear Recipes...");
				}
			}
		}

		// Modify Gear Recipes
		public static void ModifyGearRecipes(ObjectDB objDB, bool amountsWasChanged, bool materialsWasChanged)
		{
			// cheaperGearChanges Dictionary ============================================
			var cheaperGearChanges = new Dictionary<string, Dictionary<string, (int? amount, int? amountPerLevel)>>()
				{
					// == Weapons, projeciles & shields ==
					// Wood
					{ "Recipe_Club", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 2) }, // 6, 2
							{ "BoneFragments", (null, 2) } // 0, 5
						}
					},
					{ "Recipe_SledgeStagbreaker", new Dictionary<string, (int?, int?)>
						{
							{ "BoneFragments", (2, 8) } // 0, 10
						}
					},
					{ "Recipe_Bow", new Dictionary<string, (int?, int?)>
						{
							{ "LeatherScraps", (null, 3) } // 8, 4
						}
					},
					{ "Recipe_ShieldWood", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (8, null) }, // 10, 5
							{ "Resin", (0, 0) } // 4, 2
						}
					},
					{ "Recipe_ArrowWood", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) } // 8, null
						}
					},
					{ "Recipe_ArrowFire", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) }, // 8, null
							{ "Resin", (5, null) } // 8, null
						}
					},

					// Early tools
					{ "Recipe_AxeStone", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 1) } // 5, 0
						}
					},

					// Flint
					{ "Recipe_KnifeFlint", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 1) } // 2, 0
						}
					},
					{ "Recipe_SpearFlint", new Dictionary<string, (int?, int?)>
						{
							{ "Flint", (8, null) } // 10, 5
						}
					},
					{ "Recipe_AxeFlint", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 1) }, // 4, 0
							{ "LeatherScraps", (1, 1) } // 0, 2
						}
					},
					{ "Recipe_ArrowFlint", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) } // 8, null
						}
					},

					// Copper
					{ "Recipe_KnifeCopper", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 1) }, // 2, 0
							{ "Copper", (null, 3) }, // 8, 4
							{ "GreydwarfEye", (null, 6) } // 0, 8
						}
					},

					// Bronze
					{ "Recipe_MaceBronze", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 1) }, // 4, 0
							{ "LeatherScraps", (null, 1) }, // 3, 0
							{ "Bronze", (null, 3) } // 8, 4
						}
					},
					{ "Recipe_SwordBronze", new Dictionary<string, (int?, int?)>
						{
							{ "Bronze", (null, 3) }, // 8, 4
							{ "LeatherScraps", (4, null) } // 2, 1
						}
					},
					{ "Recipe_AxeBronze", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 1) }, // 4, 0
							{ "Bronze", (null, 3) } // 8, 4
						}
					},
					{ "Recipe_AtgeirBronze", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (8, 2) }, // 10, 0
							{ "LeatherScraps", (null, 1) } // 2, 0
						}
					},
					{ "Recipe_ShieldBronzeBuckler", new Dictionary<string, (int?, int?)>
						{
							{ "Bronze", (null, 4) } // 10, 5
						}
					},
					{ "Recipe_PickaxeBronze", new Dictionary<string, (int?, int?)>
						{
							{ "Bronze", (7, 4) } // 10, 5
						}
					},
					{ "Recipe_ArrowBronze", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) } // 8, null
						}
					},

					// Iron
					{ "Recipe_MaceIron", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 2) }, // 4, 0
							{ "Iron", (10, 5) }, // 20, 10
							{ "LeatherScraps", (null, 1) } // 3, 0
						}
					},
					{ "Recipe_SledgeIron", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (15, 7) }, // 30, 15
							{ "YmirRemains", (6, 0) } // 4, 2
						}
					},
					{ "Recipe_Battleaxe", new Dictionary<string, (int?, int?)>
						{
							{ "ElderBark", (20, null) }, // 30, 5
							{ "Iron", (18, 7) }, // 35, 15
							{ "LeatherScraps", (null, 1) } // 4, 0
						}
					},
					{ "Recipe_SwordIron", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 2) }, // 2, 1
							{ "Iron", (10, 5) } // 20, 10
						}
					},
					{ "Recipe_AxeIron", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 2) }, // 4, 0
							{ "Iron", (10, 5) }, // 20, 10
							{ "LeatherScraps", (null, 2) } // 2, 1
						}
					},
					{ "Recipe_AtgeirIron", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (null, 3) }, // 10, 0
							{ "Iron", (15, 7) } // 30, 15
						}
					},
					{ "Recipe_BowHuntsman", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (10, 5) } // 20, 10
						}
					},
					{ "Recipe_ShieldIronBuckler", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (6, 3) } // 10, 5
						}
					},
					{ "Recipe_PickaxeIron", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (10, 5) } // 20, 10
						}
					},
					{ "Recipe_ArrowIron", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) } // 8, null
						}
					},

					// Silver & Obsidian
					{ "Recipe_KnifeSilver", new Dictionary<string, (int?, int?)>
						{
							{ "Silver", (7, 4) } // 10, 5
						}
					},
					{ "Recipe_MaceSilver", new Dictionary<string, (int?, int?)>
						{
							{ "ElderBark", (15, 3) }, // 10, 0
							{ "Silver", (20, 7) } // 30, 15
						}
					},
					{ "Recipe_SwordSilver", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (15, 3) }, // 2, 1
							{ "Silver", (20, 7) }, // 40, 20
							{ "LeatherScraps", (5, null) }, // 3, 1
							{ "Iron", (3, 1) } // 5, 3
						}
					},
					{ "Recipe_BowDraugrFang", new Dictionary<string, (int?, int?)>
						{
							{ "Silver", (10, 5) }, // 20, 10
							{ "Guck", (7, null) } // 10, 2
						}
					},
					{ "Recipe_Battleaxe_Crystal", new Dictionary<string, (int?, int?)>
						{
							{ "Silver", (15, 7) }, // 30, 15
							{ "Crystal", (3, 1) } // 10, 0
						}
					},
					{ "Recipe_FistFenrirClaw", new Dictionary<string, (int?, int?)>
						{
							{ "Silver", (5, null) } // 10, 1
						}
					},
					{ "Recipe_ArrowObsidian", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) }, // 8, null
							{ "Obsidian", (3, null) } // 4, null
						}
					},
					{ "Recipe_ArrowFrost", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) }, // 8, null
							{ "Obsidian", (3, null) } // 4, null
						}
					},
					{ "Recipe_ArrowPoison", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) }, // 8, null
							{ "Obsidian", (3, null) } // 4, null
						}
					},
					{ "Recipe_ArrowSilver", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) } // 8, null
						}
					},

					// Ocean
					{ "Recipe_KnifeChitin", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (null, 2) }, // 4, 0
							{ "Chitin", (10, 5) }, // 20, 10
							{ "LeatherScraps", (null, 1) } // 2, 0
						}
					},
					{ "Recipe_SpearChitin", new Dictionary<string, (int?, int?)>
						{
							{ "Chitin", (15, null) } // 30, null
						}
					},
					{ "Recipe_ShieldSerpentscale", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (20, null) }, // 10, 10
							{ "Iron", (3, 1) } // 4, 2
						}
					},

					// Plains
					{ "Recipe_MaceNeedle", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (4, 2) }, // 5, 0
							{ "Iron", (10, null) }, // 20, 2
							{ "LinenThread", (5, 5) } // 10, 0
						}
					},
					{ "Recipe_KnifeBlackmetal", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (null, 1) } // 4, 0
						}
					},
					{ "Recipe_SwordBlackmetal", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (3, 2) }, // 2, 0
							{ "BlackMetal", (10, 5) } // 20, 10
						}
					},
					{ "Recipe_AtgeirBlackmetal", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (null, 3) }, // 10, 0
							{ "BlackMetal", (15, 7) } // 30, 15
						}
					},
					{ "Recipe_AxeBlackMetal", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (null, 2) }, // 6, 0
							{ "BlackMetal", (10, 5) } // 20, 10
						}
					},
					{ "Recipe_PickaxeBlackMetal", new Dictionary<string, (int?, int?)>
						{
							{ "BlackMetal", (10, 5) } // 25, 15
						}
					},
					{ "Recipe_ShieldBlackmetal", new Dictionary<string, (int?, int?)>
						{
							{ "Chain", (null, 0) } // 5, 2
						}
					},
					{ "Recipe_ShieldBlackmetalTower", new Dictionary<string, (int?, int?)>
						{
							{ "Chain", (6, 0) } // 7, 2
						}
					},
					{ "Recipe_ArrowNeedle", new Dictionary<string, (int?, int?)>
						{
							{ "Needle", (3, null) } // 4, null
						}
					},

					// Mistlands
					{
						"Recipe_AxeJotunBane", new Dictionary<string, (int?, int?)>
						{
							{ "YggdrasilWood", (null, 1) }, // 5, 0
							{ "Iron", (10, 5) }, // 15, 10
							{ "Eitr", (null, 2) } // 10, 1
						}
					},
					{
						"Recipe_SpearCarapace", new Dictionary<string, (int?, int?)>
						{
							{ "YggdrasilWood", (null, 2) } // 10, 5
						}
					},
					{
						"Recipe_SwordMistwalker", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (5, 1) }, // 3, 0
							{ "Iron", (10, 5) }, // 15, 10
							{ "Eitr", (null, 2) } // 10, 5
						}
					},
					{
						"Recipe_AtgeirHimminAfl", new Dictionary<string, (int?, int?)>
						{
							{ "YggdrasilWood", (null, 2) }, // 10, 0
							{ "Eitr", (10, 5) } // 15, 15
						}
					},
					{
						"Recipe_KnifeSkollAndHati", new Dictionary<string, (int?, int?)>
						{
							{ "FineWood", (5, 1) } // 4, 0
						}
					},
					{
						"Recipe_SledgeDemolisher", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (10, 5) } // 20, 15
						}
					},
					{
						"Recipe_SwordKrom", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (15, 5) }, // 30, 15
							{ "Bronze", (10, 5) } // 20, 10
						}
					},
					{
						"Recipe_ShieldCarapace", new Dictionary<string, (int?, int?)>
						{
							{ "Eitr", (null, 2) } // 10, 3
						}
					},
					{
						"Recipe_ShieldCarapaceBuckler", new Dictionary<string, (int?, int?)>
						{
							{ "Carapace", (15, 7) }, // 16, 8
							{ "Eitr", (null, 2) } // 10, 3
						}
					},
					{
						"Recipe_BowSpineSnap", new Dictionary<string, (int?, int?)>
						{
							{ "BoneFragments", (30, 15) }, // 40, 20
							{ "Eitr", (null, 2) } // 10, 0
						}
					},
					{
						"Recipe_ArrowCarapace", new Dictionary<string, (int?, int?)>
						{
							{ "Carapace", (2, null) }, // 4, null
							{ "Wood", (5, null) } // 8, null
						}
					},
					{
						"Recipe_BoltBlackmetal", new Dictionary<string, (int?, int?)>
						{
							{ "BlackMetal", (1, null) }, // 2, null
							{ "Wood", (5, null) } // 8, null
						}
					},
					{
						"Recipe_BoltBone", new Dictionary<string, (int?, int?)>
						{
							{ "BoneFragments", (5, null) } // 8, null
						}
					},
					{
						"Recipe_BoltCarapace", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) } // 8, null
						}
					},
					{
						"Recipe_BoltIron", new Dictionary<string, (int?, int?)>
						{
							{ "Wood", (5, null) } // 8, null
						}
					},
					{
						"Recipe_StaffSkeleton", new Dictionary<string, (int?, int?)>
						{
							{ "Eitr", (15, 5) } // 16, 8
						}
					},
					{
						"Recipe_StaffFireball", new Dictionary<string, (int?, int?)>
						{
							{ "YggdrasilWood", (15, null) }, // 20, 10
							{ "Eitr", (15, 5) } // 16, 8
						}
					},
					{
						"Recipe_StaffIceShards", new Dictionary<string, (int?, int?)>
						{
							{ "YggdrasilWood", (15, null) }, // 20, 10
							{ "Eitr", (15, 5) } // 16, 8
						}
					},
					{
						"Recipe_StaffShield", new Dictionary<string, (int?, int?)>
						{
							{ "YggdrasilWood", (15, null) }, // 20, 10
							{ "Eitr", (15, 5) } // 16, 8
						}
					},

					// Ashlands
					{
						"Recipe_MaceEldner", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (10, 5) } // 15, 8
						}
					},
					{
						"Recipe_MaceEldner_Blood", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 8, 8
						}
					},
					{
						"Recipe_MaceEldner_Lightning", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 8, 8
						}
					},
					{
						"Recipe_MaceEldner_Nature", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 8, 8
						}
					},
					{
						"Recipe_SpearSplitner", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (10, 5) }, // 6, 6
							{ "BonemawSerpentTooth", (null, 2) } // 3, 3
						}
					},
					{
						"Recipe_SpearSplitner_Blood", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 6, 6
						}
					},
					{
						"Recipe_SpearSplitner_Lightning", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 6, 6
						}
					},
					{
						"Recipe_SpearSplitner_Nature", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 6, 6
						}
					},
					{
						"Recipe_SwordNiedhogg", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (10, 5) }, // 12, 10
							{ "CharredBone", (null, 1) } // 3, 0
						}
					},
					{
						"Recipe_SwordNiedhogg_Blood", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 6, 6
						}
					},
					{
						"Recipe_SwordNiedhogg_Lightning", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 6, 6
						}
					},
					{
						"Recipe_SwordNiedhogg_Nature", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 6, 6
						}
					},
					{
						"Recipe_SwordSlayer", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (15, 7) }, // 30, 15
							{ "AskHide", (null, 4) } // 5, 5
						}
					},
					{
						"Recipe_SwordSlayer_Blood", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 7) } // 15, 15
						}
					},
					{
						"Recipe_SwordSlayer_Lightning", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 7) } // 15, 15
						}
					},
					{
						"Recipe_SwordSlayer_Nature", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 7) } // 15, 15
						}
					},
					{
						"Recipe_AxeBerzerkr", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (15, 7) }, // 24, 15
							{ "CharredBone", (null, 2) } // 15, 0
						}
					},
					{
						"Recipe_BowAshlands", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (8, 4) }, // 5, 5
							{ "CharredBone", (15, null) }, // 16, 10
							{ "BonemawSerpentTooth", (4, 4) } // 5, 5
						}
					},
					{
						"Recipe_CrossbowRipper_Blood", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 8, 8
						}
					},
					{
						"Recipe_CrossbowRipper_Lightning", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 8, 8
						}
					},
					{
						"Recipe_CrossbowRipper_Nature", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 5) } // 8, 8
						}
					},
					{
						"Recipe_ArrowCharred", new Dictionary<string, (int?, int?)>
						{
							{ "Blackwood", (5, null) } // 8, null
						}
					},
					{
						"Recipe_BoltCharred", new Dictionary<string, (int?, int?)>
						{
							{ "Blackwood", (5, null) } // 8, null
						}
					},
					{
						"Recipe_StaffGreenRoots", new Dictionary<string, (int?, int?)>
						{
							{ "CelestialFeather", (null, 2) } // 3, 3
						}
					},
					{
						"Recipe_StaffLightning", new Dictionary<string, (int?, int?)>
						{
							{ "CelestialFeather", (null, 2) } // 3, 3
						}
					},
					{
						"Recipe_StaffRedTroll", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (null, 2) } // 3, 3
						}
					},

					// == Armor ==
					// Rag
					{
						"Recipe_ArmorRagsChest", new Dictionary<string, (int?, int?)>
						{
							{ "LeatherScraps", (6, 4) } // 5, 5
						}
					},
					{
						"Recipe_ArmorRagsLegs", new Dictionary<string, (int?, int?)>
						{
							{ "LeatherScraps", (6, 4) } // 5, 5
						}
					},

					// Leather
					{
						"Recipe_HelmetLeather", new Dictionary<string, (int?, int?)>
						{
							{ "DeerHide", (7, 5) }, // 6, 6
							{ "BoneFragments", (null, 4) } // 0, 5
						}
					},
					{
						"Recipe_ArmorLeatherChest", new Dictionary<string, (int?, int?)>
						{
							{ "DeerHide", (7, 5) }, // 6, 6
							{ "BoneFragments", (null, 4) } // 0, 5
						}
					},
					{
						"Recipe_ArmorLeatherLegs", new Dictionary<string, (int?, int?)>
						{
							{ "DeerHide", (7, 5) }, // 6, 6
							{ "BoneFragments", (null, 4) } // 0, 5
						}
					},
					{
						"Recipe_CapeDeerHide", new Dictionary<string, (int?, int?)>
						{
							{ "DeerHide", (5, 3) }, // 4, 4
							{ "BoneFragments", (0, 2) } // 5, 5
						}
					},

					// Troll
					{
						"Recipe_HelmetTrollLeather", new Dictionary<string, (int?, int?)>
						{
							{ "TrollHide", (null, 3) }, // 5, 2
							{ "BoneFragments", (0, 0) } // 3, 1
						}
					},
					{
						"Recipe_ArmorTrollLeatherChest", new Dictionary<string, (int?, int?)>
						{
							{ "TrollHide", (6, 3) } // 5, 2
						}
					},
					{
						"Recipe_ArmorTrollLeatherLegs", new Dictionary<string, (int?, int?)>
						{
							{ "TrollHide", (6, 3) } // 5, 2
						}
					},
					{
						"Recipe_CapeTrollHide", new Dictionary<string, (int?, int?)>
						{
							{ "TrollHide", (4, 3) }, // 10, 5
							{ "BoneFragments", (5, 3) } // 10, 5
						}
					},

					// Root
					{
						"Recipe_HelmetRoot", new Dictionary<string, (int?, int?)>
						{
							{ "Root", (5, null) }, // 10, 2
							{ "ElderBark", (null, 4) } // 10, 5
						}
					},
					{
						"Recipe_ArmorRootChest", new Dictionary<string, (int?, int?)>
						{
							{ "Root", (5, null) }, // 10, 2
							{ "ElderBark", (null, 4) } // 10, 5
						}
					},
					{
						"Recipe_ArmorRootLegs", new Dictionary<string, (int?, int?)>
						{
							{ "Root", (5, null) }, // 10, 2
							{ "ElderBark", (null, 4) } // 10, 5
						}
					},

					// Iron
					{
						"Recipe_HelmetIron", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (10, 4) } // 20, 5
						}
					},
					{
						"Recipe_ArmorIronChest", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (10, 4) } // 20, 5
						}
					},
					{
						"Recipe_ArmorIronLegs", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (10, 4) } // 20, 5
						}
					},

					// Fenris
					{
						"Recipe_HelmetFenrir", new Dictionary<string, (int?, int?)>
						{
							{ "WolfHairBundle", (10, 4) }, // 20, 5
							{ "WolfPelt", (3, 1) } // 2, 4
						}
					},
					{
						"Recipe_ArmorFenrirChest", new Dictionary<string, (int?, int?)>
						{
							{ "WolfHairBundle", (10, 4) }, // 20, 5
							{ "WolfPelt", (4, 2) }, // 5, 3
							{ "LeatherScraps", (7, 3) } // 10, 4
						}
					},
					{
						"Recipe_ArmorFenrirLegs", new Dictionary<string, (int?, int?)>
						{
							{ "WolfHairBundle", (10, 4) }, // 20, 5
							{ "WolfPelt", (4, 2) }, // 5, 3
							{ "LeatherScraps", (7, 3) } // 10, 4
						}
					},

					// Silver armor
					{
						"Recipe_HelmetDrake", new Dictionary<string, (int?, int?)>
						{
							{ "Silver", (10, 4) }, // 20, 5
							{ "WolfPelt", (null, 1) }, // 2, 0
							{ "TrophyHatchling", (1, null) } // 2, 0
						}
					},
					{
						"Recipe_ArmorWolfChest", new Dictionary<string, (int?, int?)>
						{
							{ "Silver", (10, 4) }, // 20, 5
							{ "WolfPelt", (null, 1) } // 5, 2
						}
					},
					{
						"Recipe_ArmorWolfLegs", new Dictionary<string, (int?, int?)>
						{
							{ "Silver", (10, 4) }, // 20, 5
							{ "WolfPelt", (null, 1) }, // 5, 2
							{ "WolfFang", (null, 0) } // 4, 1
						}
					},
					{
						"Recipe_CapeWolf", new Dictionary<string, (int?, int?)>
						{
							{ "Silver", (2, 1) }, // 4, 2
							{ "WolfPelt", (5, 1) } // 6, 4
						}
					},

					// Linen armor
					{
						"Recipe_HelmetPadded", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (5, 3) } // 10, 5
						}
					},
					{
						"Recipe_ArmorPaddedCuirass", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (5, null) } // 10, 3
						}
					},
					{
						"Recipe_ArmorPaddedGreaves", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (5, null) } // 10, 3
						}
					},
					{
						"Recipe_CapeLinen", new Dictionary<string, (int?, int?)>
						{
							{ "LinenThread", (10, 2) } // 20, 4
						}
					},

					// Other Plains Armor
					{
						"Recipe_CapeLox", new Dictionary<string, (int?, int?)>
						{
							{ "LoxPelt", (5, null) } // 6, 2
						}
					},

					// Mistlands armor
					{
						"Recipe_HelmetMage", new Dictionary<string, (int?, int?)>
						{
							{ "LinenThread", (15, 5) }, // 16, 8
							{ "Iron", (0, null) } // 2, 0
						}
					},
					{
						"Recipe_ArmorMageChest", new Dictionary<string, (int?, int?)>
						{
							{ "Eitr", (15, null) } // 20, 5
						}
					},
					{
						"Recipe_ArmorMageLegs", new Dictionary<string, (int?, int?)>
						{
							{ "Eitr", (15, null) } // 20, 5
						}
					},
					{
						"Recipe_HelmetCarapace", new Dictionary<string, (int?, int?)>
						{
							{ "Carapace", (15, 5) } // 16, 8
						}
					},

					// Other Mistlands
					{
						"Recipe_CapeFeather", new Dictionary<string, (int?, int?)>
						{
							{ "ScaleHide", (7, 3) }, // 5, 5
							{ "Eitr", (10, null) } // 20, 3
						}
					},
					{
						"Recipe_Lantern", new Dictionary<string, (int?, int?)>
						{
							{ "Bronze", (1, null) } // 2, null
						}
					},
					{
						"Recipe_MechanicalSpring", new Dictionary<string, (int?, int?)>
						{
							{ "Iron", (1, null) } // 3, null
						}
					},

					// Ashlands armor
					{
						"Recipe_HelmetMage_Ashlands", new Dictionary<string, (int?, int?)>
						{
							{ "LinenThread", (15, 5) } // 16, 8
						}
					},
					{
						"Recipe_ArmorMageChest_Ashlands", new Dictionary<string, (int?, int?)>
						{
							{ "Eitr", (15, null) }, // 20, 5
							{ "FlametalNew", (0, 0) } // 5, 2
						}
					},
					{
						"Recipe_ArmorMageLegs_Ashlands", new Dictionary<string, (int?, int?)>
						{
							{ "Eitr", (15, null) } // 20, 5
						}
					},
					{
						"Recipe_HelmetMedium_Ashlands", new Dictionary<string, (int?, int?)>
						{
							{ "AskHide", (null, 4) } // 10, 5
						}
					},
					{
						"Recipe_ArmorMediumChest_Ashlands", new Dictionary<string, (int?, int?)>
						{
							{ "AskHide", (null, 4) } // 10, 5
						}
					},
					{
						"Recipe_ArmorMediumLegs_Ashlands", new Dictionary<string, (int?, int?)>
						{
							{ "AskHide", (null, 4) } // 10, 5
						}
					},
					{
						"Recipe_HelmetFlametal", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (10, 5) }, // 16, 8
							{ "CharredBone", (3, 1) }, // 2, 0
							{ "Eitr", (null, 0) } // 4, 2
						}
					},
					{
						"Recipe_ArmorFlametalChest", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (10, 5) }, // 20, 10
							{ "CharredBone", (null, 1) } // 5, 0
						}
					},
					{
						"Recipe_ArmorFlametalLegs", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (10, 5) }, // 20, 10
							{ "CharredBone", (null, 1) } // 5, 0
						}
					},

					// Other Ashlands
					{
						"Recipe_CapeAsh", new Dictionary<string, (int?, int?)>
						{
							{ "FlametalNew", (0, null) } // 5, 0
						}
					}
				};

			// alternateGearChanges Dictionary ==========================================
			var alternateGearRecipes = new Dictionary<string, Dictionary<string, (string newResItem, int? amount, int? amountPerLevel)>>()
				{
					{
						"Recipe_Club", new Dictionary<string, (string, int?, int?)>
						{
							{ "BoneFragments", ("LeatherScraps", null, null) }
						}
					},
					{
						"Recipe_KnifeCopper", new Dictionary<string, (string, int?, int?)>
						{
							{ "GreydwarfEye", ("LeatherScraps", null, 2) }
						}
					},
					{
						"Recipe_KnifeSilver", new Dictionary<string, (string, int?, int?)>
						{
							{ "Wood", ("FineWood", null, null) },
							{ "Iron", ("Obsidian", null, null) }
						}
					},
					{
						"Recipe_SwordSilver", new Dictionary<string, (string, int?, int?)>
						{
							{ "Wood", ("FineWood", 10, 2) },
							{ "Iron", ("Obsidian", 4, 2) }
						}
					},
					{
						"Recipe_ShieldSerpentscale", new Dictionary<string, (string, int?, int?)>
						{
							{ "Iron", ("Chitin", 4, 1) }
						}
					},
					{
						"Recipe_MaceNeedle", new Dictionary<string, (string, int?, int?)>
						{
							{ "Iron", ("BlackMetal", null, null) }
						}
					},
					{
						"Recipe_ArmorWolfChest", new Dictionary<string, (string, int?, int?)>
						{
							{ "Chain", ("WolfFang", 3, null) }
						}
					},
					{
						"Recipe_HelmetPadded", new Dictionary<string, (string, int?, int?)>
						{
							{ "Iron", ("BlackMetal", 5, 3) }
						}
					},
					{
						"Recipe_ArmorPaddedCuirass", new Dictionary<string, (string, int?, int?)>
						{
							{ "Iron", ("BlackMetal", 5, null) }
						}
					},
					{
						"Recipe_ArmorPaddedGreaves", new Dictionary<string, (string, int?, int?)>
						{
							{ "Iron", ("BlackMetal", 5, null) }
						}
					},
					{
						"Recipe_CapeLinen", new Dictionary<string, (string, int?, int?)>
						{
							{ "Silver", ("BlackMetal", null, null) }
						}
					},
					{
						"Recipe_CapeLox", new Dictionary<string, (string, int?, int?)>
						{
							{ "Silver", ("BlackMetal", null, null) }
						}
					},
					{
						"Recipe_CrossbowArbalest", new Dictionary<string, (string, int?, int?)>
						{
							{ "Wood", ("FineWood", null, null) }
						}
					},
					{
						"Recipe_HelmetMage", new Dictionary<string, (string, int?, int?)>
						{
							{ "Iron", ("ScaleHide", 3, null) }
						}
					},
					{
						"Recipe_ArmorMageChest_Ashlands", new Dictionary<string, (string, int?, int?)>
						{
							{ "FlametalNew", ("SulfurStone", 5, 2) }
						}
					},
					{
						"Recipe_CapeAsh", new Dictionary<string, (string, int?, int?)>
						{
							{ "FlametalNew", ("SulfurStone", 5, null) }
						}
					}
				};

			/******************************************
			 * Loop through original list
			 If list 1 is enabled:
				If item exists in list 1:
					If item is NOT in backup list:
						Store original value in backup list
					Apply changes to item

			 Else (list 1 is disabled):
				If item exists in list 1:
					If item exists in backup list:
						Restore original value from backup list
						Remove item from backup list

			 If list 2 is enabled:
				If item exists in list 2:
					If item is NOT in backup list:
						Store original value in backup list
					Apply changes from list 2

			 Else (list 2 is disabled):
				If item exists in list 2:
					If item exists in backup list:
						Restore original value from backup list

						If list 1 is enabled AND item exists in list 1:
							Keep backup
							Apply changes from list 1
						Else:
							Remove backup
			 ******************************************/

			// Apply changes ============================================================
			foreach (Recipe recipe in objDB.m_recipes)
			{
				bool hasGearAmountsChange = ConfigManager.gearRecipeAmountsEnabled.Value && cheaperGearChanges.ContainsKey(recipe.name);
				bool hasGearMaterialsChange = ConfigManager.gearRecipeMaterialsEnabled.Value && alternateGearRecipes.ContainsKey(recipe.name);

				/*if (cheaperGearChanges.ContainsKey(recipe.name)) MarsarahTweaks.MLog($"Is in Gear Amounts list: {recipe.name}");
				if (alternateGearRecipes.ContainsKey(recipe.name)) MarsarahTweaks.MLog($"Is in Gear Material list: {recipe.name}");
				if (hasGearAmountsChange) MarsarahTweaks.MLog($"Will modify from Gear Amounts list: {recipe.name}");
				if (hasGearMaterialsChange) MarsarahTweaks.MLog($"Will modify from Gear Materials list: {recipe.name}");*/

				foreach (Piece.Requirement req in recipe.m_resources)
				{
					/*bool needsBackup = hasGearAmountsChange || hasGearMaterialsChange;

					// Ensure backup is created only once if modifications will be applied
					if (needsBackup)
					{
						CreateBackup(recipe.name, req);
					}*/

					// Apply gear amounts modifications
					if (hasGearAmountsChange && cheaperGearChanges[recipe.name].TryGetValue(req.m_resItem.name, out var amountValues))
					{
						CreateBackup(recipe.name, req, null);

						//MarsarahTweaks.MLog($"(Gear Amounts) Applying changes for: {recipe.name} - {req.m_resItem.name}");
						ApplyChanges(req, (null, amountValues.amount, amountValues.amountPerLevel), objDB, modifyResItem: false);
					}

					// Apply gear materials modifications
					if (hasGearMaterialsChange && alternateGearRecipes[recipe.name].TryGetValue(req.m_resItem.name, out var materialValues))
					{
						CreateBackup(recipe.name, req, materialValues.newResItem);

						//MarsarahTweaks.MLog($"(Gear Materials) Applying changes for: {recipe.name} - {req.m_resItem.name}");
						ApplyChanges(req, materialValues, objDB, modifyResItem: true);
					}

					// Restore backups when disabling features
					if (!ConfigManager.gearRecipeAmountsEnabled.Value && cheaperGearChanges.ContainsKey(recipe.name) && amountsWasChanged)
					{
						//MarsarahTweaks.MLog($"(Gear Amounts) Was changed: {amountsWasChanged}");
						if (RestoreBackup(recipe.name, req, objDB, false))
						{
							// Remove backup unless materials modification still needs it
							if (!hasGearMaterialsChange || !alternateGearRecipes[recipe.name].ContainsKey(req.m_resItem.name))
							{
								//MarsarahTweaks.MLog($"(Gear Amounts) Removing backup for: {recipe.name} - {req.m_resItem.name}");
								defaultGearRecipeValues[recipe.name].Remove(req.m_resItem.name);
							}
						}
					}

					if (!ConfigManager.gearRecipeMaterialsEnabled.Value && alternateGearRecipes.ContainsKey(recipe.name) && materialsWasChanged)
					{
						//MarsarahTweaks.MLog($"(Gear Materials) Was changed: {materialsWasChanged}");
						if (RestoreBackup(recipe.name, req, objDB, true))
						{
							if (hasGearAmountsChange && cheaperGearChanges[recipe.name].ContainsKey(req.m_resItem.name))
							{
								// Apply gear amounts modifications again after restoring
								if (cheaperGearChanges[recipe.name].TryGetValue(req.m_resItem.name, out var restoredValues))
								{
									//MarsarahTweaks.MLog($"(Gear Materials - Amounts) Re-applying changes for: {recipe.name} - {req.m_resItem.name}");
									ApplyChanges(req, (null, restoredValues.amount, restoredValues.amountPerLevel), objDB, false);
								}
							}
							else if (!hasGearAmountsChange || !cheaperGearChanges[recipe.name].ContainsKey(req.m_resItem.name))
							{
								//MarsarahTweaks.MLog($"(Gear Materials) Removing backup for: {recipe.name} - {req.m_resItem.name}");
								defaultGearRecipeValues[recipe.name].Remove(req.m_resItem.name);
							}
						}
					}
				}

				// Remove entire backup entry if empty
				if (defaultGearRecipeValues.ContainsKey(recipe.name) && defaultGearRecipeValues[recipe.name].Count == 0)
				{
					//MarsarahTweaks.MLog($"(Cleanup) Removing backup for: {recipe.name}");
					defaultGearRecipeValues.Remove(recipe.name);
				}
			}
		}

		// Apply Changes helper function
		private static void ApplyChanges(Piece.Requirement req, (string newResItem, int? amount, int? amountPerLevel) values, ObjectDB objDB, bool modifyResItem = false)
		{
			if (values.amount.HasValue)
			{
				req.m_amount = values.amount.Value;
			}
			if (values.amountPerLevel.HasValue)
			{
				req.m_amountPerLevel = values.amountPerLevel.Value;
			}
			if (modifyResItem && !string.IsNullOrEmpty(values.newResItem))
			{
				req.m_resItem = objDB.GetItemPrefab(values.newResItem).GetComponent<ItemDrop>();
			}
		}

		// Create Backup helper function
		private static void CreateBackup(string recipeName, Piece.Requirement req, string newResItem)
		{
			if (!defaultGearRecipeValues.TryGetValue(recipeName, out var recipeBackup))
			{
				recipeBackup = new Dictionary<string, (string originalResItem, string newResItem, int amount, int amountPerLevel)>();
				defaultGearRecipeValues[recipeName] = recipeBackup;
			}

			string currentResItem = req.m_resItem.name;

			// If there's already a backup for this resource, don't overwrite it unless newResItem is missing
			if (recipeBackup.TryGetValue(currentResItem, out var existingBackup))
			{
				if (newResItem != null && existingBackup.newResItem == null)
				{
					//MarsarahTweaks.MLog($"Updating backup for {recipeName} - {currentResItem} with newResItem: {newResItem}");
					recipeBackup[currentResItem] = (existingBackup.originalResItem, newResItem, existingBackup.amount, existingBackup.amountPerLevel);
				}
			}
			else
			{
				// Create a new backup for this resource without affecting existing ones
				//MarsarahTweaks.MLog($"Creating new backup for {recipeName} - {currentResItem}");
				recipeBackup[currentResItem] = (currentResItem, newResItem, req.m_amount, req.m_amountPerLevel);
			}
		}


		// Restore Backup helper function
		private static bool RestoreBackup(string recipeName, Piece.Requirement req, ObjectDB objDB, bool restoreMaterials)
		{
			if (!defaultGearRecipeValues.TryGetValue(recipeName, out var recipeBackup))
			{
				//MarsarahTweaks.MLog("Restore backup first check.");
				return false;
			}

			// Restoring original materials if any
			//MarsarahTweaks.MLog($"Restore backup - Recipe name: {recipeName}, Given requirement: {req.m_resItem.name}");
			foreach (var kvp in recipeBackup)
			{
				var (originalMaterial, newMaterial, amount, amountPerLevel) = kvp.Value;
				{
					//MarsarahTweaks.MLog($"Restore backup - values: {originalMaterial}, {newMaterial}, {restoreMaterials}");

					if (req.m_resItem.name == newMaterial && restoreMaterials)
					{
						//MarsarahTweaks.MLog($"Restoring original material for {recipeName} from {req.m_resItem.name} to {originalMaterial}");
						req.m_resItem = objDB.GetItemPrefab(originalMaterial).GetComponent<ItemDrop>();
						break;
					}
				}
			}

			if (recipeBackup.TryGetValue(req.m_resItem.name, out var originalValues))
			{
				//MarsarahTweaks.MLog($"Restoring backup for: {recipeName} - {req.m_resItem.name}");

				// Restore original values
				req.m_amount = originalValues.amount;
				req.m_amountPerLevel = originalValues.amountPerLevel;

				return true;
			}
			//MarsarahTweaks.MLog("Restore backup - we got to the end.");

			return false; // No backup found
		}
	}
}
