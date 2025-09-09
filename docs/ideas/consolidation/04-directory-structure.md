# Directory Structure

## Complete Consolidated Structure

### CosmereCore (All C# Code + Core Content)

```
CosmereCore/
├── About/
│   └── About.xml
├── Assemblies/
│   └── Cosmere.Core.dll              # Single consolidated assembly
├── AssetBundles/
│   └── [Platform-specific bundles]
├── Assets/                           # Core shared assets only
│   ├── Materials/
│   └── Textures/
├── Defs/                             # Core shared defs only
│   ├── Core/
│   ├── Stats/
│   ├── Resources/
│   └── [See def-structure.md for details]
├── Languages/
│   └── English/
│       └── Keyed/
├── Patches/                          # Core XML patches
├── CosmereCore/                      # C# Source - ALL consolidated here
│   ├── Ability/                     # Abstract ability system
│   ├── Investiture/                 # Investiture mechanics
│   ├── Entity/                      # Shards, splinters
│   ├── Extension/                   # Extension methods
│   ├── UI/                          # UI components
│   ├── Utility/                     # General utilities
│   ├── Settings/                    # Mod settings
│   ├── Comp/                        # Components
│   │   ├── Game/
│   │   ├── Hediff/
│   │   ├── Map/
│   │   └── Thing/
│   ├── Def/                         # Custom def classes
│   │   ├── AbilityDef.cs
│   │   ├── ShardDef.cs
│   │   ├── MetalDef.cs             # From Resources
│   │   ├── GemDef.cs               # From Resources
│   │   └── [Other def classes]
│   ├── DefOf/                       # Generated DefOf classes
│   │   ├── MetalDefOf.cs           # From Resources
│   │   ├── GemDefOf.cs             # From Resources
│   │   └── [Other DefOf files]
│   ├── DefModExtension/             # Def mod extensions
│   ├── Gene/                        # Gene-related code
│   ├── Gizmo/                       # Custom gizmos
│   ├── Hediff/                      # Core hediffs
│   ├── Job/                         # Core jobs
│   │   ├── Driver/
│   │   └── Toil/
│   ├── Need/                        # Need_Investiture, etc.
│   ├── Patch/                       # Harmony patches
│   ├── Shader/                      # Shader utilities
│   ├── Stat/                        # Stat parts
│   ├── Thing/                       # Core things
│   └── Magic/
│       ├── Roshar/
│       │   ├── Spren/
│       │   │   ├── Controller/
│       │   │   ├── ParticleSystem/
│       │   │   └── CaptureSystem/
│       │   ├── Fabrials/
│       │   │   ├── Augmenters/
│       │   │   ├── Diminishers/
│       │   │   └── Cages/
│       │   ├── Surgebinding/
│       │   │   ├── Ability/
│       │   │   ├── Hediff/
│       │   │   └── IdealChecker/
│       │   ├── Shardplate/
│       │   ├── Shardblade/
│       │   └── RadiantOrders/
│       │       ├── Windrunner/
│       │       ├── Skybreaker/
│       │       └── [Other Orders]/
│       └── Scadrial/
│           ├── Allomancy/
│           │   ├── Metal/
│           │   ├── Ability/
│           │   └── Hediff/
│           ├── Feruchemy/
│           │   ├── Metalmind/
│           │   ├── Ability/
│           │   └── Hediff/
│           └── Hemalurgy/
│               ├── Spike/
│               └── Hediff/
└── loadFolders.xml
```

### CosmereRoshar (XML/Assets Only - No C#)

```
CosmereRoshar/
├── About/
│   └── About.xml
├── Assemblies/
│   └── [Empty - uses CosmereCore.dll]
├── AssetBundles/
│   └── [Roshar-specific bundles]
├── Assets/
│   ├── Materials/
│   │   ├── CrystallineFacet.shader
│   │   └── FlowingParticleStream.shader
│   └── Textures/
│       ├── Things/
│       │   ├── Pawn/
│       │   │   └── Apparel/
│       │   │       └── Shardplate/
│       │   └── Weapon/
│       │       └── Shardblade/
│       └── UI/
├── Defs/
│   ├── Surgebinding/
│   ├── RadiantOrders/
│   ├── Things/
│   └── [See def-structure.md for details]
├── Languages/
│   └── English/
│       └── Keyed/
│           ├── Bond.xml
│           ├── Shardblade.xml
│           └── Spren.xml
├── Patches/
│   └── RadiantRequirements.xml
├── CombatExtended/                  # Mod compatibility
│   └── Patches/
└── loadFolders.xml
```

### CosmereScadrial (XML/Assets Only - No C#)

```
CosmereScadrial/
├── About/
│   └── About.xml
├── Assemblies/
│   └── [Empty - uses CosmereCore.dll]
├── AssetBundles/
│   └── [Scadrial-specific bundles]
├── Assets/
│   └── Textures/
│       ├── Things/
│       │   ├── Pawn/
│       │   │   └── Apparel/
│       │   │       └── Mistcloak/
│       │   └── Items/
│       │       └── Vials/
│       └── UI/
│           └── Icons/
│               └── Genes/
│                   └── Investiture/
│                       ├── Allomancy/
│                       └── Feruchemy/
├── Defs/
│   ├── Allomancy/
│   ├── Feruchemy/
│   ├── Hemalurgy/
│   └── [See def-structure.md for details]
├── Languages/
│   └── English/
│       └── Keyed/
│           ├── Allomancy.xml
│           ├── Feruchemy.xml
│           └── Hemalurgy.xml
├── Patches/
│   └── MetalTraders.xml
└── loadFolders.xml
```

## Key Points

### CosmereCore Contains:
- **ALL C# source code** (consolidated from all 5 projects)
- **Core/shared XML definitions** (investiture, stats, needs, metals, gems)
- **Core/shared assets** (if any)
- **Single compiled assembly**: `Cosmere.Core.dll`

### CosmereRoshar Contains:
- **Roshar-specific XML definitions**
- **Roshar-specific assets** (shardplate textures, shardblade models, etc.)
- **Roshar-specific language files**
- **NO C# source** (uses CosmereCore.dll)

### CosmereScadrial Contains:
- **Scadrial-specific XML definitions**
- **Scadrial-specific assets** (mistcloak textures, vial icons, etc.)
- **Scadrial-specific language files**
- **NO C# source** (uses CosmereCore.dll)

## File Organization Rules

1. **C# Code**: ALL in `CosmereCore/CosmereCore/`
2. **World-Specific C#**: Under `Magic/{World}/` folders
3. **Shared Definitions**: In `CosmereCore/Defs/`
4. **World Definitions**: In respective world mod `Defs/`
5. **Assets**: World-specific assets stay with their mods

## Benefits of This Structure

- **Single compile target** for all C# code
- **Clear separation** between code and content
- **Maintains mod identity** on Steam Workshop
- **Easy to navigate** with logical folder structure
- **Follows RimWorld DLC pattern** exactly