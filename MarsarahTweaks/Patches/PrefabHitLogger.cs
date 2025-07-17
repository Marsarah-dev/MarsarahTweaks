//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace MarsarahTweaks.Patches
//{
//	internal class PrefabHitLogger
//	{
//		// Patch for when MineRock is damaged
//		[HarmonyPatch(typeof(MineRock), "Damage")]
//		class MineRockLogger_Patch
//		{
//			public static void Prefix(MineRock __instance, HitData hit)
//			{
//				if (__instance != null)
//				{
//					// LogInfo the prefab name and damage information when MineRock is damaged
//					string prefabName = __instance.name;
//					float health = __instance.GetHealth(); // Current health of the rock
//					//MarsarahTweaks.LogInfo($"[MineRock] Damaged MineRock prefab: {prefabName}, Health: {health}, Hit Info: {hit}");
//					MarsarahTweaks.LogInfo($"[MineRock] Damaged MineRock prefab: {prefabName}");
//				}
//			}
//		}

//		// Patch for when MineRock5 is damaged
//		[HarmonyPatch(typeof(MineRock5), "Damage")]
//		class MineRock5Logger_Patch
//		{
//			public static void Prefix(MineRock __instance, HitData hit)
//			{
//				if (__instance != null)
//				{
//					// LogInfo the prefab name and damage information when MineRock is damaged
//					string prefabName = __instance.name;
//					float health = __instance.GetHealth(); // Current health of the rock
//					//MarsarahTweaks.LogInfo($"[Minerock5] Damaged MineRock5 prefab: {prefabName}, Health: {health}, Hit Info: {hit}");
//					MarsarahTweaks.LogInfo($"[Minerock5] Damaged MineRock5 prefab: {prefabName}");
//				}
//			}
//		}

//		// Patch for when Destructible is damaged
//		[HarmonyPatch(typeof(Destructible), "Damage")]
//		class DestructibleLogger_Patch
//		{
//			public static void Prefix(Destructible __instance, HitData hit)
//			{
//				if (__instance != null)
//				{
//					// LogInfo the prefab name and damage information when MineRock is damaged
//					string prefabName = __instance.name;
//					//MarsarahTweaks.LogInfo($"[Destructible] Damaged Destructible prefab: {prefabName}, Hit Info: {hit}");
//					MarsarahTweaks.LogInfo($"[Destructible] Damaged Destructible prefab: {prefabName}");
//				}
//			}
//		}

//		// Patch for when DropOnDestroyed is destroyed
//		[HarmonyPatch(typeof(DropOnDestroyed), "OnDestroyed")]
//		class ProgressionHaltDestroyed_Patch
//		{
//			public static void Prefix(DropOnDestroyed __instance)
//			{
//				string prefabName = __instance.name;

//				// Get the list of items that will be dropped
//				/*List<GameObject> dropList = __instance.m_dropWhenDestroyed.GetDropList();
//				List<string> dropNames = new List<string>();
//				foreach (var drop in dropList)
//				{
//					if (drop != null)
//					{
//						dropNames.Add(drop.name);
//					}
//				}*/

//				// LogInfo the destruction event
//				//MarsarahTweaks.LogInfo($"[DropOnDestroyed] Destroyed Object: {prefabName}, Drops: {string.Join(", ", dropNames)}");
//				MarsarahTweaks.LogInfo($"[DropOnDestroyed] Destroyed Object: {prefabName}");
//			}
//		}
//	}
//}
