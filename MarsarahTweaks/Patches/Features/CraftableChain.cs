using HarmonyLib;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal class CraftableChain
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class CraftableChain_Patch
		{
			static void Postfix(ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.CraftableChain.Name}...");
						UpdateChainRecipe(__instance, false);
					}
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.Configs.CraftableChain.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.Configs.CraftableChain.Name}...");
				}
			}
		}

		public static void UpdateChainRecipe(ObjectDB objDB, bool wasChanged)
		{
			if (ConfigManager.craftableChainEnabled.Value)
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
						//MarsarahTweaks.MLog($"Chain recipe added: {chainRecipe.name}");
					}
					/*else
					{
						MarsarahTweaks.MLog("Failed to create chain recipe. Missing required items.");
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
					//MarsarahTweaks.MLog($"Chain recipe removed: {chainRecipe.name}");
				}
			}
		}


		private static Recipe CreateChainRecipe(ObjectDB objDB)
		{
			Recipe dvergrLantern = objDB.m_recipes.Find(r => r.name == "Recipe_Lantern");
			if (dvergrLantern == null)
			{
				//MarsarahTweaks.MLog($"Missing required recipe: {dvergrLantern}");
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

			//MarsarahTweaks.MLog($"Successfully created chain recipe: {chainRecipe.name}");
			return chainRecipe;
		}
	}
}
