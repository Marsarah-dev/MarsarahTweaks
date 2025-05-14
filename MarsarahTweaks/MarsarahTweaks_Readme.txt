Marsarah Tweaks v1.1.2
================================================================
This mod is a port of my previous project, MarsarahMod which is now deprecated. It features numerous code optimizations and fixes, along with ServerSync integration and several new features.

PERMISSIONS
================================================================
Reuploading this mod, whether in part or in full, is not permitted.

REQUIREMENTS
================================================================
This mod requires BepInEx for Valheim which can be downloaded from Thunderstore:
https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/

INSTALLATION
================================================================
1. Unpack the .zip file.
2. Copy MarsarahTweaks.dll into your Valheim/BepInEx/plugins folder.

NOTE: This mod needs to be installed on the SERVER and all CLIENTS

CONFIGURATION
================================================================
This mod is modular and customizable.

Each feature is grouped into its own section in the config file, which is automatically created on first launch:
Valheim/BepInEx/config/Marsarah.MarsarahTweaks.cfg

Individual features can be enabled or disabled by editing this file or in-game with a config manager.

If you’re using another mod that changes similar gameplay elements (e.g. creature scaling), disable the overlapping section in this mod’s config to avoid conflicts. Specific compatibility issues will be mentioned in each config description if known.

This mod uses Server Sync. All clients need to have the same version as the server, otherwise they will be disconnected at login. All configs are synced between server and clients.

MOD CONFIGS
================================================================

========================= [Main Mod] ===========================

--------------------- [Lock Configuration] ---------------------
• If on, only server admins can change the configuration.

====================== [Grind Reduction] =======================
► All configs in this section are synced with server.

-------------------- [Double Bronze Crafting] ------------------

► Description:
  Outputs double the usual amount of Bronze when combining Copper and Tin at a Forge.
  Only affects the crafted output (does not alter resource costs).

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the Forge crafting menu to be reopened to take effect.

► Conflicts:
  Incompatible with other mods that modify the crafted output of Bronze.

----------------- [Cheaper Gear Recipe Amounts] ----------------

► Description:
  Reduces the amount of resources needed to craft/upgrade vanilla gear (especially metal). Resource types remain the same.
  Some recipes shift material balance (e.g. more leather instead of bronze) to preserve game balance.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the relevant crafting menu to be reopened to take effect.

► Conflicts:
  Incompatible with other mods that modify gear recipe resources.

▼ Changes (only modified items/resources are listed):

== Weapons ==

>> Meadows <<
- Club
  • Upgrade (Wood):              0/0/0    → 2/4/6  
  • Upgrade (Bone Fragments):    5/10/15  → 2/4/6

- Stagbreaker
  • Craft (Bone Fragments):      0        → 2
  • Upgrade (Bone Fragments):    10/20/30 → 8/16/24

- Crude Bow
  • Upgrade (Leather Scraps):    4/8/12   → 3/6/9

- Wood Shield
  • Craft (Wood):                10       → 8
  • Craft (Resin):               4        → 0
  • Upgrade (Resin):             2/4/6    → 0/0/0

- Wood Arrow
  • Craft (Wood):                8        → 5

- Fire Arrow
  • Craft (Wood):                8        → 5
  • Craft (Resin):               8        → 5

- Stone Axe
  • Upgrade (Wood):              0/0/0    → 1/2/3

- Flint Knife
  • Upgrade (Wood):              0/0/0    → 1/2/3

- Flint Spear
  • Craft (Wood):                10       → 8

- Flint Axe
  • Craft (Leather Scraps):      0        → 1
  • Upgrade (Wood):              0/0/0    → 1/2/3
  • Upgrade (Leather Scraps):    2/4/6    → 1/2/3

- Flint Arrow
  • Craft (Wood):                8        → 5

>> Black Forest <<
- Copper Knife
  • Upgrade (Wood):              0/0/0    → 1/2/3
  • Upgrade (Copper):            4/8/12   → 3/6/9
  • Upgrade (Greydwarf Eye):     8/16/24  → 6/12/18

- Bronze Mace
  • Upgrade (Wood):              0/0/0    → 1/2/3
  • Upgrade (Bronze):            4/8/12   → 3/6/9
  • Upgrade (Leather Scraps):    0/0/0    → 1/2/3

- Bronze Sword
  • Craft (Leather Scraps):      2        → 4
  • Upgrade (Bronze):            4/8/12   → 3/6/9

- Bronze Axe
  • Upgrade (Wood):              0/0/0    → 1/2/3
  • Upgrade (Bronze):            4/8/12   → 3/6/9

- Bronze Atgeir
  • Craft (Wood): 				 10 	  → 8
  • Upgrade (Wood): 			 0/0/0 	  → 2/4/6
  • Upgrade (Leather Scraps):    0/0/0    → 1/2/3

- Bronze Buckler
  • Upgrade (Bronze):            5/10/15  → 4/8/12

- Bronze Pickaxe
  • Craft (Bronze):              10       → 7
  • Upgrade (Bronze):            5/10/15  → 4/8/12

- Bronzehead Arrow
  • Craft (Wood):                8        → 5

>> Swamp <<
- Iron Mace
  • Craft (Iron):                20       → 10
  • Upgrade (Wood):              0/0/0    → 2/4/6
  • Upgrade (Iron):              10/20/30 → 5/10/15
  • Upgrade (Leather Scraps):    0/0/0    → 1/2/3

- Iron Sledge
  • Craft (Iron):                30       → 15
  • Craft (Ymir Flesh):          4        → 6
  • Upgrade (Iron):              15/30/45 → 7/14/21
  • Upgrade (Ymir Flesh):        2/4/6    → 0/0/0

- Battleaxe
  • Craft (Ancient Bark):        30       → 20
  • Craft (Iron):                35       → 18
  • Upgrade (Iron):              15/30/45 → 7/14/21
  • Upgrade (Leather Scraps):    0/0/0    → 1/2/3

- Iron Sword
  • Craft (Iron):                20       → 10
  • Upgrade (Wood):              1/2/3    → 2/4/6
  • Upgrade (Iron):              10/20/30 → 5/10/15

- Iron Axe
  • Craft (Iron):                20       → 10
  • Upgrade (Wood):              0/0/0    → 2/4/6
  • Upgrade (Iron):              10/20/30 → 5/10/15
  • Upgrade (Leather Scraps):    1/2/3    → 2/4/6

- Iron Atgeir
  • Craft (Iron):                30       → 15
  • Upgrade (Wood):              0/0/0    → 3/6/9
  • Upgrade (Iron):              15/30/45 → 7/14/21

- Huntsman Bow
  • Craft (Iron):                20       → 10
  • Upgrade (Iron):              10/20/30 → 5/10/15

- Iron Buckler
  • Craft (Iron):                10       → 6
  • Upgrade (Iron):              5/10/15  → 3/6/9

- Iron Pickaxe
  • Craft (Iron):                20       → 10
  • Upgrade (Iron):              10/20/30 → 5/10/15

- Ironhead Arrow
  • Craft (Wood):                8        → 5

>> Mountains <<
- Silver Knife
  • Craft (Silver):              10       → 7
  • Upgrade (Silver):            5/10/15  → 4/8/12

- Frostner
  • Craft (Ancient Bark):        10       → 15
  • Craft (Silver):              30       → 20
  • Upgrade (Ancient Bark):      0/0/0    → 3/6/9
  • Upgrade (Silver):            15/30/45 → 7/14/21

- Silver Sword
  • Craft (Wood):                2        → 15
  • Craft (Silver):              40       → 20
  • Craft (Leather Scraps):      3        → 5
  • Craft (Iron):                5        → 3
  • Upgrade (Wood):              1/2/3    → 3/6/9
  • Upgrade (Silver):            20/40/60 → 7/14/21
  • Upgrade (Iron):              3/6/9    → 1/2/3

- Flesh Rippers
  • Craft (Silver):              10       → 5

- Draugr Fang
  • Craft (Silver):              20       → 10
  • Craft (Guck):                10       → 7
  • Upgrade (Silver):            10/20/30 → 5/10/15

- Crystal Battle Axe
  • Craft (Silver):              30       → 15
  • Craft (Crystal):             10       → 3
  • Upgrade (Silver):            15/30/45 → 7/14/21
  • Upgrade (Crystal):           0/0/0    → 1/2/3

