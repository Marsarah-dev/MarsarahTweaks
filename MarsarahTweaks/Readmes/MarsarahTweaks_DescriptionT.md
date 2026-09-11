# <strong> Marsarah Tweaks </strong>

**Version:** 1.6.0  
**Author:** Marsarah

---

## <strong> 🧰 About the Mod </strong>

Marsarah Tweaks is a modular and optimized port of my previous project, *MarsarahMod*.  
It features numerous code improvements, cleaned-up logic, and **ServerSync** integration.  
This mod changes many aspects of the game so it's advised to look through and read the description or the config file, to decide which features to enable and which to turn off.  
Each feature is grouped into sections to make this process easier. In addition, the mod scans for already installed mods and automatically disables its own relevant configs if it finds mods that are incompatible or that may cause issues together.  
If any issues or incompatibilities are found, please post them on the mod's Nexus page.

> ⚠️ **v1.6.0 Upgrade Note:** The custom Build Pieces section, including Pocket Portal and Extra Lights, has been removed from Marsarah Tweaks and is planned to move into a separate mod. 
If an existing world uses these custom pieces, make a world backup before updating. Marsarah Tweaks no longer provides these custom prefabs.

---

## **Development Notes**

**Deep North / Spoiler Note:** Deep North content is intentionally not included in Marsarah Tweaks yet. 
I want to experience the new biome, progression, gear, build pieces, and other content myself before digging through the game files, so I can play through it without spoiling the experience for myself. 
Once I have played through the Deep North, I plan to update the mod to properly include its content across the relevant features, including things such as gear and recipe changes, 
build-piece costs and materials, progression and balance adjustments, and any other existing tweaks that should apply to the new content.

**AI Usage Disclosure:** Marsarah Tweaks grew out of my original MarsarahMod project, whose code I initially wrote myself. 
AI tools were later used to assist with porting that code into Marsarah Tweaks and are now used as part of my development workflow for tasks such as debugging, refactoring, compatibility updates, researching game API changes, and documentation. 
Development remains human-directed: I decide what features are added, how they should behave, and I write code, review and test the changes included in releases. The mod's logo was also created using generative AI.

---

## <strong> 🔒 Permissions </strong>

Reuploading this mod, whether in part or in full, is **not permitted**.  
Anyone is free to take inspiration or implement similar features, but must do so with their own code and assets.

---

## <strong> 🧱 Requirements </strong>

This mod requires **BepInEx for Valheim**, available here:  https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/  
⚠️ **Note:** BepInEx needs to be installed on the **dedicated server** as well as **all clients**.

---

## <strong> 📦 Installation </strong>

- Unpack the `.zip` file and copy `MarsarahTweaks.dll` into your `Valheim/BepInEx/plugins` folder.
- Or use a mod manager.

⚠️ **Note:** This mod must be installed on both the **server** and **all clients**.  
⚠️ **Note:** Configs may be renamed or reshuffled upon mod updates, so always check the config file when updating this mod.

---

## <strong> ⚙️ Configuration </strong>

- A config file is generated on first launch:  
  `Valheim/BepInEx/config/Marsarah.MarsarahTweaks.cfg`
- Each feature is in its own section and can be toggled individually.
- Features can be changed manually or using a Configuration Manager.
- Most features support **mid-game toggling**, but some require crafting menus to be reopened, reloading areas or **client** relogs (this is specified in their relevant sections below).
- Some configs below will have more detailed changes written in the [Docs] tab of the Nexusmods page. 

⚠️ **Note:**  All features (except UI) are synced using ServerSync.  
⚠️ **Note:**  Players will be disconnected if their mod version differs from the server.

---

## <strong> 💰 Donations </strong>

My mods are and will always be free to use. If you'd like to support my work, you can donate here:
https://paypal.me/Marsarah9

---

## <strong> ⚡ Main Features </strong>

## <strong>⚙️ Lock Configuration</strong>

Prevents non-admin players from modifying any configuration values while in-game.  
Useful for dedicated servers to maintain consistent settings.

---

## <strong>⚙️ Grind Reduction</strong>

All features below are synced with the server.  
They can be toggled mid-game, unless otherwise specified.

---

### <strong>🔧 Double Bronze Crafting</strong>

- Doubles the amount of **Bronze** produced when combining **Copper and Tin** at a Forge.
- Only affects the **output amount** — resource costs stay the same.  
- Toggling mid-game requires reopening the Forge menu for changes to take effect.
- **Conflicts:** TripleBronze by KaceCottam or Triple Bronze JVL by Digitalroot.
  - Automatically disabled if the above are detected.

---

### <strong>🔧 Cheaper Gear Recipe Amounts</strong>

- Reduces the **amount** (not type) of resources needed to craft or upgrade vanilla gear.
- Focused especially on metal for weapons and armor  
- Rebalances the material amounts for some recipes (e.g. more wood or leather) for game balance  
- Specific details can be found in the **Docs** tab of the **Nexusmods** page (very long list)
- Toggling mid-game requires reopening the relevant crafting menu.
- **Conflicts:** Mods that change gear recipe ingredients.

---

### <strong>🔧 Alternate Gear Recipe Materials</strong>

