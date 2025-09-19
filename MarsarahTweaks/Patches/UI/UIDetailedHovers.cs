using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIDetailedHovers
	{
		private static readonly LogManager log = new LogManager("UI Detailed Hover Info", LogManager.LogLevel.Info);

		// Cache the FieldInfo for performance
		private static readonly FieldInfo InventoryField = typeof(Container).GetField("m_inventory", BindingFlags.NonPublic | BindingFlags.Instance);

		[HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
		internal static class DetailedHoverContainer_Patch
		{
			private static void Postfix(Container __instance, ref string __result)
			{
				if (!ConfigManager.DetailedHoverInfo.Value)	return;

				Inventory inventory = InventoryField?.GetValue(__instance) as Inventory;
				if (inventory == null)
				{
					log.Warn("Could not access container inventory via reflection.");
					return;
				}

				// Skip empty containers to keep it clean
				if (inventory.NrOfItems() == 0)	return;

				__result = GetContainerHover(__instance, inventory);
			}
		}

		public static string GetContainerHover(Container container, Inventory inventory)
		{
			int used = inventory.NrOfItems();
			int max = inventory.GetWidth() * inventory.GetHeight();

			string ratioText = GetColoredRatio(used, max);
			string itemsLine = GetInventorySummary(inventory);

			string localizedName = Localization.instance.Localize(container.m_name);
			string localizedUse = Localization.instance.Localize("$KEY_Use");
			string localizedOpen = Localization.instance.Localize("$piece_container_open");
			string localizedStack = Localization.instance.Localize("$msg_stackall_hover");
			string useKeyColored = $"[<color=#ffff00ff><b>{localizedUse}</b></color>]";

			string composed = itemsLine != ""
				? $"{localizedName} ({ratioText})\n{itemsLine}\n{useKeyColored} {localizedOpen} {localizedStack}"
				: $"{localizedName} ({ratioText})\n{useKeyColored} {localizedOpen} {localizedStack}";

			LogHoverText(composed);

			return composed;
		}

		private static string GetColoredRatio(int used, int max)
		{
			float fill = max > 0 ? (float)used / max : 0f;
			Color col = Color.white;

			if (ConfigManager.ColoredHoverInfo.Value)
			{
				col = fill < 0.5f
					? Color.Lerp(Color.green, Color.yellow, fill / 0.5f)
					: Color.Lerp(Color.yellow, Color.red, (fill - 0.5f) / 0.5f);
			}

			string hex = ColorUtility.ToHtmlStringRGBA(col);
			return $"<color=#{hex}>{used}/{max}</color>";
		}

		private static string GetInventorySummary(Inventory inventory)
		{
			if (!ConfigManager.ShowSingleItemChestHover.Value) return "";

			var itemCounts = new Dictionary<string, int>();
			foreach (var item in inventory.GetAllItems())
			{
				if (item?.m_shared == null) continue;
				string itemName = Localization.instance.Localize(item.m_shared.m_name);
				if (!itemCounts.ContainsKey(itemName)) itemCounts[itemName] = 0;
				itemCounts[itemName] += item.m_stack;
			}

			if (itemCounts.Count == 1)
			{
				foreach (var kv in itemCounts)
					return $"{kv.Key} x{kv.Value}";
			}

			return "";
		}

		private static string _lastHoverText = "";
		private static void LogHoverText(string composed)
		{
			if (_lastHoverText != composed)
			{
				_lastHoverText = composed;
				log.Info($"UIDetailedHovers hover updated: {composed}", header: true);
			}
		}
	}
}
