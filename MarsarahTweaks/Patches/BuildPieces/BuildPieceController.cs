using HarmonyLib;
using MarsarahTweaks.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using UnityEngine;

namespace MarsarahTweaks.Patches.BuildPieces
{
	internal class BuildPieceController
	{
		private static readonly LogManager log = new LogManager("Build Piece Controller", LogManager.LogLevel.Info);

		[HarmonyPatch(typeof(Player), "Awake")]
		internal static class ObjectDB_Awake_Patch
		{
			private static bool initialized;

			static void Postfix()
			{
				if (initialized) return;
				bool toggled = false;
				
				// Run initial visibility sync
				toggled = SilverSconce.ToggleSilverSconceVisibility();
				toggled = toggled && ColoredDvergerLanterns.ToggleColoredDvergrLanternsVisibility();
				toggled = toggled && GreenStandingBrazier.ToggleGreenBrazierVisibility();
				toggled = toggled && SilverHangingBrazier.ToggleSilverHangingBrazierVisibility();

				initialized = toggled;

				if (initialized)
				{
					log.Info("Initial build piece visibility toggled.");
				}
				else
				{
					log.Info("Visibility did not initialize.");
				}
			}
		}
	}
}