- Obsidian/Frost/Poison Arrow
  • Craft (Wood):                8        → 5
  • Craft (Obsidian):            4        → 3

- Silver Arrow
  • Craft (Wood):                8        → 5

>> Ocean <<
- Abyssal Razor
  • Craft (Chitin):              20       → 10
  • Upgrade (Fine Wood):         0/0/0    → 2/4/6
  • Upgrade (Chitin):            10/20/30 → 5/10/15
  • Upgrade (Leather Scraps):    0/0/0    → 1/2/3

- Abyssal Harpoon
  • Craft (Chitin):              30       → 15

- Serpent Scale Shield
  • Craft (Fine Wood):           10       → 20
  • Craft (Iron):                4        → 3
  • Upgrade (Iron):              2/4/6    → 1/2/3

>> Plains <<
- Porcupine
  • Craft (Fine Wood):           5        → 4
  • Craft (Iron):                20       → 10
  • Craft (Linen Thread):        10       → 5
  • Upgrade (Fine Wood):         0/0/0    → 2/4/6
  • Upgrade (Linen Thread):      0/0/0    → 5/10/15

- Black Metal Knife
  • Upgrade (Fine Wood):         0/0/0    → 1/2/3

- Black Metal Sword
  • Craft (Fine Wood):           2        → 3
  • Craft (Black Metal):         20       → 10
  • Upgrade (Fine Wood):         0/0/0    → 2/4/6
  • Upgrade (Black Metal):       10/20/30 → 5/10/15

- Black Metal Atgeir
  • Craft (Black Metal):         30       → 15
  • Upgrade (Fine Wood):         0/0/0    → 3/6/9
  • Upgrade (Black Metal):       15/30/45 → 7/14/21

- Black Metal Axe
  • Craft (Black Metal):         20       → 10
  • Upgrade (Fine Wood):         0/0/0    → 2/4/6
  • Upgrade (Black Metal):       10/20/30 → 5/10/15

- Black Metal Shield
  • Upgrade (Chain):             2/4/6    → 0/0/0

- Black Metal Tower Shield
  • Craft (Chain):               7        → 6
  • Upgrade (Chain):             2/4/6    → 0/0/0

- Needle Arrow
  • Craft (Needle):              4        → 3

>> Mistlands <<
- Jotun Bane
  • Craft (Iron):                15       → 10
  • Upgrade (Yggdrasil Wood):    0/0/0    → 1/2/3
  • Upgrade (Iron):              10/20/30 → 5/10/15
  • Upgrade (Refined Eitr):      1/2/3    → 2/4/6

- Carapace Spear
  • Upgrade (Yggdrasil Wood):    5/10/15  → 2/4/6

- Mistwalker
  • Craft (Fine Wood):           3        → 5
  • Craft (Iron):                15       → 10
  • Upgrade (Fine Wood):         0/0/0    → 1/2/3
  • Upgrade (Iron):              10/20/30 → 5/10/15
  • Upgrade (Refined Eitr):      5/10/15  → 2/4/6

- Himminafl
  • Craft (Refined Eitr):        15       → 10
  • Upgrade (Yggdrasil Wood):    0/0/0    → 2/4/6
  • Upgrade (Refined Eitr):      15/30/45 → 5/10/15

- Skol and Hati
  • Craft (Fine Wood):           4        → 5
  • Upgrade (Fine Wood):         0/0/0    → 1/2/3

- Demolisher
  • Craft (Iron):                20       → 10
  • Upgrade (Iron):              15/30/45 → 5/10/15

- Krom
  • Craft (Iron):                30       → 15
  • Craft (Bronze):              20       → 10
  • Upgrade (Iron):              15/30/45 → 5/10/15
  • Upgrade (Bronze):            10/20/30 → 5/10/15

- Carapace Shield
  • Upgrade (Refined Eitr):      3/6/9    → 2/4/6

- Carapace Buckler
  • Craft (Carapace):            16       → 15
  • Upgrade (Carapace):          8/16/24  → 7/14/21
  • Upgrade (Refined Eitr):      3/6/9    → 2/4/6

- Spinesnap
  • Craft (Bone Fragments):      40       → 30
  • Upgrade (Bone Fragments):    20/40/60 → 15/30/45
  • Upgrade (Refined Eitr):      0/0/0    → 2/4/6

- Carapace Arrow
  • Craft (Carapace):            4        → 2
  • Craft (Wood):                8        → 5

- Blackmetal Bolt
  • Craft (Black Metal):         2        → 1
  • Craft (Wood ):               8        → 5

- Bone Bolt
  • Craft (Bone Fragments):      8        → 5

- Carapace Bolt
  • Craft (Wood):                8        → 5

- Iron Bolt
  • Craft (Wood):                8        → 5

- Dead Raiser
  • Craft (Refined Eitr):        16       → 15
  • Upgrade (Refined Eitr):      8/16/24  → 5/10/15

- Staff of Embers
  • Craft (Yggdrasil Wood):      20       → 15
  • Craft (Refined Eitr):        16       → 15
  • Upgrade (Refined Eitr):      8/16/24  → 5/10/15

- Staff of Frost
  • Craft (Yggdrasil Wood):      20       → 15
  • Craft (Refined Eitr):        16       → 15
  • Upgrade (Refined Eitr):      8/16/24  → 5/10/15

- Staff of Protection
  • Craft (Yggdrasil Wood):      20       → 15
  • Craft (Refined Eitr):        16       → 15
  • Upgrade (Refined Eitr):      8/16/24  → 5/10/15

>> Ashlands <<
- Flametal Mace
  • Craft (Flametal):            15       → 10
  • Upgrade (Flametal):          8/16/24  → 5/10/15

- Flametal Mace (with gemstone)
  • Upgrade (Flametal):          8/16/24  → 5/10/15

- Splintir
  • Craft (Flametal):            6        → 10
  • Upgrade (Flametal):          6/12/18  → 5/10/15
  • Upgrade (Bonemaw Tooth):     3/6/9    → 2/4/6

- Splintir (with gemstone)
  • Upgrade (Flametal):          6/12/18  → 5/10/15

- Nidhogg
  • Craft (Flametal):            12       → 10
  • Upgrade (Charred Bone):      0/0/0    → 1/2/3
  • Upgrade (Flametal):          10/20/30 → 5/10/15

- Nidhogg (with gemstone)
  • Upgrade (Flametal):          6/12/18  → 5/10/15

- Slayer
  • Craft (Flametal):            30       → 15
  • Upgrade (Flametal):          15/30/45 → 7/14/21
  • Upgrade (Asksvin Hide):      5/10/15  → 4/8/12

- Slayer (with gemstone)
  • Upgrade (Flametal):          15/30/45 → 5/14/21

- Berserkir Axes
  • Craft (Flametal):            24       → 15
  • Upgrade (Charred Bone):      0/0/0    → 2/4/6
  • Upgrade (Flametal):          15/30/45 → 7/14/21

- Ash Fang
  • Craft (Charred Bone):        16       → 15
  • Craft (Flametal):            4        → 5
  • Craft (Bonemaw Tooth):       5        → 4
  • Upgrade (Flametal):          5/10/15  → 4/8/12
  • Upgrade (Bonemaw Tooth):     5/10/15  → 4/8/12

- Ripper (with gemstone)
  • Upgrade (Flametal):          8/16/24  → 5/10/15

- Charred Arrow
  • Craft (Ashwood):             8        → 5

- Charred Bolt
  • Craft (Ashwood):             8        → 5

- Staff of the Wild
  • Upgrade (Celestial Feather): 3/6/9    → 2/4/6

- Dundr
  • Upgrade (Celestial Feather): 3/6/9    → 2/4/6

- Trollstav
  • Upgrade (Flametal):          3/6/9    → 2/4/6

== Armor ==

- Rag Tunic
  • Craft (Leather Scraps):      5        → 6
  • Upgrade (Leather Scraps):    5/10/15  → 4/8/12

- Rag Pants
  • Craft (Leather Scraps):      5        → 6
  • Upgrade (Leather Scraps):    5/10/15  → 4/8/12

