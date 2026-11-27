//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Reflection;
//using HarmonyLib;
//using UnityEngine;
//using MarsarahTweaks.Managers;

//namespace MarsarahTweaks.Debugging
//{
//	internal class EquipmentIdentityLogger
//	{
//		private static readonly LogManager log = new LogManager("Equipment Identity", LogManager.LogLevel.Info);
//		private static bool equipmentLogged;

//		private static readonly HashSet<ItemDrop.ItemData.ItemType> equipmentTypes = new HashSet<ItemDrop.ItemData.ItemType>
//		{
//			ItemDrop.ItemData.ItemType.OneHandedWeapon,
//			ItemDrop.ItemData.ItemType.TwoHandedWeapon,
//			ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft,
//			ItemDrop.ItemData.ItemType.Bow,
//			ItemDrop.ItemData.ItemType.Shield,
//			ItemDrop.ItemData.ItemType.Helmet,
//			ItemDrop.ItemData.ItemType.Chest,
//			ItemDrop.ItemData.ItemType.Legs,
//			ItemDrop.ItemData.ItemType.Hands,
//			ItemDrop.ItemData.ItemType.Shoulder,
//			ItemDrop.ItemData.ItemType.Utility,
//			ItemDrop.ItemData.ItemType.Trinket,
//			ItemDrop.ItemData.ItemType.Tool,
//			ItemDrop.ItemData.ItemType.Torch,
//			ItemDrop.ItemData.ItemType.Attach_Atgeir
//		};

//		[HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
//		private static class Player_OnSpawned_Patch
//		{
//			private static void Postfix(Player __instance)
//			{
//				if (equipmentLogged || __instance != Player.m_localPlayer || ObjectDB.instance == null) return;
//				equipmentLogged = true;

//				try
//				{
//					LogEquipment();
//				}
//				catch (Exception ex)
//				{
//					log.Error($"Equipment identity logger failed: {ex}");
//				}
//			}
//		}

//		private static void LogEquipment()
//		{
//			log.Info("=== EQUIPMENT IDENTITY LOGGER START ===", header: true);
//			int count = 0;
//			int errors = 0;

//			var recipes = ObjectDB.instance.m_recipes.Where(x => x != null && x.m_item != null).ToLookup(x => x.m_item.gameObject.name);
//			var items = ObjectDB.instance.m_items.Where(x => x != null).Select(x => x.GetComponent<ItemDrop>()).Where(x => x != null && x.m_itemData != null && x.m_itemData.m_shared != null && equipmentTypes.Contains(x.m_itemData.m_shared.m_itemType)).GroupBy(x => x.gameObject.name).Select(x => x.First()).OrderBy(x => x.gameObject.name);

//			foreach (ItemDrop item in items)
//			{
//				count++;
//				log.Info($"=== ITEM BEGIN: {item.gameObject.name} ===");
//				try
//				{
//					LogItem(item, recipes[item.gameObject.name]);
//				}
//				catch (Exception ex)
//				{
//					errors++;
//					log.Error($"Failed to log {item.gameObject.name}: {ex}");
//				}
//				log.Info($"=== ITEM END: {item.gameObject.name} ===");
//			}

//			log.Info($"Equipment prefabs processed: {count} | Item errors: {errors}");
//			log.Info("=== EQUIPMENT IDENTITY LOGGER END ===", footer: true);
//		}

//		private static void LogItem(ItemDrop item, IEnumerable<Recipe> recipes)
//		{
//			var data = item.m_itemData;
//			var shared = data.m_shared;
//			log.Info($"Name: {Localize(shared.m_name)} | Name token: {shared.m_name}");
//			log.Info($"Description: {Localize(shared.m_description)}");
//			log.Info($"Item type: {shared.m_itemType} | Skill: {shared.m_skillType} | Animation: {shared.m_animationState} | Max quality: {shared.m_maxQuality}");
//			log.Info($"Set: {Clean(shared.m_setName)} | Required pieces: {shared.m_setSize}");
//			log.Info($"Base damage: {FormatValue(shared.m_damages)} | Damage per level: {FormatValue(shared.m_damagesPerLevel)}");
//			log.Info($"Damage modifiers: {FormatValue(shared.m_damageModifiers)}");

