//using HarmonyLib;
//using UnityEngine;

//namespace MarsarahTweaks.Patches
//{
//	[HarmonyPatch(typeof(Character), nameof(Character.Jump))]
//	public class TestJumpPatch
//	{
//		static void Prefix(ref float ___m_jumpForce)
//		{
//			if (ConfigManager.testJumpEnabled.Value)
//			{
//				___m_jumpForce = 15;
//				MarsarahTweaks.MLog("Modified jump force: " + ___m_jumpForce);
//			}
//			else
//			{
//				___m_jumpForce = 8;
//				MarsarahTweaks.MLog("Default jump force: " + ___m_jumpForce);
//			}
//		}
//	}
//}