- Leather Helmet
  • Craft (Deer Hide):           6        → 7
  • Upgrade (Deer Hide):         6/12/18  → 5/10/15
  • Upgrade (Bone Fragments):    5/10/15  → 4/8/12

- Leather Tunic
  • Craft (Deer Hide):           6        → 7
  • Upgrade (Deer Hide):         6/12/18  → 5/10/15
  • Upgrade (Bone Fragments):    5/10/15  → 4/8/12

- Leather Pants
  • Craft (Deer Hide):           6        → 7
  • Upgrade (Deer Hide):         6/12/18  → 5/10/15
  • Upgrade (Bone Fragments):    5/10/15  → 4/8/12

- Deer Leather Cape
  • Craft (Deer Hide):           4        → 5
  • Craft (Bone Fragments):      5        → 0
  • Upgrade (Deer Hide):         4/8/12   → 3/6/9
  • Upgrade (Bone Fragments):    5/10/15  → 2/4/6

- Troll Leather Helmet
  • Craft (Bone Fragments):      3        → 0
  • Upgrade (Troll Hide):        2/4/6    → 3/6/9
  • Upgrade (Bone Fragments):    1/2/3    → 0/0/0

- Troll Leather Tunic
  • Craft (Troll Hide):          5        → 6
  • Upgrade (Troll Hide):        2/4/6    → 3/6/9

- Troll Leather Pants
  • Craft (Troll Hide):          5        → 6
  • Upgrade (Troll Hide):        2/4/6    → 3/6/9

- Troll Hide Cape
  • Craft (Troll Hide):          10       → 4
  • Craft (Bone Fragments):      10       → 5
  • Upgrade (Troll Hide):        5/10/15  → 3/6/9
  • Upgrade (Bone Fragments):    5/10/15  → 3/6/9

- Root Mask
  • Craft (Root):                10       → 5
  • Upgrade (Ancient Bark):      5/10/15  → 4/8/12

- Root Hrnesk
  • Craft (Root):                10       → 5
  • Upgrade (Ancient Bark):      5/10/15  → 4/8/12

- Root Leggings
  • Craft (Root):                10       → 5
  • Upgrade (Ancient Bark):      5/10/15  → 4/8/12

- Iron Helmet
  • Craft (Iron):                20       → 10
  • Upgrade (Iron):              5/10/15  → 4/8/12

- Iron Scale Mail
  • Craft (Iron):                20       → 10
  • Upgrade (Iron):              5/10/15  → 4/8/12

- Iron Greaves
  • Craft (Iron):                20       → 10
  • Upgrade (Iron):              5/10/15  → 4/8/12

- Fenris Hood
  • Craft (Fenris Hair):         20       → 10
  • Craft (Wolf Pelt):           2        → 3
  • Upgrade (Fenris Hair):       5/10/15  → 4/8/12
  • Upgrade (Wolf Pelt):         4/8/12   → 1/2/3

- Fenris Coat
  • Craft (Fenris Hair):         20       → 10
  • Craft (Wolf Pelt):           5        → 4
  • Craft (Leather Scraps):      10       → 7
  • Upgrade (Fenris Hair):       5/10/15  → 4/8/12
  • Upgrade (Wolf Pelt):         3/6/9    → 2/4/6
  • Upgrade (Leather Scraps):    4/8/12   → 3/6/9

- Fenris Leggings
  • Craft (Fenris Hair):         20       → 10
  • Craft (Wolf Pelt):           5        → 4
  • Craft (Leather Scraps):      10       → 7
  • Upgrade (Fenris Hair):       5/10/15  → 4/8/12
  • Upgrade (Wolf Pelt):         3/6/9    → 2/4/6
  • Upgrade (Leather Scraps):    4/8/12   → 3/6/9

- Drake Helmet
  • Craft (Silver):              20       → 10
  • Craft (Drake Trophy):        2        → 1
  • Upgrade (Silver):            5/10/15  → 4/8/12
  • Upgrade (Wolf Pelt):         0/0/0    → 1/2/3

- Wolf Armor Chest
  • Craft (Silver):              20       → 10
  • Upgrade (Silver):            5/10/15  → 4/8/12
  • Upgrade (Wolf Pelt):         2/4/6    → 1/2/3

- Wolf Armor Legs
  • Craft (Silver):              20       → 10
  • Upgrade (Silver):            5/10/15  → 4/8/12
  • Upgrade (Wolf Pelt):         2/4/6    → 1/2/3
  • Upgrade (Wolf Fang):         1/2/3    → 0/0/0

- Wolf Fur Cape
  • Craft (Wolf Pelt):           6        → 5
  • Craft (Silver):              4        → 2
  v Upgrade (Wolf Pelt):         4/8/12   → 1/2/3
  • Upgrade (Silver):            2/4/6    → 1/2/3

- Padded Helmet
  • Craft (Iron):                10       → 5
  • Upgrade (Iron):              5/10/15  → 3/6/9

- Padded Cuirass
  • Craft (Iron):                10       → 5

- Padded Greaves
  • Craft (Iron):                10       → 5

- Linen Cape
  • Craft (Linen Thread):        20       → 10
  • Upgrade (Linen Thread):      4/8/12   → 2/4/6

- Lox Cape
  • Craft (Lox Pelt):            6        → 5

- Eitr-Weave Hood
  • Craft (Linen Thread):        16       → 15
  • Craft (Iron):                2        → 0
  • Upgrade (Linen Thread):      8/16/24  → 5/10/15

- Eitr-Weave Robe
  • Craft (Refined Eitr):        20       → 15

- Eitr-Weave Trousers
  • Craft (Refined Eitr):        20       → 15

- Carapace Helmet
  • Craft (Carapace):            16       → 15
  • Upgrade (Carapace):          8/16/24  → 5/10/15

- Feather Cape
  • Craft (Scale Hide):          5        → 7
  • Craft (Refined Eitr):        20       → 10
  • Upgrade (Scale Hide):        5/10/15  → 3/6/9

- Hood of Embla
  • Craft (Linen Thread):        16       → 15
  • Upgrade (Linen Thread):      8/16/24  → 5/10/15

- Robes of Embla
  • Craft (Refined Eitr):        20       → 15
  • Craft (Flametal):            5        → 0
  • Upgrade (Flametal):          2/4/6    → 0/0/0

- Trousers of Embla
  • Craft (Refined Eitr):        20       → 15

- Hood of Ask
  • Upgrade (Asksvin Hide):      5/10/15  → 4/8/12

- Robes of Ask
  • Upgrade (Asksvin Hide):      5/10/15  → 4/8/12

- Trousers of Ask
  • Upgrade (Asksvin Hide):      5/10/15  → 4/8/12

- Flametal Helmet
  • Craft (Flametal):            16       → 10
  • Craft (Charred Bone):        2        → 3
  • Upgrade (Flametal):          8/16/24  → 5/10/15
  • Upgrade (Charred Bone):      0/0/0    → 1/2/3
  • Upgrade (Refined Eitr):      2/4/6    → 0/0/0

- Flametal Breastplate
  • Craft (Flametal):            20       → 10
  • Upgrade (Flametal):          10/20/30 → 5/10/15
  • Upgrade (Charred Bone):      0/0/0    → 1/2/3

- Flametal Greaves
  • Craft (Flametal):            20       → 10
  • Upgrade (Flametal):          10/20/30 → 5/10/15
  • Upgrade (Charred Bone):      0/0/0    → 1/2/3

- Ashen Cape
  • Craft (Flametal):            5        → 0

== Others ==
- Dvergr Lantern
  • Craft (Bronze):              2        → 1

- Mechanical Spring
  • Craft (Iron):                3        → 1


-------------- [Alternate Gear Recipe Materials] ---------------

► Description:
  Changes some gear material requirements for crafting or upgrading vanilla gear to match the biome the item comes from. Thus a player will not need to be pushed back to the previous biome to get the required material to craft it (where possible).

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the relevant crafting menu to be reopened to take effect.

► Conflicts:
  Incompatible with other mods that modify gear recipe resources.

▼ Changes (only modified items/resources are listed):

- Club
  • Material:         Bone Fragments → Leather Scraps

- Copper Knife
  • Material:         Greydwarf Eye  → Leather Scraps 
  • Amount Upgrade:   8/16/24        → 2/4/6 