- Changes gear crafting and upgrade materials to match the **biome progression**.  
- This prevents players from needing to backtrack to earlier biomes to gather outdated materials (not all inclusive).
- Other changes are made where it made sense (e.g. Dundr using Iolite instead of Bloodstone)
- Toggling mid-game requires reopening the relevant crafting menu.
- **Conflicts:** Mods that modify vanilla gear recipe resources.

**🔹 Changes:**
- **Club:**  
  - Material:         Bone Fragments → Leather Scraps
- **Copper Knife:**  
  - Material:         Greydwarf Eye  → Leather Scraps   
    - Upgrade Amount:   8/16/24        → 2/4/6 
- **Silver Knife:**  
  - Material:         Wood           → Fine Wood  
  - Material:         Iron           → Obsidian  
- **Silver Sword:**  
  - Material:         Wood           → Fine Wood  
    - Craft Amount:     2              → 10  
    - Upgrade Amount:   1/2/3          → 2/4/6  
  - Material:         Iron           → Obsidian  
    - Craft Amount:     5              → 4  
    - Upgrade Amount:   3/6/9          → 2/4/6  
- **Serpent Scale Shield:**  
  - Material:         Iron           → Chitin  
    - Upgrade Amount:   2/4/6          → 1/2/4  
- **Porcupine:**  
  - Material:         Iron           → Black Metal  
- **Arbalest:**  
  - Material:         Wood           → Fine Wood  
- **Wolf Armor Chest:**  
  - Material:         Chain          → Wolf Fang  
    - Craft Amount:     1              → 3  
- **Padded Helmet:**  
  - Material:         Iron           → Black Metal  
- **Padded Cuirass:**  
  - Material:         Iron           → Black Metal  
- **Padded Greaves:**  
  - Material:         Iron           → Black Metal  
- **Linen Cape:**  
  - Material:         Silver         → Black Metal  
- **Eitr-Weave Hood:**  
  - Material:         Iron           → Scale Hide  
    - Craft Amount:     2              → 3  
- **Robes of Embla:**  
  - Material:         Flametal       → Sulfur  
- **Ashen Cape:**  
  - Material:         Flametal       → Sulfur  
- **Dundr:**  
  - Material:         Bloodstone     → Iolite  

---

### <strong>🔧 Cheaper Build Piece Amounts</strong>

- Reduces the **amount** of materials required to craft vanilla build pieces.
- Specific details can be found in the **Docs** tab of the **Nexusmods** page.
- Toggling mid-game requires reopening the build menu.
- **Conflicts:** Mods that modify vanilla build piece costs.
  - Does not affect mods that add new build pieces

---

### <strong>🔧 Alternate Build Piece Materials</strong>

