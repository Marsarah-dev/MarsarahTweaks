using HarmonyLib;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Features
{
	internal class CraftableChain
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class CraftableChain_Patch
		{
			private static void Postfix(ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateChainRecipe(__instance, false);
			}
		}

		public static void UpdateChainRecipe(ObjectDB objDB, bool wasChanged)
		{
			if (ConfigManager.CraftableChainEnabled.Value)
			{
				// Check if the chain recipe already exists
				Recipe chainRecipe = objDB.m_recipes.Find(r => r.name == "Recipe_Chain");
				if (chainRecipe == null)
				{
					// Create and add the chain recipe if it doesn't exist
					chainRecipe = CreateChainRecipe(objDB);
					if (chainRecipe != null)
					{
						objDB.m_recipes.Add(chainRecipe);
						//MarsarahTweaks.LogInfo($"Chain recipe added: {chainRecipe.name}");
					}
					/*else
					{
						MarsarahTweaks.LogInfo("Failed to create chain recipe. Missing required items.");
					}*/
				}
			}
			else if (wasChanged)
			{
				// Remove the chain recipe if it exists and config is disabled
				Recipe chainRecipe = objDB.m_recipes.Find(r => r.name == "Recipe_Chain");
				if (chainRecipe != null)
				{
					objDB.m_recipes.Remove(chainRecipe);
					//MarsarahTweaks.LogInfo($"Chain recipe removed: {chainRecipe.name}");
				}
			}
		}

		private static Recipe CreateChainRecipe(ObjectDB objDB)
		{
			if (objDB == null || ZNetScene.instance == null) return null;

			ItemDrop chainItem = objDB.GetItemPrefab("Chain")?.GetComponent<ItemDrop>();
			ItemDrop ironItem = objDB.GetItemPrefab("Iron")?.GetComponent<ItemDrop>();
			GameObject blackForgePrefab = ZNetScene.instance.GetPrefab("blackforge"); ;
			CraftingStation blackForge = null;

			if (blackForgePrefab != null)
			{
				blackForge = blackForgePrefab.GetComponent<CraftingStation>();
			}

			if (chainItem == null || ironItem == null || blackForge == null)
			{
				MarsarahTweaks.LogWarn("Failed to create chain recipe: missing required item or crafting station.");
				return null;
			}

			Recipe chainRecipe = ScriptableObject.CreateInstance<Recipe>();
			chainRecipe.name = "Recipe_Chain";
			chainRecipe.m_item = chainItem;
			chainRecipe.m_amount = 2;
			chainRecipe.m_minStationLevel = 1;
			chainRecipe.m_enabled = true;
			chainRecipe.m_craftingStation = blackForge;
			chainRecipe.m_repairStation = blackForge;
			chainRecipe.m_resources = new Piece.Requirement[]
			{
				new Piece.Requirement
				{
					m_resItem = ironItem,
					m_amount = 1,
					m_amountPerLevel = 1,
					m_recover = true
				}
			};

			return chainRecipe;
		}
	}
}