- Silver Knife
  • Material:         Wood           → Fine Wood
  • Material:         Iron           → Obsidian

- Silver Sword
  • Material:         Wood           → Fine Wood
  • Amount Craft:     2              → 10 
  • Amount Upgrade:   1/2/3          → 2/4/6
  • Material:         Iron           → Obsidian
  • Amount Craft:     5              → 4
  • Amount Upgrade:   3/6/9          → 2/4/6

- Serpent Scale Shield
  • Material:         Iron           → Chitin
  • Amount Upgrade:   2/4/6          → 1/2/4

- Porcupine
  • Material:         Iron           → Black Metal

- Arbalest
  • Material:         Wood           → Fine Wood

- Wolf Armor Chest
  • Material:         Chain          → Wolf Fang
  • Amount Craft:     1              → 3

- Padded Helmet
  • Material:         Iron           → Black Metal

- Padded Cuirass
  • Material:         Iron           → Black Metal

- Padded Greaves
  • Material:         Iron           → Black Metal

- Linen Cape
  • Material:         Silver         → Black Metal

- Eitr-Weave Hood
  • Material:         Iron           → Scale Hide
  • Amount Craft:     2              → 3

- Robes of Embla
  • Material:         Flametal       → Sulfur

- Ashen Cape
  • Material:         Flametal       → Sulfur


--------------- [Cheaper Build Pieces Amounts] -----------------

► Description:
  Reduces the amount of resources needed for vanilla build pieces

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the build menu to be reopened to take effect.

► Conflicts:
  Incompatible with other mods that modify vanilla build pieces costs.
  Works with mods that add new build pieces, but will not apply changes to them.

▼ Changes (only modified build pieces are listed):

- Iron cooking Station 
  • Iron:         3  → 2
  • Chain:        3  → 2

- Item Stand Vertical and Horizontal
  • Fine Wood:    4  → 2

- Blast Furnace
  • Iron:         10 → 5

- Oven
  • Iron:         15 → 5

- Grinding Wheel
  • Wood:         25 → 15

- Smith's Anvil
  • Iron:         20 → 7

- Forge Cooler
  • Fine Wood:    25 → 10

- Forge Toolrack
  • Iron:         15 → 5

- Wood Window
  • Wood:         4  → 2

- Darkwood Gate
  • Iron:         4  → 3

- Iron Gate
  • Iron:         4  → 3

- Iron Chest
  • Iron:         2  → 1

- Private Chest
  • Iron:         8  → 4

- Blackmetal Chest
  • Black Metal:  6  → 4

- Chandelier
  • Bronze:       5  → 3

- Portal 
  • Fine Wood:    20 → 10

- All Rugs
  • Their respective hide: 4 → 3

- All Banners
  • Leather Scraps: 6 → 5

- Fermenter
  • Fine Wood:    30 → 15

- Bath Tub
  • Iron:         10 → 5

- Crystal Wall
  • Crystal:      2  → 1

- Incinerator
  • Iron:         8  → 5

- Stone Wall 1x1
  • Stone:        3  → 2

- Stone Wall 2x1
  • Stone:        4  → 3

- Stone Pillar
  • Stone:        5  → 3

- Stone Arch
  • Stone:        4  → 3

- Stone Floor
  • Stone:        6  → 4

- Stone Stair
  • Stone:        8  → 3

- Black Marble 2x1x1
  • Black Marble: 4  → 3

- Black Marble Stair
  • Black Marble: 8  → 3

- Black Marble Plinth
  • Black Marble: 5  → 4

- Black Marble Plinth Corner
  • Black Marble: 6  → 5

- Black Marble Cornice
  • Black Marble: 5  → 4

- Black Marble Cornice Corner
  • Black Marble: 6  → 5

- Black Marble Arch
  • Black Marble: 5  → 4

- Dvergr Stake Wall
  • Yggdrasil Wood: 8 → 4
  • Iron:           8 → 2

- Sharp Stakes
  • Wood:         6  → 4
  • Core Wood:    4  → 2

- Dvergr Sharp Stakes
  • Yggdrasil Wood: 5 → 4
  • Iron:           2 → 1

- Trap 
  • Black Metal:    5  → 3
  • Bronze Nails:   10 → 5

- Turret 
  • Black Metal:       10 → 7
  • Yggdrasil Wood:    10 → 7
  • Mechanical Spring: 3  → 2

- Eitr Refinery
  • Black Marble:   20 → 10

- Vice
  • Copper:         8  → 5

- Sap Collector
  • Yggdrasil Wood: 10 → 5
  • Black Metal:    5  → 3

- Galdr Table
  • Yggdrasil Wood: 20 → 10
  • Black Metal:    10 → 5

- Rune Table
  • Black Marble:   10 → 5
  • Refined Eitr:   10 → 5

- Unfading Candles
  • Black Marble:   10 → 5
  • Refined Eitr:   10 → 5

- Hexagonal Gate
  • Copper:         8  → 4

- Dvergr Spiral Stair Left
  • Yggdrasil Wood: 5  → 3
  • Copper:         2  → 1

- Dvergr Spiral Stair Right
  • Yggdrasil Wood: 5  → 3
  • Copper:         2  → 1

- Black Marble Bench
  • Black Marble:   6  → 5
  • Copper:         3  → 2

- Round Table
  • Iron Nails:     20 → 10

- Black Marble Table
  • Black Marble:   6  → 5
  • Copper:         3  → 2

- Standing Brazier
  • Bronze:         5  → 3
  • Fenris Claw:    3  → 2

- Blue Standing Brazier
  • Bronze:         5  → 3
  • Fenris Claw:    3  → 2

- Red Jute Carpet
  • Red Jute:       4  → 3

- Blue Jute Carpet
  • Blue Jute:      4  → 3

- Hare Rug
  • Scale Hide:     4  → 2

- Sconce
  • Bronze:         2  → 1

- Standing Iron Torch
  • Iron            2  → 1

- Standing Green Torch
  • Iron            2  → 1

- Standing Blue Torch
  • Iron            2  → 1

- Dvergr Wall Lantern
  • Copper:         2  → 1

- Dvergr Pole Lantern
  • Copper:         3  → 2

- Red Jute Curtain
  • Red Jute:       4  → 3

- Blue Jute Drapes
  • Blue Jute:      4  → 3

- Blue Jute Curtain
  • Blue Jute:      4  → 3

- Stone portal 
  • Grausten:       30 → 20

- Feathery Wreath 
  • Refined Eitr:   10 → 5

- Ashwood Arched Wall 
  • Ashwood:        2  → 1

- Grausten Steep Stairs 
  • Grausten:       5  → 3

- Grausten Stairs 
  • Grausten:       8  → 3

- Grausten Floor 1x1 
  • Grausten:       2  → 1

- Grausten Medium Pillar 
  • Grausten:       3  → 2

- Grausten Tapered Pillar 
  • Grausten:       5  → 4

- Grausten Tapered Pillar Inverted 
  • Grausten:       5  → 4

- Grausten Medium Beam 
  • Grausten:       3  → 2

- Grausten Wall 1x2 
  • Grausten:       4  → 2

- Grausten Wall 2x2 
  • Grausten:       6  → 4

- Grausten Wall 4x2 
  • Grausten:       12 → 8

- Grausten Window 4x2 
  • Grausten:       10 → 8

- Grausten Roof Corner 
  • Grausten:       5  → 4

- Grausten Roof Corner 
  • Grausten:       5  → 4

- Grausten Roof Arched Corner 
  • Grausten:       5  → 4

- Grausten Roof Arched Corner 
  • Grausten:       5  → 4

- Flametal Gate 
  • Flametal:       16 → 8

- Asksvin Rug 
  • Hide:           4  → 3


------------- [Alternate Build Pieces Materials] ---------------

► Description:
  Modifies some material requirements for vanilla build pieces. Complementary to Cheaper Build Pieces Amounts.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the build menu to be reopened to take effect.

► Conflicts:
  Incompatible with other mods that modify vanilla build pieces costs.
  Works with mods that add new build pieces.

▼ Changes (only modified build pieces are listed):

