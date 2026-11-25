
using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace MarsarahTweaks.Patches.Balance
{
	internal class PotionUpgradeChanges
	{
		private static readonly LogManager log = new LogManager("Potion Upgrade", LogManager.LogLevel.Warning);

		private const string CauldronPrefab = "piece_cauldron";
		private const int PotionConversionRatio = 3;

		// Recipe name -> Input potion, Output potion, Required Cauldron level
		private static readonly Dictionary<string, (string input, string output, int stationLevel)> potionUpgradeRecipes = new Dictionary<string, (string input, string output, int stationLevel)>()
		{
			{ "Recipe_PotionUpgrade_HealthMinorToMedium", ("MeadHealthMinor", "MeadHealthMedium", 3) },
			{ "Recipe_PotionUpgrade_StaminaMinorToMedium", ("MeadStaminaMinor", "MeadStaminaMedium", 4) },
			{ "Recipe_PotionUpgrade_HealthMediumToMajor", ("MeadHealthMedium", "MeadHealthMajor", 5) }
		};

		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class PotionUpgradeChanges_Patch
		{
			private static void Postfix(ObjectDB __instance)
			{
				if (__instance == null || ZNet.instance == null) return;

				UpdatePotionUpgrade(__instance, false);
			}
		}

		public static void UpdatePotionUpgrade(ObjectDB objDB, bool wasChanged)
		{
			if (objDB == null || objDB.m_items == null || objDB.m_items.Count == 0 || objDB.m_recipes == null) return;
			if (ZNetScene.instance == null) return;

			bool enabled = ConfigManager.PotionUpgradeEnabled.Value;
			int recipesChanged = 0;

			foreach (var entry in potionUpgradeRecipes)
			{
				string recipeName = entry.Key;
				var recipeData = entry.Value;

				Recipe existingRecipe = objDB.m_recipes.Find(r => r != null && r.name == recipeName);

				if (enabled)
				{
					// Prevent duplicate recipes
					if (existingRecipe != null) continue;

					Recipe newRecipe = CreatePotionUpgradeRecipe(objDB, recipeName, recipeData.input, recipeData.output, recipeData.stationLevel);

					if (newRecipe == null) continue;

					objDB.m_recipes.Add(newRecipe);
					recipesChanged++;

					log.Info($"Added {recipeName}: 3 {recipeData.input} -> 1 {recipeData.output} | Cauldron level {recipeData.stationLevel}");
				}
				else
				{
					if (existingRecipe == null) continue;

					objDB.m_recipes.Remove(existingRecipe);
					recipesChanged++;

					log.Info($"Removed potion upgrade recipe: {recipeName}");
				}
			}

			// Refresh known recipes after live config changes
			if (wasChanged && recipesChanged > 0 && Player.m_localPlayer != null)
			{
				var refreshMethod = AccessTools.Method(typeof(Player), "UpdateKnownRecipesList");

				if (refreshMethod != null)
				{
					refreshMethod.Invoke(Player.m_localPlayer, null);
					log.Info("Refreshed player known recipes.");
				}
				else
				{
					log.Warn("Could not find Player.UpdateKnownRecipesList.");
				}
			}

			log.Info($"Potion Upgrade {(enabled ? "enabled" : "disabled")}: {recipesChanged} recipes {(enabled ? "added" : "removed")}.");
		}

		private static Recipe CreatePotionUpgradeRecipe(ObjectDB objDB, string recipeName, string inputPrefab, string outputPrefab, int stationLevel)
		{
			ItemDrop inputPotion = objDB.GetItemPrefab(inputPrefab)?.GetComponent<ItemDrop>();
			ItemDrop outputPotion = objDB.GetItemPrefab(outputPrefab)?.GetComponent<ItemDrop>();

			GameObject cauldronObject = ZNetScene.instance.GetPrefab(CauldronPrefab);
			CraftingStation cauldron = cauldronObject?.GetComponent<CraftingStation>();

			if (inputPotion == null || outputPotion == null || cauldron == null)
			{
				log.Warn($"Could not create {recipeName}: Missing input potion, output potion or Cauldron.");
				return null;
			}

			Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
			recipe.name = recipeName;
			recipe.m_item = outputPotion;
			recipe.m_amount = 1;
			recipe.m_enabled = true;
			recipe.m_craftingStation = cauldron;
			recipe.m_minStationLevel = stationLevel;

			recipe.m_resources = new Piece.Requirement[]
			{
				new Piece.Requirement
				{
					m_resItem = inputPotion,
					m_amount = PotionConversionRatio,
					m_amountPerLevel = 0,
					m_recover = false
				}
			};

			return recipe;
		}
	}
}
