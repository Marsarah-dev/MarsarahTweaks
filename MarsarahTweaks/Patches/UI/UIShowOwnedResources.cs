using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.UI
{
	internal class UIShowOwnedResources
	{
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement))]
		class ShowOwnedResources_Patch
		{
			static void Postfix(Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality, int craftMultiplier)
			{
				if (!ConfigManager.ShowOwnedResources.Value) return;
				if (req?.m_resItem == null || player == null) return;

				int requiredAmount = req.GetAmount(quality) * craftMultiplier;
				int ownedAmount = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);

				var resAmountTransform = elementRoot.Find("res_amount");
				if (resAmountTransform == null)
					return;

				var resAmountText = resAmountTransform.GetComponent<TMP_Text>();
				if (resAmountText == null)
					return;

				resAmountText.text = $"{requiredAmount}/{ownedAmount}";

				bool noCost = (!craft && ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoBuildCost)) || (craft && ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost));

				if (ownedAmount < requiredAmount && !noCost)
				{
					resAmountText.color = Mathf.Sin(Time.time * 10f) > 0f ? Color.red : Color.white;
				}
				else
				{
					resAmountText.color = Color.white;
				}

			}
		}
	}
}
