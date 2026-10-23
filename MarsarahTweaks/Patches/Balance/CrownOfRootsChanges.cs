
using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Trader;

namespace MarsarahTweaks.Patches.Balance
{
	internal class CrownOfRootsChanges
	{
		private static readonly LogManager log = new LogManager("Crown of Roots Changes", LogManager.LogLevel.Info);

		private const string CrownPrefabName = "HelmetRootCrown";
		private const string WorkbenchPrefabName = "piece_workbench";
		private const int NewPrice = 2000;
		private const int RepairStationLevel = 3;

		private static readonly Dictionary<TradeItem, int> originalPrices = new Dictionary<TradeItem, int>();
		private static readonly System.Reflection.FieldInfo traderItemsField = AccessTools.Field(typeof(Trader), "m_items");

		private static bool repairLogged;

		[HarmonyPatch(typeof(Trader), "GetAvailableItems")]
		private static class Trader_GetAvailableItems_Patch
		{
			private static void Prefix(Trader __instance)
			{
				UpdateTraderPrice(__instance);
			}
		}

		[HarmonyPatch(typeof(InventoryGui), "CanRepair")]
		private static class InventoryGui_CanRepair_Patch
		{
			private static void Postfix(ItemDrop.ItemData item, ref bool __result)
			{
				if (!ConfigManager.CrownOfRootsEnabled.Value) return;
				if (__result || item == null || item.m_shared == null) return;
				if (!item.m_shared.m_useDurability || !item.m_shared.m_canBeReparied) return;

				GameObject crownPrefab = ObjectDB.instance?.GetItemPrefab(CrownPrefabName);
				if (crownPrefab == null) return;

				ItemDrop crownDrop = crownPrefab.GetComponent<ItemDrop>();
				if (crownDrop == null) return;

				if (item.m_shared.m_name != crownDrop.m_itemData.m_shared.m_name) return;

				Player player = Player.m_localPlayer;
				if (player == null) return;

				CraftingStation station = player.GetCurrentCraftingStation();
				if (station == null) return;

				if (!station.gameObject.name.StartsWith(WorkbenchPrefabName, StringComparison.Ordinal)) return;
				if (station.GetLevel() < RepairStationLevel) return;

				__result = true;

				if (!repairLogged)
				{
					log.Info($"Crown of Roots repair enabled at level {station.GetLevel()} Workbench.");
					repairLogged = true;
				}
			}
		}

		public static void UpdateTraderPrices()
		{
			foreach (Trader trader in UnityEngine.Object.FindObjectsByType<Trader>(FindObjectsSortMode.None))
			{
				UpdateTraderPrice(trader);
			}
		}

		private static void UpdateTraderPrice(Trader trader)
		{
			if (trader == null) return;

			if (traderItemsField == null)
			{
				log.Warn("Could not find Trader.m_items.");
				return;
			}

			IList items = traderItemsField.GetValue(trader) as IList;
			if (items == null) return;

			foreach (object entry in items)
			{
				if (!(entry is TradeItem tradeItem)) continue;
				if (tradeItem.m_prefab == null) continue;
				if (tradeItem.m_prefab.name != CrownPrefabName) continue;

				if (ConfigManager.CrownOfRootsEnabled.Value)
				{
					if (!originalPrices.ContainsKey(tradeItem))
					{
						originalPrices[tradeItem] = tradeItem.m_price;
						log.Info($"Backed up Crown of Roots purchase price: {tradeItem.m_price}");
					}

					if (tradeItem.m_price != NewPrice)
					{
						tradeItem.m_price = NewPrice;
						log.Info($"Changed Crown of Roots purchase price to {NewPrice} coins.");
					}
				}
				else if (originalPrices.TryGetValue(tradeItem, out int originalPrice))
				{
					tradeItem.m_price = originalPrice;
					originalPrices.Remove(tradeItem);

					log.Info($"Restored Crown of Roots purchase price to {originalPrice} coins.");
				}
			}
		}
	}
}
