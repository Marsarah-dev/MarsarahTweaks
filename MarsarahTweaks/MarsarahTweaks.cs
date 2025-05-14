
/***************************************************
 * DONE, NOT RELEASED
 * 
 * A. Fixes
 *    - Progression Halt compatibility with Location Reset
 *    - Added config to halt ocean behind Elder instead of Bonemass
 *    
 * B. New QOL
 * 
 * TODO
 * 
 * A. Fixes
 *    1. ShowOnlinePlayers 
 *       - Fix redundancy in code when positioning header depending on party area location.
 *       - Right now the end result is the same, but the code treats it different
 *    2. Do not halt Skeletal Remains from Meadows in Progression Halt
 * 
 * B. New UI
 *    1. Add arrow slots (selectable with a key combination)
 *    2. Expand on Equipment and Quickslots and add tools slots + Arrow slots
 *       OR add a second row of keybinds 1-8 that can be toggled with a key (so one row can be for combat, one for non-combat)
 *    3. Show heat threshold in Ashlands
 *    4. Better enemy nameplates, better item level indicator
 *    5. Show player HP and death count next to online indicator (or color their names to with red if they have the corpse run buff)
 *    6. Display total resource amount in inventory when building something, next to the current required amount of the piece
 *    7. Show boss power expiration message
 *    8. Add message when a player logs out
 *    9. Create Smart Pins - admin predefines pins and players can only put the predefined pins on map
 * 
 * C. New Features
 *    ** Gear
 *    1. Staff of Fracturing - increase blast radius of splinters
 *    2. Add better gear set bonuses
 *    3. Change gem type resource for Ashlands weapons. I think one is using Bloodstone where it's supposed to use Jade
 *       - Dundr (uses Bloodstone instead of Iolite)
 *    4. Add secondary attack to Staff of Protection that heals (like the Dverger Mage heal)
 *    
 *    ** Building
 *    1. Add Silver Sconce
 *    2. Add more Silver/Obsidian build pieces
 *    3. Add pre-attached signs to existing chests and counter + image with what it contains
 *       - Add snapping points to chests
 *    4. Create a smart dropbox - Everything placed in it will automatically be moved to nearby chests that have an item of the respective type
 *    5. Reduce Barrel Wood cost from 10 to 5
 *    
 *    ** Other
 *    1. Add Better Roads (increase player speed on roads)
 *    2. Add pocket portal (need to figure out how to build just two of those)
 *    3. Do something to wards
 *       - Increase HP, Stamina and Eitr (if the player has Eitr) of players within range
 *       - Passively heal surrounding build pieces
 *    4. Increase Scrap Iron drop amount from Muddy Piles
 *    5. Recreate the Better Sorting section
 *    6. Create a Vote to Sleep section, where a player can trigger a vote to sleep and if the majority votes yes, then time is skipped as sleeping.
 *       - If one or more players are in combat (enemies nearby), then the vote is entirely skipped and no tome is passed.
 *    
 *    ** Overhaul
 *    1. Add magic to Plains
 *    2. Add more pre-built structures to existing world (need to learn how to manipulate world generator)
 * 
 * D. New QOL
 *    1. Rested timer reduced to 10 sec (from 20)
 *    2. Increase Maypole range
 *    3. Remove character speed slow when performing specific actions (equip, eat)
 *    4. Destroy ship with a hammer
 *    5. 
 *    6. Make Ancient Bark usable at the Kiln
 *    7. Add Withered Bone to Shield Generator
 *    8. Faster smelting from all smelters and kiln
 * 
 * 
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
		internal const string ModVersion = "1.1.2";
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