- Tool Rack (Workbench Extension): Obsidian → Coal (This will move this extension one biome earlier)
- Darkwood Gate:                   Iron     → Black Metal
- Bath Tub:                        Iron     → Black Metal


---------------- [Food And Mead Modifications] -----------------

► Description:
  Modifies recipe amounts, amounts crafted, and material requirements for some food and mead recipes.
  Changes the stack size of all foods to 20.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the food crafting menu to be reopened to take effect.

► Conflicts:
  Incompatible with other mods that modify vanilla foods and meads.
  Works with mods that add new recipes.

▼ Changes (only modified foods are listed):

>> Food <<

- Queen's Jam 
  • Resource amounts:
    - Raspberries:         8  → 5
    - Blueberries:         6  → 5

- Turnip Stew 
  • Amount crafted:        1  → 2

- Black Soup
  • Resource amounts:
    - Bloodbag:            1  → 2
    - Honey:               1  → 2
    - Turnip:              1  → 2
  • Amount crafted:        1  → 2

- Carrot Soup 
  • Amount crafted:        1  → 2

- Deer Stew 
  • Resource amounts:
    - Bueberries:          1  → 2
    - Carrot:              1  → 2
    - Cooked Deer Meat:    1  → 2
  • Amount crafted:        1  → 2

- Muckshake
  • Amount crafted:        1  → 2

- Mince Meat Sauce 
  • Resource amounts:
    - Boar Meat:           1  → 2
    - Neck Tail:           1  → 2
    - Carrot:              1  → 2
  • Amount crafted:        1  → 2

- Eyescream
  • Amount crafted:        1  → 2

- Onion soup
  • Amount crafted:        1  → 2

- Wolf Skewer
  • Resource amounts:
    - Wolf Meat:           1  → 2
    - Onion:               1  → 2
  • Amount crafted:        1  → 2

- Serpent Stew
  • Resource amounts:
    - Mushroom:            1  → 2
    - Cooked Serpent Meat: 1  → 2
  • Amount crafted:        1  → 2

- Blood Pudding
  • Amount crafted:        1  → 2

- Fish Wraps
  • Amount crafted:        1  → 2

- Unbaked Lox Pie
  • Amount crafted:        1  → 2

- Bread Dough
  • Resource amounts:
    - Barley flour:        10 → 6

- Uncooked Fish & Bread
  • Amount crafted:        1  → 2

- Uncooked Honey Glazed Chicken
  • Resource amounts:
    - Chicken Meat:        1  → 2
  • Amount crafted:        1  → 2

- Uncooked Stuffed Mushroom
  • Resource amounts:
    - Blood Clot:          1  → 2
  • Amount crafted:        1  → 2

- Uncooked Meat Platter
  • Resource amounts:
    - Seeker Meat:         1  → 2
    - Lox Meat:            1  → 2
    - Hare Meat:           1  → 2
  • Amount crafted:        1  → 2

- Uncooked Misthare Supreme
  • Resource amounts:
    - Hare Meat:           1  → 2
  • Amount crafted:        1  → 2

- Mushroom Omelette
  • Resource amounts:
    - Egg:                 3  → 2
  • Amount crafted:        1  → 2

- Yggdrasil Porridge
  • Resource amounts:
    - Sap:                 4  → 2
    - Barley:              3  → 2
  • Amount crafted:        1  → 2

- Salad
  • Resource amounts:
    - Jotun Puffs:         3  → 4
    - Onion:               3  → 4
    - Cloudberries:        3  → 4
  • Amount crafted:        3  → 4

- Fiery Svinstew
  • Resource amounts:
    - Asksvin Tail:        1  → 2
    - Smoke Puff:          1  → 2
  • Amount crafted:        1  → 2

- Marinated Greens
  • Resource amounts:
    - Sap:                 3  → 2
  • Amount crafted:        1  → 2

- Mashed Meat
  • Resource amounts:
    - Asksvin Tail:        1  → 2
    - Volture Meat:        1  → 2
    - Fiddlehead:          1  → 2
  • Amount crafted:        1  → 2

- Scorching Medley
  • Resource amounts:
    - Jotun Puffs:         3  → 4
    - Onion:               3  → 4
    - Fiddlehead:          3  → 4
  • Amount crafted:        3  → 4

- Sizzling Berry Broth
  • Resource amounts:
    - Sap:                 3  → 2
  • Amount crafted:        1  → 2

- Sparkling Shroomshake
  • Resource amounts:
    - Sap:                 3  → 2
  • Amount crafted:        1  → 2

- Spicy Marmalade
  • Resource amounts:
    - Vineberry Cluster:   3  → 2
    - Honey:               1  → 2
    - Fiddlehead:          1  → 2
  • Amount crafted:        1  → 2

- Uncooked Roasted Crust Pie
  • Resource amounts:
    - Volture Egg:         1  → 2
  • Amount crafted:        1  → 2

>> Mead <<

- Mead Base: Lingering Eitr
  • Sap:                   10 → 5
  • Vineberry Cluster:     10 → 5
  • Magecap:               10 → 5

- Mead Base: Lingering Health
  • Sap:                   10 → 5
  • Vineberry Cluster:     10 → 5
  • Smoke Puff:            10 → 5

- Mead Base: Lingering Stamina
  • Sap:                   10 → 5
  • Cloudberries:          10 → 5
  • Jotun Puff:            10 → 5

- Mead Base: Minor Eitr
  • Honey:                 10 → 5
  • Sap:                   5  → 3
  • Magecap:               5  → 3

- Mead Base: Major Healing
  • Honey:                 10 → 5
  • Blod Clot:             4  → 2
  • Royal Jelly → Jotun Puffs : 5 → 3

- Mead Base: Medium Healing
  • Honey:                 10 → 5
  • Blodbag:               4  → 3
  • Raspberries:           10 → 5

- Mead Base: Minor Healing
  • Honey:                 10 → 5
  • Raspberries:           10 → 5

- Barley Wine Base: Fire Resistance
  • Barley:                10 → 5
  • Cloudberries:          10 → 5 

- Mead Base: Frost Resistance
  • Honey:                 10 → 5
  • Thistle:               5  → 3

- Mead Base: Poison Resistance
  • Honey:                 10 → 5
  • Thistle:               5  → 3
  • Coal:                  10 → 5

- Mead Base: Medium Stamina
  • Honey:                 10 → 5
  • Cloudberries:          10 → 5

- Mead Base: Minor Healing
  • Honey:                 10 → 5
  • Raspberries:           10 → 5

- Mead Base: Tasty
  • Honey:                 10 → 5
  • Raspberries:           10 → 5

>> Food stacks <<
The following foods have the stack size increased to 20

- Queens Jam
- Carrot Soup
- Deer Stew
- Turnip Stew
- Black Soup
- Eyescream
- Muckshake
- Mince Meat Sauce
- Onion Soup
- Serpent Stew
- Blood Pudding
- Fish Wraps
- Fish And Bread
- Lox Pie
- Mushroom Omelette
- Yggdrasil Porridge
- Cooked Egg
- Salad
- Seeker Aspic
- Honey Glazed Chicken
- Magically Stuffed Mushroom
- Meat Platter
- Misthare Supreme
- Fiery Svinstew
- Marinated Greens
- Mashed Meat
- Scorching Medley
- Sizzling Berry Broth
- Spicy Marmalade
- Sparkling Shroomshake
- Roasted CrustPie
- Piquant Pie
- Uncooked Fish And Bread
- Uncooked Lox Pie
- Uncooked Honey Glazed Chicken
- Uncooked Magically Stuffed Mushroom
- Uncooked Meat Platter
- Uncooked Misthare Supreme
- Uncooked Roasted Crust Pie
- Uncooked Piquant Pie  


========================= [Features] ===========================
► All configs in this section are synced with server.

----------------- [Automatic Progression Halt] -----------------

► Description:
  Creatures and destroyable objects do not drop any items unless the previous biome boss has been defeated.
  Pickable items and chests cannot be picked/opened under the same conditions.
  Resources from each biome are automatically unlocked when the relevant boss is defeated. There is no need for CLIENT relogs or SERVER restarts.

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog or reloading area. Reloading area means walking/teleporting away from the current zone and coming back. This only applies for destroyable objects like Copper Mines. Pickables and chests will be affected immediately. Creatures will be affected if they are new.

