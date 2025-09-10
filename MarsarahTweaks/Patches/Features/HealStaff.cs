//using HarmonyLib;
//using MarsarahTweaks.Managers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace MarsarahTweaks.Patches.Features
//{
//	internal class HealStaff
//	{
//		private static readonly LogManager log = new LogManager("Heal Staff", LogManager.LogLevel.Info);

//		private static bool initialized = false;
//		private static GameObject HealStaffPrefab;

//		[HarmonyPatch(typeof(ZNetScene), "Awake")]
//		public static class ZNetScene_Awake_Patch
//		{
//			static void Postfix(ZNetScene __instance)
//			{
//				if (__instance == null || initialized)
//					return;

//				initialized = true;

//				CreateHealStaff();
//			}
//		}

//		private static void CreateHealStaff()
//		{
//			if (MPrefabManager.GetPrefab("DvergerStaffHealPlayer") != null ) return;

//			// Clone dverger staff
//			HealStaffPrefab = CloneDvergerStaffPrefab("DvergerStaffHeal", "DvergerStaffHealPlayer");
//			if (HealStaffPrefab == null) return;

//			// Add Item Drop and configure other needed data
//			AddItemDrop();

//			// Add new staff to ZNetScene
//			MPrefabManager.RegisterToZNetScene(HealStaffPrefab);

//			// Configue Item Drop stuff?

//			// Toggle visibility

//			HealStaffPrefab.SetActive(true);
//			log.Info("New heal staff registered and ready.");
//		}

//		private static GameObject CloneDvergerStaffPrefab(string sourcePrefabName, string newPrefabName)
//		{
//			GameObject prefab = MPrefabManager.ClonePrefab(sourcePrefabName, newPrefabName);
//			if (prefab == null)
//			{
//				log.Error($"Cloning of {sourcePrefabName} failed.");
//			}

//			return prefab;
//		}

//		private static void AddItemDrop()
//		{
//			var itemDrop = HealStaffPrefab.GetComponent<ItemDrop>();
//			if (itemDrop == null)
//			{
//				log.Info("Adding component: ItemDrop");
//				itemDrop = HealStaffPrefab.AddComponent<ItemDrop>();
//			}

//			if (itemDrop.m_itemData == null)
//			{
//				log.Info("Adding: ItemDrop.m_itemData");
//				itemDrop.m_itemData = new ItemDrop.ItemData();
//			}
//			if (itemDrop.m_itemData.m_shared == null)
//			{
//				log.Info("Adding: ItemDrop.m_itemData.m_shared");
//				itemDrop.m_itemData.m_shared = new ItemDrop.ItemData.SharedData();
//			}

//			var shared = itemDrop.m_itemData.m_shared;
//			shared.m_name = "Staff of Healing";
//			shared.m_description = "Staff used by Dverger Mages to heal their allies";
//			shared.m_weight = 2f;
//			shared.m_itemType = ItemDrop.ItemData.ItemType.TwoHandedWeapon;
//			shared.m_maxStackSize = 1;
//			shared.m_maxQuality = 1;
//			shared.m_variants = 1;
//			shared.m_useDurability = true;
//			shared.m_durabilityDrain = 0.1f;
//			shared.m_destroyBroken = false;
//			shared.m_skillType = Skills.SkillType.BloodMagic;

//			if (!HealStaffPrefab.GetComponent<ZNetView>())
//			{
//				log.Info("Adding component: ZNetView");
//				HealStaffPrefab.AddComponent<ZNetView>();
//			}
//		}
//	}
//}
