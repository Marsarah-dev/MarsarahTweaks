
/***************************************************
 * DONE, NOT RELEASED
 * 
 * A. Fixes/Others
 *    - Fixed an issue with Progression Halt where players could get the contents of halted chests by simply destroying them
 *    - Online Players also shows the total number of players logged in. Also swapped the order of header and player list when displayed at the botom right corner
 *     - TODO: fix logging out on boat camera
 *    
 * B. New UI
 *    - Added section: Show Owned Resources In Build Menu
 *    - Added section: Show Boss Power Expiration Message
 *    - Added section: Player Logout Announce 
 *    - Added section: Show Heat Meter in Ashlands
 *    - Added section: Use Symbols for UI Elements
 *    - Removed "F" from boat forward speed (kept "R" for reverse);
 * 
 * C. New Features
 * 
 * B. New QOL
 *    - Added section: Shorter Rested Delay
 *    - Added section: More Usable Fuel
 * 
 * 
 * TODO
 * 
 * A. Fixes
 * 
 * B. New UI
 *    1. 
 *    2. 
 *    3. 
 *    4. Better enemy nameplates, better item level indicator
 *    5. Show player HP and death count next to online indicator (or color their names with red if they have the corpse run buff)
 *    6. 
 *    7. 
 *    8. 
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
 *       - If one or more players are in combat (enemies nearby), then the vote is entirely skipped and no time is passed.
 *    7. Add an Alternate Lights Fuel config (exclusive toggle with Permanent Lights) that makes lights use less fuel during the day (maybe turn them off or make them dimmer during the day) and work normal at night
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



🎯 Legend:

🟢 Easy: Can be done with patches, UI edits, or config alone.

🟡 Medium: Requires more extensive UI handling, gameplay state tracking, or syncing.

🔴 Hard: Needs asset additions, major reverse engineering, or world generation hooks.

B. New UI
Feature	Complexity	Notes
1. 
2. 
3. 
4. Better nameplates + item level				🟡 Medium	Needs patching enemy nameplate drawing + item quality display.
5. Show HP/deaths next to name					🔴 Hard		Needs syncing player HP/death count to others, complex in multiplayer.
9. Smart Pins									🟡→🔴 Medium to Hard	Requires UI pin menu patching + pin system restriction logic.

C. New Features
Gear
Feature	Complexity	Notes
1. Staff of Fracturing effect		🟡 Medium	Modify AoE code, easy with custom logic in projectile/explosion.
2. Gear set bonuses					🟡 Medium	Track gear sets and apply effects, like how Troll set works.
3. Change gem for Ashlands weapons	🟢 Easy		Replace crafting recipe entries.
4. Healing Staff of Protection		🟡 Medium	Add alt-attack behavior and healing; reuse dvergr logic.

Building
Feature	Complexity	Notes
1. Silver Sconce			🟢 Easy		Reuse bronze/gold sconce logic, change visuals.
2. Silver/Obsidian pieces	🔴 Hard		Needs new 3D assets and integration with build tables.
3. Chest snapping + signs	🔴 Hard		Requires placement logic + sign prefab/UI work.
4. Smart dropbox			🔴 Hard		Needs scanning nearby chests and item categorization logic.
5. Barrel cost reduction	🟢 Easy		Patch recipe.

Other
Feature	Complexity	Notes
1. Better Roads (speed buff)		🟡 Medium	Detect if player is on road (terrain or pathfinding zone).
2. Pocket Portal (only 2 allowed)	🔴 Hard		Requires portal pairing system and strict instancing.
3. Wards buffs + repair				🟡 Medium	Hook ward logic, apply SEs + invoke WearNTear.Repair periodically.
4. More iron from piles				🟢 Easy		Patch drop table.
5. Better Sorting UI				🟡 Medium	Redo Inventory UI, tag-based sort.
6. Vote to Sleep					🔴 Hard		Network voting, combat detection, time skip sync.
7. Alternate Lights Fuel			🟡 Medium	Time-of-day detection + light component handling.

Overhaul
Feature	Complexity	Notes
1. Plains magic			🔴 Hard			Needs SEs, AI changes, magic behaviors.
2. Pre-built structures	🔴 Very Hard	Deep world generation patching or prefab injection.

D. New QOL
Feature	Complexity	Notes
2. Increase Maypole range	🟢 Easy		Patch ComfortManager radius logic.
3. Remove equip/eat slow	🟡 Medium	Find and patch delays or UseItem debuffs.
4. Destroy ship with hammer	🟢 Easy		Add hammer destruction behavior for ships.
8. Faster smelting			🟢 Easy		Patch smelter tick timers or fuel usage.
 *
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
		internal const string ModVersion = "1.2.0";
		internal const string Author = "Marsarah";
		public const string ModGUID = Author + "." + ModName;

		private static readonly bool showLogs = true; // Set to true to display logs

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
		public static void LogInfo(string log, bool header = false, bool footer = false)
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

		public static void LogWarn(string log, bool header = false, bool footer = false)
		{
			if (showLogs)
			{
				if (header)
				{
					Debug.Log("===================================================");
				}

				Debug.LogWarning($"[Marsarah Tweaks] : {log}");

				if (footer)
				{
					Debug.Log("===================================================");
				}
			}
		}

		public static void LogError(string log, bool header = false, bool footer = false)
		{
			if (showLogs)
			{
				if (header)
				{
					Debug.Log("===================================================");
				}

				Debug.LogError($"[Marsarah Tweaks] : {log}");

				if (footer)
				{
					Debug.Log("===================================================");
				}
			}
		}
	}
}
