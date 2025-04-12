# Marsarah Tweaks

**Version:** 1.0.0  
**Author:** Marsarah

---

## 🧰 About the Mod

Marsarah Tweaks is a modular and optimized port of my previous project, *MarsarahMod*.  
It features numerous code improvements, cleaned-up logic, and **ServerSync** integration — all wrapped into a cleaner, more maintainable package.

Each feature is grouped and configurable, allowing players or server admins to tweak Valheim’s grind and balance just the way they want.

---

## 🔒 Permissions

Reuploading this mod, whether in part or in full, is **not permitted**.

---

## 🧱 Requirements

This mod requires **BepInEx for Valheim**, available here:  
https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/

---

## 📦 Installation

1. Unpack the `.zip` file.
2. Copy `MarsarahTweaks.dll` into your `Valheim/BepInEx/plugins` folder.

> ⚠️ **Note:** This mod must be installed on both the **server** and **all clients**.

---

## ⚙️ Configuration

This mod is **modular and customizable**.

- A config file is generated on first launch:  
  `Valheim/BepInEx/config/Marsarah.MarsarahTweaks.cfg`
- Each feature is in its own section and can be toggled individually.
- Features can be changed manually or via a config manager (e.g. Configuration Manager).
- Most features support **mid-game toggling**, but some require crafting menus to be reopened, reloading areas or **client** relogs (this is specified in each config).
- Some configs below will have more detailed changes written in the [Docs] tab of this page. 

> ✅ All gameplay-affecting features are synced using ServerSync.  
> ❌ UI-related features are local and not synced.  
> 🔄 Players will be disconnected if their mod version differs from the server.

---

## ⚡ Main Features

<details>
<summary><strong>🔐 Lock Configuration</strong></summary>

Prevents non-admin players from modifying any configuration values while in-game.  
Useful for dedicated servers to maintain consistent settings.

</details>

---

<details>
<summary><strong>🛠️ Grind Reduction</strong></summary>

All features below are synced with the server and can be toggled mid-game.

<details>
<summary><strong>🧪 Double Bronze Crafting</strong></summary>

📜 Doubles the amount of **Bronze** produced when combining **Copper and Tin** at a Forge.

- Only affects the **output amount** — resource costs stay the same.  

🔄 Requires reopening the Forge menu to take effect if toggled during gameplay.

**❗ Conflicts:** Mods that modify Bronze crafting output.

</details>

<details>
<summary><strong>🛡️ Cheaper Gear Recipe Amounts</strong></summary>

📜 Reduces the **amount** (not type) of resources needed to craft or upgrade vanilla gear.

- Focused especially on expensive items like metal weapons and armor  
- Some recipes rebalance material use (e.g. replacing bronze with leather) for game balance  

🔄 Requires reopening the relevant crafting menu after toggling the config mid-game

**❗ Conflicts:** Mods that change gear recipe ingredients.

</details>

<details>
<summary><strong>⚔️ Alternate Gear Recipe Materials</strong></summary>

📜 Changes gear crafting and upgrade materials to match the **biome progression**.  
- This prevents players from needing to backtrack to earlier biomes to gather outdated materials.

🔄 Requires reopening the relevant crafting menu after toggling the config mid-game

**❗ Conflicts:** Mods that modify vanilla gear recipe resources.

</details>

<details>
<summary><strong>🏗️ Cheaper Build Piece Amounts</strong></summary>

📜 Reduces the **amount** of materials required to craft vanilla build pieces.

- Works with mods that add new build pieces (but does not affect them)  

🔄 Requires reopening the build menu after toggling the config mid-game

**❗ Conflicts:** Mods that modify vanilla build piece costs.

</details>

<details>
<summary><strong>🏚️ Alternate Build Piece Materials</strong></summary>

📜 Alters some **material types** required for specific vanilla build pieces.  
- Pairs well with the Cheaper Build Piece Amounts option
- Note: This will move the Workbenck Toolrack extension from the Mountain to Swamp biome

🔄 Requires reopening the build menu after toggling the config mid-game

