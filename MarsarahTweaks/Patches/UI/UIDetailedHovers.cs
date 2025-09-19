using HarmonyLib;
using MarsarahTweaks.Managers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using TMPro;
using UnityEngine;
using static MarsarahTweaks.Managers.ConfigManager;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIDetailedHovers
	{
		private static readonly LogManager log = new LogManager("UI Detailed Hover Info", LogManager.LogLevel.Info);

		// Cache the FieldInfo for performance
		private static readonly FieldInfo InventoryField = typeof(Container).GetField("m_inventory", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo NViewField = typeof(Beehive).GetField("m_nview", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetHoneyLevelMethod = typeof(Beehive).GetMethod("GetHoneyLevel", BindingFlags.NonPublic | BindingFlags.Instance);

		// Log cache
		private static string _lastHoverText = "";

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

			private static void LogHoverText(string composed)
			{
				if (_lastHoverText != composed)
				{
					_lastHoverText = composed;
					log.Info($"UIDetailedHovers hover updated: {composed}", header: true);
				}
			}
		}

		[HarmonyPatch(typeof(Beehive), nameof(Beehive.GetHoverText))]
		internal static class DetailedHoverBeehive_Patch
		{
			private static void Postfix(Beehive __instance, ref string __result)
			{
				if (!ConfigManager.DetailedHoverInfo.Value) return;

				// Skip if player has no access
				/*if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false))
				{
					__result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
					return;
				}*/

				__result = GetBeehiveHover(__instance, __result);
			}

			private static string GetBeehiveHover(Beehive beehive, string originalHoverText = null)
			{
				if (GetHoneyLevelMethod == null || NViewField == null)
				{
					log.Warn("Beehive reflection fields not found.");
					return Localization.instance.Localize(beehive.m_name);
				}

				int honeyLevel = (int)GetHoneyLevelMethod.Invoke(beehive, null);

				// If beehive is full, return the original hover text if provided
				if (honeyLevel >= beehive.m_maxHoney && !string.IsNullOrEmpty(originalHoverText))
				{
					return originalHoverText;
				}

				ZNetView nview = NViewField.GetValue(beehive) as ZNetView;
				if (nview == null)
				{
					log.Warn("Beehive m_nview is null.");
					return Localization.instance.Localize(beehive.m_name);
				}

				string productName = Localization.instance.Localize(beehive.m_honeyItem.m_itemData.m_shared.m_name);
				string progressText = "";
				BeeHoverMode beeMode = ConfigManager.BeehiveHoverMode.Value;

				if (honeyLevel < beehive.m_maxHoney)
				{
					float produced = nview.GetZDO().GetFloat("product");
					float remaining = beehive.m_secPerUnit - produced;
					if (beeMode == BeeHoverMode.RemainingTime)
						progressText = $"{FormatTime(remaining)}";
					else if (beeMode == BeeHoverMode.Percent)
						progressText = $"{produced / beehive.m_secPerUnit:P0}";
					else if (beeMode == BeeHoverMode.PercentAndTime)
						progressText = $"{produced / beehive.m_secPerUnit:P0} - {FormatTime(remaining)}";
				}

				string useKeyColored = $"[<color=#ffff00ff><b>{Localization.instance.Localize("$KEY_Use")}</b></color>]";

				string hoverText;
				if (honeyLevel > 0)
				{
					hoverText = $"{Localization.instance.Localize(beehive.m_name)} ( {progressText}, {productName} x {honeyLevel} )\n{useKeyColored} {Localization.instance.Localize("$piece_beehive_extract")}";
				}
				else
				{
					hoverText = $"{Localization.instance.Localize(beehive.m_name)} ( {progressText}, {Localization.instance.Localize("$piece_container_empty")} )\n{useKeyColored} {Localization.instance.Localize("$piece_beehive_check")}";
				}

				if (_lastHoverText != hoverText)
				{
					_lastHoverText = hoverText;
					log.Info($"UIDetailedHovers hover updated: {hoverText}", header: true);
				}

				return hoverText;
			}

			private static string FormatTime(float seconds)
			{
				int mins = Mathf.FloorToInt(seconds / 60f);
				int secs = Mathf.FloorToInt(seconds % 60f);
				return mins > 0f ? $"{mins}m {secs}s" : $"{secs}s";
			}
		}		
	}
}