► Conflicts:
  This only applies to vanilla game prefabs. Any mod that adds new prefabs (creatures, resources, pickables) will NOT be included in the Progression Halt system.

► Specific boss halts

- Eikthyr: Black Forest objects and creatures
- The Elder: Swamp objects and creatures
- Bonemass: Mountain and Ocean objects and creatures
- Moder: Plains objects and creatures
- Yagluth: Mistlands objects and creatures
- The Queen: Ashlands objects and creatures


------------------- [Halt Ocean Behind Elder] ------------------

► Description:
  Halts Ocean biome resources (Leviathans and Serpents) behind The Elder instead of Bonemass. Requires Automatic Progression Halt to be enabled.   
  NOTE: Ocean biome resources provide Mountain-tier gear and food (so it makes more sense to halt them behind Bonemass), but this option is here if players still want ocean resources earlier.

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog or reloading area.

► Conflicts:
  Same as above.


--------------------- [Early Linen Cape] -----------------------

► Description:
  Renames Linen Cape to Fine Cape.
  Moves it to the Swamp biome.
  Adds Posion resistance.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the crafting menu to be reopened to take effect.
  If wearing the item when toggling, a CLIENT relog is needed to refresh the poison resist modifier.

► Conflicts:
  Incompatible with other mods that modify back pieces.

▼ Changes:
- Material:       Linen thread → Deer Hide
- Craft Amount:   20     → 5
- Upgrade Amount: 4/8/12 → 2/4/6
- Material:       Silver → Iron


------------------ [Gear Speed Modifications] ------------------

► Description:
  Removes speed penalty for heavy and mage armors.
  Adds speed bonus to light armor.

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog as speed changes won't take effect immediately.

► Conflicts:
  Incompatible with other mods that modify vanilla gear speed modifiers.

▼ Changes:
- All metal chest and leg pieces speed penalty:  -5%  → 0%
- All mage chest and leg pieces speed penalty:   -2%  → 0%
- Troll & Ask chest and leg pieces speed bonus:  0%   → 2% 
- Root armor chest and leg pieces speed bonus:   0%   → 1%
- Battleaxe and Crystal Battleaxe speed penalty: -20% → -5% 
- Tower Shields speed penalty:                   -20% → -10%


---------------- [Forsaken Powers Modifications] ---------------

► Description:
  Modifies Forsaken Powers cooldown and duration timers individually.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with other mods that modify Forsaken Powers.

▼ Changes:
- Eikthyr power: 
  • Cooldown: 15 mins
  • Duration: 10 mins

- Elder power: 
  • Cooldown: 15 mins
  • Duration: 10 mins

- Bonemass power: 
  • Cooldown: 17.5 mins
  • Duration: 7.5 mins

- Moder power: 
  • Cooldown: 15 mins
  • Duration: 10 mins

- Yagluth power: 
  • Cooldown: 17.5 mins
  • Duration: 7.5 mins

- Queen power: 
  • Cooldown: 15 mins
  • Duration: 7.5 mins

- Fader power: 
  • Cooldown: 15 mins
  • Duration: 7.5 mins


------------------- [Faster Character Speed] -------------------

► Description:
  Increases player character movement speeds.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with other mods that modify player speed.

▼ Changes:
- Crouch Speed: 2   → 2.2
- Swim Speed:   2   → 2.2
- Walk Speed:   1.6 → 2.5
- Run Speed:    Unchanged


-------------- [Shorter Wet And Potion Cooldowns] --------------

► Description:
  Modifies the cooldown timers for potions.
  Reduces the wet effect cooldown timer.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with other mods that modify Status Effects related to potions and being wet.

▼ Changes:
- Wet Status Effect:     120s → 60s
- Minor Eitr Potion:     120s → 60s
- Major Health Potion:   120s → 75s
- Medium Health Potion:  120s → 60s
- Minor Health Potion:   120s → 45s
- Medium Stamina Potion: 120s → 60s
- Minor Stamina Potion:  120s → 45s


-------------------- [Less Stamina Usage]-----------------------

► Description:
  Reduces stamina use of all actions (combat, running, building) by 15%

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with other mods that modify all player stamina usage generally.
  Compatible with mods that modify stamina values independently for weapons for example.


---------------------- [Extra Armor Stats] ---------------------

► Description:
  Heavy armor provides extra HP. 
  Light armor provides extra Stamina.
  Mage armor provides extra Eitr (Not Eitr regen).

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with other mods that add hp/stamina/eitr to armor.
  Compatible with custom armor mods, since this only modifies vanilla gear.

▼ Changes:
- HP values:      total (head, chest, legs):
  • Bronze set:   10    (2,    4,     4)
  • Iron set:     20    (4,    8,     8)
  • Silver set:   30    (8,    12,    10)
  • Padded set:   40    (10,   16,    14)
  • Carapace set: 50    (12,   20,    18)
  • Flametal set: 60    (14,   24,    22)
  • Ask set:      25    (5,    10,    10)
- Stamina values: total (head, chest, legs):
  • Troll set:    5     (1,    2,     2)
  • Root set:     10    (3,    4,     3)
  • Fenris set:   15    (4,    6,     5)
  • Ask set:      20    (5,    8,     7)
- Eitr values:    total (head, chest, legs):
  • Eitr-Weave:   50    (10,   20,    20)
  • Embla set:    75    (15,   30,    30)


------------------- [Better Death Raiser] ----------------------

► Description:
  The primary attack will only spawn melee skeletons.
  Adds a secondary attack that spawns archer skeletons.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with other mods that modify how the Death Raiser functions or what it summons.
  Compatible with other mods that modify skeleton spawn stats or gear.


------------------ [Better Summoned Skeletons] -----------------

► Description:
  Increases summoned skeleton speed.
  Adds better looking gear according to Death Raiser level (Skeleton stats are not affected)

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog for Summoned Skeleton speed changes to take effect.
  Toggling mid-game works immediately for the Summoned Skeleton gear when summoning new Skeletons.

► Conflicts:
  Incompatible with other mods that modify Summoned Skeleton speed stats and gear. 

▼ Changes:
- Summoned Skeleton Speed:
  • Speed:     1 → 4
  • Run speed: 4 → 8

- Summoned Skeleton Gear:
  • Lvl 1 Death Raiser:
    - Melee gear: 
      - Weapons: Bronze Sword, Bronze Mace
      - Shields: Wood Shield,  Bronze Buckler
      - Chest:   Bronze Chest
      - Legs:    Bronze Legs
      - Cape:    Deer Hide Cape
    - Archer gear: 
      - Weapons: Finewood Bow
      - Chest:   Troll Leather Chest
      - Legs:    Troll Leather Legs
      - Cape:    Troll Leather Cape
  • Lvl 2 Death Raiser:
    - Melee gear: 
      - Weapons: Iron Sword,   Iron Mace
      - Shields: Iron Buckler, Banded Shield
      - Chest:   Iron Chest
      - Legs:    Iron Legs
      - Cape:    Linen Cape
    - Archer gear: 
      - Weapons: Huntsman Bow
      - Chest:   Root Chest
      - Legs:    Root Legs
      - Cape:    Deer Hide Cape
  • Lvl 3 Death Raiser:
    - Melee gear: 
      - Weapons: Silver Sword, Frostner
      - Shields: Silver Shield, Serpent Scale Shield
      - Chest:   Wolf Armor Chest
      - Legs:    Wolf Armor Legs
      - Cape:    Wolf Cape
    - Archer gear: 
      - Weapons: Draugr Fang
      - Chest:   Fenring Chest
      - Legs:    Fenring Legs
      - Cape:    Linen Cape
  • Lvl 4 Death Raiser:
    - Melee gear: 
      - Weapons: Blackmetal Sword, Porcupine
      - Shields: Blackmetal Shield, Carapace Shield
      - Chest:   Carapace Chest
      - Legs:    Carapace Legs
      - Cape:    Lox Cape
    - Archer gear: 
      - Weapons: Draugr Fang, Spinesnap
      - Chest:   Padded Cuirass
      - Legs:    Padded Greaves
      - Cape:    Feather Cape


---------------- [Better Frost Staff Accuracy] -----------------

