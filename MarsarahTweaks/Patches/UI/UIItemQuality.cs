using HarmonyLib;
using Jotunn;
using MarsarahTweaks.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MarsarahTweaks.Managers.ConfigManager;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIItemQuality
	{
		private static readonly LogManager log = new LogManager("UI Item Quality", LogManager.LogLevel.Warning);

		// Reflection cache
		private static readonly FieldInfo inventoryField = null;
		private static readonly FieldInfo elementsField = null;
		private static readonly Type elementType = null;
		private static readonly FieldInfo qualityField = null;
		private static readonly FieldInfo iconField = null;

		static UIItemQuality()
		{
			inventoryField = typeof(InventoryGrid).GetField("m_inventory", BindingFlags.NonPublic | BindingFlags.Instance);
			elementsField = typeof(InventoryGrid).GetField("m_elements", BindingFlags.NonPublic | BindingFlags.Instance);
			elementType = elementsField.FieldType.GetGenericArguments()[0]; // Element type inside List<Element>
			qualityField = elementType.GetField("m_quality", BindingFlags.Public | BindingFlags.Instance);
			iconField = elementType.GetField("m_icon", BindingFlags.Public | BindingFlags.Instance);
		}

		[HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
		public static class ItemQuality_Patch
		{
			private static void Postfix(ref InventoryGrid __instance, ref Player player, ItemDrop.ItemData dragItem)
			{
				if (!ConfigManager.BetterItemQualityIndicator.Value) return;

				var inventory = inventoryField.GetValue(__instance) as Inventory;
				if (inventory == null) return;

				int width = inventory.GetWidth();
				foreach (var item in inventory.GetAllItems())
				{
					int index = item.m_gridPos.y * width + item.m_gridPos.x;
					var elements = elementsField.GetValue(__instance) as IList; // use cached
					if (elements == null || index < 0 || index >= elements.Count)
						continue;

					var elemObj = elements[index];
					var qualityText = qualityField.GetValue(elemObj) as TMP_Text;
					if (qualityText != null && item.m_shared.m_maxQuality > 1)
						DrawSymbols(qualityText, item.m_quality);
				}
			}
		}

		// Helper
		private static void DrawSymbols(TMP_Text textComponent, int quality)
		{
			if (textComponent == null) return;

			bool vertical = ConfigManager.ItemQualityIndicatorVertical.Value;
			textComponent.textWrappingMode = TextWrappingModes.PreserveWhitespaceNoWrap;

			// Resolve symbol based on config
			char symbol = ConfigManager.ItemQualitySymbolChoice.Value switch
			{
				ItemQualitySymbol.Star => '★',
				ItemQualitySymbol.Circle => '●',
				ItemQualitySymbol.Diamond => '◆',
				ItemQualitySymbol.EmptyDiamond => '◇',
				_ => '★'
			};

			// Build symbol text
			string symbolText;
			if (quality >= 5)
				symbolText = $"{quality}x {symbol}";
			else
				symbolText = vertical
					? string.Join("\n", new string(symbol, quality).ToCharArray())
					: new string(symbol, quality);

			// Resolve color based on config
			Color symbolColor = ConfigManager.ItemQualityColorChoice.Value switch
			{
				ItemQualityColor.White => Color.white,
				ItemQualityColor.Yellow => Color.yellow,
				ItemQualityColor.Green => Color.green,
				ItemQualityColor.Red => Color.red,
				ItemQualityColor.Blue => Color.blue,
				ItemQualityColor.Cyan => Color.cyan,
				_ => Color.yellow
			};

			textComponent.text = symbolText;
			textComponent.color = symbolColor;
			textComponent.fontSize = 7f; 
			textComponent.alignment = (!vertical) ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.TopRight;
			textComponent.rectTransform.pivot = (!vertical) ? new Vector2(1f, 0.5f) : new Vector2(1f, 1f);
			textComponent.lineSpacing = (!vertical) ? 0f : -4f;

			// Base dimensions and position
			float charWidth = textComponent.fontSize;
			float baseWidth = charWidth * 2; // 20f;
			float baseHeight = charWidth * 2;
			float height = (!vertical) ? baseHeight : Mathf.Max(quality * charWidth, baseHeight);
			float width = (!vertical) ? Mathf.Max(quality * charWidth, baseWidth) : baseWidth; // Grow width with quality, but enforce minimum
			float offsetX = -2f;
			float offsetY = (!vertical) ? -9f : 0f;

			textComponent.rectTransform.sizeDelta = new Vector2(width, height);
			textComponent.rectTransform.anchoredPosition = new Vector2(offsetX, offsetY);

			// Remove Outline component if present
			var outline = textComponent.GetComponent<Outline>();
			if (outline != null)
				UnityEngine.Object.Destroy(outline);
		}
	}
}
