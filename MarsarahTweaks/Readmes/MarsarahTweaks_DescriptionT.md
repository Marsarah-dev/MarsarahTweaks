# <strong> Marsarah Tweaks </strong>

**Version:** 1.3.2   
**Author:** Marsarah

---

## <strong> 🧰 About the Mod </strong>

Marsarah Tweaks is a modular and optimized port of my previous project, *MarsarahMod*.  
It features numerous code improvements, cleaned-up logic, and **ServerSync** integration — all wrapped into a cleaner, more maintainable package.

Each feature is grouped and configurable, allowing players or server admins to tweak Valheim’s grind and balance just the way they want.

---

## <strong> 🔒 Permissions </strong>

Reuploading this mod, whether in part or in full, is **not permitted**.

---

## <strong> 🧱 Requirements </strong>

This mod requires **BepInEx for Valheim**, available here:  
https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/  
From version 1.3.0 **Jotunn** is also required:  
https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/  
  ⚠️ **Note:** This needs to be installed on the **dedicated server** as well as **all clients**.

---

## <strong> 📦 Installation </strong>

1. Unpack the `.zip` file.
2. Copy `MarsarahTweaks.dll` into your `Valheim/BepInEx/plugins` folder.

Or use a mod manager.

  ⚠️ **Note:** This mod must be installed on both the **server** and **all clients**.

---

## <strong> ⚙️ Configuration </strong>

This mod is **modular and customizable**.

- A config file is generated on first launch:  
  `Valheim/BepInEx/config/Marsarah.MarsarahTweaks.cfg`
- Each feature is in its own section and can be toggled individually.
- Features can be changed manually or using a Configuration Manager.
- Most features support **mid-game toggling**, but some require crafting menus to be reopened, reloading areas or **client** relogs (this is specified in each section below).
- Some configs below will have more detailed changes written in the [Docs] tab of the Nexusmods page. 

  ✅ All features (except UI) are synced using ServerSync.  
  🔄 Players will be disconnected if their mod version differs from the server.

---

## <strong> 💰 Donations </strong>

My mods are and will always be free to use. If you'd like to support my work, you can donate here:
https://paypal.me/Marsarah9

---

## <strong> ⚡ Main Features </strong>

## <strong>🔐 Lock Configuration</strong>

Prevents non-admin players from modifying any configuration values while in-game.  
Useful for dedicated servers to maintain consistent settings.

---

## <strong>🛠️ Grind Reduction</strong>

All features below are synced with the server and can be toggled mid-game.

---

### <strong>🧪 Double Bronze Crafting</strong>

📜 Doubles the amount of **Bronze** produced when combining **Copper and Tin** at a Forge.

- Only affects the **output amount** — resource costs stay the same.  

🔄 Toggling mid-game requires reopening the Forge menu for changes to take effect.

**❗ Conflicts:** TripleBronze by KaceCottam or Triple Bronze JVL by Digitalroot.

---

### <strong>🛡️ Cheaper Gear Recipe Amounts</strong>

📜 Reduces the **amount** (not type) of resources needed to craft or upgrade vanilla gear.

- Focused especially on metal for weapons and armor  
- Rebalances the material amounts for some recipes (e.g. more wood or leather) for game balance  
- Specific details can be found in the **Docs** tab of the **Nexusmods** page

🔄 Toggling mid-game requires reopening the relevant crafting menu.

**❗ Conflicts:** Mods that change gear recipe ingredients.

---

### <strong>⚔️ Alternate Gear Recipe Materials</strong>

📜 Changes gear crafting and upgrade materials to match the **biome progression**.  
- This prevents players from needing to backtrack to earlier biomes to gather outdated materials.
- Other changes are made where it made sense (e.g. Dundr using Iolite instead of Bloodstone)

🔄 Toggling mid-game requires reopening the relevant crafting menu.

**❗ Conflicts:** Mods that modify vanilla gear recipe resources.

**🔧 Changes:**
- **Club:**  
  • Material:         Bone Fragments → Leather Scraps
- **Copper Knife:**  
  • Material:         Greydwarf Eye  → Leather Scraps   
  • Amount Upgrade:   8/16/24        → 2/4/6 
- **Silver Knife:**  
  • Material:         Wood           → Fine Wood  
  • Material:         Iron           → Obsidian  
- **Silver Sword:**  
  • Material:         Wood           → Fine Wood  
  • Amount Craft:     2              → 10  
  • Amount Upgrade:   1/2/3          → 2/4/6  
  • Material:         Iron           → Obsidian  
  • Amount Craft:     5              → 4  
  • Amount Upgrade:   3/6/9          → 2/4/6  
