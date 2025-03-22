//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace MarsarahTweaks.Patches
//{
//	internal class WeaponStaminaChanges
//	{
//		[HarmonyPatch(typeof(ObjectDB), "Awake")]
//		class WeaponStamina_Patch
//		{
//			static void Postfix(ref ObjectDB __instance)
//			{
//				if (__instance == null) return;

//				if (ZNet.instance != null)
//				{
//					if (!ZNet.instance.IsDedicated())
//					{
//						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.WeaponStaminaModifications.Name}...");
//						UpdateWeaponStaminaCosts(__instance, false);
//					}
//					else
//					{
//						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.Configs.WeaponStaminaModifications.Name}...");
//					}
//				}
//				else
//				{
//					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.Configs.WeaponStaminaModifications.Name}...");
//				}
//			}
//		}

//		public static readonly Dictionary<string, int> weaponStaminaUsage = new Dictionary<string, int>
//		{
//			{ "$item_atgeir_blackmetal", 18 },
//			{ "$item_atgeir_bronze", 12 },
//			{ "$item_atgeir_himminafl", 20 },
//			{ "$item_atgeir_iron", 14 },
//			{ "$item_axe_berzerkr", 16 },
//			{ "$item_axe_berzerkr_blood", 16 },
//			{ "$item_axe_berzerkr_lightning", 16 },
//			{ "$item_axe_berzerkr_nature", 16 },
//			{ "$item_axe_blackmetal", 14 },
//			{ "$item_axe_bronze", 8 },
//			{ "$item_axe_flint", 6 },
//			{ "$item_axe_iron", 10 },
//			{ "$item_axe_jotunbane", 16 },
//			{ "$item_axe_stone", 6 },
//			{ "$item_battleaxe", 16 },
//			{ "$item_battleaxe_crystal", 18 },
//			{ "$item_bilebomb", 8 },
//			{ "$item_lavabomb", 8 },
//			{ "$item_oozebomb", 8 },
//			{ "$item_smokebomb", 8 },
//			{ "$item_club", 6 },
//			{ "$item_crossbow_arbalest", 0 },
//			{ "$item_fishingrod", 0 },
//			{ "$item_fistweapon_fenris", 10 },
//			{ "$item_knife_blackmetal", 12 },
//			{ "$item_knife_butcher", 5 },
//			{ "$item_knife_chitin", 8 },
//			{ "$item_knife_copper", 6 },
//			{ "$item_knife_flint", 4 },
//			{ "$item_knife_silver", 10 },
//			{ "$item_knife_skollandhati", 14 },
//			{ "$item_mace_bronze", 8 },
//			{ "$item_mace_eldner", 16 },
//			{ "$item_mace_eldner_blood", 16 },
//			{ "$item_mace_eldner_lightning", 16 },
//			{ "$item_mace_eldner_nature", 16 },
//			{ "$item_mace_iron", 10 },
//			{ "$item_mace_needle", 14 },
//			{ "$item_mace_silver", 12 },
//			{ "$item_pickaxe_antler", 6 },
//			{ "$item_pickaxe_blackmetal", 14 },
//			{ "$item_pickaxe_bronze", 8 },
//			{ "$item_pickaxe_iron", 10 },
//			{ "$item_pickaxe_stone", 4 },
//			{ "$item_scythe", 5 },
//			{ "$item_sledge_demolisher", 28 },
//			{ "$item_sledge_iron", 20 },
//			{ "$item_stagbreaker", 12 },
//			{ "$item_spear_bronze", 8 },
//			{ "$item_spear_carapace", 16 },
//			{ "$item_spear_chitin", 15 },
//			{ "$item_spear_ancientbark", 10 },
//			{ "$item_spear_flint", 6 },
//			{ "$item_spear_splitner", 16 },
//			{ "$item_spear_splitner_blood", 16 },
//			{ "$item_spear_splitner_lightning", 16 },
//			{ "$item_spear_splitner_nature", 16 },
//			{ "$item_spear_wolffang", 12 },
//			{ "$item_staffclusterbomb", 0 },
//			{ "$item_stafffireball", 0 },
//			{ "$item_staffgreenroots", 0 },
//			{ "$item_stafficeshards", 0 },
//			{ "$item_staff_lightning", 0 },
//			{ "$item_staffredtroll", 0 },
//			{ "$item_staffshield", 0 },
//			{ "$item_staffskeleton", 0 },
//			{ "$item_sword_blackmetal", 14 },
//			{ "$item_sword_bronze", 8 },
//			{ "$item_sword_dyrnwyn", 16 },
//			{ "$item_sword_iron", 10 },
//			{ "$item_sword_fire", 14 },
//			{ "$item_sword_mistwalker", 16 },
//			{ "$item_sword_niedhogg", 16 },
//			{ "$item_sword_niedhogg_blood", 16 },
//			{ "$item_sword_niedhogg_lightning", 16 },
//			{ "$item_sword_niedhogg_nature", 16 },
//			{ "$item_sword_silver", 12 },
//			{ "$item_tankard", 0 },
//			{ "$item_dvergrtankard", 0 },
//			{ "$item_tankard_anniversary", 0 },
//			{ "$item_tankard_odin", 0 },
//			{ "$item_sword_krom", 20 },
//			{ "$item_sword_slayer", 20 },
//			{ "$item_sword_slayer_blood", 20 },
//			{ "$item_sword_slayer_lightning", 20 },
//			{ "$item_sword_slayer_nature", 20 }
//		};

//		public static void UpdateWeaponStaminaCosts(ObjectDB objDB, bool wasChanged)
//		{
//			foreach (GameObject obj in objDB.m_items)
//			{
//				ItemDrop item = obj.GetComponent<ItemDrop>();
//				if (item == null) continue;

//				ItemDrop.ItemData.SharedData shared = item.m_itemData.m_shared;

//				if (shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon || shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon || shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft)
//				{
//					float staminaUsage = shared.m_attack.m_attackStamina;
//					MarsarahTweaks.MLog($"Weapon: {shared.m_name}, Stamina Usage: {staminaUsage}");
//				}
//			}
//		}
//	}
//}
