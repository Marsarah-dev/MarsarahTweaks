//using System;
//using System.Collections.Generic;
//using System.Linq;
//using HarmonyLib;
//using UnityEngine;
//using MarsarahTweaks.Managers;

//namespace MarsarahTweaks.Debugging
//{
//	internal class ForgeOfPotentialLogger
//	{
//		private static readonly LogManager log = new LogManager("Forge of Potential", LogManager.LogLevel.Info);

//		[HarmonyPatch(typeof(Game), "Start")]
//		private static class Game_Start_Patch
//		{
//			private static void Postfix()
//			{
//				try
//				{
//					LogForgeOfPotential();
//				}
//				catch (Exception ex)
//				{
//					log.Error($"Failed to log Forge of Potential data: {ex}");
//				}
//			}
//		}

//		private static void LogForgeOfPotential()
//		{
//			if (ObjectDB.instance == null || ZNetScene.instance == null)
//			{
//				log.Warn("Cannot log Forge of Potential data because ObjectDB or ZNetScene is unavailable.");
//				return;
//			}

//			log.Info("=== FORGE OF POTENTIAL LOGGER START ===", header: true);

//			Dictionary<string, ItemDrop> idols = GetIdols();

//			LogIdolStats(idols);
//			LogEquipmentMappings(idols);
//			LogDropSources(idols);

//			log.Info("=== FORGE OF POTENTIAL LOGGER END ===", footer: true);
//		}

//		private static Dictionary<string, ItemDrop> GetIdols()
//		{
//			Dictionary<string, ItemDrop> idols = new Dictionary<string, ItemDrop>();

//			foreach (Recipe recipe in ObjectDB.instance.m_recipes)
//			{
//				if (recipe == null || recipe.m_resources == null) continue;

//				foreach (Piece.Requirement requirement in recipe.m_resources)
//				{
//					if (requirement == null || requirement.m_resItem == null || !requirement.m_upgraderResource) continue;

//					ItemDrop idol = requirement.m_resItem;
//					idols[idol.gameObject.name] = idol;
//				}
//			}

//			foreach (GameObject prefab in ObjectDB.instance.m_items)
//			{
//				if (prefab == null || !prefab.name.StartsWith("Upgrader", StringComparison.OrdinalIgnoreCase)) continue;

//				ItemDrop idol = prefab.GetComponent<ItemDrop>();

//				if (idol != null)
//				{
//					idols[prefab.name] = idol;
//				}
//			}

//			return idols;
//		}

//		private static void LogIdolStats(Dictionary<string, ItemDrop> idols)
//		{
//			log.Info("=== IDOL PROPERTIES ===", header: true);
//			log.Info($"Total unique idols: {idols.Count}");

//			foreach (var entry in idols.OrderBy(x => x.Key))
//			{
//				var shared = entry.Value.m_itemData.m_shared;

//				float successChance = Mathf.Clamp01(shared.m_upgradeChance);
//				float breakChance = Mathf.Min(Mathf.Clamp01(shared.m_breakChance), 1f - successChance);
//				float downgradeChance = Mathf.Max(0f, 1f - successChance - breakChance);

//				log.Info($"Idol: {entry.Key}");
//				log.Info($"   Name: {shared.m_name}");
//				log.Info($"   Success chance: {successChance:P2}");
//				log.Info($"   Downgrade chance: {downgradeChance:P2}");
//				log.Info($"   Destruction chance: {breakChance:P2}");
//				log.Info($"   Raw success chance: {shared.m_upgradeChance:P2}");
//				log.Info($"   Raw break chance: {shared.m_breakChance:P2}");
//				log.Info($"   Material refund: {shared.m_breakReturnIngreientsAmount:P2}");
//				log.Info($"   Success upgrade steps: {shared.m_successUpgradeSteps}");
//				log.Info($"   Maximum stack: {shared.m_maxStackSize}");
//			}
//		}

//		private static void LogEquipmentMappings(Dictionary<string, ItemDrop> idols)
//		{
//			log.Info("=== EQUIPMENT TO IDOL MAPPINGS ===", header: true);

//			foreach (var idol in idols.OrderBy(x => x.Key))
//			{
//				int equipmentCount = 0;