//			// Raw scalar fields preserve zero values, defaults and hidden modifiers.
//			LogFields("Shared", shared);

//			bool hasRecipe = false;
//			foreach (Recipe recipe in recipes)
//			{
//				hasRecipe = true;
//				string idols = string.Join(", ", (recipe.m_resources ?? new Piece.Requirement[0]).Where(x => x != null && x.m_resItem != null && x.m_upgraderResource).Select(x => $"{x.m_resItem.gameObject.name} (amount={x.m_amount}, perLevel={x.m_amountPerLevel})"));
//				log.Info($"Recipe: {recipe.name} | Enabled: {recipe.m_enabled} | Upgrade only: {recipe.m_noCraftOnlyUpgrade} | Station: {(recipe.m_craftingStation != null ? recipe.m_craftingStation.name : "None")} | Minimum station level: {recipe.m_minStationLevel} | Idols: {(idols.Length > 0 ? idols : "None")}");
//			}
//			if (!hasRecipe) log.Info("Recipe: None registered");

//			LogStatusEffect("Equip", shared.m_equipStatusEffect);
//			LogStatusEffect("Set", shared.m_setStatusEffect);
//			LogStatusEffect("Attack", shared.m_attackStatusEffect);
//			LogStatusEffect("Perfect block", shared.m_perfectBlockStatusEffect);
//			LogStatusEffect("Full adrenaline", shared.m_fullAdrenalineSE);
//			LogStatusEffect("Consume", shared.m_consumeStatusEffect);

//			var visited = new HashSet<GameObject>();
//			LogAttack("Primary", shared.m_attack, visited);
//			LogAttack("Secondary", shared.m_secondaryAttack, visited);
//			LogRelatedPrefab("Shared spawn on hit", shared.m_spawnOnHit, visited, 0);
//			LogRelatedPrefab("Shared spawn on terrain hit", shared.m_spawnOnHitTerrain, visited, 0);

//			LogTooltip("Q1 tooltip", data, 1);
//			if (shared.m_maxQuality > 1) LogTooltip("Max quality tooltip", data, shared.m_maxQuality);
//			if (shared.m_appendToolTip != null)
//			{
//				log.Info($"Appended tooltip item: {shared.m_appendToolTip.gameObject.name}");
//				LogTooltip("Appended item tooltip", shared.m_appendToolTip.m_itemData, shared.m_maxQuality);
//			}
//		}

//		private static void LogStatusEffect(string label, StatusEffect effect)
//		{
//			if (effect == null)
//			{
//				log.Info($"{label} status effect: None");
//				return;
//			}

//			log.Info($"{label} status effect: {effect.name} | Type: {effect.GetType().Name} | Name: {Localize(effect.m_name)}");
//			LogFields($"{label} effect", effect);
//			try
//			{
//				log.Info($"{label} effect tooltip: {Localize(effect.GetTooltipString())}");
//			}
//			catch (Exception ex)
//			{
//				log.Warn($"{label} effect tooltip unavailable: {ex.Message}");
//			}
//		}

//		private static void LogAttack(string label, Attack attack, HashSet<GameObject> visited)
//		{
//			if (attack == null)
//			{
//				log.Info($"{label} attack: None");
//				return;
//			}

//			LogFields($"{label} attack", attack);
//			LogRelatedPrefab($"{label} projectile", attack.m_attackProjectile, visited, 0);
//			LogRelatedPrefab($"{label} spawn on hit", attack.m_spawnOnHit, visited, 0);
//			LogRelatedPrefab($"{label} spawn on trigger", attack.m_spawnOnTrigger, visited, 0);
//		}

//		private static void LogRelatedPrefab(string label, GameObject prefab, HashSet<GameObject> visited, int depth)
//		{
//			if (prefab == null) return;
//			log.Info($"{label}: {prefab.name}");
//			if (depth >= 4 || !visited.Add(prefab)) return;

//			foreach (Projectile projectile in prefab.GetComponentsInChildren<Projectile>(true))
//			{
//				LogFields($"Projectile {projectile.gameObject.name}", projectile);
//				LogRelatedPrefab("Projectile spawn on hit", projectile.m_spawnOnHit, visited, depth + 1);
//				if (projectile.m_randomSpawnOnHit != null)
//				{
//					foreach (GameObject spawn in projectile.m_randomSpawnOnHit) LogRelatedPrefab("Projectile random spawn", spawn, visited, depth + 1);
//				}
//			}