- **Serpent Scale Shield:**  
  • Material:         Iron           → Chitin  
  • Amount Upgrade:   2/4/6          → 1/2/4  
- **Porcupine:**  
  • Material:         Iron           → Black Metal  
- **Arbalest:**  
  • Material:         Wood           → Fine Wood  
- **Wolf Armor Chest:**  
  • Material:         Chain          → Wolf Fang  
  • Amount Craft:     1              → 3  
- **Padded Helmet:**  
  • Material:         Iron           → Black Metal  
- **Padded Cuirass:**  
  • Material:         Iron           → Black Metal  
- **Padded Greaves:**  
  • Material:         Iron           → Black Metal  
- **Linen Cape:**  
  • Material:         Silver         → Black Metal  
- **Eitr-Weave Hood:**  
  • Material:         Iron           → Scale Hide  
  • Amount Craft:     2              → 3  
- **Robes of Embla:**  
  • Material:         Flametal       → Sulfur  
- **Ashen Cape:**  
  • Material:         Flametal       → Sulfur  
- **Dundr:**  
  • Material:         Bloodstone     → Iolite  

---

### <strong>🏗️ Cheaper Build Piece Amounts</strong>

📜 Reduces the **amount** of materials required to craft vanilla build pieces.

- Specific details can be found in the **Docs** tab of the **Nexusmods** page

🔄 Toggling mid-game requires reopening the build menu.

**❗ Conflicts:** Mods that modify vanilla build piece costs.
- Works with mods that add new build pieces (but does not affect them)  

---

### <strong>🏚️ Alternate Build Piece Materials</strong>

📜 Alters some **material types** required for specific vanilla build pieces.  
- Pairs well with the Cheaper Build Piece Amounts option
- Note: This will move the Workbench Toolrack extension from the Mountain to Swamp biome

🔄 Toggling mid-game requires reopening the build menu.

