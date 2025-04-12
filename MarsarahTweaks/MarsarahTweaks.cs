
/***************************************************
 * TODO
 * 
 * 0. ShowOnlinePlayers 
 *    - Fix redundancy in code when positioning header depending on party area location.
 *    - Right now the end result is the same, but the code treats it different
 * 
 * 1. Rested timer reduced to 10 sec (from 20)
 * 2. Add arrow slots (selectable with a key combination)
 * 3. Staff of Fracturing - increase blast radius of splinters
 * 4. Add better gear set bonuses
 * 5. Add Silver Sconce
 * 6. Add Better Roads (increase player speed on roads)
 * 7. Increase Maypole range
 * 8. Remove character speed slow when performing specific actions (equip, eat)
 * 9. Change gem type resource for Ashlands weapons. I think one is using Bloodstone where it's supposed to use Jade
 *    - Dundr (uses Bloodstone instead of Iolite)
 * 10. Add pocket portal (need to figure out how to build just two of those)
 * 11. Destroy ship with a hammer
 * 12. Show heat threshold in Ashlands
 * 13. Add magic to Plains
 * 14. Do something to wards
 *     - Increase HP, Stamina and Eitr (if the player has Eitr) within range
 * 15. Add secondary attack to Staff of Protection that heals (like the Dverger Mage heal)
 * 16. Add more pre-built structures to existing world (need to learn how to manipulate world generator)
 * 17. Better enemy nameplates, better item level indicator
 * 18. Show player HP and death count next to online indicator
 * 19. Add pre-attached signs to existing chests and counter + image with what it contains
 *     - Add snapping points to chests
 * 20. Create a smart dropbox - Everything placed in it will automatically be moved to nearby chests that have an item of the respective type
 * 21. Make my own teleportation pictures according to destination biome
 * 22. Display total resource amount in inventory when building something, next to the current required amount of the piece
 * 23. Increase Scrap Iron drop amount from Muddy Piles
 * 24. Move camera up when sailing. Possible more zoom out when sailing
 * Tin: 14
 * Copper: 22
 * Bronze: 5
 * Iron: 21
 * Silver: 2
 ***************************************************/

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MarsarahTweaks.Patches;
using MarsarahTweaks.Patches.UI;
using ServerSync;
using System.IO;
using UnityEngine;

namespace MarsarahTweaks
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahTweaks : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahTweaks";
		internal const string ModVersion = "1.0.1";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		private static readonly bool showLogs = false; // Set to true to display logs

		private readonly Harmony harmony = new Harmony(ModGUID);

		void Awake()
		{
			ConfigManager.Init(Config);
			//CustomConsoleCommandHandler.Init(); // Register new console commands
			UISmartBiome.UpdateBiomeWeights(); // Set the correct biome weight dictionary at startup

			harmony.PatchAll();
		}

		void Update()
		{
			// Hide/display UI
			UIController.UpdateUIDisplay();
		}

		private void OnDestroy()
		{
			Config.Save();
		}

		// Logger =====================================================================
		public static void MLog(string log, bool header = false, bool footer = false)
		{
			if (showLogs)
			{
				if (header)
				{
					Debug.Log("===================================================");
				}

				Debug.Log($"[Marsarah Tweaks] : {log}");

				if (footer)
				{
					Debug.Log("===================================================");
				}
			}
		}
	}
}
