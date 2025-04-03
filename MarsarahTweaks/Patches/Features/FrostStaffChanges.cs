using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsarahTweaks.Patches.Features
{
	internal class FrostStaffChanges
	{
		[HarmonyPatch(typeof(Attack), "FireProjectileBurst")]
		public class IceStaffAccuracy_Patch
		{
			private static float originalAccuracy = -1;

			public static void Prefix(Attack __instance, ref float ___m_projectileAccuracy)
			{
				// Run on both client and server - no conditions placed

				if (__instance.GetWeapon().m_shared.m_name == "$item_stafficeshards")
				{
					if (ConfigManager.betterFrostStaffAccuracyEnabled.Value)
					{
						// Backup accuracy
						if (originalAccuracy == -1)
						{
							originalAccuracy = ___m_projectileAccuracy;
						}

						// Apply new accuracy
						___m_projectileAccuracy = 1; //  default 5
					}
					else
					{
						// Restore accuracy
						if (originalAccuracy != -1)
						{
							___m_projectileAccuracy = originalAccuracy;
						}
					}
				}
			}
		}
	}
}
