using HarmonyLib;
using Jotunn.Configs;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
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
		private static readonly FieldInfo NViewFieldBeehive = typeof(Beehive).GetField("m_nview", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo NViewFieldCookingStation = typeof(CookingStation).GetField("m_nview", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo FermenterExposedField = typeof(Fermenter).GetField("m_exposed", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo HaveRoofField = typeof(Smelter).GetField("m_haveRoof", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo GetHoneyLevelMethod = typeof(Beehive).GetMethod("GetHoneyLevel", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetTimeSincePlantedMethod = typeof(Plant).GetMethod("TimeSincePlanted", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetGrowTimeMethod = typeof(Plant).GetMethod("GetGrowTime", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetFermenterStatusMethod = typeof(Fermenter).GetMethod("GetStatus", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetFermenterContentNameMethod = typeof(Fermenter).GetMethod("GetContentName", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetFermentationTimeMethod = typeof(Fermenter).GetMethod("GetFermentationTime", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetSlotMethod = typeof(CookingStation).GetMethod("GetSlot", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetItemConversionMethod = typeof(CookingStation).GetMethod("GetItemConversion", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetHoverTextMethod = typeof(CookingStation).GetMethod("HoverText", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetCSFuelMethod = typeof(CookingStation).GetMethod("GetFuel", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo OnHoverFuelSwitchMethod = typeof(CookingStation).GetMethod("OnHoverFuelSwitch", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo GetProcessedQueueSizeMethod = typeof(Smelter).GetMethod("GetProcessedQueueSize", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo GetQueueSizeMethod = typeof(Smelter).GetMethod("GetQueueSize", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo GetFuelMethod = typeof(Smelter).GetMethod("GetFuel", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo GetBakeTimerMethod = typeof(Smelter).GetMethod("GetBakeTimer", BindingFlags.Instance | BindingFlags.NonPublic);


		// Log cache
		private static string _lastHoverText = "";

		[HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
		internal static class DetailedHoverContainer_Patch
		{
			private static void Postfix(Container __instance, ref string __result)
			{
				if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off) return;

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
				ContainerHoverMode containerMode = ConfigManager.ContainerHoverModeChoice.Value;

				int max = inventory.GetWidth() * inventory.GetHeight();
				string containerText = "";

				switch (containerMode)
				{
					case ContainerHoverMode.CurrentPerMax:
						int used = inventory.NrOfItems();
						string usedPerMaxText = $"{used}/{max}";
						containerText = PaintTextIfEnabled(usedPerMaxText, GetInventoryRatioColor(used, max));
						break;
					case ContainerHoverMode.AmountOfFreeSlots:
						int emptySlots = inventory.GetEmptySlots();
						string emptySlotsText = $"{emptySlots}";
						containerText = $"Free Slots: {PaintTextIfEnabled(emptySlotsText, GetInventoryEmptySlotsColor(emptySlots, max))}";
						break;
					case ContainerHoverMode.Percent:
						float usedPercentRaw = inventory.SlotsUsedPercentage();
						float usedPercentNormalized = usedPercentRaw / 100f;
						string usedPercentText = $"{usedPercentRaw}%";
						containerText = PaintTextIfEnabled(usedPercentText, GetPercentColorInverted(usedPercentNormalized));
						break;
				}

				string localizedName = Localization.instance.Localize(container.m_name);
				string localizedOpen = Localization.instance.Localize("$piece_container_open");
				string localizedStack = Localization.instance.Localize("$msg_stackall_hover");
				string useKeyColored = $"[{PaintTextIfEnabled(Localization.instance.Localize("$KEY_Use"), Color.yellow, bold: true)}]";
				string oneItemsLine = GetOneItemInventory(inventory);

				string finalText = oneItemsLine != ""
					? $"{localizedName} ({containerText})\n{oneItemsLine}\n{useKeyColored} {localizedOpen} {localizedStack}"
					: $"{localizedName} ({containerText})\n{useKeyColored} {localizedOpen} {localizedStack}";

				LogHoverText(finalText);

				return finalText;
			}

			private static string GetOneItemInventory(Inventory inventory)
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

			private static Color GetInventoryRatioColor(int used, int max)
			{
				float fill = max > 0 ? (float)used / max : 0f;
				return fill < 0.5f
					? Color.Lerp(Color.green, Color.yellow, fill / 0.5f)
					: Color.Lerp(Color.yellow, Color.red, (fill - 0.5f) / 0.5f);
			}

			private static Color GetInventoryEmptySlotsColor(int empty, int max)
			{
				float fill = max > 0 ? (float)empty / max : 0f;
				return fill < 0.5f
					? Color.Lerp(Color.red, Color.yellow, fill / 0.5f)
					: Color.Lerp(Color.yellow, Color.green, (fill - 0.5f) / 0.5f);
			}
		}

		[HarmonyPatch(typeof(Beehive), nameof(Beehive.GetHoverText))]
		internal static class DetailedHoverBeehive_Patch
		{
			private static bool Prefix(Beehive __instance, ref string __result)
			{
				if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
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

				if (GetHoneyLevelMethod == null || NViewFieldBeehive == null)
				{
					log.Warn("Beehive reflection fields not found.");
					return name;
				}

				int honeyLevel = (int)GetHoneyLevelMethod.Invoke(beehive, null);
				ZNetView nview = NViewFieldBeehive.GetValue(beehive) as ZNetView;
				if (nview == null)
				{
					log.Warn("Beehive m_nview is null.");
					return name;
				}

				float produced = nview.GetZDO().GetFloat("product");
				float remaining = beehive.m_secPerUnit - produced;

				BeeHoverMode beeMode = ConfigManager.BeehiveHoverModeChoice.Value;
				string progressText = "";
				float honeyPercent;
				string percentText;
				string timeText;

				switch (beeMode)
				{
					case BeeHoverMode.Percent:
						// Percentage colored based on honey level like growth percent
						honeyPercent = Mathf.Clamp01(produced / beehive.m_secPerUnit);
						percentText = $"{honeyPercent:0%}";
						progressText = PaintTextIfEnabled(percentText, GetPercentColor(honeyPercent));
						break;

					case BeeHoverMode.RemainingTime:
						// Time left is cyan
						timeText = FormatTime(remaining);
						progressText = PaintTextIfEnabled(timeText, Color.cyan);
						break;

					case BeeHoverMode.PercentAndTime:
						honeyPercent = Mathf.Clamp01(produced / beehive.m_secPerUnit);
						percentText = $"{honeyPercent:0%}";
						string percentColored = PaintTextIfEnabled(percentText, GetPercentColor(honeyPercent));

						timeText = FormatTime(remaining);
						string timeColored = PaintTextIfEnabled(timeText, Color.cyan);

						progressText = $"{percentColored} - {timeColored}";
						break;
				}

				string productName = Localization.instance.Localize(beehive.m_honeyItem.m_itemData.m_shared.m_name);
				string productColored = PaintTextIfEnabled(productName, GetHoneyColor(honeyLevel, beehive.m_maxHoney));
				string honeyCountColored = PaintTextIfEnabled("x" + honeyLevel, GetHoneyColor(honeyLevel, beehive.m_maxHoney));
				string useKeyColored = $"[{PaintTextIfEnabled(Localization.instance.Localize("$KEY_Use"), Color.yellow, bold: true)}]";

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
				if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
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
					return name;
				}

				PlantHoverMode plantMode = ConfigManager.PlantHoverModeChoice.Value;

				double age = (double)GetTimeSincePlantedMethod.Invoke(plant, null);
				float growTime = (float)GetGrowTimeMethod.Invoke(plant, null);

				string growthLine = "";
				float growthPercent;
				string percentText;
				float remaining;
				string timeLeftText;

				switch (plantMode)
				{
					case PlantHoverMode.Percent:
						growthPercent = Mathf.Clamp01((float)(age / growTime));
						percentText = $"{growthPercent:0%}";
						growthLine = PaintTextIfEnabled(percentText, GetPercentColor(growthPercent));
						break;
					case PlantHoverMode.RemainingTime:
						remaining = Mathf.Max(0f, growTime - (float)age);
						timeLeftText = FormatTime(remaining);
						growthLine = PaintTextIfEnabled(timeLeftText, Color.cyan);
						break;
					case PlantHoverMode.PercentAndTime:
						growthPercent = Mathf.Clamp01((float)(age / growTime));
						percentText = $"{growthPercent:0%}";
						string percentColored = PaintTextIfEnabled(percentText, GetPercentColor(growthPercent));

						remaining = Mathf.Max(0f, growTime - (float)age);
						timeLeftText = FormatTime(remaining);
						string timeLeftColored = PaintTextIfEnabled(timeLeftText, Color.cyan);

						growthLine = $"{percentColored} - {timeLeftColored}";
						break;
				}

				string useKeyColored = $"[{PaintTextIfEnabled(Localization.instance.Localize("$KEY_Use"), Color.yellow, bold: true)}]";

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
		}

		[HarmonyPatch(typeof(Fermenter), nameof(Fermenter.GetHoverText))]
		internal static class DetailedHoverFermenter_Patch
		{
			private static bool Prefix(Fermenter __instance, ref string __result)
			{
				if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
					return true; // fall back to vanilla

				// Skip if player has no access
				if (!PrivateArea.CheckAccess(__instance.transform.position, 0f, flash: false))
				{
					__result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess");
					return false;
				}

				string customFermenterString = GetFermenterHover(__instance);
				if (customFermenterString == null)
					return true; // fall back to vanilla

				__result = customFermenterString;
				return false;
			}

			// ---------- Hover builder ----------
			private static string GetFermenterHover(Fermenter fermenter)
			{
				string name = Localization.instance.Localize(fermenter.m_name);

				if (FermenterExposedField == null || GetFermenterStatusMethod == null || GetFermenterContentNameMethod == null || GetFermentationTimeMethod == null)
				{
					log.Warn("Fermenter reflection fields not found.");
					return null;
				}

				object statusObj = GetFermenterStatusMethod.Invoke(fermenter, null);
				if (statusObj == null)
				{
					log.Warn("Fermenter.GetStatus returned null.");
					return null;
				}
				Type statusType = GetFermenterStatusMethod.ReturnType;
				string statusName = Enum.GetName(statusType, statusObj);

				switch (statusName)
				{
					case "Fermenting":
					{
						LogHoverText($"Switch case - {statusName}");
						FermenterHoverMode fermenterMode = ConfigManager.FermenterHoverModeChoice.Value;
						string contentName = Localization.instance.Localize((string)GetFermenterContentNameMethod.Invoke(fermenter, null));
						string localizedExposed = Localization.instance.Localize("$piece_fermenter_exposed");
						string localizedFermenting = Localization.instance.Localize("$piece_fermenter_fermenting");
						string hoverText = null;

						double timePassed = (double)GetFermentationTimeMethod.Invoke(fermenter, null);
						float totalTime = fermenter.m_fermentationDuration;
						float percent;
						string percentText;
						string percentColored;
						float remaining;
						string timeText;
						string timeColored;

						// If exposed → special message
						bool fermenterExposed = (bool)FermenterExposedField.GetValue(fermenter);
						if (fermenterExposed)
						{
							return $"{name} ( {contentName} )\n{localizedExposed}";
						}

						switch (fermenterMode)
						{
							case FermenterHoverMode.Percent:
								percent = Mathf.Clamp01((float)(timePassed / totalTime));
								percentText = $"{percent:0%}";
								percentColored = PaintTextIfEnabled(percentText, GetPercentColor(percent));
								hoverText = $"{name} ( {contentName} )\n{localizedFermenting}: {percentColored}";
								break;
							case FermenterHoverMode.RemainingTime:
								remaining = Mathf.Max(0f, totalTime - (float)timePassed);
								timeText = FormatTime(remaining);
								timeColored = PaintTextIfEnabled(timeText, Color.cyan);
								hoverText = $"{name} ( {contentName} )\n{localizedFermenting}: {timeColored}";
								break;
							case FermenterHoverMode.PercentAndTime:
								percent = Mathf.Clamp01((float)(timePassed / totalTime));
								percentText = $"{percent:0%}";
								percentColored = PaintTextIfEnabled(percentText, GetPercentColor(percent));
								remaining = Mathf.Max(0f, totalTime - (float)timePassed);
								timeText = FormatTime(remaining);
								timeColored = PaintTextIfEnabled(timeText, Color.cyan);
								hoverText = $"{name} ( {contentName} )\n{localizedFermenting}: {percentColored} - {timeColored}";
								break;
						}

						return hoverText;
					}

					case "Ready":
					{
						LogHoverText($"Switch case - {statusName}");
						string contentName = (string)GetFermenterContentNameMethod.Invoke(fermenter, null);
						string useKeyColored = $"[{PaintTextIfEnabled(Localization.instance.Localize("$KEY_Use"), Color.yellow, bold: true)}]";
						string localizedReady = PaintTextIfEnabled(Localization.instance.Localize("$piece_fermenter_ready"), Color.green);
						string localizedTap = Localization.instance.Localize("$piece_fermenter_tap");

						return Localization.instance.Localize($"{name} ( {localizedReady} )\n{contentName}\n{useKeyColored} {localizedTap}");
					}

					default:
						LogHoverText($"Switch case - {statusName}");
						// Vanilla handles "Empty" state
						return null;
				}
			}
		}

		[HarmonyPatch(typeof(CookingStation), "Awake")]
		internal static class CookingStation_AddFoodSwitchHoverPatch
		{
			private static void Postfix(CookingStation __instance)
			{
				// Skip if no switch
				if (__instance.m_addFoodSwitch == null)
					return;

				// Avoid overwriting existing delegates
				if (__instance.m_addFoodSwitch.m_onHover != null)
					return;

				ZNetView nview = NViewFieldCookingStation.GetValue(__instance) as ZNetView;
				if (nview == null)
				{
					log.Warn("CookingStation m_nview is null.");
					return;
				}

				if (!nview.IsOwner())
					return;

				// Assign a hover delegate to the food interaction hover
				__instance.m_addFoodSwitch.m_onHover = () =>
				{
					string vanillaText = Localization.instance.Localize(GetHoverTextMethod.Invoke(__instance, null) as string);

					if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
						return vanillaText;

					string hover = GetCookingStationHover(__instance);
					if (!string.IsNullOrEmpty(hover))
						return hover;

					return vanillaText;
				};

				// Assign a hover delegate to the wood interaction hover
				__instance.m_addFuelSwitch.m_onHover = () =>
				{
					string vanillaText = Localization.instance.Localize(OnHoverFuelSwitchMethod.Invoke(__instance, null) as string);

					if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
						return vanillaText;

					// Compute remaining fuel time
					float fuel = (float)GetCSFuelMethod.Invoke(__instance, null);
					float remainingSeconds = fuel * __instance.m_secPerFuel;
					string hover = "";
					if (remainingSeconds <= 0)
						hover = vanillaText;
					else
						hover = $"{vanillaText}\nTime Left: {PaintTextIfEnabled(FormatTime(remainingSeconds), Color.cyan)}";

					return hover;
				};
			}
		}

		[HarmonyPatch(typeof(CookingStation), "GetHoverText")]
		internal static class CookingStationHoverPatch
		{
			private static bool Prefix(CookingStation __instance, ref string __result)
			{
				if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
					return true; // vanilla

				// Only owners see slot timers
				ZNetView nview = NViewFieldCookingStation.GetValue(__instance) as ZNetView;
				if (nview == null)
				{
					log.Warn("CookingStation m_nview is null.");
					return true;
				}

				if (!nview.IsOwner())
					return true;

				bool isOven = __instance.m_useFuel && !__instance.m_requireFire;
				if (isOven)
					return true;

				string hover = GetCookingStationHover(__instance);
				if (hover != null)
				{
					__result = hover;
					return false;
				}

				return true; // fallback to vanilla
			}
		}

		private static string GetCookingStationHover(CookingStation station)
		{
			if (GetSlotMethod == null || GetItemConversionMethod == null)
			{
				log.Warn("CookingStation reflection not found.");
				return null;
			}

			string stationName = Localization.instance.Localize(station.m_name);
			string slotInfo = "";
			int activeSlots = 0;
			bool hasReadyItem = false;
			bool hasOvercookedItem = false;

			for (int i = 0; i < station.m_slots.Length; i++)
			{
				object[] args = { i, null, 0f, null };
				GetSlotMethod.Invoke(station, args);

				string itemName = args[1] as string;
				float cookedTime = (float)args[2];

				if (string.IsNullOrEmpty(itemName))
					continue;

				// overcooked items still count!
				if (itemName == station.m_overCookedItem?.name)
				{
					hasOvercookedItem = true;
					continue;
				}

				var itemConv = (CookingStation.ItemConversion)GetItemConversionMethod.Invoke(station, new object[] { itemName });
				if (itemConv == null)
					continue;

				activeSlots++;

				// check "ready to take" state
				if (cookedTime >= itemConv.m_cookTime && cookedTime < itemConv.m_cookTime * 2f)
					hasReadyItem = true;

				string displayText = BuildCookingSlotText(itemConv, cookedTime, station, itemName);
				slotInfo += "\n" + displayText;
			}

			if (activeSlots == 0 && !hasOvercookedItem)
				return null; // vanilla handles empty

			string useKeyColored = $"[{PaintTextIfEnabled(Localization.instance.Localize("$KEY_Use"), Color.yellow, bold: true)}]";
			string localizedCook = Localization.instance.Localize("$piece_cstand_cook");
			string localizedTake = Localization.instance.Localize("$piece_itemstand_take");

			if (hasReadyItem || hasOvercookedItem)
				return $"{stationName}\n{useKeyColored} {localizedTake}{slotInfo}";

			return (activeSlots >= station.m_slots.Length)
				? $"{stationName}{slotInfo}"
				: $"{stationName}\n{useKeyColored} {localizedCook} {slotInfo}";
		}

		private static string BuildCookingSlotText(CookingStation.ItemConversion conv, float cookedTime, CookingStation station, string currentItemName)
		{
			float cookTime = conv.m_cookTime;
			float overCookTime = cookTime * 2f;

			// localize the current item instead of future product
			GameObject currentPrefab = ObjectDB.instance.GetItemPrefab(currentItemName);
			string currentName = currentPrefab != null
				? Localization.instance.Localize(currentPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)
				: currentItemName;

			switch (ConfigManager.CookingStationHoverModeChoice.Value)
			{
				case CookingStationHoverMode.Percent:
					if (cookedTime > cookTime)
					{
						float percent = Mathf.Clamp01((cookedTime - cookTime) / cookTime);
						string percentText = $"{percent:0%}";
						return $"{currentName}: {PaintTextIfEnabled(percentText, GetPercentColorInverted(percent))}";
					}
					else
					{
						float percent = Mathf.Clamp01(cookedTime / cookTime);
						string percentText = $"{percent:0%}";
						return $"{currentName}: {PaintTextIfEnabled(percentText, GetPercentColor(percent))}";
					}

				case CookingStationHoverMode.RemainingTime:
					if (cookedTime > cookTime)
					{
						float remaining = Mathf.Max(0f, overCookTime - cookedTime);
						string time = FormatTime(remaining);
						return $"{currentName}: {PaintTextIfEnabled(time, Color.red)}";
					}
					else
					{
						float remaining = Mathf.Max(0f, cookTime - cookedTime);
						string time = FormatTime(remaining);
						return $"{currentName}: {PaintTextIfEnabled(time, Color.cyan)}";
					}

				case CookingStationHoverMode.PercentAndTime:
					if (cookedTime > cookTime)
					{
						float percent = Mathf.Clamp01((cookedTime - cookTime) / cookTime);
						string percentText = $"{percent:0%}";
						string percentColored = PaintTextIfEnabled(percentText, GetPercentColorInverted(percent));

						float remaining = Mathf.Max(0f, overCookTime - cookedTime);
						string time = PaintTextIfEnabled(FormatTime(remaining), Color.red);

						return $"{currentName}: {percentColored} - {time}";
					}
					else
					{
						float percent = Mathf.Clamp01(cookedTime / cookTime);
						string percentText = $"{percent:0%}";
						string percentColored = PaintTextIfEnabled(percentText, GetPercentColor(percent));

						float remaining = Mathf.Max(0f, cookTime - cookedTime);
						string time = PaintTextIfEnabled(FormatTime(remaining), Color.cyan);

						return $"{currentName}: {percentColored} - {time}";
					}
			}

			return null;
		}

		[HarmonyPatch(typeof(Smelter), "OnHoverAddOre")]
		internal static class SmelterHoverAddPatch
		{
			private static void Postfix(Smelter __instance, ref string __result)
			{
				if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
					return;

				__result = GetSmelterHover(__instance, __result);
			}
		}

		[HarmonyPatch(typeof(Smelter), "OnHoverAddFuel")]
		internal static class SmelterHoverFuelPatch
		{
			private static void Postfix(Smelter __instance, ref string __result)
			{
				if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
					return;

				__result = GetSmelterHover(__instance, __result);
			}
		}

		[HarmonyPatch(typeof(Smelter), "OnHoverEmptyOre")]
		internal static class SmelterHoverEmptyPatch
		{
			private static void Postfix(Smelter __instance, ref string __result)
			{
				if (ConfigManager.DetailedHoverInfoChoice.Value == HoverInfoMode.Off)
					return;

				__result = GetSmelterHover(__instance, __result);
			}
		}

		private static string GetSmelterHover(Smelter smelter, string result)
		{
			if (!smelter.IsActive())
				return result;

			float fuel = (float)GetFuelMethod.Invoke(smelter, null);
			int queueSize = (int)GetQueueSizeMethod.Invoke(smelter, null);
			float bakeTimer = (float)GetBakeTimerMethod.Invoke(smelter, null);

			// Avoid division by zero
			if (queueSize <= 0)
				return result;

			// Base processing duration (seconds)
			float durationPerItem = smelter.m_secPerProduct;
			int fuelPerProduct = smelter.m_fuelPerProduct;

			// How many additional whole items we can process after the current one
			int additionalItems;
			if (fuelPerProduct > 0)
			{
				// number of full items that can be produced using remaining fuel
				int fullItemsFromFuel = Mathf.FloorToInt(fuel / fuelPerProduct);
				// we can only count items that actually exist after the current one
				int maxAvailableAfterCurrent = Mathf.Max(0, queueSize - 1);
				additionalItems = Mathf.Clamp(fullItemsFromFuel, 0, maxAvailableAfterCurrent);
			}
			else
			{
				// windmill - no-fuel case: everything in the queue is processable (after the current one is queueSize-1)
				additionalItems = Mathf.Max(0, queueSize - 1);
			}

			// Remaining time for the current item (never negative; never more than a single item duration)
			float remainingCurrent = Mathf.Clamp(durationPerItem - bakeTimer, 0f, durationPerItem);

			// Total remaining seconds for current + additional fully-processable items
			float remainingSeconds = remainingCurrent + additionalItems * durationPerItem;

			// Apply windmill power modifier
			float power = 1f;
			if (smelter.m_windmill != null)
			{
				power = Mathf.Max(smelter.m_windmill.GetPowerOutput(), 0.0001f);
				remainingSeconds /= power;
			}

			// Percent (this is for current item only, not all items in queue. Need to store initial state to calculate total percentage properly)
			//float percent = Mathf.Clamp01(bakeTimer / Mathf.Max(durationPerItem, 0.0001f)) * 100f;

			// Format additions
			string hover = "";
			if (SmelterHoverModeChoice.Value == SmelterHoverMode.RemainingTime)
			{
				// Wind power line 
				if (smelter.m_windmill != null)
				{
					int percentPower = Mathf.RoundToInt(power * 100f);
					hover += $"\nWind: {PaintTextIfEnabled($"{percentPower}%", GetPercentColor(power))}";
				}

				// Time line 
				hover += $"\n{PaintTextIfEnabled(FormatTime(remainingSeconds), Color.cyan)}";
			}

			return Localization.instance.Localize($"{result} {hover}");
		}

		// ---------- Generic time formatter ----------
		private static string FormatTime(float seconds)
		{
			int totalSeconds = Mathf.FloorToInt(seconds);
			int hrs = totalSeconds / 3600;
			int mins = (totalSeconds % 3600) / 60;
			int secs = totalSeconds % 60;

			if (hrs > 0)
				return $"{hrs}h {mins}m {secs}s";
			else if (mins > 0)
				return $"{mins}m {secs}s";
			else
				return $"{secs}s";
		}

		// ---------- Percent color -----------
		private static Color GetPercentColor(float percent)
		{
			// 0% = red, 50% = yellow, 100% = green
			return percent < 0.5f
				? Color.Lerp(Color.red, Color.yellow, percent / 0.5f)
				: Color.Lerp(Color.yellow, Color.green, (percent - 0.5f) / 0.5f);
		}

		private static Color GetPercentColorInverted(float percent)
		{
			// 0% = green, 50% = yellow, 100% = red
			return percent < 0.5f
				? Color.Lerp(Color.green, Color.yellow, percent / 0.5f)
				: Color.Lerp(Color.yellow, Color.red, (percent - 0.5f) / 0.5f);
		}

		// ---------- Generic painter ----------
		private static string PaintText(string text, Color col)
		{
			string hex = ColorUtility.ToHtmlStringRGBA(col);
			return $"<color=#{hex}>{text}</color>";
		}

		// ---------- Generic painter if enabled ----------
		private static string PaintTextIfEnabled(string text, Color col, bool bold = false)
		{
			if (ConfigManager.DetailedHoverInfoChoice.Value != HoverInfoMode.ColoredText)
				return bold ? $"<b>{text}</b>" : text;

			string colored = PaintText(text, col);
			return bold ? $"<b>{colored}</b>" : colored;
		}

		// ---------- Log text only when changed ----------
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
