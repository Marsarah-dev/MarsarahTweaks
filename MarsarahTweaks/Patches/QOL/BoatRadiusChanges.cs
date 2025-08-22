using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarsarahTweaks.Managers;

namespace MarsarahTweaks.Patches.QOL
{
	internal class BoatRadiusChanges
	{
		private static readonly LogManager log = new LogManager("Boat Radius", LogManager.LogLevel.Warning);

		[HarmonyPatch(typeof(Minimap), "UpdateExplore")]
		class LargerBoatExploreRadius_Patch
		{
			private static float originalExploreRadius = -1f;

			private static void Prefix(ref float ___m_exploreRadius)
			{
				if (ZNet.instance != null && ZNet.instance.IsDedicated()) return; // Do not run on dedicated servers

				if (ConfigManager.LargerBoatExploreRadiusEnabled.Value)
				{
					if (Ship.GetLocalShip() && Ship.GetLocalShip().HasPlayerOnboard())
					{
						if (originalExploreRadius == -1f)
						{
							log.Info($"Backing up explore radius: {___m_exploreRadius}");
							originalExploreRadius = ___m_exploreRadius;
						}

						if (___m_exploreRadius != 100f)
						{
							log.Info($"Applying new explore radius: 100");
							___m_exploreRadius = 100f;
						}
					}
					else
					{
						if (originalExploreRadius != -1f && ___m_exploreRadius != originalExploreRadius)
						{
							log.Info($"Restoring original explore radius: {originalExploreRadius}");
							___m_exploreRadius = originalExploreRadius; // 50
						}						
					}
				}
				else
				{
					if (originalExploreRadius != -1f && ___m_exploreRadius != originalExploreRadius)
					{
						log.Info($"Restoring original explore radius: {originalExploreRadius} due to config OFF");
						___m_exploreRadius = originalExploreRadius; // 50
					}
				}
			}
		}
	}
}
