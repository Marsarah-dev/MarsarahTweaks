using HarmonyLib;
using UnityEngine;

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
			Recipe dvergrLantern = objDB.m_recipes.Find(r => r.name == "Recipe_Lantern");
			if (dvergrLantern == null)
			{
				//MarsarahTweaks.LogInfo($"Missing required recipe: {dvergrLantern}");
				return null;
			}

			// Create the new chain recipe
			Recipe chainRecipe = ScriptableObject.CreateInstance<Recipe>();
			chainRecipe.m_item = objDB.GetItemPrefab("Chain").GetComponent<ItemDrop>();
			chainRecipe.m_amount = 2;
			chainRecipe.m_minStationLevel = 1;
			chainRecipe.m_resources = new Piece.Requirement[1];

			chainRecipe.m_resources[0] = new Piece.Requirement
			{
				m_amount = 1,
				m_amountPerLevel = 1,
				m_recover = true,
				m_resItem = objDB.GetItemPrefab("Iron").GetComponent<ItemDrop>()
			};

			// Set the crafting and repair stations from the lantern recipe
			chainRecipe.hideFlags = dvergrLantern.hideFlags;
			chainRecipe.m_craftingStation = dvergrLantern.m_craftingStation;
			chainRecipe.m_repairStation = dvergrLantern.m_repairStation;
			chainRecipe.name = "Recipe_Chain";
			chainRecipe.m_enabled = true;

			//MarsarahTweaks.LogInfo($"Successfully created chain recipe: {chainRecipe.name}");
			return chainRecipe;
		}
	}
}
