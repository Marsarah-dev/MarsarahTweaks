using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MarsarahTweaks.Patches
{
	public class SkeletonAttackType : MonoBehaviour
	{
		public bool IsSecondaryAttack { get; set; }
	}
	public static class ModState
	{
		public static bool LastAttackWasSecondary { get; set; }
		public static GameObject LastSpawnedSkeleton;
	}


	internal class DeathRaiserChanges
	{
		/*[HarmonyPatch(typeof(ObjectDB), "Awake")]
		class BetterDeathRaiserHPPercent_Patch
		{
			static void Prefix(ref ObjectDB __instance)
			{
				if (__instance == null) return;

				if (ZNet.instance != null)
				{
					bool isDedicatedServer = ZNet.instance.IsDedicated();
					if (!isDedicatedServer)
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: Updating {ConfigManager.Configs.DeathRaiserModifications.Name}...");
						//UpdateDeathRaiser(__instance, false);
						GameObject item = __instance.m_items.FirstOrDefault(r => r.name == "StaffSkeleton");
						if (item == null) return;

						ItemDrop itemDrop = item.GetComponent<ItemDrop>();
						if (itemDrop != null)
						{
							if (ConfigManager.betterDeathRaiserEnabled.Value)
							{
								if (itemDrop.m_itemData.m_shared.m_secondaryAttack != itemDrop.m_itemData.m_shared.m_attack)
								{
									//itemDrop.m_itemData.m_shared.m_secondaryAttack = itemDrop.m_itemData.m_shared.m_attack;

									Attack primaryAttack = itemDrop.m_itemData.m_shared.m_attack;
									Attack secondaryAttack = new Attack
									{
										// Core values
										m_attackType = primaryAttack.m_attackType,
										m_attackAnimation = primaryAttack.m_attackAnimation,
										m_attackChainLevels = primaryAttack.m_attackChainLevels,
										m_attackRange = primaryAttack.m_attackRange,
										m_attackAngle = primaryAttack.m_attackAngle,

										// HP & Stamina Usage
										m_attackHealthPercentage = 50, // Original 40
										m_attackHealth = primaryAttack.m_attackHealth,
										m_attackHealthReturnHit = primaryAttack.m_attackHealthReturnHit,
										m_attackStamina = primaryAttack.m_attackStamina,

										// Eitr Consumption
										m_attackEitr = primaryAttack.m_attackEitr, // 100
										m_reloadEitrDrain = primaryAttack.m_reloadEitrDrain,
										m_drawEitrDrain = primaryAttack.m_drawEitrDrain,

										// Damage & multipliers
										m_damageMultiplier = primaryAttack.m_damageMultiplier,
										m_damageMultiplierPerMissingHP = primaryAttack.m_damageMultiplierPerMissingHP,
										m_damageMultiplierByTotalHealthMissing = primaryAttack.m_damageMultiplierByTotalHealthMissing,
										m_forceMultiplier = primaryAttack.m_forceMultiplier,
										m_staggerMultiplier = primaryAttack.m_staggerMultiplier,

										// Projectile
										m_attackProjectile = primaryAttack.m_attackProjectile,
										m_projectileVel = primaryAttack.m_projectileVel,
										m_projectileVelMin = primaryAttack.m_projectileVelMin,
										m_projectiles = primaryAttack.m_projectiles,
										m_projectileAccuracy = primaryAttack.m_projectileAccuracy,
										m_projectileAccuracyMin = primaryAttack.m_projectileAccuracyMin,

										// Attack behavior
										m_hitThroughWalls = primaryAttack.m_hitThroughWalls,
										m_multiHit = primaryAttack.m_multiHit,

										// Effects
										m_hitEffect = primaryAttack.m_hitEffect
									};

									// Assign secondary attack
									itemDrop.m_itemData.m_shared.m_secondaryAttack = secondaryAttack;

									//MarsarahTweaks.MLog($"Death Raiser attack HP percentage: {itemDrop.m_itemData.m_shared.m_attack.m_attackHealthPercentage}");
									//MarsarahTweaks.MLog($"Death Raiser secondaryAttack HP percentage: {itemDrop.m_itemData.m_shared.m_secondaryAttack.m_attackHealthPercentage}");
								}
								
								itemDrop.m_itemData.m_shared.m_attack.m_attackHealthPercentage = 30; // default 40
								itemDrop.m_itemData.m_shared.m_attack.m_attackEitr = 75; // default 100
								//MarsarahTweaks.MLog($"Death Raiser attack HP percentage: {itemDrop.m_itemData.m_shared.m_attack.m_attackHealthPercentage}");
								//MarsarahTweaks.MLog($"Death Raiser secondaryAttack HP percentage: {itemDrop.m_itemData.m_shared.m_secondaryAttack.m_attackHealthPercentage}");

								//MarsarahTweaks.MLog($"Death Raiser attack eitr consumption: {itemDrop.m_itemData.m_shared.m_attack.m_attackEitr}");
								//MarsarahTweaks.MLog($"Death Raiser secondaryAttack eitr consumption: {itemDrop.m_itemData.m_shared.m_secondaryAttack.m_attackEitr}");
							}
						}
					}
					else
					{
						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to {ConfigManager.Configs.DeathRaiserModifications.Name}...");
					}
				}
				else
				{
					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to {ConfigManager.Configs.DeathRaiserModifications.Name}...");
				}
			}
		}*/

		

		// Modify Skeleton Summons
		[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.StartAttack))]
		public static class Patch_StartAttack
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
				if (!ConfigManager.betterDeathRaiserEnabled.Value)
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

				ModState.LastAttackWasSecondary = secondaryAttack;

				// Ensure secondary attack mirrors the primary attack when using secondary
				if (secondaryAttack && currentWeapon.m_shared.m_secondaryAttack != currentWeapon.m_shared.m_attack)
				{
					currentWeapon.m_shared.m_secondaryAttack = currentWeapon.m_shared.m_attack;
				}
			}
		}

		[HarmonyPatch(typeof(UnityEngine.Object), "Instantiate", new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
		public static class Patch_Object_Instantiate
		{
			static void Postfix(UnityEngine.Object __result)
			{
				if (__result == null)
				{
					MarsarahTweaks.MLog("[Instantiate Patch] Error: Instantiated object is null!");
					return;
				}

				if (ZNet.instance == null || ZNet.instance.IsDedicated()) return;

				// Do nothing if the feature is disabled
				if (!ConfigManager.betterDeathRaiserEnabled.Value) return;

				if (__result is GameObject gameObject && gameObject.name.Contains("Skeleton_Friendly"))
				{
					ModState.LastSpawnedSkeleton = gameObject;
					//MarsarahTweaks.MLog($"Captured instantiated skeleton: {gameObject.name}");

					// Set attack type
					SkeletonAttackType skeletonAttack = gameObject.AddComponent<SkeletonAttackType>();
					skeletonAttack.IsSecondaryAttack = ModState.LastAttackWasSecondary;
					//MarsarahTweaks.MLog($"Set IsSecondaryAttack = {skeletonAttack.IsSecondaryAttack} for {gameObject.name}");
				}
			}
		}

		[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GiveDefaultItems))]
		public static class Patch_GiveDefaultItems
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
				if (!ConfigManager.betterDeathRaiserEnabled.Value)
				{
					__runOriginal = true;
					return;
				}

				// Check if this is a friendly skeleton
				if (__instance.name.Contains("Skeleton_Friendly"))
				{
					// Check if it has a custom attack type assigned
					var attackTypeComponent = __instance.GetComponent<SkeletonAttackType>();
					bool isSecondaryAttack = attackTypeComponent != null && attackTypeComponent.IsSecondaryAttack;

					// Assign new weapon based on attack type
					string weaponPrefabName = isSecondaryAttack ? "skeleton_bow2" : "skeleton_sword2";
					var weaponPrefab = ObjectDB.instance.GetItemPrefab(weaponPrefabName);

					if (weaponPrefab != null)
					{
						giveItem(__instance, weaponPrefab);
						//MarsarahTweaks.MLog($"Assigned {weaponPrefabName} to Skeleton_Friendly (Secondary Attack: {isSecondaryAttack})");

						// Only melee skeletons get shields
						if (!isSecondaryAttack)
						{
							AssignShield(__instance);
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
						//MarsarahTweaks.MLog($"Assigned {shieldPrefabName} to Skeleton_Friendly");
					}
				}
			}

			private static void giveItem(Humanoid human, GameObject prefab)
			{
				ItemDrop.ItemData itemData = human.PickupPrefab(prefab, 0, autoequip: false);
				if (itemData != null && !itemData.IsWeapon())
				{
					human.EquipItem(itemData, triggerEquipEffects: false);
				}
			}
		}
	}
}