//			foreach (Aoe aoe in prefab.GetComponentsInChildren<Aoe>(true))
//			{
//				LogFields($"AOE {aoe.gameObject.name}", aoe);
//				LogRelatedPrefab("AOE chain object", aoe.m_chainObj, visited, depth + 1);
//				LogRelatedPrefab("AOE spawn on terrain hit", aoe.m_spawnOnHitTerrain, visited, depth + 1);
//			}
//		}

//		private static void LogTooltip(string label, ItemDrop.ItemData original, int quality)
//		{
//			var copies = new List<StatusEffect>();
//			try
//			{
//				// Vanilla GetTooltip calls SetLevel on shared status effects. Use isolated copies.
//				var data = original.Clone();
//				var shared = new ItemDrop.ItemData.SharedData();
//				foreach (FieldInfo field in typeof(ItemDrop.ItemData.SharedData).GetFields(BindingFlags.Public | BindingFlags.Instance))
//				{
//					object value = field.GetValue(original.m_shared);
//					if (value is StatusEffect effect && effect != null)
//					{
//						StatusEffect copy = UnityEngine.Object.Instantiate(effect);
//						copies.Add(copy);
//						value = copy;
//					}
//					field.SetValue(shared, value);
//				}

//				// Appended tooltips are logged separately to avoid mutating their shared effects.
//				shared.m_appendToolTip = null;
//				data.m_shared = shared;
//				data.m_quality = quality;
//				data.m_worldLevel = 0;
//				data.m_durability = data.GetMaxDurability();
//				log.Info($"{label}: {Localize(ItemDrop.ItemData.GetTooltip(data, quality, false, 0f))}");
//			}
//			catch (Exception ex)
//			{
//				log.Warn($"{label} unavailable: {ex.Message}");
//			}
//			finally
//			{
//				foreach (StatusEffect copy in copies) UnityEngine.Object.Destroy(copy);
//			}
//		}

//		private static void LogFields(string label, object instance)
//		{
//			foreach (FieldInfo field in instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name.StartsWith("m_", StringComparison.Ordinal)).OrderBy(x => x.Name))
//			{
//				Type type = field.FieldType;
//				if (!type.IsPrimitive && !type.IsEnum && type != typeof(string) && type != typeof(Vector2) && type != typeof(Vector3) && type != typeof(HitData.DamageTypes) && type != typeof(List<HitData.DamageModPair>) && type != typeof(GameObject) && !typeof(StatusEffect).IsAssignableFrom(type)) continue;
//				log.Info($"{label} | {field.Name} = {FormatValue(field.GetValue(instance))}");
//			}
//		}

//		private static string FormatValue(object value)
//		{
//			if (value == null) return "None";
//			if (value is UnityEngine.Object unityObject) return unityObject != null ? unityObject.name : "None";
//			if (value is List<HitData.DamageModPair> modifiers) return modifiers.Count > 0 ? string.Join(", ", modifiers.Select(x => $"{x.m_type}: {x.m_modifier}")) : "None";
//			if (value is HitData.DamageTypes damage)
//			{
//				var fields = typeof(HitData.DamageTypes).GetFields(BindingFlags.Public | BindingFlags.Instance).Where(x => x.FieldType == typeof(float));
//				string result = string.Join(", ", fields.Where(x => (float)x.GetValue(damage) != 0f).Select(x => $"{x.Name}: {FormatValue(x.GetValue(damage))}"));
//				return result.Length > 0 ? result : "None";
//			}
//			if (value is IFormattable formattable) return Clean(formattable.ToString(null, CultureInfo.InvariantCulture));
//			return Clean(value.ToString());
//		}

//		private static string Localize(string value)
//		{
//			return Clean(Localization.instance != null ? Localization.instance.Localize(value ?? string.Empty) : value);
//		}

//		private static string Clean(string value)
//		{
//			return (value ?? string.Empty).Replace("\r", "").Replace("\n", "\\n");
//		}
//	}
//}
