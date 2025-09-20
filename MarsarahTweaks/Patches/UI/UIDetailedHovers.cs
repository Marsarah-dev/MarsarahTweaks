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
		private static readonly MethodInfo GetTimeSincePlantedMethod = typeof(Plant).GetMethod("TimeSincePlanted", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetGrowTimeMethod = typeof(Plant).GetMethod("GetGrowTime", BindingFlags.NonPublic | BindingFlags.Instance);

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
			private static bool Prefix(Beehive __instance, ref string __result)
			{
				if (!ConfigManager.DetailedHoverInfo.Value)
					return true; // fall back to vanilla

				// Skip if player has no access and add custom message
				if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false))
				{
					__result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
					return false;
				}

				__result = GetBeehiveHover(__instance);
				return false;
			}

			private static string GetBeehiveHover(Beehive beehive)
			{
				string name = Localization.instance.Localize(beehive.m_name);

				if (GetHoneyLevelMethod == null || NViewField == null)
				{
					log.Warn("Beehive reflection fields not found.");
					return name;
				}

				int honeyLevel = (int)GetHoneyLevelMethod.Invoke(beehive, null);
				ZNetView nview = NViewField.GetValue(beehive) as ZNetView;
				if (nview == null)
				{
					log.Warn("Beehive m_nview is null.");
					return name;
				}

				float produced = nview.GetZDO().GetFloat("product");
				float remaining = beehive.m_secPerUnit - produced;

				BeeHoverMode beeMode = ConfigManager.BeehiveHoverModeChoice.Value;
				string progressText = "";

				switch (beeMode)
				{
					case BeeHoverMode.Percent:
						// Percentage colored based on honey level like growth percent
						float honeyPercent = Mathf.Clamp01(produced / beehive.m_secPerUnit);
						string percentText = $"{honeyPercent:0%}";
						progressText = PaintTextIfEnabled(percentText, GetHoneyColor(honeyLevel, beehive.m_maxHoney));
						break;

					case BeeHoverMode.RemainingTime:
						// Time left is cyan
						string timeText = FormatTime(remaining);
						progressText = PaintTextIfEnabled(timeText, Color.cyan);
						break;

					case BeeHoverMode.PercentAndTime:
						float honeyPct = Mathf.Clamp01(produced / beehive.m_secPerUnit);
						string pctText = $"{honeyPct:0%}";
						string pctColored = PaintTextIfEnabled(pctText, GetHoneyColor(honeyLevel, beehive.m_maxHoney));

						string timeLeft = FormatTime(remaining);
						string timeColored = PaintTextIfEnabled(timeLeft, Color.cyan);

						progressText = $"{pctColored} - {timeColored}";
						break;
				}

				string productName = Localization.instance.Localize(beehive.m_honeyItem.m_itemData.m_shared.m_name);
				string productColored = PaintTextIfEnabled(productName, GetHoneyColor(honeyLevel, beehive.m_maxHoney));
				string honeyCountColored = PaintTextIfEnabled("x" + honeyLevel, GetHoneyColor(honeyLevel, beehive.m_maxHoney));

				string useKeyColored = $"[<color=#ffff00ff><b>{Localization.instance.Localize("$KEY_Use")}</b></color>]";

				string hoverText;
				if (honeyLevel == beehive.m_maxHoney)
				{
					hoverText = $"{name} ( {productColored} {honeyCountColored} )\n{useKeyColored} {Localization.instance.Localize("$piece_beehive_extract")}";
				}
				else if (honeyLevel > 0)
				{
					hoverText = $"{name} ( {progressText}, {productColored} {honeyCountColored} )\n{useKeyColored} {Localization.instance.Localize("$piece_beehive_extract")}";
				}
				else
				{
					string emptyText = PaintTextIfEnabled(Localization.instance.Localize("$piece_container_empty"), GetEmptyColor());
					hoverText = $"{name} ( {progressText}, {emptyText} )\n{useKeyColored} {Localization.instance.Localize("$piece_beehive_check")}";
				}

				return hoverText;
			}

			// ---------- Colors ----------
			private static Color GetProgressColor(int honeyLevel, int maxHoney, float produced, float secPerUnit)
			{
				return (honeyLevel >= maxHoney && produced >= secPerUnit) ? Color.green : Color.cyan;
			}

			private static Color GetHoneyColor(int honeyLevel, int maxHoney)
			{
				float fill = maxHoney > 0 ? (float)honeyLevel / maxHoney : 0f;
				return fill < 0.5f
					? Color.Lerp(Color.red, Color.yellow, fill / 0.5f)
					: Color.Lerp(Color.yellow, Color.green, (fill - 0.5f) / 0.5f);
			}

			private static Color GetEmptyColor() => new Color(1f, 0.4f, 0f);
		}

		[HarmonyPatch(typeof(Plant), nameof(Plant.GetHoverText))]
		internal static class DetailedHoverPlant_Patch
		{
			private static bool Prefix(Plant __instance, ref string __result)
			{
				if (!ConfigManager.DetailedHoverInfo.Value)
					return true; // fall back to vanilla 

				if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false))
				{
					__result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
					return false;
				}

				__result = GetPlantHover(__instance);
				return false;
			}

			private static string GetPlantHover(Plant plant)
			{
				string name = Localization.instance.Localize(plant.m_name);

				// Growth % (0.0–1.0)
				if (GetTimeSincePlantedMethod == null)
				{
					log.Warn("Plant reflection fields not found.");
					return Localization.instance.Localize(plant.m_name);
				}

				PlantHoverMode plantMode = ConfigManager.PlantHoverModeChoice.Value;

				double age = (double)GetTimeSincePlantedMethod.Invoke(plant, null);
				float growTime = (float)GetGrowTimeMethod.Invoke(plant, null);

				string growthLine = "";
				float growthPercent = -1f;
				string percentText = "";

				switch (plantMode)
				{
					case PlantHoverMode.Percent:
						growthPercent = Mathf.Clamp01((float)(age / growTime));
						percentText = $"{growthPercent:0%}";
						growthLine = PaintTextIfEnabled(percentText, GetColorForGrowth(growthPercent));
						break;
					case PlantHoverMode.RemainingTime:
						float remaining = Mathf.Max(0f, growTime - (float)age);
						string timeLeftText = FormatTime(remaining);
						growthLine = PaintTextIfEnabled(timeLeftText, Color.cyan);
						break;
					case PlantHoverMode.PercentAndTime:
						growthPercent = Mathf.Clamp01((float)(age / growTime));
						percentText = $"{growthPercent:0%}";
						string percentColored = PaintTextIfEnabled(percentText, GetColorForGrowth(growthPercent));

						remaining = Mathf.Max(0f, growTime - (float)age);
						timeLeftText = FormatTime(remaining);
						string timeLeftColored = PaintTextIfEnabled(timeLeftText, Color.cyan);

						growthLine = $"{percentColored} - {timeLeftColored}";
						break;
				}

				string useKeyColored = $"[<color=#ffff00ff><b>{Localization.instance.Localize("$KEY_Use")}</b></color>]";

				string hoverText;
				if (age >= growTime) // Plant is grown
				{
					hoverText = $"{name} ( {Localization.instance.Localize("$hud_ready")} )\n{useKeyColored} {Localization.instance.Localize("$inventory_pickup")}";
				}
				else
				{
					hoverText = $"{name} ( {growthLine} )";
				}

				return hoverText;
			}

			// ---------- Colors ----------
			private static Color GetColorForGrowth(float growFactor)
			{
				// 0% = red, 50% = yellow, 100% = green
				return growFactor < 0.5f
					? Color.Lerp(Color.red, Color.yellow, growFactor / 0.5f)
					: Color.Lerp(Color.yellow, Color.green, (growFactor - 0.5f) / 0.5f);
			}
		}

		// ---------- Generic time formatter ----------
		private static string FormatTime(float seconds)
		{
			int mins = Mathf.FloorToInt(seconds / 60f);
			int secs = Mathf.FloorToInt(seconds % 60f);
			return mins > 0f ? $"{mins}m {secs}s" : $"{secs}s";
		}

		// ---------- Generic painter ----------
		private static string PaintText(string text, Color col)
		{
			string hex = ColorUtility.ToHtmlStringRGBA(col);
			return $"<color=#{hex}>{text}</color>";
		}

		// ---------- Generic painter if enabled ----------
		private static string PaintTextIfEnabled(string text, Color col)
		{
			return ConfigManager.ColoredHoverInfo.Value ? PaintText(text, col) : text;
		}
	}
}
