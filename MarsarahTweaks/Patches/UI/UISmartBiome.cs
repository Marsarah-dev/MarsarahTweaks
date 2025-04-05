using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace MarsarahTweaks.Patches.UI
{
	internal class UISmartBiome : UIController
	{
		// UI data
		private static float playerArmor;
		private static int playerArmorWeight;
		private static string currentBiome = null;

		// UI elements
		private static Text UIBiomeText;

		// Dictionaries and other data
		private struct BiomeWeights
		{
			public BiomeWeights(int min, int max)
			{
				_min = min;
				_max = max;
			}

			public int _min;
			public int _max;
		}

		private static readonly Dictionary<string, BiomeWeights> defaultBiomeWeights = new Dictionary<string, BiomeWeights>()
		{
			{"Meadows", new BiomeWeights(1, 11)}, // 19 from armor but no upgrades available
			{"BlackForest", new BiomeWeights(11, 24)}, // 28 from armor but no upgrades available
			{"Swamp", new BiomeWeights(24, 37)},
			{"Mountain", new BiomeWeights(33, 46)},
			{"Plains", new BiomeWeights(42, 55)},
			{"Mistlands", new BiomeWeights(51, 60)}, // 64 from armor but no upgrades available
			{"AshLands", new BiomeWeights(60, 69)} // 73 from armor but no upgrades available
		};

		private static readonly Dictionary<string, BiomeWeights> unlockedBiomeWeights = new Dictionary<string, BiomeWeights>()
		{
			{"Meadows", new BiomeWeights(1, 15)},
			{"BlackForest", new BiomeWeights(11, 28)},
			{"Swamp", new BiomeWeights(24, 37)},
			{"Mountain", new BiomeWeights(33, 46)},
			{"Plains", new BiomeWeights(42, 55)},
			{"Mistlands", new BiomeWeights(51, 64)},
			{"AshLands", new BiomeWeights(60, 73)}
		};

		private static Dictionary<string, BiomeWeights> biomeWeightsDict = defaultBiomeWeights;

		public static void UpdateBiomeWeights()
		{
			biomeWeightsDict = ConfigManager.GearUpgradeUnlockEnabled.Value ? unlockedBiomeWeights : defaultBiomeWeights;
		}

		private static Dictionary<string, int> armorWeightsDict = new Dictionary<string, int>()
		{
			// Capes
			{ "$item_cape_deerhide", 1 },
			{ "$item_cape_trollhide", 1 },
			{ "$item_cape_linen", 1 },
			{ "$item_cape_wolf", 1 },
			{ "$item_cape_lox", 1 },
			{ "$item_cape_feather", 1 },
			{ "$item_cape_ash", 1 },
			{ "$item_cape_asksvin", 1 },

			// Meadows
			{ "$item_chest_rags", 1 },
			{ "$item_legs_rags", 1 },
			{ "$item_helmet_leather", 2 },
			{ "$item_chest_leather", 2 },
			{ "$item_legs_leather", 2 },

			// Black Forest
			{ "$item_helmet_trollleather", 5 },
			{ "$item_chest_trollleather", 5 },
			{ "$item_legs_trollleather", 5 },
			{ "$item_helmet_bronze", 5 },
			{ "$item_chest_bronze", 5 },
			{ "$item_legs_bronze", 5 },

			// Swamp
			{ "$item_helmet_root", 8 },
			{ "$item_chest_root", 8 },
			{ "$item_legs_root", 8 },
			{ "$item_helmet_iron", 8 },
			{ "$item_chest_iron", 8 },
			{ "$item_legs_iron", 8 },

			// Mountain
			{ "$item_helmet_fenris", 11 },
			{ "$item_chest_fenris", 11 },
			{ "$item_legs_fenris", 11 },
			{ "$item_helmet_drake", 11 },
			{ "$item_chest_wolf", 11 },
			{ "$item_legs_wolf", 11 },

			// Plains
			{ "$item_helmet_padded", 14 },
			{ "$item_chest_pcuirass", 14 },
			{ "$item_legs_pgreaves", 14 },

			// Mistlands
			{ "$item_helmet_mage", 17 },
			{ "$item_chest_mage", 17 },
			{ "$item_legs_mage", 17 },
			{ "$item_helmet_carapace", 17 },
			{ "$item_chest_carapace", 17 },
			{ "$item_legs_carapace", 17 },

			// Ashlands
			{ "$item_helmet_mage_ashlands", 20 },
			{ "$item_chest_mage_ashlands", 20 },
			{ "$item_legs_mage_ashlands", 20 },
			{ "$item_helmet_medium_ashlands", 20 },
			{ "$item_chest_medium_ashlands", 20 },
			{ "$item_legs_medium_ashlands", 20 },
			{ "$item_helmet_flametal", 20 },
			{ "$item_chest_flametal", 20 },
			{ "$item_legs_flametal", 20 },
		};
		private static Dictionary<string, int> equippedArmorWeightsDict = new Dictionary<string, int>();

		[HarmonyPatch(typeof(Player), "Update")]
		class SmartBiome_PlayerPatch
		{
			private static void Prefix(ref Player ___m_localPlayer)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (___m_localPlayer == null) return;

				if (ConfigManager.ShowSmartBiome.Value && showUI)
				{
					playerArmor = ___m_localPlayer.GetBodyArmor();
					Inventory playerInventory = ___m_localPlayer.GetInventory();
					List<ItemDrop.ItemData> playerEquippedItems = playerInventory.GetEquippedItems();

					foreach (ItemDrop.ItemData equippedItem in playerEquippedItems)
					{
						if (armorWeightsDict.ContainsKey(equippedItem.m_shared.m_name))
						{
							if (!equippedArmorWeightsDict.ContainsKey(equippedItem.m_shared.m_name))
							{
								equippedArmorWeightsDict.Add(equippedItem.m_shared.m_name, armorWeightsDict[equippedItem.m_shared.m_name] + equippedItem.m_quality - 1);
							}
							else
							{
								equippedArmorWeightsDict[equippedItem.m_shared.m_name] = armorWeightsDict[equippedItem.m_shared.m_name] + equippedItem.m_quality - 1;
							}
						}
					}

					// clean non equipped items
					List<string> itemsToDelete = new List<string>();
					foreach (KeyValuePair<string, int> potentialEquippedItem in equippedArmorWeightsDict)
					{
						bool itemFound = false;
						foreach (ItemDrop.ItemData equippedIem in playerEquippedItems)
						{
							if (equippedIem.m_shared.m_name == potentialEquippedItem.Key)
							{
								itemFound = true;
								break;
							}
						}

						if (!itemFound)
						{
							itemsToDelete.Add(potentialEquippedItem.Key);
						}
					}

					foreach (string itemToDelete in itemsToDelete)
					{
						equippedArmorWeightsDict.Remove(itemToDelete);
					}
				}
			}
		}

		[HarmonyPatch(typeof(Minimap), "UpdateBiome")]
		class MoveBiomeMinimapText_Patch
		{
			private static void Prefix(ref Text ___m_biomeNameSmall, ref Player player)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (ConfigManager.ShowSmartBiome.Value && showUI)
				{
					___m_biomeNameSmall.enabled = false;

					currentBiome = player.GetCurrentBiome().ToString();
				}
				else
					___m_biomeNameSmall.enabled = true;
			}
		}

		[HarmonyPatch(typeof(Hud), "Update")]
		class SmartBiome_HUDUpdatePatch
		{
			private static void Postfix(Hud __instance)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return;

				if (__instance == null) return;

				if (ConfigManager.ShowSmartBiome.Value)
				{
					CreateUI(__instance); // Create UI if missing

					UIBiomeText.enabled = showUI && Minimap.instance.m_mapSmall.activeInHierarchy;

					if (showUI && Minimap.instance.m_mapSmall.activeInHierarchy)
					{

						if (currentBiome != null && biomeWeightsDict.ContainsKey(currentBiome))
						{
							// calculate total player armor weight
							playerArmorWeight = 0;
							foreach (KeyValuePair<string, int> equippedArmorWeight in equippedArmorWeightsDict)
							{
								playerArmorWeight += equippedArmorWeight.Value;
							}

							float min = biomeWeightsDict[currentBiome]._min;
							float max = biomeWeightsDict[currentBiome]._max;
							float range = max - min;
							float startValue = playerArmorWeight - min;
							float playerArmorWeightPercent = (startValue * 100) / range;

							UIBiomeText.color = GetColorFromPercent(playerArmorWeightPercent);
						}
						else
						{
							UIBiomeText.color = Color.white;
						}
						UIBiomeText.text = (currentBiome == "BlackForest") ? "Black forest" : ((currentBiome == "AshLands") ? "Ashlands" : currentBiome);
					}
				}
				else
				{
					if (UIBiomeText != null)
						UIBiomeText.enabled = false;
				}
			}

			private static void CreateUI(Hud hud)
			{
				if (UIBiomeText != null)
					return;  // UI already exists, no need to create again

				int UITextFontSize = 16;
				string UITextFontName = "AveriaSansLibre-Bold";
				Vector2 UIBiomeAreaSize = new Vector2(150f, 30f); // width, height

				// Biome area object
				GameObject UIBiomeArea = new GameObject("BiomeArea");
				UIBiomeArea.layer = 5;
				UIBiomeArea.transform.SetParent(hud.m_rootObject.transform);
				RectTransform biomeAreaTransform = UIBiomeArea.AddComponent<RectTransform>();
				biomeAreaTransform.anchorMin = new Vector2(1f, 1f);
				biomeAreaTransform.anchorMax = new Vector2(1f, 1f);
				biomeAreaTransform.anchoredPosition = new Vector2(-125f, -55f);
				biomeAreaTransform.sizeDelta = UIBiomeAreaSize;
				UIBiomeArea.transform.localScale = Vector3.one;  // Ensure correct scale

				// Background texture
				// skip...

				UIBiomeText = CreateTextObject("BiomeText", UIBiomeArea, Color.white, UITextFontName, UITextFontSize, TextAnchor.MiddleRight, new Vector2(0f, 0f), UIBiomeAreaSize);
			}

			private static Color GetColorFromPercent(float percent)
			{
				if (percent < 0f) return new Color(0.298039f, 0f, 0.6f); // purple
				if (percent < 25f) return Color.red;
				if (percent < 75f) return new Color(1f, 0.549019f, 0f); // orange
				if (percent < 100f) return Color.yellow;
				return Color.green;
			}
		}
	}
}
