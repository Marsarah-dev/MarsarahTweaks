//using HarmonyLib;
//using UnityEngine;

//namespace MarsarahTweaks.Patches
//{
//	[HarmonyPatch(typeof(Character), nameof(Character.Jump))]
//	public class TestJumpPatch
//	{
//		private static readonly LogManager log = new LogManager("Test Jump", LogManager.LogLevel.Info);
//		static void Prefix(ref float ___m_jumpForce)
//		{
//			if (ConfigManager.testJumpEnabled.Value)
//			{
//				___m_jumpForce = 15;
//				log.Info("Modified jump force: " + ___m_jumpForce);
//			}
//			else
//			{
//				___m_jumpForce = 8;
//				log.Info("Default jump force: " + ___m_jumpForce);
//			}
//		}
//	}
//}