**❗ Conflicts:** Mods that modify vanilla build piece costs.  
✅ Compatible with mods that add new build pieces (changes won't apply to them).

**🔧 Changes:**
- **Tool Rack (Workbench Extension):** Obsidian → Coal (This will move this extension one biome earlier)
- **Darkwood Gate:**                   Iron     → Black Metal
- **Bath Tub:**                        Iron     → Black Metal

---

### <strong>🍲 Food and Mead Modifications</strong>

📜 Adjusts food and mead recipes.

- Changes recipe amounts, outputs, and ingredients  
- All food stack sizes increased to **20**
- Specific details can be found in the **Docs** tab of the **Nexusmods** page

🔄 Requires **client** relog after toggling the config mid-game. 

**❗ Conflicts:** Mods that modify vanilla food or mead recipes.  
✅ Compatible with mods that add new recipes.

---

## <strong>✨ Features</strong>

All features below are synced with the server and can be toggled mid-game (with relog/crafting menu reopen as needed).

---

### <strong>⏸️ Automatic Progression Halt</strong>

📜 Creatures and destroyable objects do not drop any items unless the previous biome boss has been defeated.  
📜 Pickable items and chests cannot be picked/opened under the same conditions.  
- Resources from each biome are automatically unlocked when the relevant boss is defeated. There is no need for client relogs or server restarts. However, the immediate area will need to be reloaded by leaving until unloaded by distance.

🔄 Toggling mid-game requires **client** relog or reloading area. 
- Reloading area means walking/teleporting away from the current zone and coming back. This only applies for destroyable objects like Copper Mines. Pickables and chests will be affected immediately. Creatures will be affected if they are new.

**❗ Conflicts:** This only applies to vanilla game prefabs. Any mod that adds new prefabs (creatures, resources, pickables) will **not** be included in the Progression Halt system.

**🔧 Specific boss halts**
- Eikthyr: Black Forest objects and creatures
- The Elder: Swamp objects and creatures
- Bonemass: Mountain and Ocean objects and creatures (ocean halted by Bonemass by default)
- Moder: Plains objects and creatures
- Yagluth: Mistlands objects and creatures
- The Queen: Ashlands objects and creatures

---

### <strong>⏸️ Halt Ocean Behind Elder</strong>

📜 Halts Ocean biome resources (Leviathans and Serpents) behind The Elder instead of Bonemass. Requires Automatic Progression Halt to be enabled.   
- NOTE: Ocean biome resources provide Mountain-tier gear and food (so it makes more sense to halt them behind Bonemass), but this option is here if players still want ocean resources earlier.

🔄 Toggling mid-game requires **client** relog or reloading area. 

**❗ Conflicts:** Same as above.

---

### <strong>🧥 Early Linen Cape</strong>

📜 Moves the Linen Cape to the **Swamp biome**, renames it to **Fine Cape**, and adds **Poison Resistance**.

- Adjusts material and upgrade requirements  

🔄 Reopen the crafting menu to apply changes if toggling mid-game. If wearing the cape while toggling, a **client relog** is required for Poison Resist to apply.

**❗ Conflicts:** Mods that modify back pieces

**🔧 Changes:**
- **Material:**       Linen thread → Deer Hide
- **Craft Amount:**   20     → 5
- **Upgrade Amount:** 4/8/12 → 2/4/6
- **Material:**       Silver → Iron

---

### <strong>🏃‍♂️ Gear Speed Modifications</strong>

📜 Removes movement speed penalties from heavy/mage gear and adds small bonuses to light armor sets.

🔄 Requires **client relog** to apply changes if toggled mid-game

**❗ Conflicts:** Mods that change gear speed modifiers

**🔧 Changes:**
- **Heavy Gear:** -5% → 0%  
- **Mage Gear:** -2% → 0%  
- **Light Gear Bonuses:**  
  • Troll/Ask: +2%  
  • Root: +1%  
- **Weapon Penalties:**  
  • Battleaxe & Crystal Battleaxe: -20% → -5%  
- **Shield Penalties:**  
  • Tower Shields: -20% → -10%

---

### <strong>⚡ Forsaken Powers Modifications</strong>

📜 Adjusts **cooldowns** and **durations** of each Forsaken Power for more flexible usage.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:** Mods that modify Forsaken Powers

**🔧 Power Timing Adjustments:**

| Power      | Cooldown | Duration |
|------------|----------|----------|
| Eikthyr    | 15 min   | 10 min   |
| Elder      | 15 min   | 10 min   |
| Bonemass   | 17.5 min | 7.5 min  |
| Moder      | 15 min   | 10 min   |
| Yagluth    | 17.5 min | 7.5 min  |
| Queen      | 15 min   | 7.5 min  |
| Fader      | 15 min   | 7.5 min  |

---

### <strong>🏃‍♀️ Faster Character Speed</strong>

📜 Boosts overall character movement speeds for smoother gameplay.

- Terrain is not taken in consideration when modifying movement speed.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:** Mods that modify player movement speed

**🔧 Changes:**
- **Crouch Speed:** 2.0 → 2.2  
- **Swim Speed:** 2.0 → 2.2  
- **Walk Speed:** 1.6 → 2.5  
- **Run Speed:** Unchanged

---

### <strong>💧 Shorter Wet & Potion Cooldowns</strong>

📜 Reduces cooldowns for **potion usage** and the **wet status effect**, making recovery smoother during exploration and combat.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:** Mods that modify status effects for wetness or potions

**🔧 Cooldown Changes:**

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

### <strong>💨 Less Stamina Usage</strong>

📜 Reduces all stamina costs across actions (combat, running, building, etc.) by **15%**.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:** Mods that globally modify player stamina  
✅ Compatible with mods that modify stamina on a per-item basis

---

### <strong>🛡️ Extra Armor Stats</strong>

📜 Enhances gear with **extra stats** based on armor class:

- **Heavy Armor:** Adds HP  
- **Light Armor:** Adds Stamina  
- **Mage Armor:** Adds Eitr (not regen)

🔄 Changes apply instantly without relog if toggled mid-game (may need **client** relog for tooltip refresh)

**❗ Conflicts:** Mods that add HP/Stamina/Eitr to armor  
✅ Compatible with custom armor mods — only vanilla gear is affected

**🔧 Stat Bonuses by Armor Set:**

**HP (Heavy Armor):**

| Set           | Total HP | Breakdown (Head / Chest / Legs) |
|---------------|----------|----------------------------------|
| Bronze        | 10       | 2 / 4 / 4                        |
| Iron          | 20       | 4 / 8 / 8                        |
| Silver        | 30       | 8 / 12 / 10                     |
| Padded        | 40       | 10 / 16 / 14                    |
| Carapace      | 50       | 12 / 20 / 18                    |
| Flametal      | 60       | 14 / 24 / 22                    |
| Ask           | 25       | 5 / 10 / 10                     |

**Stamina (Light Armor):**

| Set       | Total Stamina | Breakdown (Head / Chest / Legs) |
|-----------|----------------|----------------------------------|
| Troll     | 5              | 1 / 2 / 2                        |
| Root      | 10             | 3 / 4 / 3                        |
| Fenris    | 15             | 4 / 6 / 5                        |
| Ask       | 20             | 5 / 8 / 7                        |

**Eitr (Mage Armor):**

| Set         | Total Eitr | Breakdown (Head / Chest / Legs) |
|-------------|------------|----------------------------------|
| Eitr-Weave  | 50         | 10 / 20 / 20                     |
| Embla       | 75         | 15 / 30 / 30                     |

---

### <strong>💀 Better Death Raiser</strong>

📜 Modifies the way the Death Raiser summons Skeletons:

- The primary attack will only spawn melee skeletons. 
- Adds a secondary attack that spawns archer skeletons

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:**  Mods that change how the Death Raiser functions or what it summons.  
✅  Compatible with other mods that modify skeleton spawn stats or gear.

---

### <strong>🦴 Better Summoned Skeletons</strong>

📜 Increases summoned skeleton speed and adds better looking gear according to Death Raiser level (Skeleton stats are not affected).
- Specific details can be found in the **Docs** tab of the **Nexusmods** page

🔄 Toggling mid-game requires **client** relog for Summoned Skeleton speed changes to take effect. New gear is applied immediately for newly Summoned Skeletons without relogging.

**❗ Conflicts:**  Mods that modify Summoned Skeleton speed stats and gear.

---

### <strong>❄️ Better Frost Staff Accuracy</strong>

📜 Improves Staff of Frost Accuracy.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:**  Mods that modify the Staff of Frost.

---

### <strong>🏹 Reduced Crossbows Reload Time</strong>

📜 Crossbows Reload Time Reduced by 1s (3.5s → 2.5s).

🔄 Toggling mid-game requires **client** relog

**❗ Conflicts:** Mods that modify Crossbows reload time.

---

### <strong>🔥 Less Ashlands Enemies</strong>

📜 Reduced the spawn rate and numbers of enemies in Ashlands.  
- This does **not** modify enemy levels or levelup chances (starred enemies).
- Specific details can be found in the **Docs** tab of the **Nexusmods** page

🔄 Toggling mid-game requires reloading area. 
- This means moving to a new unloaded area will apply the new/old spawn values depending on the toggle. Areas that are currently active (having a player present) will retain the old config spawn data.  
- Teleporting or simply walking away for a certain distance will make changes take effect if this config is toggled mid-game.

**❗ Conflicts:** Mods that modify Ashlands enemy spawn data.

---

### <strong>🔧 Gear Upgrade Unlock</strong>

📜 Gear from Meadows, Black Forest, Mistlands and Ashlands can be upgraded to lvl 4 within their respective biomes.  
- This is accomplished by reducing the crafting workstation level requirements for Leather Set, Troll Set, Bronze Set, all Mistlands sets, and all Ashlands sets.  
- NOTE: Special Ashlands weapons that use gems can be crafted to lvl 3 (e.g. Klossen max level upgrade is 3 (from 2)).

🔄 Can be enabled/disabled during gameplay, but requires the relevant crafting menu to be reopened to take effect.

**❗ Conflicts:** Mods that modify the required station level for the named sets.

---

### <strong>💡 Permanent Lights</strong>

📜 Makes all light sources permanent and modifies the build costs of light source pieces to use maximum amount of their respective fuel type (wood, resin, coal etc).  
- Fireplace now has an additional 20 Wood cost.  
- Campfire and Fireplace Wood is refundable when destroyed.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:** Mods that modify light source fuel or build piece costs.  
✅ Compatible with mods that add new light sources. This will make them permanent, but will not modify their build costs.

---

### <strong>🌫️ Clear Mistlands After Queen</strong>

📜 Clears Mistlands mist after defeating the Queen.  
- Changes will be applied immediately when the Queen is defeated. No relogs or restarts needed.

🔄 Disabling mid-game requires **client** relog if the Queen is defeated. Enabling mid-game will apply changes immediately.

**❗ Conflicts:** Mods that modify Mistlands mist.

---

### <strong>⚙️ Craftable Chain</strong>

📜 Adds a Chain recipe at the Black Forge.

🔄 Can be enabled/disabled during gameplay, but requires the Black Forge crafting menu to be reopened to take effect.

**❗ Conflicts:** Possibly mods that add the same recipe at the normal Forge (not tested).

---

### <strong>💡 Brighter Lanterns</strong>

📜 Makes Dverger Lanterns brighter.  
- Standing lanterns are a bit brighter than the wall mount lanterns.

🔄 Toggling mid-game requires either **client** relog or reloading area (moving/teleporting away from current zone and coming back).

**❗ Conflicts:**  Mods that modify Dverger Lanterns luminosity.

---

### <strong>🌤️ Clearer Weather</strong>

📜 Reduces chance for mist and snowstorms in Meadows, Plains, Ocean and Mountains respectively.

🔄 Toggling mid-game requires **client** relog.

**❗ Conflicts:**  Weather or seasons mods.

---

### <strong>👹 Creature Unleveler By Boss</strong>

📜 Increases chance of creatures to spawn with a star or two after defeating their relevant biome boss.  
- This happens in a staggered way and will not affect creatures from a biome whose boss has not been defeated.  
- Changes are automatically applied when a boss is defeated, including when reverting the boss global key with console commands.  
- If defeating bosses in an unordered way, the changes still apply in their natural order.  
- Some creatures that did not have stars will now gain stars (e.g. Abomination, Lox, Deathsquito)
- Specific details can be found in the **Docs** tab of the **Nexusmods** page

🔄 Can be enabled/disabled during gameplay, but changes will only be applied to new areas and newly spawned creatures.  
- For example, if the mod was enabled and a 2-star Troll was spawned, the Troll will retain its level even if the config is toggled OFF afterwards. Same applies when toggling ON, when defeating bosses or reverting with console commands.

**❗ Conflicts:**  Mods that change creature levels. (e.g. Creature Level And Loot Control)

---

### <strong>🏋️‍♀️ Hildir Weight Rewards</strong>

📜 Increases base carry weight by 25 when turning in Hildir chests (for each chest).  
- This will apply for all characters in the world, but will require **client** relogs when a chest is turned in.  
- Changes are **not** applied when defeating the minibosses.

🔄 Toggling mid-game requires **client** relog.

**❗ Conflicts:** No known conflicts.

---

### <strong>🛠️ Station Extensions Changes</strong>

📜 Decreases space requirement for workstation extensions and increases build distance to workstations.  
- This does **not** increase workstations radius.

🔄 Can be enabled/disabled during gameplay, but requires reloading the build menu.

**❗ Conflicts:** Mods that modify station extensions.

---

### <strong>🐗 Stop Running Away</strong>

📜 Boars and Necks won't flee when alerted.

🔄 Toggling mid-game only affects newly spawned creatures.

**❗ Conflicts:** Mods that change Boar and Neck AI behavior.

---

### <strong>🎯 Better Trophy Drop Rates</strong>

📜 Increases trophy drop rate for some creatures.

🔄 Can be enabled/disabled during gameplay, but the changes will only apply to newly spawned creatures.

**❗ Conflicts:** Incompatible with any mod that modifies creature trophy drop rates.

**🔧 Changes**
| Creature      | Old Drop Chance | New Drop Chance           |
|---------------|-----------------|---------------------------|
| Rancid Remains | 10%            | 20%                       |
| Surtling      | 5%              | 10%                       |
| Draugr Elite  | 10%             | 20%                       |
| Wraith        | 5%              | 20%                       |
| Cultist       | 10%             | 20%                       |
| Fenring       | 10%             | 20%                       |
| Stone Golem   | 5%              | 20%                       |
| Deathsquito   | 5%              | 10%                       |
| Fuling Berserker | 5%           | 10%                       |
| Tick          | 5%              | 10%                       |
| Dverger       | 5%              | 10%                       |
| Seeker Soldier | 5%             | 20%                       |
| Charred Warlock | 5%            | 20%                       |

---

### <strong>🚢 Tougher Ships</strong>

📜 Increases Ships HP by a flat amount.

🔄 Toggling mid-game requires **client** relog or reloading area (moving/teleporting away and back).

**❗ Conflicts:** Mods that modify ship HP (sailing mods probably).

**🔧 HP Changes:**
| Ship      | Old HP | New HP |
|-----------|--------|--------|
| Raft      | 300    | 400    |
| Karve     | 500    | 650    |
| Longship  | 1000   | 1250   |
| Drakkar   | 3000   | 4000   |

---

### <strong>🚪 Pocket Portal</strong>

📜 Adds a new portal that is functionally equal to the normal portal but is built from a special material, the Portal Core.
- The Portal Core takes takes only one inventory slot and can be crafted at a lvl 4 Workbench with resources gathered from the Mountain and Black Forest biomes.  
- The portal's shape is exactly the same as a normal portal, but has blue effects.
- It does **not** allow the carying of metal or other prohibited items through it. It functions like a normal portal and can connect to any other portal.
- Can craft and carry any number of Portal Cores, but can only build **one** Pocket Portal per player. 

🔄 Toggling mid-game requires reloading the build/crafting menu. 
- Even if this config is disabled, the prefabs are still crated and existing portals or cores will not be removed from the world or inventory. Only the ability to craft them is affected by the toggle.

**❗ Conflicts:** No known conflicts. Requires Jotunn.

**🔧 Portal Core Resource Requirements:**
- **Surtling Core:** 5
- **Fine Wood:** 20
- **Freeze Gland:** 5
- **Obsidian:** 20

---

### <strong>🚪 Max Portals Per Player</strong>

📜 Sets the number of portals that can be built by each player. This applies separately for each world.
- This number applies individually for the normal and the stone portal. E.g. If the number is set to 5, then a player can build 5 normal portals and 5 stone portals.
- This does not affect the number of Pocket Portals a player can build.
- Set to -1 for unlimited portals (default).

🔄 Changes will take effect immediately if the number is changed mid-game.

**❗ Conflicts:** Incompatible with Rare Magic Portal Plus (or any other mod that sets a limit to portals), unless the value is set to -1.

---

### <strong>🛠️ Other Section</strong>

📜 Tankard costs reduced and Iron Nails crafting output doubled.

🔄 Can be enabled/disabled during gameplay, but requires crafting menu reload.

**❗ Conflicts:** No known conflicts.

**🔧 Changes:**
- **Tankard crafting resource amount:**
  • Fine Wood: 4 → 2
- **Iron Nails**
  • Amount crafted: 10 → 20

---

## <strong>✨ QOL</strong>

All features below are synced with the server and can be toggled mid-game (with relog/reload area as specified).

---

### <strong>⚒️ Lighter Metal Weight</strong>

📜 Modifies metal weight to match Tin Ore weight.
- All ores and metals will have reduced weight for easier transport.

🔄 Toggling mid-game requires **client** relog or reloading area (moving/teleporting away and back).

**❗ Conflicts:** No known conflicts.

**🔧 Changes:**
- **All Ores:** 10 → 8
- **All Metals:** 12 → 8

---

### <strong>📦 Larger Pickup Area</strong>

📜 Item auto-pickup area slightly increased.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

**🔧 Changes:**
- **Pickup Range:** Radius increase: 2 → 3

---

### <strong>💀 No Skill Levels Loss On Death</strong>

📜 Skills will no longer drop in level when dying. However, the progress towards the next level for each skill will still drop. 
- Example: Level 17 Swords, 30% progress → after death, the progress drops to 0%, but the Swords skill stays at level 17.

🔄 Toggling mid-game requires **client** relog.

**❗ Conflicts:** No known conflicts.

---

### <strong>⛵ Larger Boat Explore Radius</strong>

📜 Double explore radius on a boat.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that modifies explore radius (sailing mods).

---

### <strong>✨ Bigger Wisp Radius</strong>

📜 Increases wisp radius.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that modifies the Wisplight (e.g. DeezMistyBalls).

**🔧 Changes:**
- **Wisp Radius:** 15 → 30

---

### <strong>🏹 Friendly Ballistas</strong>

📜 Ballistas no longer fire on players and tamed creatures.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that changes Ballista behavior (e.g. ImFriendly Dammit).

---

### <strong>🪂 Less Fall Damage</strong>

📜 Reduces fall damage by 40%.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>💨 Faster Resource Drops</strong>

📜 Enemies drop resources faster when dying.

🔄 Can be enabled/disabled during gameplay, but changes will only be applied to newly spawned creatures or creatures in inactive areas.

**❗ Conflicts:** Incompatible with any mod that changes creature resource drop timers (e.g Instant Monster Loot Drop).

**🔧 Changes:**
- **Resource Drop Speed:** 
  - Small creatures: 0.5s
  - Large creatures: 5s

---

### <strong>⚡ Faster Equip</strong>

📜 Equipping weapons is instant (skips equip animation).  
📜 Armor equip timers reduced to 1s (from 1s/2s).

🔄 Toggling mid-game requires **client** relog.

**❗ Conflicts:** Incompatible with any mod that changes equip timers (e.g. InstantEquip).

---

### <strong>⬆️ Move Camera Up While Sailing</strong>

📜 Moves the camera up by a slight amount when controlling a boat (specific for each boat) to provide enough view ahead.   

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that changes camera angles when sailing (e.g Sailing by Smoothbrain).

---

### <strong>🧘 Shorter Rested Delay</strong>

📜 Reduces the amount of time needed to get the rested buff from 20 to 10 seconds.

🔄 Toggling mid-game requires re-entering the resting area.

**❗ Conflicts:** Incompatible with any mod that changes resting delay.

---

### <strong>⛽ More Usable Fuel</strong>

📜 Ancient Bark can be used as fuel for Kilns. Withered Bones can be used in Shield Generators.

🔄 Can be enabled/disabled during gameplay. 

**❗ Conflicts:** No known conflicts.

---

## <strong>🖥️ UI</strong>

Configs in this section are **not** synced with the server.  
All configs can be toggled mid-game.

---

### <strong>📜 More Loading Tips</strong>

📜 Modifies some original loadscreen tips.   
📜 Adds new tips in addition to the original ones.
- Only applies for the English localization.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that changes/adds loading screen tips.

---

### <strong>⚖️ Show Inventory Weight and Free Slots</strong>

📜 Displays current carry weight and max weight values at the bottom left of the screen under the health bar.   
📜 Displays current number of inventory slots next to the carry weight indicator at the bottom left of the screen.
- Text color changes according to weight/free slots percentage.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>👀 Show Enemy Detector</strong>

📜 Displays an enemy detector next to the inventory weight widget at the bottom left of the screen.
- Counts the number of enemies in close proximity.
- Does not include other players, deer, hare, player-summoned creatures, or tame animals in the enemy count.
- Neutral Dverger are counted in parentheses. When attacked, the number goes into the normal enemy counter. If Alternate UI Layout is enabled, neutral Dverger are counted with a separate indicator.
- Colors change according to the number of nearby enemies. If Alternate UI Layout is enabled, the icon will change according to how many enemies are nearby.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>⛵ Show Boat Speed</strong>

📜 Displays current ship speed next to the inventory weight widget at the bottom left of the screen.
- If the Alternate UI Layout is used, this indicator is displayed above the main sailing widget. If Minimal Status Effects is installed, the speed indicator moves with the main widget.
- When going forward, only the speed value is displayed.
- When going backwards, "R" is displayed before the speed value.
- The speed counter only shows when controlling a boat.
- Colors change according to speed.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>🕰️ Show Time And Day</strong>

📜 Displays current time of day above the minimap using day sections (Dawn, Morning, Day, Afternoon, Evening, Dusk, Night).  
📜 Shows the number of days spent in the world.
- If the Alternate UI Layout is used, an additional symbol is displayed next to the time depending on the time of day.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>⏰ Time - 24 Hour Format</strong>

📜 Sub-section for the previous config, enabling 24-hour format for displaying time.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>🌍 Smart Biome Indicator</strong>

📜 Displays the current biome name on the minimap in a specific color according to equipped armor relative to the biome.
- Colors range: purple (not ready for biome), red (hard), orange (ok), yellow (normal), green (easy).
- Colors only change for the first 7 land biomes. The rest are displayed in white.
- **NOTE:** This only accounts for vanilla gear. Any custom armors will not be counted in the Smart Biome Indicator.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>⚔️ Show Summon Counter</strong>

📜 Displays a counter for summoned skeletons from the Dead Raiser. This does not count summoned trolls.
- Colors change according to the number of active summons.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>🧑‍🤝‍🧑 Show Online Players</strong>

📜 Shows a list of online players and the total number of players on the bottom right of the screen.
- Not displayed if only one player is online.
- Displays maximum 20 players.
- The list can be toggled with the **Home** key, but the total number of online players will still be shown. 

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>🗺️ Show Online Players Under Minimap</strong>

📜 Sub-section for the previous config.
- Shows online players under minimap instead of bottom right when Show Online Players is enabled.
- This option is automatically disabled on a no-map world, or when Minimal Status Effects is installed, and cannot be toggled on.  

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>🧱 Show Owned Resources In Build Menu</strong>

📜 Displays the total amount of resources in the player's inventory in addition to the required resource amount for the selected piece in the build menu.
- If a player has 20 Wood in their inventory and the build piece requires 2, then "2/20" will be displayed in the resource cost.

🔄 Toggling mid-game requires reopening the build menu.

**❗ Conflicts:** Valheim Plus, Craft From Containers  
**Note:** If Craft From Containers is installed, this config will override the available resource lookup to the player inventory instead of nearby containers. Simply disable this config to see available resources from chests again.  
**Note:** This will not disable Craft From Container's ability to craft from containers; it's just a visual incompatibility, same as Valheim Plus

---

### <strong>⏳ Show Boss Power Expiration Message</strong>

📜 Displays a message in the center of the screen when any Forsaken Power expires.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>🔈 Player Logout Announce</strong>

📜 Displays a message when a player logs out in the top-left corner of the screen and in the chat window.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>🔥 Show Heat Meter in Ashlands</strong>

📜 Shows a heat meter at the top-center of the screen when in Ashlands water or lava.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

---

### <strong>🖥️ Alternate UI Layout</strong>

📜 Alternates the layout of this mod's UI by using symbols instead of words for Inventory Weight and Slots, Enemy Counter, Summons Counter, Boat Speed, and Time of Day. 
📜 Repositions the Boat Speed widget to the minimap area.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.
✅ Compatible with Minimal Status Effects. This mod automatically detects the existence of Minimal Status Effects and repositions the Boat Speed widget accordingly.

---
---

## <strong> 🔮 Future Plans </strong>

More features will be added over time. Config descriptions will be updated with known compatibility issues.  
If using other mods that overlap in functionality, disable affected sections in this mod’s config to avoid conflicts.

---

## <strong> 💬 Feedback </strong>

Suggestions and bug reports are welcome on the [Posts] or [Bugs] tabs of the Nexusmods page.  
Thanks for checking out Marsarah Tweaks!

## <strong> 🧑‍🤝‍🧑 Credits </strong>
Azumatt - for a guide on YouTube about how to setup ServerSync in a project  
Blaxxun-bloop - for ServerSync

## <strong> 📜 Version History </strong>

v1.3.1
- All UI configs are no longer synced with server. Players can individually configure this mod's UI elements individually.
- Show Online Players Under Minimap cannot be toggled on with Minimal Status Effects installed.

v1.3.0
- **Mod now requires Jotunn**  
- Fixed an issue with Player Logout Announce not working in certain situations.
- Moved position of the Boat Speed Indicator in the Alternate UI mode to be above the wind indicator for no-map game modifier or when Minimal Status Effects in installed.
- Added symbols for the Time of Day widget according to day section in Alternate UI mode.  
- Modified symbols for Enemy Detector in Alternate UI mode according to the number of nearby enemies.
- Updated compatibility notes for: Show Owned Resources In Build Menu and Creature Unleveler By Boss.
- Updated descriptions to include specific changes for the following sections: Alternate Gear Recipe Materials, Alternate Build Piece Materials, Early Linen Cape.
- Renamed section 'Clear Mistlands' to 'Clear Mistlands After Queen'.
- **Feature addition:** Alternate Gear Recipes - Changed Dundr to use Iolite instead of Bloodstone for crafting and upgrading.
- **New Feature:** Pocket Portal
- **New Feature:** Max Portals Per Player

v1.2.0
- Fixed an issue with Progression Halt where players could get the contents of halted chests by simply destroying them.
- Fixed an issue with the Move Camera Up While Sailing section, where if a player would log out while controlling a ship, then the camera would be stuck in a high position when logging back in.
- Fixed an issue with UI widgets (Time and day; online players) not showing on worlds with no map enabled. Now they display properly with the No Map global setting on.
- **Feature change:** Show Boat Speed - Removed the letter "F" when displaying boat forward speed (kept "R" for reverse);
- **Feature change:** Show Online Players - Now also shows the total number of players logged in. Swapped the order of header and player list when displayed at the botom right corner (header is now below the player list).
- **Feature change:** Show Online Players - Added key toggle (Home key) to show/hide the player list (the total number of online players will always be displayed if the config is enabled - this toggle just hides the player list).
- **New QOL feature:** Shorter Rested Delay
- **New QOL feature:** More Usable Fuel
- **New UI feature:** Show Owned Resources In Build Menu
- **New UI feature:** Show Boss Power Expiration Message
- **New UI feature:** Player Logout Announce
- **New UI feature:** Show Heat Meter in Ashlands
- **New UI feature:** Alternate UI Layout

v1.1.2
- Modified the way Progressionn Halt reads prefab names. This change is specifically targeting a previous incompatibility with Ventrure Location Reset mod where it was changing parts of prefab names after resetting dungeons. This patch makes these two mods compatible.
- **New Feature:** Halt Ocean Behind Elder. This provides an extra option for players and sever admins if they prefer Ocean resources to be available earlier. Ocean-tier items and food are equal to Mountain-tier items and food, which is why this option is disabled by default, but still available if wanted.
- Rearranged config entries in the config file, which means that old configs will have extra unused config entries (those can be safely deleted)

v1.1.1
- Added Draugr Archers to Progression Halt (previous oversight)
- Removed Pickable Bone Piles found in Meadows from Progression Halt

v1.1.0
- **New QOL feature:** Move Camera Up While Sailing
- Added forgotten items from old mod port to Cheaper Build Piece Amounts (all banners Leather Scraps: 6 → 5)
- Added Shipwreck to Progression Halt (halted by Eikthyr)

v1.0.1
- Fixed a bug where features that were dependent on checking global keys (Progression Halt, Creature Unleveler) were dependent on having Clear Mistlands section enabled. Now these sections work as intended even if that option is off.
- Made UI sections server synced like the rest of the configs.

v1.0.0
- Initial Upload