► Description:
  Improves Staff of Frost Accuracy.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with other mods that modify the Staff of Frost.


---------------- [Reduced Crossbows Reload Time] ---------------

► Description:
  Crossbows Reload Time Reduced by 1s (3.5s → 2.5s).

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog.

► Conflicts:
  Incompatible with other mods that modify Crossbows reload time.


-------------------- [Less Ashlands Enemies] -------------------

► Description:
  Reduced the spawn rate and numbers of enemies in Ashlands.
  NOTE: This does NOT modify enemy levels or levelup chances (starred enemies).

► Mid-Game Toggling:
  Toggling mid-game requires reloading area. This means moving to a new unloaded area will apply the new/old spawn values depending on the toggle. Areas that are currently active (having a player present) will retain the old config spawn data.
  Teleporting or simply walking away for a certain distance will make changes take effect if this config is toggled mid-game.

► Conflicts:
  Incompatible with other mods that modify Ashlands enemy spawn data.

▼ Changes (only modified creatures are listed):

- Fallen Valkyrie
  • Spawn chance:   20% → 15%

- Asksvin (day)
  • Max spawned:    2   → 1
  • Min group size: 2   → 1
  • Max group size: 3   → 2
  • Spawn chance:   30% → 20%

- Asksvin (night)
  • Max spawned:    3   → 2
  • Spawn chance:   45% → 35%

- Volture
  • Max spawned:    3   → 2

- Charred Twitcher (day)
  • Max spawned:    3   → 2
  • Min group size: 2   → 1
  • Max group size: 4   → 2
  • Spawn chance:   40% → 35%

- Charred Twitcher (night)
  • Max spawned:    4   → 3
  • Min group size: 3   → 2
  • Max group size: 6   → 3

- Charred Marksman
  • Max spawned:    4   → 2
  • Spawn chance:   35% → 30%

- Charred Warrior
  • Max spawned:    4   → 2
  • Spawn chance:   35% → 30%

- Lava Blob
  • Max spawned:    2   → 1
  • Max group size: 2   → 1
  • Spawn chance:   25% → 20%


-------------------- [Gear Upgrade Unlock] ---------------------

► Description:
  Gear from Meadows, Black Forest, Mistlands and Ashlands can be upgraded to lvl 4 within their respective biomes.
  This is accomplished by reducing the crafting workstation level requirements for Leather Set, Troll Set, Bronze Set, all Mistlands sets, and all Ashlands sets.
  NOTE: Special Ashlands weapons that use gems can be crafted to lvl 3 (e.g. Klossen max level upgrade is 3 (from 2)).

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the relevant crafting menu to be reopened to take effect.

► Conflicts:
  Incompatible with other mods that modify the required station level for the named sets.


---------------------- [Permanent Lights] ----------------------

► Description:
  Makes all light sources permanent.
  Modifies the build costs of light source pieces to use maximum amount of their respective fuel type (wood, resin, coal etc).
  NOTE: Fireplace now has an additional 20 Wood cost.
  NOTE: Campfire and Fireplace Wood is refundable when destroyed.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with other mods that modify light source fuel.
  Incompatible with other mods that modify build piece costs.
  Compatible with mods that add new light sources. This will make them permanent, but will not modify their build costs.


----------------------- [Clear Mistlands] ----------------------

► Description:
  Clears Mistlands mist after defeating the Queen.
  Changes will be applied immediately when the Queen is defeated. No relogs or restarts needed.

► Mid-Game Toggling:
  Disabling mid-game requires CLIENT relog if the Queen is defeated.
  Enabling mid-game will apply changes immediately.

► Conflicts:
  Incompatible with other mods that modify Mistlands mist.


----------------------- [Craftable Chain] ----------------------

► Description:
  Adds a Chain recipe at the Black Forge.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires the Black Forge crafting menu to be reopened to take effect.

► Conflicts:
  Possibly incompatible with mods that add the same recipe at the normal Forge (not tested).


----------------------- [Brighter Lanterns] --------------------

► Description:
  Makes Dverger Lanterns brighter.
  NOTE: Standing lanterns are a bit brighter than the wall mount lanterns.

► Mid-Game Toggling:
  Toggling mid-game requires either CLIENT relog or reloading area (moving/teleporting away from current zone and coming back).

► Conflicts:
  Incompatible with other mods that modify Dverger Lanterns luminosity.


----------------------- [Clearer Weather] ----------------------

► Description:
  Reduces chance for mist and snowstorms in Meadows, Plains, Ocean and Mountains respectively.

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog.

► Conflicts:
  Incompatible with any weather or seasons mod.


------------------ [Creature Unleveler By Boss] ----------------

► Description:
  Increases chance of creatures to spawn with a star or two after defeating their relevant biome boss.
  This happens in a staggered way and will not affect creature from a biome whose boss has not been defeated.
  Changes are automatically applied when a boss is defeated, including when reverting the boss global key with console commands.
  NOTE: If defeating bosses in an unordered way, the changes still apply in their natural order.
  NOTE: Some creatures that did not have stars will now gain stars (e.g. Abomination, Lox, Deathsquito)

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but changes will only be applied to new areas and newly spawned creatures.
  For example, if the mod was enabled and a 2-star Troll was spawned, the Troll will retain its level even if the config is toggled OFF afterwards. Same applies when toggling ON, when defeating bosses or reverting with console commands.

► Conflicts:
  Incompatible with any mod that changes creature levels.

▼ Changes:

- Eikthyr Defeated
  • Deer 
    - Level up chance: 10% → 15%

  • Boar & Neck
    - Level up chance: 10% → 15%
    - Level up center distance: 800 → 400

  • Greyling
    - Level up chance: 10% → 15%
    - Max stars:       0   → 1

- Elder Defeated
  • Greydwarf
    - Level up chance: 10% → 20%

  • Boar, Neck, Troll
    - Level up chance: 10%/15% → 20%
    - Level up center distance: 800/400 → 0

  • Greyling
    - Level up chance: 15% → 20%
    - Max stars:       1   → 2

  • Greydwarf, Greydwarf Brute, Greydwarf Shaman
    - Level up chance: 10% → 20%

- Bonemass Defeated
  • Draugr, Leech, Skeleton, Draugr Elite, Surtling
    - Level up chance: 10% → 20%

  • Boar, Neck, All Greydwarves
    - Level up chance: 20% → 30%

- Moder Defeated
  • Fenring
    - Level up chance: 0% → 10%
    - Max stars:       0  → 1

  • Wolf
    - Level up chance: 10% → 20%

  • Troll, Skeleton, Draugr, Surtling
    - Level up chance: 20% → 30%

- Yagluth Defeated
  • Berserker, Fuling
    - Level up chance: 10% → 20%

  • Blob, Oozer, Wraith, Abomination, Stone Golem, Drake
    - Level up chance: 0% → 20%
    - Max stars:       0  → 1

  • Fenring
    - Level up chance: 10% → 20%
    - Max stars:       1   → 2

  • Wolf
    - Level up chance: 20% → 30%

- Queen Defeated
  • Dverger, Seeker Soldier, Tick
    - Level up chance: 10% → 20%

  • Seeker Brood
    - Level up chance: 0% → 10%
    - Max stars:       0  → 1

  • Seeker
    - Level up chance: 10% → 20%

  • Lox, Deathsquito
    - Level up chance: 0% → 10%
    - Max stars:       0  → 1

  • Fuling, Berserker
    - Level up chance: 20% → 30%

- Fader Defeated
  • Dverger
    - Level up chance: 20% → 30%

  • Charred Warrior & Archer (that spawn in other biomes)
    - Level up chance: 0% → 10%
    - Max stars:       0  → 1


------------------ [Hildir Weight Rewards] ---------------------

► Description:
  Increases base carry weight by 25 when turning in Hildir chests (for each chest).
  This will apply for all characters in the world, but will require CLIENT relogs.
  NOTE: Changes are NOT applied when defeating the minibosses, but when turning in the chests.

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog.

► Conflicts:
  No known conflicts.


----------------- [Station Extensions Changes] -----------------

► Description:
  Decreases space requirement for workstation extensions and increases build distance to workstations.
  NOTE: This does not increase workstations radius.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires reloading the build menu.

► Conflicts:
  Incompatible with any mod that modifies station extensions.