//				log.Info($"Idol: {idol.Key}");

//				foreach (Recipe recipe in ObjectDB.instance.m_recipes.OrderBy(x => x == null ? string.Empty : x.name))
//				{
//					if (recipe == null || recipe.m_item == null || recipe.m_resources == null) continue;

//					Piece.Requirement requirement = recipe.m_resources.FirstOrDefault(x => x != null && x.m_resItem != null && x.m_upgraderResource && x.m_resItem.gameObject.name == idol.Key);

//					if (requirement == null) continue;

//					var shared = recipe.m_item.m_itemData.m_shared;

//					equipmentCount++;

//					log.Info($"   Equipment: {recipe.m_item.gameObject.name} | Name: {shared.m_name}");
//					log.Info($"      Recipe: {recipe.name}");
//					log.Info($"      Item type: {shared.m_itemType}");
//					log.Info($"      Normal maximum quality: {shared.m_maxQuality}");
//					log.Info($"      Idol amount: {requirement.m_amount}");
//					log.Info($"      Idol amount per level: {requirement.m_amountPerLevel}");
//					log.Info($"      Idol recoverable: {requirement.m_recover}");

//					log.Info($"      Base damage: {shared.m_damages}");
//					log.Info($"      Damage per level: {shared.m_damagesPerLevel}");
//					log.Info($"      Base armor: {shared.m_armor}");
//					log.Info($"      Armor per level: {shared.m_armorPerLevel}");
//					log.Info($"      Base block power: {shared.m_blockPower}");
//					log.Info($"      Block power per level: {shared.m_blockPowerPerLevel}");
//					log.Info($"      Base durability: {shared.m_maxDurability}");
//					log.Info($"      Durability per level: {shared.m_durabilityPerLevel}");

//					foreach (Piece.Requirement resource in recipe.m_resources)
//					{
//						if (resource == null || resource.m_resItem == null || resource.m_upgraderResource) continue;

//						log.Info($"      Resource: {resource.m_resItem.gameObject.name} | Base amount: {resource.m_amount} | Amount per level: {resource.m_amountPerLevel} | Recoverable: {resource.m_recover}");
//					}
//				}

//				log.Info($"   Total mapped recipes: {equipmentCount}");
//			}
//		}

//		private static void LogDropSources(Dictionary<string, ItemDrop> idols)
//		{
//			log.Info("=== IDOL DROP SOURCES ===", header: true);

//			HashSet<string> foundIdols = new HashSet<string>();

//			foreach (GameObject prefab in ZNetScene.instance.m_prefabs.Concat(ZNetScene.instance.m_nonNetViewPrefabs).Where(x => x != null).Distinct().OrderBy(x => x.name))
//			{
//				foreach (CharacterDrop characterDrop in prefab.GetComponentsInChildren<CharacterDrop>(true))
//				{
//					if (characterDrop.m_drops == null) continue;

//					foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
//					{
//						if (drop == null || drop.m_prefab == null || !idols.ContainsKey(drop.m_prefab.name)) continue;

//						foundIdols.Add(drop.m_prefab.name);

//						log.Info($"Creature: {prefab.name} | Component object: {characterDrop.gameObject.name} | Idol: {drop.m_prefab.name}");
//						log.Info($"   Base drop chance: {drop.m_chance:P2}");
//						log.Info($"   Amount: {drop.m_amountMin}-{drop.m_amountMax}");
//						log.Info($"   One per player: {drop.m_onePerPlayer}");
//						log.Info($"   Level multiplier: {drop.m_levelMultiplier}");
//						log.Info($"   Level multiplier not amount: {drop.m_levelMultiplierNotAmount}");
//						log.Info($"   Ignore resource scaling: {drop.m_dontScale}");
//						log.Info($"   Eligible for pseudo drops at base chance: {drop.m_chance > 0f && drop.m_chance <= 0.3f}");
//					}
//				}

//				foreach (Container component in prefab.GetComponentsInChildren<Container>(true))
//				{
//					LogDropTable(prefab.name, $"Container/{component.gameObject.name}", component.m_defaultItems, idols, foundIdols);
//				}

