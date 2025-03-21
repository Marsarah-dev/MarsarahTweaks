using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches
{
	internal class EarlyLinenCape
	{
		[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class EarlyLinenCape_Patch
		{
			static void Postfix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();

					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.LinenCapeModifications.Name}...");
						UpdateLinenCape(__instance, false);
					}
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.Configs.LinenCapeModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.Configs.LinenCapeModifications.Name}...");
				}
			}

			public static void UpdateLinenCape(ObjectDB objDB, bool wasChanged)
			{
				if (ConfigManager.earlyLinenCapeEnabled.Value) 
				{
					
				}
			}

			// Apply Recipe Changes =============================================
			private static void ApplyRecipeChanges(ObjectDB objDB, bool wasChanged)
			{
				foreach (Recipe recipe in objDB.m_recipes)
				{
					switch (recipe.name)
					{
						case "Recipe_CapeLinen":
							foreach (Piece.Requirement req in recipe.m_resources)
							{
								switch (req.m_resItem.name)
								{
									case "LinenThread":
										req.m_amount = 5;
										req.m_amountPerLevel = 2;
										req.m_resItem = objDB.GetItemPrefab("DeerHide").GetComponent<ItemDrop>();
										break;
									case "Silver":
									case "BlackMetal":
										req.m_amount = 1;
										req.m_amountPerLevel = 0;
										req.m_resItem = objDB.GetItemPrefab("Iron").GetComponent<ItemDrop>();
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

			// Apply Poison Resist ==============================================
			private static void ApplyPoisonResist(ObjectDB objDB, bool wasChanged)
			{
				ItemDrop.ItemData.ItemType[] itemTypes = (ItemDrop.ItemData.ItemType[])Enum.GetValues(typeof(ItemDrop.ItemData.ItemType));
				ItemDrop.ItemData.ItemType shoulderType = new ItemDrop.ItemData.ItemType();

				foreach (ItemDrop.ItemData.ItemType itemType in itemTypes)
				{
					if (itemType.ToString() == "Shoulder")
					{
						shoulderType = itemType;
						break;
					}
				}

				List<ItemDrop> backItems = objDB.GetAllItems(shoulderType, "");

				foreach (ItemDrop item in backItems)
				{
					switch (item.name)
					{
						case "CapeLinen":
						case "CapeLox":
							HitData.DamageModPair damageModPairPoison = new HitData.DamageModPair();
							damageModPairPoison.m_modifier = HitData.DamageModifier.Resistant;
							damageModPairPoison.m_type = HitData.DamageType.Poison;
							if (!item.m_itemData.m_shared.m_damageModifiers.Contains(damageModPairPoison))
							{
								item.m_itemData.m_shared.m_damageModifiers.Add(damageModPairPoison);
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