--------------------- [Stop Running Away] ----------------------

► Description:
  Boars and Necks won't flee when alerted.

► Mid-Game Toggling:
  Toggling mid-game only affects newly spawned creatures.

► Conflicts:
  Incompatible with any mod that changes Boar and Neck AI behavior.


------------------ [Better Trophy Drop Rates] ------------------

► Description:
  Increases trophy drop rate for some creatures.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but the changes will only apply to newly spawned creatures.

► Conflicts:
  Incompatible with any mod that modifies creature trophy drop rates.

▼ Changes:

- Rancid Remains:   10% → 20% 
- Surtling:         5%  → 10% 
- Draugr Elite:     10% → 20% 
- Wraith:           5%  → 20% 
- Cultist:          10% → 20% 
- Fenring:          10% → 20% 
- Stone Golem:      5%  → 20% 
- Deathsquito:      5%  → 10% 
- Fuling Berserker: 5%  → 10% 
- Tick:             5%  → 10% 
- Dverger:          5%  → 10% 
- Seeker Soldier:   5%  → 20% 
- Charred Warlock:  5%  → 20% 


----------------------- [Tougher Ships] ------------------------

► Description:
  Increases Ships HP by a flat amount.

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog or reloading area (moving/teleporting away and back).

► Conflicts:
  Incompatible with any sailing mod that modifies ship HP.

▼ HP Changes:
- Raft:     300  → 400
- Karve:    500  → 650
- Longship: 1000 → 1250
- Drakkar:  3000 → 4000


----------------------- [Other Section] ------------------------

► Description:
  Tankard costs reduced and Iron Nails crafting output doubled.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but requires crafting menu reload.

► Conflicts:
  No known conflicts.

▼ Changes:

- Tankard crafting resource amount:
  • Fine Wood:      4  → 2

- Iron Nails 
  • Amount crafted: 10 → 20


======================== [QOL] ========================
► All configs in this section are synced with server.

------------------ [Lighter Metal Weight] ----------------------

► Description:
  Modifies metal weight to match Tin Ore weight.

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog or reloading area (moving/teleporting away and back).

► Conflicts:
  No known conflicts.

▼ Changes:

- All Ores:   10 → 8
- All Metals: 12 → 8


-------------------- [Larger Pickup Area] ----------------------

► Description:
  Item auto-pickup area slightly increased.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


---------------- [No Skill Levels Loss On Death] ---------------

► Description:
  Skills will no longer drop in level when dying. However, the progress towards the next level for each skill will still drop. 
  For example, if a player has level 17 in the Swords skill and 30% progress in it towards the next level, when the player dies, the percentage progress will drop, but the main level will not go below 17.

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog.

► Conflicts:
  No known conflicts.


----------------- [Larger Boat Explore Radius] -----------------

► Description:
  Double explore radius on a boat.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with any mod that modifies explore radius.


-------------------- [Bigger Wisp Radius] ----------------------

► Description:
  Increases wisp radius.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with any mod that modifies the Wisplight.


--------------------- [Friendly Ballistas] ---------------------

► Description:
  Ballistas no longer fire on players and tamed creatures.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with any mod that changes Ballista behaviour.


--------------------- [Less Fall Damage] -----------------------

► Description:
  Reduces fall damage by 40%

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


------------------- [Faster Resource Drops] --------------------

► Description:
  Enemies drop resources faster when dying
  - Small creatures: 0.5s
  - Large creatures: 5s

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay, but changes will only be applied to newly spawned creatures or creatures in inactive areas.

► Conflicts:
  Incompatible with anymod that changes creature resource drops timer.


----------------------- [Faster Equip] -------------------------

► Description:
  Equipping weapons is instant (skips equip animation). 
  Armor equip timers reduced to 1s (from 1s/2s).

► Mid-Game Toggling:
  Toggling mid-game requires CLIENT relog.

► Conflicts:
  Incompatible with any mod that changes equip timers.


--------------- [Move Camera Up While Sailing] -----------------

► Description:
  Moves the camera up by a slight amount when controlling a boat (specific for each boat) to provide enough view ahead.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with any mod that changes camera angles when sailing.


============================ [UI] ==============================
► All configs in this section are synced with server.

--------------------- [More Loading Tips]-----------------------

► Description:
  Modifies some original loadscreen tips.
  Adds new tips in addition to the original ones.
  NOTE: Only applies for the English localization.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  Incompatible with any mod that changes/adds loading screen tips.


----------- [Show Inventory Weight and Free Slots] -------------

► Description:
  Displays current carry weight and max weight values at the bottom left of the screen under the health bar.
  Displays current number of inventory slots next to the carry weight indicator at the bottom left of the screen.
  Text color changes according to weight / free slots percentage.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


-------------------- [Show Enemy Detector] ---------------------

► Description:
  Displays an enemy detector next to the inventory weight widget at the bottom left of the screen.
  Counts the number of enemies in close proximity.
  Does not include other players, deer, hare, player summoned creatures, or tame animals in the enemy count.
  Neutral Dverger are counted in parantheses. When attacked, the number goes into the normal enemy counter.
  Colors change according to number of nearby enemies.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


----------------------- [Show Boat Speed] ----------------------

► Description:
  Displays current ship speed next to the inventory weight widget at the bottom left of the screen.
  When going forward, "F" is displayed before the speed value.
  When going backwards, "R" is displayed before the speed value.
  The speed counter only shows when controlling a boat.
  Colors change according to speed.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


---------------------- [Show Time And Day] ---------------------

► Description:
  Displays current time of day above the minimap using day sections (Dawn, Morning, Day, Afternoon, Evening, Dusk, Night).
  Shows number of days spent in the world.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


-------------------- [Time - 24 Hour Format] -------------------

► Description:
  Sub-section for the previous config, enabling 24-hour format for displaying time.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


------------------- [Smart Biome Indicator] --------------------

► Description:
  Displays the current biome the player is in, replacing the vanila text from the minimap.
  Text color changes according to currently equipped gear (not including weapons) relative to the current biome.
  Colors range: purple (not ready for biome), red (hard), orange (ok), yellow (normal), green (easy). 
  Colors only change for the first 7 land biomes. The rest are displayed in white.

  NOTE: This only accounts for vanilla gear. Any custom armors will not be counted in the Smart Biome Indicator.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


--------------------- [Show Summon Counter] --------------------

► Description:
  Displays a counter for summoned skeletons from the Dead Raiser. This does not count summoned trolls.
  Colors change according to number of active summons.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


-------------------- [Show Online Players] ---------------------

► Description:
  Shows online players on the bottom right of the screen. 
  Not displayed if only one player is online.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


-------------- [Show Online Players Under Minimap] -------------

► Description:
  Sub-section for the previous config.
  Shows online players under minimap instead of bottom right when Show Online Players is enabled.

► Mid-Game Toggling:
  Can be enabled/disabled during gameplay.

► Conflicts:
  No known conflicts.


VERSION HISTORY
================================================================

v1.1.2
- Modified the way Progressionn Halt reads prefab names. This change is specifically targeting a previous incompatibility with Ventrure Location Reset mod where it was changing parts of prefab names after resetting dungeons. This patch makes these two mods compatible.
- Added section: Halt Ocean Behind Elder. This provides an extra option for players and sever admins if they prefer Ocean resources to be available earlier. Ocean-tier items and food are equal to Mountain-tier items and food, which is why this option is disabled by default, but still available if wanted.
- Rearranged config entries in the config file, which means that old configs will have extra unused config entries (those can be safely deleted)

v1.1.1
- Added Draugr Archers to Progression Halt (previous oversight)
- Removed Pickable Bone Piles found in Meadows from Progression Halt

v1.1.0
- Added Section: Move Camera Up While Sailing
- Added forgotten items from old mod port to Cheaper Build Piece Amounts (all banners Leather Scraps: 6 → 5)
- Added Shipwreck to Progression Halt (halted by Eikthyr)

v1.0.1
- Fixed a bug where features that were dependent on checking global keys (Progression Halt, Creature Unleveler) were dependent on having Clear Mistlands section enabled. Now these sections work as intended even if that option is off.
- Made UI sections server synced like the rest of the configs.

v1.0.0
- Initial upload.