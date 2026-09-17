using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class SkeletonSharedData : MonoBehaviour
	{
		public bool IsSecondaryAttack { get; set; }
		public int WeaponLevel { get; set; }
	}
	internal static class ModState
	{
		public static bool LastAttackWasSecondary { get; set; }
		public static GameObject LastSpawnedSkeleton;
		public static int LastWeaponLevel { get; set; }
	}


	internal class DeathRaiserChanges
	{
		private static readonly LogManager log = new LogManager("Death Raiser", LogManager.LogLevel.Warning);

		private const string SkeletonSecondaryAttackZdoKey = "MarsarahTweaks_SkeletonSecondaryAttack";
		private const string SkeletonWeaponLevelZdoKey = "MarsarahTweaks_SkeletonWeaponLevel";

		private static bool TryGetPersistedSkeletonData(GameObject skeleton, out bool isSecondaryAttack, out int weaponLevel)
		{
			isSecondaryAttack = false;
			weaponLevel = 0;

			ZNetView nview = skeleton.GetComponent<ZNetView>();
			if (nview == null || !nview.IsValid()) return false;

			ZDO zdo = nview.GetZDO();
			if (zdo == null) return false;

			weaponLevel = zdo.GetInt(SkeletonWeaponLevelZdoKey, 0);
			if (weaponLevel <= 0) return false;

			isSecondaryAttack = zdo.GetBool(SkeletonSecondaryAttackZdoKey, false);
			return true;
		}

		private static void SaveSkeletonData(GameObject skeleton, bool isSecondaryAttack, int weaponLevel)
		{
			if (weaponLevel <= 0) return;

			ZNetView nview = skeleton.GetComponent<ZNetView>();
			if (nview == null || !nview.IsValid()) return;

			ZDO zdo = nview.GetZDO();
			if (zdo == null) return;

			zdo.Set(SkeletonSecondaryAttackZdoKey, isSecondaryAttack);
			zdo.Set(SkeletonWeaponLevelZdoKey, weaponLevel);
		}

		// Modify Skeleton Summons
		[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.StartAttack))]
		static class DeathRaiserAttack_Patch
		{
			private static float lastAttackTime = 0f;
			private static Attack originalSecondaryAttack = null; // Store default attack

			private static void Prefix(Humanoid __instance, bool secondaryAttack)
			{
				// Check if this is a dedicated server
				if (ZNet.instance == null || ZNet.instance.IsDedicated()) return;

				// Get current weapon
				ItemDrop.ItemData currentWeapon = __instance.GetCurrentWeapon();
				if (currentWeapon == null || currentWeapon.m_shared?.m_name != "$item_staffskeleton") return;

				// If the config is disabled, restore the original secondary attack
				if (!ConfigManager.BetterDeathRaiserEnabled.Value)
				{
					if (originalSecondaryAttack != null)
					{
						currentWeapon.m_shared.m_secondaryAttack = originalSecondaryAttack;
					}
					return;
				}

				// Store original attack if not already stored
				if (originalSecondaryAttack == null && currentWeapon.m_shared.m_secondaryAttack != null)
				{
					originalSecondaryAttack = currentWeapon.m_shared.m_secondaryAttack;
				}

				// Prevent spam clicking
				float timeSinceLastAttack = Time.time - lastAttackTime;
				if (timeSinceLastAttack < 0.5f) return;
				lastAttackTime = Time.time;

				// Set weapon info
				ModState.LastAttackWasSecondary = secondaryAttack;
				ModState.LastWeaponLevel = currentWeapon.m_quality;

				// Ensure secondary attack mirrors the primary attack when using secondary
				if (secondaryAttack && currentWeapon.m_shared.m_secondaryAttack != currentWeapon.m_shared.m_attack)
				{
					currentWeapon.m_shared.m_secondaryAttack = currentWeapon.m_shared.m_attack;
				}
			}
		}

		[HarmonyPatch(typeof(UnityEngine.Object), "Instantiate", new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
		public static class SkeletonFriendlyInstantiate_Patch
		{
			static void Postfix(UnityEngine.Object __result)
			{
				if (__result == null)
				{
					log.Error("Instantiated object is null!");
					return;
				}

				if (ZNet.instance == null || ZNet.instance.IsDedicated()) return;

				if (!ConfigManager.BetterDeathRaiserEnabled.Value) return;

				if (__result is GameObject gameObject && gameObject.name.Contains("Skeleton_Friendly"))
				{
					ModState.LastSpawnedSkeleton = gameObject;

					SkeletonSharedData skeletonShared = gameObject.GetComponent<SkeletonSharedData>();
					if (skeletonShared == null)
					{
						skeletonShared = gameObject.AddComponent<SkeletonSharedData>();
					}

					// Existing skeleton loaded from the world: restore its original summon data.
					if (TryGetPersistedSkeletonData(gameObject, out bool savedSecondaryAttack, out int savedWeaponLevel))
					{
						skeletonShared.IsSecondaryAttack = savedSecondaryAttack;
						skeletonShared.WeaponLevel = savedWeaponLevel;

						log.Info($"Restored skeleton data: Secondary Attack = {savedSecondaryAttack}, Weapon Level = {savedWeaponLevel}");
						return;
					}

					// Newly summoned skeleton: use the Death Raiser attack that just created it.
					skeletonShared.IsSecondaryAttack = ModState.LastAttackWasSecondary;
					skeletonShared.WeaponLevel = ModState.LastWeaponLevel;

					if (skeletonShared.WeaponLevel > 0)
					{
						SaveSkeletonData(gameObject, skeletonShared.IsSecondaryAttack, skeletonShared.WeaponLevel);

						log.Info($"Saved skeleton data: Secondary Attack = {skeletonShared.IsSecondaryAttack}, Weapon Level = {skeletonShared.WeaponLevel}");
					}
					else
					{
						log.Info($"No valid summon data available for {gameObject.name}");
					}
				}
			}
		}

		[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GiveDefaultItems))]
		public static class GiveDefaultItems_Patch
		{
			private static void Prefix(Humanoid __instance, ref bool __runOriginal)
			{
				if (__instance == null || __instance.name == null)
				{
					__runOriginal = true;
					return;
				}

				if (ZNet.instance == null || ZNet.instance.IsDedicated())
				{
					__runOriginal = true; // Run the original on a dedicated server
					return;
				}

				// Prevent modification if feature is disabled
				if (!ConfigManager.BetterDeathRaiserEnabled.Value)
				{
					__runOriginal = true;
					return;
				}

				// Check if this is a friendly skeleton
				if (__instance.name.Contains("Skeleton_Friendly"))
				{
					// Check if it has a custom attack type assigned or a weapon level
					var skeletonSharedComponent = __instance.GetComponent<SkeletonSharedData>();

					bool isSecondaryAttack;
					int weaponLevel;

					if (TryGetPersistedSkeletonData(__instance.gameObject, out bool savedSecondaryAttack, out int savedWeaponLevel))
					{
						isSecondaryAttack = savedSecondaryAttack;
						weaponLevel = savedWeaponLevel;

						if (skeletonSharedComponent != null)
						{
							skeletonSharedComponent.IsSecondaryAttack = isSecondaryAttack;
							skeletonSharedComponent.WeaponLevel = weaponLevel;
						}
					}
					else if (skeletonSharedComponent != null && skeletonSharedComponent.WeaponLevel > 0)
					{
						isSecondaryAttack = skeletonSharedComponent.IsSecondaryAttack;
						weaponLevel = skeletonSharedComponent.WeaponLevel;

						SaveSkeletonData(__instance.gameObject, isSecondaryAttack, weaponLevel);
					}
					else
					{
						log.Info($"No valid summon data found for {__instance.name}. Falling back to vanilla equipment.");
						__runOriginal = true;
						return;
					}

					int usedWeaponLevel = Mathf.Min(weaponLevel, 4);
					log.Info($"Used Weapon Level: {usedWeaponLevel}");

					if (ConfigManager.BetterDeathRaiserSummonsEnabled.Value)
					{
						if (!SummonedSkeletonChanges.newSkeletonGear.TryGetValue(usedWeaponLevel, out var gearForLevel))
						{
							log.Warn($"No summoned skeleton gear configured for weapon level {usedWeaponLevel}. Falling back to vanilla equipment.");
							__runOriginal = true;
							return;
						}

						// Get original skeleton weapon data
						ItemDrop originalSkeletonWeaponData = null;

						string originalWeaponPrefabName = isSecondaryAttack ? "skeleton_bow2" : "skeleton_sword2";
						var originalWeaponPrefab = ObjectDB.instance.GetItemPrefab(originalWeaponPrefabName);
						if (originalWeaponPrefab != null)
						{
							originalSkeletonWeaponData = originalWeaponPrefab.GetComponent<ItemDrop>();
						}

						// Apply new fancy gear
						string skeletonType = isSecondaryAttack ? "Ranged" : "Melee";
						if (gearForLevel.TryGetValue(skeletonType, out var gear))
						{
							string weaponPrefabName = !string.IsNullOrEmpty(gear.weapon2) && UnityEngine.Random.value > 0.5f ? gear.weapon2 : gear.weapon1;
							string shieldPrefabName = !string.IsNullOrEmpty(gear.shield2) && UnityEngine.Random.value > 0.5f ? gear.shield2 : gear.shield1;

							log.Info($"Selected Weapon: {weaponPrefabName}");
							log.Info($"Selected Shield: {shieldPrefabName}");

							var weaponPrefab = ObjectDB.instance.GetItemPrefab(weaponPrefabName);
							GameObject shieldPrefab = null;
							if (shieldPrefabName != null)
							{
								shieldPrefab = ObjectDB.instance.GetItemPrefab(shieldPrefabName);
							}
							var chestPrefab = ObjectDB.instance.GetItemPrefab(gear.chest);
							var legsPrefab = ObjectDB.instance.GetItemPrefab(gear.legs);
							var capePrefab = ObjectDB.instance.GetItemPrefab(gear.cape);

							if (weaponPrefab != null)
							{
								log.Info($"Weapon Prefab: {weaponPrefab}");
								giveItem(__instance, weaponPrefab, originalSkeletonWeaponData);
							}
							if (shieldPrefab != null && skeletonType == "Melee")
							{
								log.Info($"Shield Prefab: {shieldPrefab}");
								giveItem(__instance, shieldPrefab);
							}
							if (chestPrefab != null)
							{
								log.Info($"Chest Prefab: {chestPrefab}");
								giveItem(__instance, chestPrefab);
							}
							if (legsPrefab != null)
							{
								log.Info($"Legs Prefab: {legsPrefab}");
								giveItem(__instance, legsPrefab);
							}
							if (capePrefab != null)
							{
								log.Info($"Cape Prefab: {capePrefab}");
								giveItem(__instance, capePrefab);
							}								
						}
					}
					else
					{
						// Assign new weapon based on attack type
						string weaponPrefabName = isSecondaryAttack ? "skeleton_bow2" : "skeleton_sword2";
						var weaponPrefab = ObjectDB.instance.GetItemPrefab(weaponPrefabName);

						if (weaponPrefab != null)
						{
							giveItem(__instance, weaponPrefab);
							log.Info($"Assigned {weaponPrefabName} to Skeleton_Friendly (Secondary Attack: {isSecondaryAttack})");

							// Only melee skeletons get shields
							if (!isSecondaryAttack)
							{
								AssignShield(__instance);
							}
						}
					}

					__runOriginal = false;
					return;
				}

				__runOriginal = true;
				return;
			}

			private static void AssignShield(Humanoid skeleton)
			{
				int roll = UnityEngine.Random.Range(0, 3); // 0 = No shield, 1 = Wood, 2 = Bronze
				string shieldPrefabName = roll == 1 ? "ShieldWood" : roll == 2 ? "ShieldBronzeBuckler" : null;

				if (shieldPrefabName != null)
				{
					var shieldPrefab = ObjectDB.instance.GetItemPrefab(shieldPrefabName);
					if (shieldPrefab != null)
					{
						giveItem(skeleton, shieldPrefab);
						log.Info($"Assigned {shieldPrefabName} to Skeleton_Friendly");
					}
				}
			}

			private static void giveItem(Humanoid human, GameObject prefab, ItemDrop originalWeaponPrefab = null)
			{
				ItemDrop.ItemData itemData = human.PickupPrefab(prefab, 0, autoequip: false);

				if (itemData != null)
				{
					if (originalWeaponPrefab != null)
					{
						itemData.m_shared = originalWeaponPrefab.m_itemData.m_shared;
					}
					if (!itemData.IsWeapon())
					{
						human.EquipItem(itemData, triggerEquipEffects: false);
					}
				}
			}
		}
	}

	internal class SummonedSkeletonChanges
	{
		[HarmonyPatch(typeof(Humanoid), "Awake")]
		class BetterDeathRaiserSkeletonStatsAndRatio_Patch
		{
			static void Prefix(Humanoid __instance, ref GameObject[] ___m_randomWeapon, ref GameObject[] ___m_randomShield)
			{
				// Run on both client and server - no conditions placed

				if (__instance && __instance.name.StartsWith("Skeleton_Friendly"))
				{
					if (ConfigManager.BetterDeathRaiserSummonsEnabled.Value)
					{
						// Skeleton speed
						__instance.m_speed = 4; // 1
						__instance.m_runSpeed = 8; // 4
					}
				}
			}
		}

		/*
		 * bronze/troll
		 * iron/root
		 * silver/fenris
		 * padded
		 * carapace
		 */

		// Dictionary for new skeleton gear
		internal static Dictionary<int, Dictionary<string, (string weapon1, string weapon2, string shield1, string shield2, string chest, string legs, string cape)>> newSkeletonGear 
			= new Dictionary<int, Dictionary<string, (string weapon1, string weapon2, string shield1, string shield2, string chest, string legs, string cape)>>()
		{
			{
				1, new Dictionary<string, (string weapon1, string weapon2, string shield1, string shield2, string chest, string legs, string cape)>
				{
					{ "Melee", ("SwordBronze", "MaceBronze", "ShieldWood", "ShieldBronzeBuckler", "ArmorBronzeChest", "ArmorBronzeLegs", "CapeDeerHide") },
					{ "Ranged", ("BowFineWood", null, null, null, "ArmorTrollLeatherChest", "ArmorTrollLeatherLegs", "CapeTrollHide") },
				}
			},
			{
				2, new Dictionary<string, (string weapon1, string weapon2, string shield1, string shield2, string chest, string legs, string cape)>
				{
					{ "Melee", ("SwordIron", "MaceIron", "ShieldIronBuckler", "ShieldBanded", "ArmorIronChest", "ArmorIronLegs", "CapeLinen") },
					{ "Ranged", ("BowHuntsman", null, null, null, "ArmorRootChest", "ArmorRootLegs", "CapeDeerHide") },
				}
			},
			{
				3, new Dictionary<string, (string weapon1, string weapon2, string shield1, string shield2, string chest, string legs, string cape)>
				{
					{ "Melee", ("SwordSilver", "MaceSilver", "ShieldSilver", "ShieldSerpentscale", "ArmorWolfChest", "ArmorWolfLegs", "CapeWolf") },
					{ "Ranged", ("BowDraugrFang", null, null, null, "ArmorFenringChest", "ArmorFenringLegs", "CapeLinen") },
				}
			},
				{
				4, new Dictionary<string, (string weapon1, string weapon2, string shield1, string shield2, string chest, string legs, string cape)>
				{
					{ "Melee", ("SwordBlackmetal", "MaceNeedle", "ShieldBlackmetal", "ShieldCarapace", "ArmorCarapaceChest", "ArmorCarapaceLegs", "CapeLox") },
					{ "Ranged", ("BowDraugrFang", "BowSpineSnap", null, null, "ArmorPaddedCuirass", "ArmorPaddedGreaves", "CapeFeather") },
				}
			}
		};
	}
}
