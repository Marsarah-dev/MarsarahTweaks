using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class EarlyLinenCape
	{
		private static readonly LogManager log = new LogManager("Early Linen Cape", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(ZNetScene), "Awake")]
		class EarlyLinenCape_Patch
		{
			private static void Postfix(ref ZNetScene __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				UpdateLinenCapeStats(false);
			}
		}

		public static void UpdateLinenCapeStats(bool wasChanged)
		{
			log.Info("Updating Linen Cape Stats");
			UpdatePoisonResist(wasChanged);
			RenameLinenCape(wasChanged);
		}

		// Apply Poison Resist for Linen Cape
		private static void UpdatePoisonResist(bool wasChanged)
		{
			GameObject item = ObjectDB.instance.m_items.FirstOrDefault(r => r.name == "CapeLinen");
			if (item == null) return;

			ItemDrop itemDrop = item.GetComponent<ItemDrop>();
			if (itemDrop != null)
			{
				if (ConfigManager.EarlyLinenCapeEnabled.Value)
				{
					log.Info("Setting Linen Cape Poison Resist");
					HitData.DamageModPair damageModPairPoison = new HitData.DamageModPair();
					damageModPairPoison.m_modifier = HitData.DamageModifier.Resistant;
					damageModPairPoison.m_type = HitData.DamageType.Poison;
					if (!itemDrop.m_itemData.m_shared.m_damageModifiers.Contains(damageModPairPoison))
					{
						itemDrop.m_itemData.m_shared.m_damageModifiers.Add(damageModPairPoison);
					}
				}
				else if (wasChanged)
				{
					log.Info("Removing Linen Cape Poison Resist");
					itemDrop.m_itemData.m_shared.m_damageModifiers.RemoveAll(mod => mod.m_type == HitData.DamageType.Poison);
				}
			}
		}

		// Apply new name for Linen Cape
		private static void RenameLinenCape(bool wasChanged)
		{
			log.Info("Renaming Linen Cape");

			var localizationInstance = Localization.instance;

			if (localizationInstance == null)
			{
				return;
			}

			// Access the private field 'm_translations' via reflection
			FieldInfo translationsField = typeof(Localization).GetField("m_translations", BindingFlags.NonPublic | BindingFlags.Instance);
			var translationsDict = translationsField?.GetValue(localizationInstance) as Dictionary<string, string>;

			if (translationsDict != null)
			{
				// Modify the translation
				if (ConfigManager.EarlyLinenCapeEnabled.Value)
				{
					translationsDict["item_cape_linen"] = "Fine Cape";
					translationsDict["item_cape_linen_description"] = "A finely crafted traveler's cape.";
				}
				else if (wasChanged)
				{
					translationsDict["item_cape_linen"] = "Linen Cape";
					translationsDict["item_cape_linen_description"] = "A simple traveler's cape.";
				}
				log.Info("Updated translation!");
			}
			else
			{
				log.Warn("Could not update translation");
			}

			if (Player.m_localPlayer)
			{
				Localization.instance.ReLocalizeAll(Player.m_localPlayer.transform);
			}
		}
	}
}
