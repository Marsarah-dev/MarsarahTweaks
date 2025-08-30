using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.Balance
{
	internal class FrostStaffChanges
	{
		private static readonly LogManager log = new LogManager("Frost Staff", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Attack), "FireProjectileBurst")]
		class IceStaffAccuracy_Patch
		{
			private static float originalAccuracy = -1;

			private static void Prefix(Attack __instance, ref float ___m_projectileAccuracy)
			{
				// Run on both client and server - no conditions placed

				if (__instance.GetWeapon().m_shared.m_name == "$item_stafficeshards")
				{
					if (ConfigManager.BetterFrostStaffAccuracyEnabled.Value)
					{
						// Backup accuracy
						if (originalAccuracy == -1)
						{
							originalAccuracy = ___m_projectileAccuracy;
							log.Info($"Backed up accuracy: {___m_projectileAccuracy}");
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
							log.Info($"Restored accuracy: {___m_projectileAccuracy}");
						}
					}
				}
			}
		}
	}
}
