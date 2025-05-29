using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches.Features
{
	internal static class MoreUsableFuel
	{

		[HarmonyPatch(typeof(Smelter), "Awake")]
		public static class AddAncientBarkToKiln_Patch
		{
			private static void Postfix(Smelter __instance)
			{
				//if (!ConfigManager.MoreUsableFuelEnabled.Value) return;
				UpdateKiln(__instance);
			}
		}

		[HarmonyPatch(typeof(ShieldGenerator), "Start")]
		public static class AddWitheredBonesToShieldGenerator_Patch
		{
			private static void Postfix(ShieldGenerator __instance)
			{
				//if (!ConfigManager.MoreUsableFuelEnabled.Value) return;
				UpdateShieldGenerator(__instance);
			}
		}

		public static void UpdateMoreUsableFuel()
		{
			//if (!ConfigManager.MoreUsableFuelEnabled.Value) return;

			UpdateAllKilns();
			UpdateAllShieldGenerators();
		}

		private static void UpdateAllKilns()
		{
			foreach (var kiln in UnityEngine.Object.FindObjectsOfType<Smelter>())
			{
				UpdateKiln(kiln);
			}
		}

		private static void UpdateKiln(Smelter kiln)
		{
			if (!kiln.m_conversion.Exists(conv => conv.m_to.name == "Coal")) return;

			//if (kiln.m_conversion.Any(conv => conv.m_from.name == "ElderBark")) return;

			var barkPrefab = ObjectDB.instance?.GetItemPrefab("ElderBark");
			var coalPrefab = ObjectDB.instance?.GetItemPrefab("Coal");

			if (!barkPrefab || !coalPrefab)
			{
				//MarsarahTweaks.LogInfo("Missing prefab: AncientBark or Coal.");
				return;
			}

			var barkDrop = barkPrefab.GetComponent<ItemDrop>();
			var coalDrop = coalPrefab.GetComponent<ItemDrop>();

			var witheredBoneConversion = kiln.m_conversion.FirstOrDefault(conv =>
				conv.m_from == barkDrop && conv.m_to == coalDrop);

			if (ConfigManager.MoreUsableFuelEnabled.Value)
			{
				if (witheredBoneConversion == null)
				{
					var newWitheredBoneConversion = new Smelter.ItemConversion
					{
						m_from = barkDrop,
						m_to = coalDrop
					};
					kiln.m_conversion.Add(newWitheredBoneConversion);
					//MarsarahTweaks.LogInfo("Added Ancient Bark as fuel for Kiln.");
				}
			}
			else
			{
				if (witheredBoneConversion != null)
				{
					kiln.m_conversion.Remove(witheredBoneConversion);
					//MarsarahTweaks.LogInfo("Removed Ancient Bark as fuel from Kiln.");
				}
			}
		}

		private static void UpdateAllShieldGenerators()
		{
			foreach (var sg in UnityEngine.Object.FindObjectsOfType<ShieldGenerator>())
			{
				UpdateShieldGenerator(sg);
			}
		}

		private static void UpdateShieldGenerator(ShieldGenerator sg)
		{
			var prefab = ObjectDB.instance?.GetItemPrefab("WitheredBone");
			if (!prefab)
			{
				//MarsarahTweaks.LogInfo("WitheredBone prefab not found.");
				return;
			}

			var itemDrop = prefab.GetComponent<ItemDrop>();
			if (!itemDrop)
			{
				//MarsarahTweaks.LogInfo("WitheredBone does not have ItemDrop component.");
				return;
			}

			if (ConfigManager.MoreUsableFuelEnabled.Value)
			{
				if (!sg.m_fuelItems.Contains(itemDrop))
				{
					sg.m_fuelItems.Add(itemDrop);
					//MarsarahTweaks.LogInfo("Added WitheredBone as fuel for Shield Generator.");
				}
			}
			else
			{
				if (sg.m_fuelItems.Contains(itemDrop))
				{
					sg.m_fuelItems.Remove(itemDrop);
					//MarsarahTweaks.LogInfo("Removed WitheredBone as fuel from Shield Generator.");
				}
			}			
		}
	}
}

