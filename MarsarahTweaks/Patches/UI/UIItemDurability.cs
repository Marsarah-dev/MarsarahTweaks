using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIItemDurability
	{
		private static readonly LogManager log = new LogManager("UI Item Durability", LogManager.LogLevel.Warning);

		// Reflection cache for HotkeyBar
		private static readonly FieldInfo hotkeyItemsField = null;
		private static readonly FieldInfo hotkeyElementsField = null;
		private static readonly FieldInfo hotkeyDurabilityField = null;

		// Reflection cache for InventoryGrid
		private static readonly FieldInfo inventoryField = null;
		private static readonly FieldInfo elementsField = null;
		private static readonly Type elementType = null;
		private static readonly FieldInfo durabilityField = null;

		// Custom sprite
		private static readonly Sprite CustomSprite = null;
		private static Sprite DefaultSprite = null;

		// Static constructor for all reflection and other data
		static UIItemDurability()
		{
			// HotkeyBar
			hotkeyItemsField = AccessTools.Field(typeof(HotkeyBar), "m_items");
			hotkeyElementsField = AccessTools.Field(typeof(HotkeyBar), "m_elements");
			var hotkeyElementType = typeof(HotkeyBar).GetNestedType("ElementData", BindingFlags.NonPublic);
			hotkeyDurabilityField = AccessTools.Field(hotkeyElementType, "m_durability");

			// InventoryGrid
			inventoryField = typeof(InventoryGrid).GetField("m_inventory", BindingFlags.NonPublic | BindingFlags.Instance);
			elementsField = typeof(InventoryGrid).GetField("m_elements", BindingFlags.NonPublic | BindingFlags.Instance);
			elementType = elementsField.FieldType.GetGenericArguments()[0]; // Element type inside List<Element>
			durabilityField = elementType.GetField("m_durability", BindingFlags.Public | BindingFlags.Instance);

			// Other stuff
			CustomSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "bar_stagger"); // bar_monster_hp_5, bar_food_8, bar_stagger
			InitDefaultSprite();

			// Debug: log all available sprites once
			/*foreach (var s in Resources.FindObjectsOfTypeAll<Sprite>())
			{
				log.Info($"Found sprite: {s.name}");
			}*/
		}

		private static void InitDefaultSprite()
		{
			if (DefaultSprite != null) return;

			var anyBar = Resources.FindObjectsOfTypeAll<GuiBar>().FirstOrDefault();
			if (anyBar != null)
			{
				var img = anyBar.m_bar?.GetComponent<Image>();
				if (img != null)
				{
					DefaultSprite = img.sprite;
					log.Info($"Cached default sprite at startup: {(DefaultSprite != null ? DefaultSprite.name : "null")}");
				}
			}
		}

		[HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
		public static class ItemDurability_HotkeyBarPatch
		{
			private static void Postfix(HotkeyBar __instance, Player player)
			{
				if (!player || player.IsDead()) return;

				var items = (List<ItemDrop.ItemData>)hotkeyItemsField.GetValue(__instance);
				var elements = (IList)hotkeyElementsField.GetValue(__instance);

				for (int i = 0; i < items.Count && i < elements.Count; i++)
				{
					var item = items[i];
					if (item == null || !item.m_shared.m_useDurability)
						continue;

					var elementData = elements[item.m_gridPos.x];
					var durabilityBar = (GuiBar)hotkeyDurabilityField.GetValue(elementData);
					if (durabilityBar == null) continue;

					// Update color
					if (ConfigManager.ColoredItemDurabilityBar.Value)
					{
						float durabilityPercent = item.GetDurabilityPercentage();
						durabilityBar.SetColor(GetDurabilityColor(durabilityPercent));
					}

					var barImage = durabilityBar.m_bar?.GetComponent<Image>();
					if (barImage == null) continue;

					// Apply sprite based on config
					barImage.sprite = ConfigManager.ColoredItemDurabilityBar.Value && CustomSprite != null ? CustomSprite : DefaultSprite;
				}
			}
		}


		[HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
		public static class ItemDurability_InventoryGridPatch
		{
			private static void Postfix(InventoryGrid __instance)
			{
				//if (!ConfigManager.ColoredItemDurabilityBar.Value) return;

				var inventory = inventoryField.GetValue(__instance) as Inventory;
				if (inventory == null) return;

				int width = inventory.GetWidth();
				var elements = elementsField.GetValue(__instance) as IList;
				if (elements == null) return;

				foreach (var item in inventory.GetAllItems())
				{
					if (item == null || !item.m_shared.m_useDurability) continue;

					int index = item.m_gridPos.y * width + item.m_gridPos.x;
					if (index < 0 || index >= elements.Count) continue;

					var elemObj = elements[index];
					var durabilityBar = durabilityField.GetValue(elemObj) as GuiBar;
					if (durabilityBar == null) continue;

					// Update color
					if (ConfigManager.ColoredItemDurabilityBar.Value)
					{
						float durabilityPercent = item.GetDurabilityPercentage();
						durabilityBar.SetColor(GetDurabilityColor(durabilityPercent));
					}

					if (CustomSprite == null) continue;
					var barImage = durabilityBar.m_bar.GetComponent<Image>();
					if (barImage == null) continue;

					// Apply sprite based on config
					barImage.sprite = ConfigManager.ColoredItemDurabilityBar.Value && CustomSprite != null ? CustomSprite : DefaultSprite;
				}
			}
		}

		// Shared Helper
		private static Color GetDurabilityColor(float percent)
		{
			if (percent > 0.5f)
				return Color.Lerp(Color.yellow, Color.green, (percent - 0.5f) * 2f);
			else
				return Color.Lerp(Color.red, Color.yellow, percent * 2f);
		}
	}
}