//				foreach (LootSpawner component in prefab.GetComponentsInChildren<LootSpawner>(true))
//				{
//					LogDropTable(prefab.name, $"LootSpawner/{component.gameObject.name}", component.m_items, idols, foundIdols);
//				}

//				foreach (DropOnDestroyed component in prefab.GetComponentsInChildren<DropOnDestroyed>(true))
//				{
//					LogDropTable(prefab.name, $"DropOnDestroyed/{component.gameObject.name}", component.m_dropWhenDestroyed, idols, foundIdols);
//				}

//				foreach (MineRock component in prefab.GetComponentsInChildren<MineRock>(true))
//				{
//					LogDropTable(prefab.name, $"MineRock/{component.gameObject.name}", component.m_dropItems, idols, foundIdols);
//				}

//				foreach (MineRock5 component in prefab.GetComponentsInChildren<MineRock5>(true))
//				{
//					LogDropTable(prefab.name, $"MineRock5/{component.gameObject.name}", component.m_dropItems, idols, foundIdols);
//				}

//				foreach (Pickable component in prefab.GetComponentsInChildren<Pickable>(true))
//				{
//					if (component.m_itemPrefab != null && idols.ContainsKey(component.m_itemPrefab.name))
//					{
//						foundIdols.Add(component.m_itemPrefab.name);

//						log.Info($"Pickable: {prefab.name} | Idol: {component.m_itemPrefab.name} | Amount: {component.m_amount}");
//					}

//					LogDropTable(prefab.name, $"Pickable/{component.gameObject.name}", component.m_extraDrops, idols, foundIdols);
//				}

//				foreach (TreeBase component in prefab.GetComponentsInChildren<TreeBase>(true))
//				{
//					LogDropTable(prefab.name, $"TreeBase/{component.gameObject.name}", component.m_dropWhenDestroyed, idols, foundIdols);
//				}

//				foreach (TreeLog component in prefab.GetComponentsInChildren<TreeLog>(true))
//				{
//					LogDropTable(prefab.name, $"TreeLog/{component.gameObject.name}", component.m_dropWhenDestroyed, idols, foundIdols);
//				}

//				foreach (Fish component in prefab.GetComponentsInChildren<Fish>(true))
//				{
//					LogDropTable(prefab.name, $"Fish/{component.gameObject.name}", component.m_extraDrops, idols, foundIdols);
//				}
//			}

//			log.Info("=== IDOLS WITHOUT IDENTIFIED DROP SOURCES ===", header: true);

//			foreach (string idol in idols.Keys.OrderBy(x => x))
//			{
//				if (!foundIdols.Contains(idol))
//				{
//					log.Info($"No registered prefab drop source found: {idol}");
//				}
//			}
//		}

//		private static void LogDropTable(string prefabName, string sourceType, DropTable table, Dictionary<string, ItemDrop> idols, HashSet<string> foundIdols)
//		{
//			if (table == null || table.m_drops == null || table.m_drops.Count == 0) return;

//			float totalWeight = table.m_drops.Sum(x => x.m_weight);

//			foreach (DropTable.DropData drop in table.m_drops)
//			{
//				if (drop.m_item == null || !idols.ContainsKey(drop.m_item.name)) continue;

//				foundIdols.Add(drop.m_item.name);

//				float weightShare = totalWeight > 0f ? drop.m_weight / totalWeight : 0f;

//				log.Info($"Source: {prefabName} | Type: {sourceType} | Idol: {drop.m_item.name}");
//				log.Info($"   Table activation chance: {table.m_dropChance:P2}");
//				log.Info($"   Table drop count: {table.m_dropMin}-{table.m_dropMax}");
//				log.Info($"   One of each: {table.m_oneOfEach}");
//				log.Info($"   Item weight: {drop.m_weight}");
//				log.Info($"   Total table weight: {totalWeight}");
//				log.Info($"   Initial weight share: {weightShare:P2}");
//				log.Info($"   Stack amount: {drop.m_stackMin}-{drop.m_stackMax}");
//				log.Info($"   Ignore resource scaling: {drop.m_dontScale}");
//			}
//		}
//	}
//}