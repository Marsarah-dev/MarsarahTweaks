using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Player;
using static ZRoutedRpc;

namespace MarsarahTweaks.Patches.QOL
{
	internal class FasterEquipChanges
	{
		[HarmonyPatch(typeof(Player), "QueueEquipAction")]
		private static class EquipActionSpeed_Patch
		{
			private static void Prefix(ItemDrop.ItemData item)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.fasterEquipEnabled.Value)
				{
					// Reduce equip durations
					if (item.IsWeapon())
					{
						if (item.m_shared.m_equipDuration > 0)
						{
							item.m_shared.m_equipDuration = 0;
						}
						//MarsarahTweaks.MLog($"Weapon {item.m_shared.m_name} - anim state: {item.m_shared.m_animationState}");
					}
					if (item.IsEquipable() && !item.IsWeapon())
					{
						if (item.m_shared.m_equipDuration > 1)
						{
							item.m_shared.m_equipDuration = 1;
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(Player), "QueueUnequipAction")]
		private static class UnequipActionSpeed_Patch
		{
			private static void Prefix(ItemDrop.ItemData item)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.fasterEquipEnabled.Value)
				{
					if (item.IsWeapon())
					{
						if (item.m_shared.m_equipDuration > 0)
						{
							item.m_shared.m_equipDuration = 0;
						}
					}
					if (item.IsEquipable() && !item.IsWeapon())
					{
						if (item.m_shared.m_equipDuration > 1)
						{
							item.m_shared.m_equipDuration = 1;
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(ZSyncAnimation), "SetTrigger")]
		private class RemoveEquipAndEatAnimation
		{
			private static bool Prefix(ZSyncAnimation __instance, string name)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return true; // Do not run on dedicated servers

				if (ConfigManager.fasterEquipEnabled.Value && name == "equip_hip")
				{
					return false;
				}
				return true;
			}
		}

		/*[HarmonyPatch(typeof(ZSyncAnimation), "SetTrigger")]
		public class ZSyncAnimation_SetTrigger_Patch
		{
			static void Prefix(ZSyncAnimation __instance, string name, ref Animator ___m_animator, ref int ___s_forwardSpeedID)
			{
				if (name == "equip_hip")
				{
					float forwardSpeed = ___m_animator.GetFloat(___s_forwardSpeedID);
					MarsarahTweaks.MLog($"[Equip] equip_hip triggered in ZSyncAnimation on {__instance.gameObject.name} - forward speed: {forwardSpeed}");

					//___m_animator.SetFloat(___s_forwardSpeedID, 5.0f);  // Reset speed - -surprisingly, this does not work
					//MarsarahTweaks.MLog($"equip_hip triggered - Resetting forward speed to 5.0");

					// __instance.m_smoothCharacterSpeeds = false; // nogo
				}
			}
		}

		[HarmonyPatch(typeof(ZSyncAnimation), "SyncParameters")]
		public class ZSyncAnimation_SyncParameters_Patch
		{
			static void Prefix(ZSyncAnimation __instance, ref float fixedDeltaTime, ref bool init, ref ZNetView ___m_nview, ref Animator ___m_animator, ref int ___s_forwardSpeedID)
			{
				if (!___m_nview.IsOwner()) return;

				float newForwardSpeed = ___m_animator.GetFloat(___s_forwardSpeedID);
				MarsarahTweaks.MLog($"SyncParameters - Forward Speed: {newForwardSpeed}");
			}
		}*/


		/*[HarmonyPatch(typeof(ZSyncAnimation), "SyncParameters")]
		public class ZSyncAnimation_Awake_Patch
		{
			static void Postfix(ZSyncAnimation __instance, float fixedDeltaTime, ref ZNetView ___m_nview, ref int[] ___m_floatHashes, ref float[] ___m_floatDefaults, ref int ___s_forwardSpeedID, ref int ___s_sidewaySpeedID)
			{
				ZDO zDO = ___m_nview.GetZDO();
				for (int j = 0; j < ___m_floatHashes.Length; j++)
				{
					int num2 = ___m_floatHashes[j];
					//if (num2 != -1489184366 && num2 != -1489560162 && num2 != -1344470313)
					if (num2 == ___s_forwardSpeedID || num2 == ___s_sidewaySpeedID)
					{
						float @float = zDO.GetFloat(438569 + num2, ___m_floatDefaults[j]);

						if (Mathf.Abs(@float) > 0.01)
						{
							MarsarahTweaks.MLog($"num2: {num2} - float: {@float}");
						}
					}
				}
			}
		}*/


		/*[HarmonyPatch(typeof(Animator), "SetFloat", new Type[] { typeof(int), typeof(float) })]
		public class Animator_SetFloat_Patch
		{
			static void Prefix(Animator __instance, int id, float value)
			{
				if (id != 473926134)
				{
					string caller = new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().DeclaringType.Name;
					Debug.LogWarning($"[Animator] SetFloat({id}, {value}) called by {caller} on {__instance.gameObject.name}");
				}
			}
		}*/
	}
}

