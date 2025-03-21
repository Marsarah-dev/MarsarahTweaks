//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace MarsarahTweaks.Patches
//{
//	internal class OtherPatches
//	{
//		[HarmonyPatch(typeof(ObjectDB), "Awake")]
//		class OthersSection_Patch
//		{
//			static void Postfix(ref ObjectDB __instance)
//			{
//				if (__instance == null) return;

//				if (ZNet.instance != null)
//				{
//					bool isDedicatedServer = ZNet.instance.IsDedicated();
//					//bool isLocalWorld = ZNet.instance.IsServer() && !isDedicatedServer;
//					if (!isDedicatedServer)
//					{
//						MarsarahTweaks.MLog($"ObjectDB Awake: Updating ...");
//					}
//					else
//					{
//						MarsarahTweaks.MLog($"ObjectDB Awake: I am a server. No changes made to ...");
//					}
//				}
//				else
//				{
//					MarsarahTweaks.MLog($"ObjectDB Awake: Too early to do anything. No changes made to ...");
//				}
//			}
//		}
//	}
//}
