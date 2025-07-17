using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Grind
{
	internal class FoodAndMeadChanges
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class FoodAndMeadChanges_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateFoodAndMead(__instance, false);
			}
		}

		// Recipe Modification Class
		private class RecipeModification
		{
			public Dictionary<string, int> ResourceChanges { get; set; } = new Dictionary<string, int>();
			public Dictionary<string, string> ResourceReplacements { get; set; } = new Dictionary<string, string>();
			public Dictionary<string, (string originalResItem, string newResItem)> OriginalResourceReplacements { get; set; } = new Dictionary<string, (string originalResItem, string newResItem)>();
			public int? RecipeAmount { get; set; }
		}

		// Dictionaries
		private static Dictionary<string, RecipeModification> foodRecipeModifications = new Dictionary<string, RecipeModification>
		{
			{ "Recipe_QueensJam", new RecipeModification
				{
					ResourceChanges = { { "Raspberry", 5 }, { "Blueberries", 5 } }
				}
			},
			{ "Recipe_TurnipStew", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_Blacksoup", new RecipeModification
				{
					ResourceChanges = { { "Bloodbag", 2 }, { "Honey", 2 }, { "Turnip", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_CarrotSoup", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_DeerStew", new RecipeModification
				{
					ResourceChanges = { { "CookedDeerMeat", 2 }, { "Blueberries", 2 }, { "Carrot", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_ShocklateSmoothie", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_MinceMeatSauce", new RecipeModification
				{
					ResourceChanges = { { "RawMeat", 2 }, { "NeckTail", 2 }, { "Carrot", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_Eyescream", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_Onionsoup", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_WolfSkewer", new RecipeModification
				{
					ResourceChanges = { { "WolfMeat", 2 }, { "Onion", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_SerpentStew", new RecipeModification
				{
					ResourceChanges = { { "Mushroom", 2 }, { "SerpentMeatCooked", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_BloodPudding", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_FishWraps", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_FishAndBread", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_LoxPie", new RecipeModification
				{
					RecipeAmount = 2
				}
			},
			{ "Recipe_Bread", new RecipeModification
				{
					ResourceChanges = { { "BarleyFlour", 6 } }
				}
			},
			{ "Recipe_HoneyGlazedChicken", new RecipeModification
				{
					ResourceChanges = { { "ChickenMeat", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_MagicallyStuffedShroom", new RecipeModification
				{
					ResourceChanges = { { "GiantBloodSack", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_MeatPlatter", new RecipeModification
				{
					ResourceChanges = { { "BugMeat", 2 }, { "LoxMeat", 2 }, { "HareMeat", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_MisthareSupreme", new RecipeModification
				{
					ResourceChanges = { { "HareMeat", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_MushroomOmelette", new RecipeModification
				{
					ResourceChanges = { { "ChickenEgg", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_Salad", new RecipeModification
				{
					ResourceChanges = { { "MushroomJotunPuffs", 4 }, { "Onion", 4 }, { "Cloudberry", 4 } },
					RecipeAmount = 4
				}
			},
			{ "Recipe_YggdrasilPorridge", new RecipeModification
				{
					ResourceChanges = { { "Sap", 2 }, { "Barley", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_FierySvinstew", new RecipeModification
				{
					ResourceChanges = { { "AsksvinMeat", 2 }, { "MushroomSmokePuff", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_MarinatedGreens", new RecipeModification
				{
					ResourceChanges = { { "Sap", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_SizzlingBerryBroth", new RecipeModification
				{
					ResourceChanges = { { "Sap", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_SparklingShroomshake", new RecipeModification
				{
					ResourceChanges = { { "Sap", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_MashedMeat", new RecipeModification
				{
					ResourceChanges = { { "AsksvinMeat", 2 }, { "VoltureMeat", 2 }, { "Fiddleheadfern", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_ScorchingMedley", new RecipeModification
				{
					ResourceChanges = { { "MushroomJotunPuffs", 4 }, { "Onion", 4 }, { "Fiddleheadfern", 4 } },
					RecipeAmount = 4
				}
			},
			{ "Recipe_SpiceInducedMarmalade", new RecipeModification
				{
					ResourceChanges = { { "Vineberry", 2 }, { "Honey", 2 }, { "Fiddleheadfern", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_RoastedCrustPie", new RecipeModification
				{
					ResourceChanges = { { "VoltureEgg", 2 } },
					RecipeAmount = 2
				}
			},
			{ "Recipe_PiquantPie", new RecipeModification
				{
					RecipeAmount = 2
				}
			}
		};
		private static Dictionary<string, RecipeModification> meadAndWineModifications = new Dictionary<string, RecipeModification>()
		{
			{ "Recipe_MeadBaseEitrLingering", new RecipeModification
				{
					ResourceChanges = { { "Sap", 5 }, { "Vineberry", 5 }, { "MushroomMagecap", 5 } }
				}
			},
			{ "Recipe_MeadBaseHealthLingering", new RecipeModification
				{
					ResourceChanges = { { "Sap", 5 }, { "Vineberry", 5 }, { "MushroomSmokePuff", 5 } }
				}
			},
			{ "Recipe_BarleyWineBase", new RecipeModification
				{
					ResourceChanges = { { "Barley", 5 }, { "Cloudberry", 5 } }
				}
			},
			{ "Recipe_MeadBaseEitrMinor", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "Sap", 3 }, { "MushroomMagecap", 3 } }
				}
			},
			{ "Recipe_MeadBaseFrostResist", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "Thistle", 3 } }
				}
			},
			{ "Recipe_MeadBaseHealthMajor", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "GiantBloodSack", 2 }, { "RoyalJelly", 3 } },
					ResourceReplacements = { { "RoyalJelly", "MushroomJotunPuffs"} }
				}
			},
			{ "Recipe_MeadBaseHealthMedium", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "Bloodbag", 3 }, { "Raspberry", 5 } }
				}
			},
			{ "Recipe_MeadBaseHealthMinor", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "Raspberry", 5 } }
				}
			},
			{ "Recipe_MeadBasePoisonResist", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "Thistle", 3 }, { "Coal", 5 } }
				}
			},
			{ "Recipe_MeadBaseStaminaLingering", new RecipeModification
				{
					ResourceChanges = { { "Sap", 5 }, { "Cloudberry", 5 }, { "MushroomJotunPuffs", 5 } }
				}
			},
			{ "Recipe_MeadBaseStaminaMedium", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "Cloudberry", 5 } }
				}
			},
			{ "Recipe_MeadBaseStaminaMinor", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "Raspberry", 5 } }
				}
			},
			{ "Recipe_MeadBaseTasty", new RecipeModification
				{
					ResourceChanges = { { "Honey", 5 }, { "Raspberry", 5 } }
				}
			},
			{ "Recipe_MeadBaseHasty", new RecipeModification // Ratatosk
				{
					ResourceChanges = { { "Honey", 5 }, { "Blueberries", 5 } }
				}
			},
			{ "Recipe_MeadBaseStrength", new RecipeModification // Troll Endurance
				{
					ResourceChanges = { { "Honey", 5 } }
				}
			},
			{ "Recipe_MeadBaseSwimmer", new RecipeModification // Draught of Vananidir
				{
					ResourceChanges = { { "Dandelion", 7 } }
				}
			},
			{ "Recipe_MeadBaseTamer", new RecipeModification // Animal Whispers
				{
					ResourceChanges = { { "Carrot", 5 } }
				}
			}
		};
		private static Dictionary<string, int> foodStacksModifications = new Dictionary<string, int>()
		{
			{ "QueensJam", 20 },
			{ "CarrotSoup", 20 },
			{ "DeerStew", 20 },
			{ "TurnipStew", 20 },
			{ "BlackSoup", 20 },
			{ "Eyescream", 20 },
			{ "ShocklateSmoothie", 20 }, // Muckshake
			{ "MinceMeatSauce", 20 },
			{ "OnionSoup", 20 },
			{ "SerpentStew", 20 },
			{ "BloodPudding", 20 },
			{ "FishWraps", 20 },
			{ "FishAndBread", 20 },
			{ "LoxPie", 20 },
			{ "MushroomOmelette", 20 },
			{ "YggdrasilPorridge", 20 },
			{ "CookedEgg", 20 },
			{ "Salad", 20 },
			{ "SeekerAspic", 20 },
			{ "HoneyGlazedChicken", 20 },
			{ "MagicallyStuffedShroom", 20 },
			{ "MeatPlatter", 20 },
			{ "MisthareSupreme", 20 },
			{ "FierySvinstew", 20 },
			{ "MarinatedGreens", 20 },
			{ "MashedMeat", 20 },
			{ "ScorchingMedley", 20 },
			{ "SizzlingBerryBroth", 20 },
			{ "SpicyMarmalade", 20 },
			{ "SparklingShroomshake", 20 },
			{ "RoastedCrustPie", 20 },
			{ "PiquantPie", 20 },
			{ "FishAndBreadUncooked", 20 },
			{ "LoxPieUncooked", 20 },
			{ "HoneyGlazedChickenUncooked", 20 },
			{ "MagicallyStuffedShroomUncooked", 20 },
			{ "MeatPlatterUncooked", 20 },
			{ "MisthareSupremeUncooked", 20 },
			{ "RoastedCrustPieUncooked", 20 },
			{ "PiquantPieUncooked", 20 }
		};
		private static Dictionary<string, RecipeModification> recipeModifications;

		private static Dictionary<string, RecipeModification> GetRecipeModifications()
		{
			if (recipeModifications == null)
			{
				recipeModifications = foodRecipeModifications;

				foreach (var entry in meadAndWineModifications)
				{
					recipeModifications[entry.Key] = entry.Value;
				}
			}

			return recipeModifications;
		}

		// Dictionary for original values
		private static Dictionary<string, RecipeModification> originalRecipeValues = new Dictionary<string, RecipeModification>();
		private static Dictionary<string, int> originalFoodStacks = new Dictionary<string, int>();

		// Update Food And Mead ====================================================================
		public static void UpdateFoodAndMead (ObjectDB objDB, bool wasModified)
		{
			/*foreach (Recipe recipe in objDB.m_recipes)
			{
				// show item recipe info
				foreach (Piece.Requirement req in recipe.m_resources)
				{
					Debug.Log($"[Marsarah Mod] :: {recipe.name} - Requirement: {req.m_resItem.name} - Amount: {req.m_amount} - Upgrade: {req.m_amountPerLevel} ");
				}
			}*/

			// Get Dictionary for all recipe modifications
			var recipeModifications = GetRecipeModifications();

			if (ConfigManager.FoodAndMeadModificationsEnabled.Value)
			{
				// Apply recipe changes ============================================================
				foreach (KeyValuePair<string, RecipeModification> entry in recipeModifications)
				{
					string recipeName = entry.Key;
					RecipeModification recipeModification = entry.Value;

					Recipe recipe = objDB.m_recipes.FirstOrDefault(r => r.name == recipeName);
					if (recipe == null) continue;

					// Backup entry if it doesn't exist
					if (!originalRecipeValues.ContainsKey(recipeName))
					{
						//MarsarahTweaks.LogInfo($"Initial backup entry for {recipeName}");
						originalRecipeValues[recipeName] = new RecipeModification()
						{
							ResourceChanges = new Dictionary<string, int>(),
							ResourceReplacements = new Dictionary<string, string>(),
							RecipeAmount = recipe.m_amount
							//OriginalRecipeAmount = recipe.m_amount // Backup original recipe amount
						};
					}

					// Apply recipe amount
					if (recipeModification.RecipeAmount.HasValue)
					{
						//MarsarahTweaks.LogInfo($"Apply recipe amount for {recipeName}");
						recipe.m_amount = recipeModification.RecipeAmount.Value;
					}

					// Apply resource modifications
					foreach (Piece.Requirement req in recipe.m_resources)
					{
						// Adjust amounts if applicable
						if (recipeModification.ResourceChanges.TryGetValue(req.m_resItem.name, out int newAmount))
						{
							// Backup original resource amounts if not already saved
							if (!originalRecipeValues[recipeName].ResourceChanges.ContainsKey(req.m_resItem.name))
							{
								//MarsarahTweaks.LogInfo($"Backup resource amount for {recipeName} - {req.m_resItem.name}");
								originalRecipeValues[recipeName].ResourceChanges[req.m_resItem.name] = req.m_amount;
							}

							//MarsarahTweaks.LogInfo($"Apply new resource amount for {recipeName} - {req.m_resItem.name} to: {newAmount}");
							req.m_amount = newAmount;
						}

						// Replace m_resItem if needed
						if (recipeModification.ResourceReplacements.TryGetValue(req.m_resItem.name, out string newItemName))
						{
							// Backup original m_resItem if not already saved
							if (!originalRecipeValues[recipeName].ResourceReplacements.ContainsKey(req.m_resItem.name))
							{
								//MarsarahTweaks.LogInfo($"Backup resource material for {recipeName} - {req.m_resItem.name}");
								originalRecipeValues[recipeName].OriginalResourceReplacements[req.m_resItem.name] = (req.m_resItem.name, newItemName);
							}

							//MarsarahTweaks.LogInfo($"Apply resource material for {recipeName} - {req.m_resItem.name} to: {newItemName}");
							req.m_resItem = objDB.GetItemPrefab(newItemName).GetComponent<ItemDrop>();
						}
					}
				}

				// Apply stack changes =============================================================
				foreach (KeyValuePair<string, int> entry in foodStacksModifications)
				{
					string itemName = entry.Key;
					int itemStack = entry.Value;

					GameObject item = objDB.m_items.FirstOrDefault(r => r.name == itemName);
					if (item == null) continue;

					ItemDrop itemDrop = item.GetComponent<ItemDrop>();
					if (itemDrop != null)
					{
						// Backup original stack if not already saved
						if (!originalFoodStacks.ContainsKey(itemName))
						{
							//MarsarahTweaks.LogInfo($"Backup stack for {itemName}");
							originalFoodStacks[itemName] = itemDrop.m_itemData.m_shared.m_maxStackSize;
						}

						// Apply new changes
						//MarsarahTweaks.LogInfo($"Apply new stack for {itemName} to: {itemStack}");
						itemDrop.m_itemData.m_shared.m_maxStackSize = itemStack;
					}
				}
			}
			else if (wasModified)
			{
				// Revert recipe changes
				foreach (KeyValuePair<string, RecipeModification> entry in originalRecipeValues)
				{
					string recipeName = entry.Key;
					RecipeModification recipeModificationOriginal = entry.Value;

					Recipe recipe = objDB.m_recipes.FirstOrDefault(r => r.name == recipeName);
					if (recipe == null) continue;

					// Restore recipe amount
					if (recipeModificationOriginal.RecipeAmount.HasValue)
					{
						//MarsarahTweaks.LogInfo($"Restoring backup: amount for {recipeName}");
						recipe.m_amount = recipeModificationOriginal.RecipeAmount.Value;
					}

					foreach (Piece.Requirement req in recipe.m_resources)
					{
						// Restore requirement material
						foreach (var kvp in recipeModificationOriginal.OriginalResourceReplacements)
						{
							var (originalResItem, newResItem) = kvp.Value;

							if (req.m_resItem.name == newResItem)
							{
								//MarsarahTweaks.LogInfo($"Restoring backup: requirement material for {recipeName} - {newResItem} back to: {originalResItem}");
								req.m_resItem = objDB.GetItemPrefab(originalResItem).GetComponent<ItemDrop>();
								break;
							}
						}

						// Restore requirement amount
						if (recipeModificationOriginal.ResourceChanges.TryGetValue(req.m_resItem.name, out int originalAmount))
						{
							//MarsarahTweaks.LogInfo($"Restoring backup: requirement amount for {recipeName} - {req.m_resItem.name}");
							req.m_amount = originalAmount;
						}
					}
				}

				// Revert stacks
				foreach (KeyValuePair<string, int> entry in originalFoodStacks)
				{
					string itemName = entry.Key;
					int itemStack = entry.Value;

					GameObject item = objDB.m_items.FirstOrDefault(r => r.name == itemName);
					if (item == null) continue;

					ItemDrop itemDrop = item.GetComponent<ItemDrop>();
					if (itemDrop != null)
					{
						//MarsarahTweaks.LogInfo($"Restoring backup: stacks for {itemName}");
						itemDrop.m_itemData.m_shared.m_maxStackSize = itemStack;
					}
				}

				//MarsarahTweaks.LogInfo($"(Cleanup) Removing backups");
				originalRecipeValues.Clear();
				originalFoodStacks.Clear();
			}
		}
	}
}
