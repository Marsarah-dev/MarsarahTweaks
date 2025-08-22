
/***************************************************
 * DONE, NOT RELEASED
 * 
 * A. Fixes
 *    - More Usable Fuel error message fixed when building or being nearby any Shield Generators
 *    
 * B. New UI
 * 
 * C. New Features
 *    - Better Creature Drops
 *      - Ghost: Added Silver Necklace drop
 *      - Fenring: Added Fenris Hair 1-2, Added Fenris Claw (replacing Wolf Fang)
 *      - Bat: Added Blood Bag 50% chance drop
 *      - Dverger Rogue, Mage and Ashlands Dvergr: Increased Soft Tissue chance to 100% (from 25%)
 *      
 *    - Stop Nighttime Invasion
 *      - Fulings, Seeker and Charred do not spawn in early biomes at night when their relevant bosses are defeated
 *      - Fulings: Mountain
 *      - Seeker: Plains
 *      - Charred: Plains, Mistlands
 *      
 *    - Silver Sconce
 *      - Added 3 tyes of Silver Sconces (red, blue and green flame) available in the Mountain biome
 *      
 *    - Green Standing Brazier 
 *      - Added a green version of the Standing Brazier
 * 
 * B. New QOL
 * 
 * D. Other changes 
 *    
 *  
 * TODO
 * 
 * A. Fixes
 *    - Remove fireplace pieces from the permament lights mod OR find an alternative solution for permanent lights
 *    - Handle Dvergr Lantern for Smart Biome (see if it's needed to handle in the first place)
 * 
 * B. New UI
 *    1. Add 🧑‍🤝‍🧑 for Online players in alternate UI (too much headache to add now)
 *    2. Add taming indicator for each creature in taming progress
 *    3. Better enemy nameplates
 *    4. Better item level indicator
 *    5. Show player HP and death count next to online indicator (or color their names with red if they have the corpse run buff)
 *    6. Make my UI elements draggable
 *    7. 
 *    8. 
 *    9. Create Smart Pins - admin predefines pins and players can only put the predefined pins on map
 * 
 * C. New Features
 *    ** Gear
 *    1. Staff of Fracturing - increase blast radius of splinters
 *    2. Add better gear set bonuses
 *    3.
 *    4. Add secondary attack to Staff of Protection that heals (like the Dverger Mage heal)
 *    
 *    ** Building
 *    1. 
 *    2. Add more Silver/Obsidian build pieces
 *    3. Add pre-attached signs to existing chests and counter + image with what it contains
 *       - Add snapping points to chests
 *    4. Create a smart dropbox - Everything placed in it will automatically be moved to nearby chests that have an item of the respective type
 *    5. 
 *    
 *    ** Crafting
 *    
 *    ** Cooking       
 *    
 *    ** Other
 *    1. Add Better Roads (increase player speed on roads)
 *    2. 
 *    3. Do something to wards
 *       - Increase HP, Stamina and Eitr (if the player has Eitr) of players within range
 *       - Passively heal surrounding build pieces
 *    4. Increase Scrap Iron drop amount from Muddy Piles
 *    5. Recreate the Better Sorting section
 *    6. Create a Vote to Sleep section, where a player can trigger a vote to sleep and if the majority votes yes, then time is skipped as sleeping.
 *       - If one or more players are in combat (enemies nearby), then the vote is entirely skipped and no time is passed.
 *    7. Add an Alternate Lights Fuel config (exclusive toggle with Permanent Lights) that makes lights use less fuel during the day (maybe turn them off or make them dimmer during the day) and work normal at night
 *    8. Swap relevant configs from bool to numbers where applicable
 *       - Less Stamina Usage            -> Rename to Stamina Usage          - Value indicates percentage of stamina used. Posisive numbers increase it
 *       - Reduced Crossbows Reload Time -> Rename to Crossbows Reload Time  - Change the mod to set it to the indicated value instead of subtracting/adding
 *       - Hildir Weight Rewards         ->                                  - Set extra weight per chest delivered
 *       - Lighter Metal Weight          -> Rename to Metal Weight           - Set number for all metals (default values between 8 and 12)
 *       - Larger Pickup Area            -> Rename to Pickup Area            - Set to indicated number (default 2)
 *       - Larger Boat Explore Radius    -> Rename to Boat Explore Radius    - Set to multiplier (2 means 2x the radius) / Or set to indicated value
 *       - Bigger Wisp Radius            -> Rename to Wisp Radius            - Set to indicated value
 *       - Less Fall Damage              -> Rename to Fall Damage Multiplier - Set to multiplier (0.6 means 40% less fall damage)
 *       - Shorter Rested Delay          -> Rename to Rested Delay           - Set to indicated value (default 20s)
 *    9. Make Fulings, Seekers and Charred spawn at night in later biomes (not in Meadows, BF and Swamp)
 *    
 *    ** Overhaul
 *    1. Add magic to Plains
 *    2. Add more pre-built structures to existing world (need to learn how to manipulate world generator)
 * 
 * D. New QOL
 *    1. 
 *    2. Increase Maypole range
 *    3. Remove character speed slow when performing specific actions (equip, eat)
 *    4. Destroy ship with a hammer
 *    5. 
 *    6. 
 *    7. 
 *    8. Faster smelting from all smelters and kiln
 *    
 *
 ***************************************************/

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using MarsarahTweaks.Managers;
using MarsarahTweaks.Patches;
using MarsarahTweaks.Patches.Features;
using MarsarahTweaks.Patches.QOL;
using MarsarahTweaks.Patches.UI;
using ServerSync;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MarsarahTweaks
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class MarsarahTweaks : BaseUnityPlugin
	{
		internal const string ModName = "MarsarahTweaks";
		internal const string ModVersion = "1.4.0";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		private readonly Harmony harmony = new Harmony(ModGUID);

		void Awake()
		{
			LogManager.SetGlobalLogLevel(LogManager.LogLevel.Info); // None, Error, Warning, Info
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
	}
}
