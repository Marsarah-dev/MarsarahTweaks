using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class OtherChanges
	{
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
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.OtherModifications.Name}...");

						UpdateOthers(__instance, false);
					}
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.Configs.OtherModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.Configs.OtherModifications.Name}...");
				}
			}
		}

		// Dictionaries
		private static Dictionary<string, (int recipeAmount, Dictionary<string, int> resourceAmounts)> recipeBackups = new Dictionary<string, (int, Dictionary<string, int>)>();
		private static Dictionary<string, Dictionary<string, int>> otherRecipeModifiers = new Dictionary<string, Dictionary<string, int>>
			{
				{ "Recipe_Tankard", new Dictionary<string, int>
					{
						{ "FineWood", 2 }
					}
				},
				{ "Recipe_IronNails", new Dictionary<string, int>
					{
						{ "Amount", 20 } // Special key to modify `recipe.m_amount`
					}
				}
			};

		public static void UpdateOthers(ObjectDB objDB, bool wasChanged)
		{
			if (ConfigManager.otherEnabled.Value)
			{
				foreach (var entry in otherRecipeModifiers)
				{
					string recipeName = entry.Key;
					Recipe recipe = objDB.m_recipes.Find(r => r.name == recipeName);
					if (recipe == null) continue;

					// Backup original values only for modified fields
					if (!recipeBackups.ContainsKey(recipeName))
					{
						var resourceBackup = new Dictionary<string, int>();

						foreach (var mod in entry.Value) // Only check the modifications we're applying
						{
							if (mod.Key == "Amount")
							{
								// Backup recipe.m_amount
								recipeBackups[recipeName] = (recipe.m_amount, resourceBackup);
							}
							else
							{
								foreach (var req in recipe.m_resources)
								{
									if (req.m_resItem.name == mod.Key)
									{
										// Backup only modified resource amounts
										resourceBackup[req.m_resItem.name] = req.m_amount;
									}
								}
							}
						}

						// Store the backup only if any resources were actually modified
						if (resourceBackup.Count > 0 || entry.Value.ContainsKey("Amount"))
						{
							recipeBackups[recipeName] = (recipe.m_amount, resourceBackup);
						}
					}

					// Apply modifications
					foreach (var mod in entry.Value)
					{
						if (mod.Key == "Amount")
						{
							recipe.m_amount = mod.Value;
						}
						else
						{
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								if (req.m_resItem.name == mod.Key)
								{
									req.m_amount = mod.Value;
								}
							}
						}
					}
				}
			}
			else if (wasChanged)
			{
				// Restore original values
				foreach (var entry in recipeBackups)
				{
					Recipe recipe = objDB.m_recipes.Find(r => r.name == entry.Key);
					if (recipe == null) continue;

					// Restore recipe amount only if it was modified
					if (otherRecipeModifiers.TryGetValue(recipe.name, out var modifications) && modifications.ContainsKey("Amount"))
					{
						recipe.m_amount = entry.Value.recipeAmount;
					}

					// Restore only modified resource amounts
					foreach (var mod in modifications)
					{
						if (mod.Key == "Amount") continue; // Skip, already restored above

						foreach (var req in recipe.m_resources)
						{
							if (entry.Value.resourceAmounts.TryGetValue(req.m_resItem.name, out int originalAmount))
							{
								req.m_amount = originalAmount;
							}
						}
					}
				}

				// Clear backup after restoring
				recipeBackups.Clear();
			}
		}
	}
}