**❗ Conflicts:** Mods that modify vanilla build piece costs.  
✅ Compatible with mods that add new build pieces (changes won't apply to them).

</details>

<details>
<summary><strong>🍲 Food and Mead Modifications</strong></summary>

📜 Adjusts food and mead recipes for a smoother experience:

- Changes recipe amounts, outputs, and ingredients  
- All food stack sizes increased to **20**

🔄 Requires **client** relog after toggling the config mid-game. 

**❗ Conflicts:** Mods that modify vanilla food or mead recipes.  
✅ Compatible with mods that add new recipes.

</details>

</details>

---

<details>
<summary><strong>✨ Features</strong></summary>

All features below are synced with the server and can be toggled mid-game (with relog/crafting menu reopen as needed).

---

<details>
<summary><strong>🧥 Early Linen Cape</strong></summary>

📜 Moves the Linen Cape to the **Swamp biome**, renames it to **Fine Cape**, and adds **Poison Resistance**.

- Adjusts material and upgrade requirements  

🔄 Reopen the crafting menu to apply changes if toggling mid-game. If wearing the cape while toggling, a **client relog** is required for Poison Resist to apply

**❗ Conflicts:** Mods that modify back pieces

</details>

<details>
<summary><strong>🏃‍♂️ Gear Speed Modifications</strong></summary>

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

</details>

<details>
<summary><strong>⚡ Forsaken Powers Modifications</strong></summary>

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

</details>

<details>
<summary><strong>🏃‍♀️ Faster Character Speed</strong></summary>

📜 Boosts overall character movement speeds for smoother gameplay.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:** Mods that modify player movement speed

**🔧 Changes:**
- **Crouch Speed:** 2.0 → 2.2  
- **Swim Speed:** 2.0 → 2.2  
- **Walk Speed:** 1.6 → 2.5  
- **Run Speed:** Unchanged

</details>

<details>
<summary><strong>💧 Shorter Wet & Potion Cooldowns</strong></summary>

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

</details>

<details>
<summary><strong>💨 Less Stamina Usage</strong></summary>

📜 Reduces all stamina costs across actions (combat, running, building, etc.) by **15%**.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:** Mods that globally modify player stamina  
✅ Compatible with mods that modify stamina on a per-item basis

</details>

<details>
<summary><strong>🛡️ Extra Armor Stats</strong></summary>

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

</details>

<details>
<summary><strong>💀 Better Death Raiser</strong></summary>

📜 Modifies the way the Death Raiser summons Skeletons:

- The primary attack will only spawn melee skeletons. 
- Adds a secondary attack that spawns archer skeletons

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:**  Mods that change how the Death Raiser functions or what it summons.  
✅  Compatible with other mods that modify skeleton spawn stats or gear.

</details>

<details>
<summary><strong>🦴 Better Summoned Skeletons</strong></summary>

📜 Increases summoned skeleton speed and adds better looking gear according to Death Raiser level (Skeleton stats are not affected).

🔄 Toggling mid-game requires **client** relog for Summoned Skeleton speed changes to take effect. New gear is applied immediately for newly Summoned Skeletons without relogging.

**❗ Conflicts:**  Mods that modify Summoned Skeleton speed stats and gear.

</details>


<details>
<summary><strong>❄️ Better Frost Staff Accuracy</strong></summary>

📜 Improves Staff of Frost Accuracy.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:**  Mods that modify the Staff of Frost.

</details>

<details>
<summary><strong>🏹 Reduced Crossbows Reload Time</strong></summary>

📜 Crossbows Reload Time Reduced by 1s (3.5s → 2.5s).

🔄 Toggling mid-game requires **client** relog

**❗ Conflicts:** Mods that modify Crossbows reload time.

</details>

<details>
<summary><strong>🔥 Less Ashlands Enemies</strong></summary>

📜 Reduced the spawn rate and numbers of enemies in Ashlands.  
- This does **not** modify enemy levels or levelup chances (starred enemies).

🔄 Toggling mid-game requires reloading area. 
- This means moving to a new unloaded area will apply the new/old spawn values depending on the toggle. Areas that are currently active (having a player present) will retain the old config spawn data.  
- Teleporting or simply walking away for a certain distance will make changes take effect if this config is toggled mid-game.

**❗ Conflicts:** Mods that modify Ashlands enemy spawn data.

</details>

<details>
<summary><strong>🔧 Gear Upgrade Unlock</strong></summary>

📜 Gear from Meadows, Black Forest, Mistlands and Ashlands can be upgraded to lvl 4 within their respective biomes.  
- This is accomplished by reducing the crafting workstation level requirements for Leather Set, Troll Set, Bronze Set, all Mistlands sets, and all Ashlands sets.  
- NOTE: Special Ashlands weapons that use gems can be crafted to lvl 3 (e.g. Klossen max level upgrade is 3 (from 2)).

🔄 Can be enabled/disabled during gameplay, but requires the relevant crafting menu to be reopened to take effect.

**❗ Conflicts:** Mods that modify the required station level for the named sets.

</details>

<details>
<summary><strong>💡 Permanent Lights</strong></summary>

📜 Makes all light sources permanent and modifies the build costs of light source pieces to use maximum amount of their respective fuel type (wood, resin, coal etc).  
- Fireplace now has an additional 20 Wood cost.  
- Campfire and Fireplace Wood is refundable when destroyed.

🔄 Changes apply instantly without relog if toggled mid-game

**❗ Conflicts:** Mods that modify light source fuel or build piece costs.  
✅ Compatible with mods that add new light sources. This will make them permanent, but will not modify their build costs.

</details>

<details>
<summary><strong>🌫️ Clear Mistlands</strong></summary>

📜 Clears Mistlands mist after defeating the Queen.  
- Changes will be applied immediately when the Queen is defeated. No relogs or restarts needed.

🔄 Disabling mid-game requires **client** relog if the Queen is defeated. Enabling mid-game will apply changes immediately.

**❗ Conflicts:** Mods that modify Mistlands mist.

</details>

<details>
<summary><strong>⚙️ Craftable Chain</strong></summary>

📜 Adds a Chain recipe at the Black Forge.

🔄 Can be enabled/disabled during gameplay, but requires the Black Forge crafting menu to be reopened to take effect.

**❗ Conflicts:** Possibly mods that add the same recipe at the normal Forge (not tested).

</details>

<details>
<summary><strong>💡 Brighter Lanterns</strong></summary>

📜 Makes Dverger Lanterns brighter.  
- Standing lanterns are a bit brighter than the wall mount lanterns.

🔄 Toggling mid-game requires either **client** relog or reloading area (moving/teleporting away from current zone and coming back).

**❗ Conflicts:**  Mods that modify Dverger Lanterns luminosity.

</details>

<details>
<summary><strong>🌤️ Clearer Weather</strong></summary>

📜 Reduces chance for mist and snowstorms in Meadows, Plains, Ocean and Mountains respectively.

🔄 Toggling mid-game requires **client** relog.

**❗ Conflicts:**  Weather or seasons mods.

</details>

<details>
<summary><strong>👹 Creature Unleveler By Boss</strong></summary>

📜 Increases chance of creatures to spawn with a star or two after defeating their relevant biome boss.  
- This happens in a staggered way and will not affect creatures from a biome whose boss has not been defeated.  
- Changes are automatically applied when a boss is defeated, including when reverting the boss global key with console commands.  
- If defeating bosses in an unordered way, the changes still apply in their natural order.  
- Some creatures that did not have stars will now gain stars (e.g. Abomination, Lox, Deathsquito)

🔄 Can be enabled/disabled during gameplay, but changes will only be applied to new areas and newly spawned creatures.  
- For example, if the mod was enabled and a 2-star Troll was spawned, the Troll will retain its level even if the config is toggled OFF afterwards. Same applies when toggling ON, when defeating bosses or reverting with console commands.

**❗ Conflicts:**  Mods that change creature levels.

</details>

<details>
<summary><strong>🏋️‍♀️ Hildir Weight Rewards</strong></summary>

📜 Increases base carry weight by 25 when turning in Hildir chests (for each chest).  
- This will apply for all characters in the world, but will require **client** relogs when a chest is turned in.  
- Changes are NOT applied when defeating the minibosses.

🔄 Toggling mid-game requires **client** relog.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>🛠️ Station Extensions Changes</strong></summary>

📜 Decreases space requirement for workstation extensions and increases build distance to workstations.  
- This does not increase workstations radius.

🔄 Can be enabled/disabled during gameplay, but requires reloading the build menu.

**❗ Conflicts:** Mods that modify station extensions.

</details>

<details>
<summary><strong>🐗 Stop Running Away</strong></summary>

📜 Boars and Necks won't flee when alerted.

🔄 Toggling mid-game only affects newly spawned creatures.

**❗ Conflicts:** Mods that change Boar and Neck AI behavior.

</details>

<details>
<summary><strong>⏸️ Automatic Progression Halt</strong></summary>

📜 Creatures and destroyable objects do not drop any items unless the previous biome boss has been defeated.  
📜 Pickable items and chests cannot be picked/opened under the same conditions.  
- Resources from each biome are automatically unlocked when the relevant boss is defeated. There is no need for client relogs or server restarts.

🔄 Toggling mid-game requires **client** relog or reloading area. 
- Reloading area means walking/teleporting away from the current zone and coming back. This only applies for destroyable objects like Copper Mines. Pickables and chests will be affected immediately. Creatures will be affected if they are new.

**❗ Conflicts:** This only applies to vanilla game prefabs. Any mod that adds new prefabs (creatures, resources, pickables) will **not** be included in the Progression Halt system.

</details>

<details>
<summary><strong>🎯 Better Trophy Drop Rates</strong></summary>

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

</details>

<details>
<summary><strong>🚢 Tougher Ships</strong></summary>

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

</details>

<details>
<summary><strong>🛠️ Other Section</strong></summary>

📜 Tankard costs reduced and Iron Nails crafting output doubled.

🔄 Can be enabled/disabled during gameplay, but requires crafting menu reload.

**❗ Conflicts:** No known conflicts.

**🔧 Changes:**
- **Tankard crafting resource amount:**
  • Fine Wood: 4 → 2
- **Iron Nails**
  • Amount crafted: 10 → 20

</details>

</details>

---

<details>
<summary><strong>✨ QOL</strong></summary>

All features below are synced with the server and can be toggled mid-game (with relog/reload area as specified).

---

<details>
<summary><strong>⚒️ Lighter Metal Weight</strong></summary>

📜 Modifies metal weight to match Tin Ore weight.
- All ores and metals will have reduced weight for easier transport.

🔄 Toggling mid-game requires **client** relog or reloading area (moving/teleporting away and back).

**❗ Conflicts:** No known conflicts.

**🔧 Changes:**
- **All Ores:** 10 → 8
- **All Metals:** 12 → 8

</details>

<details>
<summary><strong>📦 Larger Pickup Area</strong></summary>

📜 Item auto-pickup area slightly increased.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

**🔧 Changes:**
- **Pickup Range:** Radius increase: 2 → 3

</details>

<details>
<summary><strong>💀 No Skill Levels Loss On Death</strong></summary>

📜 Skills will no longer drop in level when dying. However, the progress towards the next level for each skill will still drop. 
- Example: Level 17 Swords, 30% progress → after death, the progress drops to 0%, but the Swords skill stays at level 17.

🔄 Toggling mid-game requires **client** relog.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>⛵ Larger Boat Explore Radius</strong></summary>

📜 Double explore radius on a boat.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that modifies explore radius.

</details>

<details>
<summary><strong>✨ Bigger Wisp Radius</strong></summary>

📜 Increases wisp radius.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that modifies the Wisplight.

**🔧 Changes:**
- **Wisp Radius:** 15 → 30

</details>

<details>
<summary><strong>🏹 Friendly Ballistas</strong></summary>

📜 Ballistas no longer fire on players and tamed creatures.
- Prevents accidental damage from ballistas to players and their pets.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that changes Ballista behavior.

</details>

<details>
<summary><strong>🪂 Less Fall Damage</strong></summary>

📜 Reduces fall damage by 40%.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>💨 Faster Resource Drops</strong></summary>

📜 Enemies drop resources faster when dying.

🔄 Can be enabled/disabled during gameplay, but changes will only be applied to newly spawned creatures or creatures in inactive areas.

**❗ Conflicts:** Incompatible with any mod that changes creature resource drop timers.

**🔧 Changes:**
- **Resource Drop Speed:** 
  - Small creatures: 0.5s
  - Large creatures: 5s

</details>

<details>
<summary><strong>⚡ Faster Equip</strong></summary>

📜 Equipping weapons is instant (skips equip animation).  
📜 Armor equip timers reduced to 1s (from 1s/2s).

🔄 Toggling mid-game requires **client** relog.

**❗ Conflicts:** Incompatible with any mod that changes equip timers.

</details>

</details>

---

<details>
<summary><strong>🖥️ UI</strong></summary>

Configs in this section are **not** synced with server

---

<details>
<summary><strong>📜 More Loading Tips</strong></summary>

📜 Modifies some original loadscreen tips.   
📜 Adds new tips in addition to the original ones.
- Only applies for the English localization.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** Incompatible with any mod that changes/adds loading screen tips.

</details>

<details>
<summary><strong>⚖️ Show Inventory Weight and Free Slots</strong></summary>

📜 Displays current carry weight and max weight values at the bottom left of the screen under the health bar.   
📜 Displays current number of inventory slots next to the carry weight indicator at the bottom left of the screen.
- Text color changes according to weight/free slots percentage.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>👀 Show Enemy Detector</strong></summary>

📜 Displays an enemy detector next to the inventory weight widget at the bottom left of the screen.
- Counts the number of enemies in close proximity.
- Does not include other players, deer, hare, player-summoned creatures, or tame animals in the enemy count.
- Neutral Dverger are counted in parentheses. When attacked, the number goes into the normal enemy counter.
- Colors change according to the number of nearby enemies.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>⛵ Show Boat Speed</strong></summary>

📜 Displays current ship speed next to the inventory weight widget at the bottom left of the screen.
- When going forward, "F" is displayed before the speed value.
- When going backwards, "R" is displayed before the speed value.
- The speed counter only shows when controlling a boat.
- Colors change according to speed.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>🕰️ Show Time And Day</strong></summary>

📜 Displays current time of day above the minimap using day sections (Dawn, Morning, Day, Afternoon, Evening, Dusk, Night).  
📜 Shows the number of days spent in the world.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>⏰ Time - 24 Hour Format</strong></summary>

📜 Sub-section for the previous config, enabling 24-hour format for displaying time.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>🌍 Smart Biome Indicator</strong></summary>

📜 Displays the current biome name on the minimap in a specific color according to equipped armor relative to the biome.
- Colors range: purple (not ready for biome), red (hard), orange (ok), yellow (normal), green (easy).
- Colors only change for the first 7 land biomes. The rest are displayed in white.
- **NOTE:** This only accounts for vanilla gear. Any custom armors will not be counted in the Smart Biome Indicator.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>⚔️ Show Summon Counter</strong></summary>

📜 Displays a counter for summoned skeletons from the Dead Raiser. This does not count summoned trolls.
- Colors change according to the number of active summons.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>🧑‍🤝‍🧑 Show Online Players</strong></summary>

📜 Shows online players on the bottom right of the screen.
- Not displayed if only one player is online.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>

<details>
<summary><strong>🗺️ Show Online Players Under Minimap</strong></summary>

📜 Sub-section for the previous config.
- Shows online players under minimap instead of bottom right when Show Online Players is enabled.

🔄 Can be enabled/disabled during gameplay.

**❗ Conflicts:** No known conflicts.

</details>


</details>

---

## 🔮 Future Plans

More features will be added over time. Config descriptions will be updated with known compatibility issues.  
If using other mods that overlap in functionality, disable affected sections in this mod’s config to avoid conflicts.

---

## 💬 Feedback

Suggestions and bug reports are welcome on the [Posts] tab.  
Thanks for checking out Marsarah Tweaks!

## 🧑‍🤝‍🧑 Credits
Azumatt - for a guide on YouTube about how to setup ServerSync in a project   
Blaxxun-bloop - for ServerSync