- Alters some **material types** required for specific vanilla build pieces.  
- Pairs well with the Cheaper Build Piece Amounts option
- **Note:** This will move the Workbench Toolrack extension from the Mountain to Swamp biome
- Toggling mid-game requires reopening the build menu.
- **Conflicts:** Mods that modify vanilla build piece costs.  
  - Compatible with mods that add new build pieces (changes won't apply to them).

**🔹 Changes:**
- **Tool Rack (Workbench Extension):** Obsidian → Coal (This will move this extension one biome earlier)
- **Darkwood Gate:**                   Iron     → Black Metal
- **Bath Tub:**                        Iron     → Black Metal

---

### <strong>🔧 Food and Mead Modifications</strong>

- Adjusts food and mead recipes.
- Changes recipe amounts, outputs, and ingredients.  
- All food stack sizes increased to **20**
- Specific details can be found in the **Docs** tab of the **Nexusmods** page.
- Toggling mid-game requires **client** relog. 
- **Conflicts:** Mods that modify vanilla food or mead recipes.  
  - Does not modify new recipes addded by other mods.

---

## <strong>⚙️ Balance</strong>

All features below are synced with the server.  
They can be toggled mid-game, unless otherwise specified.

---

### <strong>🔧 Early Linen Cape</strong>

- Moves the Linen Cape to the **Swamp biome**, renames it to **Fine Cape**, and adds **Poison Resistance**.
- Adjusts material and upgrade requirements.
- Toggling mid-game requires reopening the crafting menu to apply changes. If wearing the cape while toggling, a **client relog** is required for the Poison Resist to apply.
- **Conflicts:** Mods that modify back pieces.

**🔹 Changes:**
- **Material:**       Linen thread → Deer Hide
  - **Craft Amount:**   20     → 5
  - **Upgrade Amount:** 4/8/12 → 2/4/6
- **Material:**       Silver → Iron

---

### <strong>🔧 Gear Speed Modifications</strong>

- Removes movement speed penalties from heavy/mage gear and adds small bonuses to light armor sets.
- Toggling mid-game requires **client relog**.
- **Conflicts:** Mods that change gear speed modifiers.

**🔹 Changes:**
- **Heavy Gear:** -5% → 0%  
- **Mage Gear:** -2% → 0%  
- **Light Gear Bonuses:**  
  • Troll, Bear, Vilebone and Ask: +2%  
  • Root: +1%  
- **Weapon Penalties:**  
  • Battleaxes & Sledgehammers: -15% → -10%  
- **Shield Penalties:**  
  • Tower Shields: -15% → -10%

---

### <strong>🔧 Extra Armor Stats</strong>

- Enhances gear with **extra stats** based on armor class:
  - **Heavy Armor:** Adds HP  
  - **Light Armor:** Adds Stamina  
  - **Mage Armor:** Adds Eitr (not regen)
- Toggling mid-game requires **client relog** for tooltip refresh.
- **Conflicts:** Mods that add HP/Stamina/Eitr to armor.  
  - Compatible with custom armor mods — only vanilla gear is affected.

**🔹 Stat Bonuses by Armor Set:**

**HP (Heavy Armor):**

| Set           | Total HP | Breakdown (Head / Chest / Legs)  |
|---------------|----------|----------------------------------|
| Bronze        | 10       | 2 / 4 / 4                        |
| Iron          | 20       | 4 / 8 / 8                        |
| Silver        | 30       | 8 / 12 / 10                      |
| Padded        | 40       | 10 / 16 / 14                     |
| Carapace      | 50       | 12 / 20 / 18                     |
| Flametal      | 60       | 14 / 24 / 22                     |
| Ask           | 25       | 5 / 10 / 10                      |

**Stamina (Light Armor):**

| Set       | Total Stamina | Breakdown (Head / Chest / Legs)   |
|-----------|----------------|----------------------------------|
| Troll     | 5              | 1 / 2 / 2                        |
| Bear      | 5              | 1 / 2 / 2                        |
| Root      | 10             | 3 / 4 / 3                        |
| Fenris    | 15             | 4 / 6 / 5                        |
| Vilebone  | 20             | 5 / 8 / 7                        |
| Ask       | 30             | 8 / 12 / 10                      |

**Eitr (Mage Armor):**

| Set         | Total Eitr | Breakdown (Head / Chest / Legs)  |
|-------------|------------|----------------------------------|
| Eitr-Weave  | 50         | 10 / 20 / 20                     |
| Embla       | 75         | 15 / 30 / 30                     |

---

### <strong>🔧 Gear Upgrade Unlock</strong>

- Gear from Meadows, Black Forest, Mistlands and Ashlands can be upgraded to lvl 4 within their respective biomes.  
- This is accomplished by reducing the crafting workstation level requirements for Leather Set, Troll Set, Bronze Set, all Mistlands sets, and all Ashlands sets.  
- **Note**: Special Ashlands weapons that use gems can be crafted to lvl 3 (e.g. Klossen max level upgrade is 3 (from 2)).
- The Vilebone Set recipe is moved to the Workbench as a crafting station (from Forge).
- Toggling mid-game requires the relevant crafting menu to be reopened.
- **Conflicts:** Mods that modify the required station level for the named sets.

---

### <strong>🔧 Forsaken Powers Cooldowns</strong>

- Adjusts **cooldowns** and **durations** of each Forsaken Power for more flexible usage.
- **Conflicts:** Mods that modify Forsaken Powers (e.g. ForsakenPowerOverhaul by JuneGame)
  - Automatically disabled if the above is detected.

**🔹 Power Timing Adjustments:**

| Power      | Cooldown | Duration |
|------------|----------|----------|
| Eikthyr    | 15 min   | 10 min   |
| Elder      | 15 min   | 10 min   |
| Bonemass   | 15 min   | 7.5 min  |
| Moder      | 15 min   | 10 min   |
| Yagluth    | 15 min   | 7.5 min  |
| Queen      | 15 min   | 7.5 min  |
| Fader      | 15 min   | 7.5 min  |

---

### <strong>🔧 Faster Character Speed</strong>

- Boosts overall character movement speeds.
- Terrain is not taken in consideration when modifying movement speed.
- **Conflicts:** Mods that modify player movement speed

**🔹 Changes:**
- **Crouch Speed:** 2.0 → 2.2  
- **Swim Speed:** 2.0 → 2.2  
- **Walk Speed:** 1.6 → 2.5  
- **Run Speed:** Unchanged

---

### <strong>🔧 Shorter Wet & Potion Cooldowns</strong>

- Reduces cooldowns for **potion usage** and the **wet status effect**.
- **Conflicts:** Mods that modify status effects for wetness or potions.

**🔹 Cooldown Changes:**

| Effect / Potion         | Old  | New  |
|-------------------------|------|------|
| Wet                     | 120s | 60s  |
| Minor Eitr Potion       | 120s | 60s  |
| Minor Health Potion     | 120s | 45s  |
| Medium Health Potion    | 120s | 60s  |
| Major Health Potion     | 120s | 75s  |
| Minor Stamina Potion    | 120s | 45s  |
| Medium Stamina Potion   | 120s | 60s  |

---

### <strong>🔧 Less Stamina Usage</strong>

- Reduces all stamina costs across actions (combat, running, building, etc.) by **15%**.
- **Conflicts:** Mods that globally modify player stamina.  
  - Compatible with mods that modify stamina on a per-item basis.

---

### <strong>🔧 Better Death Raiser</strong>

- Modifies the way the Death Raiser summons Skeletons:
  - The primary attack will only spawn melee skeletons. 
  - Adds a secondary attack that spawns archer skeletons.
- **Conflicts:**  Mods that change how the Death Raiser functions or what it summons.  
  - Compatible with other mods that modify skeleton spawn stats or gear.

---

### <strong>🔧 Better Summoned Skeletons</strong>

- Increases summoned skeleton speed and adds better looking gear according to Death Raiser level (Skeleton stats are not affected).
- Specific details can be found in the **Docs** tab of the **Nexusmods** page.
- Toggling mid-game requires **client** relog for Summoned Skeleton speed changes to take effect. New gear is applied immediately for newly Summoned Skeletons without relogging.
- **Conflicts:**  Mods that modify Summoned Skeleton speed stats and gear.

---

### <strong>🔧 Better Frost Staff Accuracy</strong>

- Improves Staff of Frost Accuracy.
- **Conflicts:**  Mods that modify the Staff of Frost.

---

### <strong>🔧 Reduced Crossbows Reload Time</strong>

- Crossbows Reload Time Reduced by 1s (3.5s → 2.5s).
- Toggling mid-game requires **client** relog.
- **Conflicts:** Mods that modify Crossbows reload time.

---

### <strong>🔧 Better Trophy Drop Rates</strong>

- Increases trophy drop rate for some creatures.
- Toggling mid-game will only affect newly spawned creatures.
- **Conflicts:** Incompatible with any mod that modifies creature drop rates.

**🔹 Changes**
| Creature      | Old Drop Chance | New Drop Chance           |
|---------------|-----------------|---------------------------|
| Rancid Remains | 10%            | 20%                       |
| Ghost         | 10%             | 20%                       |
| Bear          | 10%             | 20%                       |
| Surtling      | 5%              | 10%                       |
| Draugr Elite  | 10%             | 20%                       |
| Wraith        | 5%              | 20%                       |
| Cultist       | 10%             | 20%                       |
| Fenring       | 10%             | 20%                       |
| Stone Golem   | 5%              | 20%                       |
| Deathsquito   | 5%              | 10%                       |
| Fuling Berserker | 5%           | 10%                       |
| Vile          | 10%             | 20%                       |
| Tick          | 5%              | 10%                       |
| Dverger       | 5%              | 10%                       |
| Seeker Soldier | 5%             | 20%                       |
| Charred Warlock | 5%            | 20%                       |

---

### <strong>🔧 Better Creature Drops</strong>

- Modifies the drop rates and type of items for Fenring, Bat and Dverger.
- Toggling mid-game will only affect newly spawned creatures.
- **Conflicts:** Incompatible with any mod that modifies creature drop rates and drop types.

**🔹 Changes**
- Fenring:    
  - Added Fenris Hair: (between 1 - 2 drops)
  - Replaced Wolf Fang with Fenris Claw
- Bat: 
  - Added Blood Bag (50% chance drop, in addition to Leather Scraps)
- Dverger Rogue, Mage and Ashlands Dvergr: 
  - Increased Soft Tissue drop chance to 100% (from 25%)

---

## <strong>⚙️ Features</strong>

All features below are synced with the server.  
They can be toggled mid-game, unless otherwise specified.

---

### <strong>🔧 Automatic Progression Halt</strong>

- Creatures and destroyable objects do not drop any items unless the previous biome boss has been defeated.  
- Pickable items and chests cannot be picked/opened under the same conditions.  
- Resources from each biome are automatically unlocked when the relevant boss is defeated. There is no need for client relogs or server restarts. However, the immediate area will need to be reloaded by leaving until unloaded by distance.
- Toggling mid-game requires **client** relog or reloading area. 
  - Reloading area means walking/teleporting away from the current zone and coming back. This only applies for destroyable objects like Copper Mines. Pickables and chests will be affected immediately. Creatures will be affected if they are new.
- **Conflicts:** This only applies to vanilla game prefabs. Any mod that adds new prefabs (creatures, resources, pickables) will **not** be included in the Progression Halt system.

**🔹 Specific boss halts**
- Eikthyr: Black Forest objects and creatures
- The Elder: Swamp objects and creatures
- Bonemass: Mountain and Ocean objects and creatures (ocean halted by Bonemass by default)
- Moder: Plains objects and creatures
- Yagluth: Mistlands objects and creatures
- The Queen: Ashlands objects and creatures

---

### <strong>🔧 Halt Ocean Behind Elder</strong>

- Halts Ocean biome resources (Leviathans and Serpents) behind The Elder instead of Bonemass. Requires Automatic Progression Halt to be enabled.   
- **Note:** Ocean biome resources provide Mountain-tier gear and food (so it makes more sense to halt them behind Bonemass), but this option is here if players still want ocean resources earlier.
- Toggling mid-game requires **client** relog or reloading area. 
- **Conflicts:** Same as above.

---

### <strong>🔧 Creature Unleveler By Boss</strong>

- Increases chance of creatures to spawn with a star or two after defeating their relevant biome boss.  
- This happens in a staggered way and will not affect creatures from a biome whose boss has not yet been defeated.  
- Changes are automatically applied when a boss is defeated, including when reverting the boss global key with console commands.  
- If defeating bosses in an unordered way, the changes still apply in their natural order.  
- Some creatures that did not have stars will now gain stars (e.g. Abomination, Lox, Deathsquito), but no creature will pass two stars.
- Specific details can be found in the **Docs** tab of the **Nexusmods** page.
- Toggling mid-game will only affect new areas and newly spawned creatures.  
  - For example, if the mod was enabled and a 2-star Troll was spawned, the Troll will retain its level even if the config is toggled OFF afterwards. Same applies when toggling ON, when defeating bosses or reverting with console commands.
- **Conflicts:**  Mods that change creature levels. (e.g. Creature Level And Loot Control)
  - Automatically disabled if the above is detected.

---

### <strong>🔧 No End-game Raids in Early Biomes</strong>

- Seeker and Charred raids do not trigger in the Black Forest biome.
- **Conflicts:** Incompatible with other mods that modify base raids.

---

### <strong>🔧 Stop Nighttime Invasion</strong>

- Fulings, Seeker and Charred do not spawn in early biomes at night when their relevant bosses are defeated.
- Toggling mid-game requires reloading area if there are already night spawns in place.
- **Conflicts:** Incompatible with other mods that modify creature spawner data.

**🔹 Changes**
| Creatures     | Nighttime Biome Spawns |
|---------------|------------------------|
| Fulings       | Mountain               |
| Seeker        | Plains                 |
| Charred       | Plains                 |

---

### <strong>🔧 Less Ashlands Enemies</strong>

- Reduced the spawn rate and numbers of enemies in Ashlands.  
- This does **not** modify enemy levels or levelup chances (starred enemies).
- Specific details can be found in the **Docs** tab of the **Nexusmods** page.
- Toggling mid-game requires reloading area. 
  - This means moving to a new unloaded area will apply the new/old spawn values depending on the toggle. Areas that are currently active (having a player present) will retain the old config spawn data.  
  - Teleporting or simply walking away for a certain distance will make changes take effect if this config is toggled mid-game.
- **Conflicts:** Mods that modify Ashlands enemy spawn data.

---

### <strong>🔧 Clear Mistlands After Queen</strong>

- Clears Mistlands mist after defeating the Queen.  
- Changes will be applied immediately when the Queen is defeated. No relogs or restarts needed.
- Disabling mid-game requires **client** relog if the Queen is defeated. Enabling mid-game will apply changes immediately.
- **Conflicts:** Mods that modify Mistlands mist. (e.g. MistBeGone by Azumatt)
  - Automatically disabled if the above is detected.

---

### <strong>🔧 Clearer Weather</strong>

- Reduces chance for mist, rain and snowstorms in Meadows, Plains, and Mountains respectively.
- For the Ocean biome, only the mist chance is reduced.
- Toggling mid-game requires **client** relog.
- **Conflicts:**  Weather or seasons mods (Seasons by shundal or Seasonality by RustyMods).
  - Automatically disabled if the above are detected.

---

### <strong>🔧 Craftable Chain</strong>

- Adds a Chain recipe at the Black Forge.
- Toggling mid-game requires the Black Forge crafting menu to be reopened.
- **Conflicts:** Possibly mods that add the same recipe at the normal Forge (not tested).

---

### <strong>🔧 Brighter Lanterns</strong>

- Makes Dverger Lanterns brighter.  
  - Standing lanterns are a bit brighter than the wall mount lanterns.
- Toggling mid-game requires either **client** relog or reloading area (moving/teleporting away from current zone and coming back).
- **Conflicts:**  Mods that modify Dverger Lanterns luminosity.

---

### <strong>🔧 Station Extensions Changes</strong>

- Decreases space requirement for workstation extensions and increases build distance to workstations.  
  - This does **not** increase workstations radius.
- Toggling mid-game require reloading the build menu.
- **Conflicts:** Mods that modify station extensions.

---

### <strong>🔧 Hildir Weight Rewards</strong>

- Increases base carry weight by 25 when turning in Hildir chests (for each chest).  
- This will apply for all characters in the world, but will require **client** relogs when a chest is turned in.  
- Changes are **not** applied when defeating the minibosses.
- Toggling mid-game requires **client** relog.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Stop Running Away</strong>

- Boars and Necks won't flee when alerted.
- Toggling mid-game only affects newly spawned creatures.
- **Conflicts:** Mods that change Boar and Neck AI behavior.

---

### <strong>🔧 Tougher Ships</strong>

- Increases Ships HP by a flat amount.
- Toggling mid-game requires **client** relog or reloading area (moving/teleporting away and back).
- **Conflicts:** Mods that modify ship HP (e.g. Sailing by Smoothbrain).
  - Automatically disabled if the above is detected.

**🔹 HP Changes:**
| Ship      | Old HP | New HP |
|-----------|--------|--------|
| Raft      | 300    | 400    |
| Karve     | 500    | 650    |
| Longship  | 1000   | 1250   |
| Drakkar   | 3000   | 4000   |

---

### <strong>🔧 Max Portals Per Player</strong>

- Sets the number of vanilla portals that can be built by each player. This applies separately for each world.
- The limit is tracked individually for the normal Wood Portal and the Stone Portal. E.g. if the number is set to 5, then a player can build 5 Wood Portals and 5 Stone Portals.
- Modded/custom portal types are not affected.
- Set to -1 for unlimited portals (default).
- Changes will take effect immediately if the number is changed mid-game.
- **Conflicts:** Incompatible with Rare Magic Portal Plus (or any other mod that sets a limit to portals), unless the value is set to -1.

---

### <strong>🔧 Other Section</strong>

- Tankard costs reduced and Iron Nails crafting output doubled.
- Toggling mid-game requires reloading the crafting menu.
- **Conflicts:** No known conflicts.

**🔹 Changes:**
- **Tankard crafting resource amount:**
  • Fine Wood: 4 → 2
- **Iron Nails**
  • Craft Amounted: 10 → 20

---

## <strong>⚙️ QOL</strong>

All features below are synced with the server.  
They can be toggled mid-game, unless otherwise specified.

---

### <strong>🔧 Lighter Metal Weight</strong>

- Modifies ore and metal weight to match Tin Ore weight.
- Toggling mid-game requires **client** relog or reloading area (moving/teleporting away and back).
- **Conflicts:** No known conflicts.

**🔹 Changes:**
- **All Ores:** 10 → 8
- **All Metals:** 12 → 8

---

### <strong>🔧 Larger Pickup Area</strong>

- Item auto-pickup area radius increased from 2 to 3.
- **Conflicts:** This config will automatically be disabled if **BiggerPickupRadius** by mtnewton is detected.

---

### <strong>🔧 No Skill Levels Loss On Death</strong>

- Skills will no longer drop in level when dying. However, the progress towards the next level for each skill will still drop. 
- Example: Level 17 Swords, 30% progress → after death, the progress drops to 0%, but the Swords skill stays at level 17.
- Toggling mid-game requires **client** relog.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Larger Boat Explore Radius</strong>

- Double explore radius on a boat.
- **Conflicts:** Incompatible with any mod that modifies explore radius (e.g. Sailing by Smoothbrain).
  - Automatically disabled if the above is detected.

---

### <strong>🔧 Bigger Wisp Radius</strong>

- Increases wisp radius from 15 to 30.
- **Conflicts:** Incompatible with any mod that modifies the Wisplight (e.g. DeezMistyBalls by Azumatt).
  - Automatically disabled if the above is detected.

---

### <strong>🔧 Friendly Ballistas</strong>

- Ballistas no longer fire on players and tamed creatures.
- **Conflicts:** Incompatible with any mod that changes Ballista behavior (e.g. ImFriendly Dammit by Azumatt).
  - Automatically disabled if the above is detected.

---

### <strong>🔧 Less Fall Damage</strong>

- Reduces fall damage by 40%.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Faster Resource Drops</strong>

- Enemies drop resources faster when dying.
  - Small creatures: 0.5s
  - Large creatures: 5s
- Toggling mid-game will only be affect newly spawned creatures or creatures in inactive areas.
- **Conflicts:** Incompatible with any mod that changes creature resource drop timers (e.g Instant Monster Loot Drop by cjayride).
  - Automatically disabled if the above is detected.

---

### <strong>🔧 Faster Equip</strong>

- Equipping weapons is instant (skips equip animation).  
- Armor equip timers reduced to 1s (from 1s/2s).
- Toggling mid-game requires **client** relog.
- **Conflicts:** Incompatible with any mod that changes equip timers (e.g. InstantEquip by Smoothbrain).
  - Automatically disabled if the above is detected.

---

### <strong>🔧 Move Camera Up While Sailing</strong>

- Moves the camera up by a slight amount when controlling a boat (specific for each boat) to provide enough view ahead.
- **Conflicts:** Incompatible with any mod that changes camera angles when sailing (e.g Sailing by Smoothbrain).
  - Automatically disabled if the above is detected.

---

### <strong>🔧 Shorter Rested Delay</strong>

- Reduces the amount of time needed to get the rested buff from 20 to 10 seconds.
- Toggling mid-game requires re-entering the resting area.
- **Conflicts:** Incompatible with any mod that changes resting delay.

---

### <strong>🔧 More Usable Fuel</strong>

- Ancient Bark can be used as fuel for Kilns. Withered Bones can be used in Shield Generators.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Permanent Lights</strong>

- Makes all light sources permanent and modifies the build costs of light source pieces to use maximum amount of their respective fuel type (wood, resin, coal etc).  
- Fireplace now has an additional 20 Wood cost.  
- Campfire and Fireplace Wood is refundable when destroyed.
- **Conflicts:** Mods that modify light source fuel or build piece costs (Eternal Fire by Digitalroot, FuelEternal by Marf, TorchesEternal by Xenofell, TorchesEternal by wildbill22).  
  - Compatible with mods that add new light sources. This will make them permanent, but will not modify their build costs.
  - Automatically disabled if the above mods are detected.

---

## <strong>⚙️ UI</strong>

Configs in this section are **not** synced with the server.  
All configs can be toggled mid-game.

---

### <strong>🔧 More Loading Tips</strong>

- Modifies some original loadscreen tips.   
- Adds new tips in addition to the original ones.
- Only applies for the English localization.
- **Conflicts:** Incompatible with any mod that changes/adds loading screen tips.

---

### <strong>🔧 Layout for Inventory/Detector/Boat</strong>

- Options to choose the layout of the inventory weight, enemy detector and boat speed. 
  - **New** version uses symbols and filling bars, with the boat speed indicator next to the ship wind indicator. 
  - **Old** version uses text and has the boat speed indicator next to the enemy detector.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Show Inventory Weight and Free Slots</strong>

- Displays current carry weight and max weight values at the bottom left of the screen under the health bar.   
- Displays current number of inventory slots next to the carry weight indicator at the bottom left of the screen.
- Text color changes according to weight/free slots percentage.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Show Enemy Detector</strong>

- Displays an enemy detector next to the inventory weight widget at the bottom left of the screen that counts the number of enemies in close proximity.
- Does not include other players, deer, hare, player-summoned creatures, or tame animals in the enemy count.
- Colors change according to the number of nearby enemies.
- In the **New** UI layout
  - Neutral Dverger are counted and shown in a separate indicator. When attacked, they are counted in the normal enemy indicator.
  - The icon indicating enemies will change according to how many enemies are nearby.
- In the **Old** UI layout
  - Neutral Dverger are counted in parentheses. When attacked, they are counted in the normal enemy counter. 
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Show Boat Speed</strong>

- Displays current ship speed when controlling a boat. Colors change according to speed.
- In the **New** UI layout
  - The speed indicator is displayed above the main sailing widget. If Minimal Status Effects is installed, the speed indicator moves with the main widget.
- In the **Old** UI layout
  - The speed indicator is displayed next to the inventory weight widget at the bottom left of the screen.
- When going forward, only the speed value is displayed.
- When going backwards, "R" is displayed before the speed value.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Show Current Day</strong>

- Displays the number of days spent in the world above the minimap.
- **Conflicts:** This config will automatically be disabled if **MyLittleUI** is detected.

---

### <strong>🔧 Show Current Time</strong>

- Displays the current time above the minimap. Can choose between digital clock and day sections.
- **Digital clock** option shows the time in 24h format.
- **Day sections** option splits the day in the following format: Dawn, Morning, Day, Afternoon, Evening, Dusk, Night
- **Conflicts:** This config will automatically be disabled if **MyLittleUI** is detected.

---

### <strong>🔧 Show Weather Forecast Indicator</strong>

- Displays the next scheduled weather as an icon at the bottom-right of the minimap as well as a time until that weather will change.
- This indicator shows the upcoming weather based on the current biome and weather weights. If the biome has a single weather (Swamp and Ashlands Ocean), then the indicator shows the current weather and the timer is set to --:--. 
- **Conflicts:** No known conflicts.
  - Compatible with weathers added by Seasons and Seasonality mods

---

### <strong>🔧 Smart Biome Indicator</strong>

- Displays the current biome name on the minimap in a specific color according to equipped armor relative to the biome.
- Colors range: purple (not ready for biome), red (hard), orange (ok), yellow (normal), green (easy).
- Colors only change for the first 7 land biomes. The rest are displayed in white.
- **Note:** This only accounts for vanilla gear. Any custom armors will not be counted in the Smart Biome Indicator.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Show Summon Counter</strong>

- Displays a counter for summoned skeletons from the Dead Raiser. This does not count summoned trolls.
- Colors change according to the number of active summons.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Show Online Players</strong>

- Shows a list of online players and the total number of players logged in the current world. Can choose to show it at the bottom-right of the screen or under the minimap.
- Displays maximum 20 players and is hidden if only one player is online.
- The list can be toggled with the **Home** key, but the total number of online players will still be shown. 
- **Conflicts:** If either **Minimal Status Effects** by RandyKnapp or **MyLittleUI** by shundal is installed, this config will automatically be forced on the Bottom-right setting. The same is true for a "no-map" world.

---

### <strong>🔧 Show Owned Resources In Build Menu</strong>

- Displays the total amount of resources in the player's inventory in addition to the required resource amount for the selected piece in the build menu.
- If a player has 20 Wood in their inventory and the build piece requires 2, then "2/20" will be displayed in the resource cost.
- Toggling mid-game requires reopening the build menu.
- **Conflicts:** Valheim Plus, Craft From Containers  
  - Automatically disabled if **Craft From Containers** is detected.

---

### <strong>🔧 Show Boss Power Expiration Message</strong>

- Displays a message in the center of the screen when any Forsaken Power expires.
- **Conflicts:** No known conflicts.

---

### <strong>🔧Player Logout Announce</strong>

- Displays a message when a player logs out in the top-left corner of the screen and in the chat window.
- **Conflicts:** No known conflicts.

---

### <strong>🔧Show Heat Meter in Ashlands</strong>

- Shows a heat meter at the top-center of the screen when in Ashlands water or lava.
- **Conflicts:** No known conflicts.

---

### <strong>🔧 Enemy Nameplate Mode</strong>

- Changes the way enemy nameplates are displayed by changing the  bar style and colors of the nameplate.
- Alerted/aggravated status now changes the color of the creature name (yellow for alerted, red for aggravated).
- Has options for choosing how to display the HP (value or percentage), and can choose between showing both, one of the two, or none.
- Players with PVP status enabled are shown in a different color.
- **Conflicts:** This config will automatically be disabled if either **BetterUI** or **Enhuddlement** is detected.

---

### <strong>🔧 Show Taming Progress</strong>

- Displays current taming percentage of animals that are acclamatizing under the HP bar. 
- This is independent of Enemy Nameplate Mode.
- **Conflicts:** This config will automatically be disabled if **MyLittleUI** by shundal is detected.

---

### <strong>🔧 Item Quality Indicator Mode</strong>

- Options to change the way item quality is displayed by converting the vanilla number to symbols.
- Can arrange the symbols horizontally or vertically.
- **Conflicts:** This config will automatically be disabled if either **BetterUI** or **MyLittleUI** is detected.

---

### <strong>🔧 Symbol For Item Quality</strong>

- Options to choose the symbol used for the item quality indicator.
- Available symbols: Star ★, Circle ●, Diamond ◆, EmptyDiamond ◇
- **Note:** Directly dependent on Item Quality Indicator Mode.

---

### <strong>🔧 Color For Item Quality</strong>

- Options to choose the color used for the item quality indicator.
- Available colors: White, Yellow, Green, Red, Blue, Cyan
- **Note:** Directly dependent on Item Quality Indicator Mode.

---

### <strong>🔧 Better Item Durability Bar</strong>

- Colors the item durability bar gradually, according to curent durability and modifies the sprite to a non-flat texture.
- 100% durability: Green, 50% durability: Yellow, 0% durability: Red
- **Conflicts:** This config will automatically be disabled if either **BetterUI** or **MyLittleUI** is detected.

---

### <strong>🔧 Detailed Hover Information</strong>

- Adds more information when hovering over objects. Master toggle for the following 8 configs.
- Has options for displaying text colored according to fill/progress percentage, or simply white.
- **Conflicts:** This config will automatically be disabled if either **BetterUI** or **MyLittleUI** is detected.

---

### <strong>🔧 Show Container Contents</strong>

- Displays the contents of a chest or container when hovering over it.
- **Conflicts:** This config will automatically be disabled if **MyLittleUI** is detected.

---

### <strong>🔧 Container Hover Mode</strong>

- Choice for the method of displaying Container hover info.
- Available options: current filled / max available, amount of free slots, or percent filled
- **Note:** Directly dependent on Detailed Hover Information.

---

### <strong>🔧 Beehive Hover Mode</strong>

- Choice for the method of displaying Beehive hover info.
- Available options: remaining time, percent, percent and remaining time
- **Note:** Directly dependent on Detailed Hover Information.

---

### <strong>🔧 Plant Hover Mode</strong>

- Choice for the method of displaying Plant hover info.
- Available options: remaining time, percent, percent and remaining time
- **Note:** Directly dependent on Detailed Hover Information.

---

### <strong>🔧 Fermenter Hover Mode</strong>

- Choice for the method of displaying Fermenter hover info.
- Available options: remaining time, percent, percent and remaining time
- **Note:** Directly dependent on Detailed Hover Information.

---

### <strong>🔧 CookingStation Hover Mode</strong>

- Choice for the method of displaying CookingStation hover info.
- Available options: remaining time, percent, percent and remaining time
- **Note:** Directly dependent on Detailed Hover Information.

---

### <strong>🔧 Smelter Hover Mode</strong>

- Choice for the method of displaying Smelter hover info.
- Available options: remaining time. (Future plans to implement bars)
- **Note:** Directly dependent on Detailed Hover Information.

---

### <strong>🔧 Egg Hover Mode</strong>

- Choice for the method of displaying Egg hover info.
- Available options: remaining time, percent, percent and remaining time
- **Note:** Directly dependent on Detailed Hover Information.

---
---

## <strong> 🔮 Future Plans </strong>

- Add the Bear and Vile to the Creature Unleveler feature, but will need to play it out first before I see how it feels.
- Look into the new trinket system and see if there is a need to balance build costs or adrenaline costs.
- Add an alternate way to Permanent Lights that makes things more immersive than the current option. Will probably keep both options available when I implement the alternative.
- Look into adding a secondary attack tot he Staff of Protection that heals, or add the Dverger Heal Staff as a playable item.
- Look into the sorting options for crafting recipes. My previous mod had a section that would reorder the crafting recipes by biome, but it broke with the Bog witch update. I'm looking into how to re-implement that.
- Remove the player charatcter speed slow when eating.
- Create a Smart Dropbox
- Add a skill xp notification
- Add player stats at character menu
- Bar hover mode for progress indicators when hovering over smelters

The above are just ideas I gathered and are not guaranteed to be implemented.  
More features will be added over time. Config descriptions will be updated with known compatibility issues.  

---

## <strong> 💬 Feedback </strong>

Suggestions and bug reports are welcome on the [Posts] or [Bugs] tabs of the Nexusmods page.  
Thanks for checking out Marsarah Tweaks!

## <strong> 🧑‍🤝‍🧑 Credits </strong>
Blaxxun-bloop - for ServerSync

## <strong> 📜 Version History </strong>

Check the Changelog tab.