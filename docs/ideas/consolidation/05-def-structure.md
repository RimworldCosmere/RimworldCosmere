# XML Def Structure

## CosmereCore Def Structure

```
CosmereCore/
├── Defs/
│   ├── Core/                           # Core Cosmere systems
│   │   ├── Investiture/
│   │   │   ├── Needs.xml              # Need_Investiture
│   │   │   ├── Hediffs.xml            # Investiture-related hediffs
│   │   │   └── Traits.xml             # Invested traits
│   │   ├── Connection/
│   │   │   ├── ConnectionTypes.xml     # Different connection types
│   │   │   └── Hediffs.xml            # Connection hediffs
│   │   └── Shards/
│   │       └── ShardDefs.xml          # Honor, Cultivation, Preservation, Ruin, etc.
│   │
│   ├── Stats/
│   │   ├── StatCategories.xml         # Cosmere stat categories
│   │   └── StatDefs.xml               # Investiture capacity, connection strength, etc.
│   │
│   ├── Resources/                      # Metals and Gems
│   │   ├── Metals/
│   │   │   ├── Base/
│   │   │   │   ├── BasicMetals.xml    # Iron, Steel, Tin, Pewter, etc.
│   │   │   │   └── HigherMetals.xml   # Chromium, Nickel, Aluminum, etc.
│   │   │   ├── Alloys/
│   │   │   │   ├── BasicAlloys.xml    # Brass, Bronze, etc.
│   │   │   │   └── HigherAlloys.xml   # Duralumin, Bendalloy, etc.
│   │   │   └── GodMetals/
│   │   │       └── GodMetals.xml      # Atium, Lerasium, Harmonium, etc.
│   │   └── Gems/
│   │       ├── Polestones.xml         # 10 polestones
│   │       └── Spheres.xml            # Currency spheres
│   │
│   ├── ThingDefs/
│   │   ├── Buildings/
│   │   │   └── Production/
│   │   │       ├── AlloyMaker.xml     # Alloy crafting bench
│   │   │       ├── Forge.xml          # Metal working
│   │   │       └── GemCutter.xml      # Gem cutting bench
│   │   └── Items/
│   │       └── Currency.xml           # Boxings, other shared currency
│   │
│   ├── RecipeDefs/
│   │   ├── AlloyRecipes.xml          # Metal alloy recipes
│   │   └── GemCutting.xml            # Gem cutting recipes
│   │
│   ├── ResearchDefs/
│   │   └── CoreResearch.xml          # Investiture theory, metallurgy basics
│   │
│   ├── JobDefs/
│   │   └── CoreJobs.xml              # Bond to thing, etc.
│   │
│   └── ThinkTrees/
│       └── Splinters.xml              # Splinter AI
```

## CosmereRoshar Def Structure

```
CosmereRoshar/
├── Defs/
│   ├── Surgebinding/
│   │   ├── Abilities/
│   │   │   ├── Adhesion.xml
│   │   │   ├── Gravitation.xml
│   │   │   ├── Division.xml
│   │   │   ├── Illumination.xml
│   │   │   ├── Transformation.xml
│   │   │   ├── Transportation.xml
│   │   │   ├── Cohesion.xml
│   │   │   ├── Tension.xml
│   │   │   ├── Abrasion.xml
│   │   │   └── Progression.xml
│   │   ├── Hediffs/
│   │   │   ├── SurgeHediffs.xml      # Surge-specific hediffs
│   │   │   └── StormlightHealing.xml  # Healing hediffs
│   │   └── BreatheStormlight/
│   │       ├── Ability.xml
│   │       └── Hediff.xml
│   │
│   ├── RadiantOrders/
│   │   ├── Windrunner.xml            # Adhesion + Gravitation
│   │   ├── Skybreaker.xml            # Gravitation + Division
│   │   ├── Dustbringer.xml           # Division + Abrasion
│   │   ├── Edgedancer.xml            # Abrasion + Progression
│   │   ├── Truthwatcher.xml          # Progression + Illumination
│   │   ├── Lightweaver.xml           # Illumination + Transformation
│   │   ├── Elsecaller.xml            # Transformation + Transportation
│   │   ├── Willshaper.xml            # Transportation + Cohesion
│   │   ├── Stoneward.xml             # Cohesion + Tension
│   │   └── Bondsmith.xml            # Tension + Adhesion
│   │
│   ├── Shards/
│   │   ├── SummonShardblade/
│   │   │   └── Ability.xml
│   │   └── SummonShardplate/
│   │       └── Ability.xml
│   │
│   ├── ThingDefs/
│   │   ├── Apparel/
│   │   │   ├── Shardplate.xml
│   │   │   └── SpherePouches.xml
│   │   ├── Weapons/
│   │   │   └── Shardblades.xml
│   │   ├── Buildings/
│   │   │   ├── Fabrials/
│   │   │   │   ├── Attractors.xml
│   │   │   │   ├── Diminishers.xml
│   │   │   │   ├── Augmenters.xml
│   │   │   │   └── PowerGenerators.xml
│   │   │   └── SprenTrapper.xml
│   │   ├── Pawn/
│   │   │   └── Animal/
│   │   │       └── Spren.xml          # True spren definitions
│   │   └── Items/
│   │       └── StormlightLanterns.xml
│   │
│   ├── Genes/
│   │   └── Surgebinder.xml            # Surgebinder gene
│   │
│   ├── HediffDefs/
│   │   ├── ShardbladeSummoning.xml
│   │   └── Sprenrials.xml             # Fabrial hediffs
│   │
│   ├── IncidentDefs/
│   │   └── Highstorm.xml
│   │
│   ├── GameConditionDefs/
│   │   └── Highstorm.xml
│   │
│   ├── JobDefs/
│   │   ├── CaptureSpren.xml
│   │   └── RefuelSpheres.xml
│   │
│   ├── ResearchDefs/
│   │   └── FabrialResearch.xml
│   │
│   ├── Traits/
│   │   └── RadiantTraits.xml         # Life before death, etc.
│   │
│   └── LetterDefs/
│       └── RadiantLetters.xml        # Choose order letter
```

## CosmereScadrial Def Structure

```
CosmereScadrial/
├── Defs/
│   ├── Allomancy/
│   │   ├── Abilities/
│   │   │   ├── Physical/
│   │   │   │   ├── IronPull.xml
│   │   │   │   ├── SteelPush.xml
│   │   │   │   ├── TinSenses.xml
│   │   │   │   └── PewterStrength.xml
│   │   │   ├── Mental/
│   │   │   │   ├── ZincRiot.xml
│   │   │   │   ├── BrassSoothe.xml
│   │   │   │   ├── CopperCloud.xml
│   │   │   │   └── BronzeSeek.xml
│   │   │   ├── Temporal/
│   │   │   │   ├── CadmiumSlow.xml
│   │   │   │   ├── BendalloySpeed.xml
│   │   │   │   ├── GoldPast.xml
│   │   │   │   └── ElectrumFuture.xml
│   │   │   └── Enhancement/
│   │   │       ├── AluminumWipe.xml
│   │   │       ├── DuraluminBoost.xml
│   │   │       ├── ChromiumLeech.xml
│   │   │       └── NicrosilBoost.xml
│   │   └── Hediffs/
│   │       ├── MetalBurning.xml      # Burning metal hediffs
│   │       └── MetalFlaring.xml      # Flaring effects
│   │
│   ├── Feruchemy/
│   │   ├── Abilities/
│   │   │   ├── Physical/
│   │   │   │   ├── IronWeight.xml
│   │   │   │   ├── SteelSpeed.xml
│   │   │   │   ├── TinSenses.xml
│   │   │   │   └── PewterStrength.xml
│   │   │   ├── Cognitive/
│   │   │   │   ├── ZincMentalSpeed.xml
│   │   │   │   ├── BrassWarmth.xml
│   │   │   │   ├── CopperMemory.xml
│   │   │   │   └── BronzeWakefulness.xml
│   │   │   ├── Spiritual/
│   │   │   │   ├── ChromiumFortune.xml
│   │   │   │   ├── NicrosilInvestiture.xml
│   │   │   │   ├── AluminumIdentity.xml
│   │   │   │   └── DuraluminConnection.xml
│   │   │   └── Hybrid/
│   │   │       ├── CadmiumBreath.xml
│   │   │       ├── BendalloyNutrition.xml
│   │   │       ├── GoldHealth.xml
│   │   │       └── ElectrumDetermination.xml
│   │   └── Hediffs/
│   │       ├── Storing.xml           # Storing attributes
│   │       └── Tapping.xml           # Tapping metalminds
│   │
│   ├── Hemalurgy/
│   │   ├── Spikes/
│   │   │   ├── IronSpike.xml         # Steals strength
│   │   │   ├── SteelSpike.xml        # Steals physical allomancy
│   │   │   ├── TinSpike.xml          # Steals senses
│   │   │   ├── PewterSpike.xml       # Steals physical feruchemy
│   │   │   └── [Other spikes...]
│   │   └── Hediffs/
│   │       └── HemalurgicSpikes.xml
│   │
│   ├── ThingDefs/
│   │   ├── Apparel/
│   │   │   ├── Mistcloak.xml
│   │   │   └── Metalminds/
│   │   │       ├── Bracers.xml
│   │   │       ├── Rings.xml
│   │   │       └── Earrings.xml
│   │   ├── Weapons/
│   │   │   ├── Coins.xml             # For coinshots
│   │   │   ├── DuelingCanes.xml
│   │   │   └── ObsidianDaggers.xml
│   │   ├── Items/
│   │   │   ├── Vials/
│   │   │   │   ├── BasicVial.xml
│   │   │   │   └── MistbornVial.xml
│   │   │   └── MetalFlakes.xml
│   │   └── Buildings/
│   │       └── MetalDetector.xml
│   │
│   ├── Genes/
│   │   ├── Mistborn.xml              # Full Mistborn gene
│   │   ├── Misting/
│   │   │   ├── Coinshot.xml
│   │   │   ├── Lurcher.xml
│   │   │   ├── Tineye.xml
│   │   │   ├── Thug.xml
│   │   │   ├── Smoker.xml
│   │   │   ├── Seeker.xml
│   │   │   ├── Rioter.xml
│   │   │   └── Soother.xml
│   │   └── Feruchemy/
│   │       ├── FullFeruchemist.xml
│   │       └── Ferrings.xml
│   │
│   ├── Factions/
│   │   ├── FinalEmpire.xml
│   │   ├── Skaa.xml
│   │   └── NobleHouses.xml
│   │
│   ├── ResearchDefs/
│   │   ├── AllomancyResearch.xml
│   │   └── MetallurgyResearch.xml
│   │
│   └── Scenarios/
│       └── SkaaRebellion.xml
```

## Key Organizational Principles

### 1. Logical Grouping
- Group by system (Allomancy/Feruchemy) then by category
- Keep related defs together (all fabrial types in one folder)
- Separate abilities from their hediffs

### 2. Clear Separation
- Core shared defs vs world-specific defs
- Resources (metals/gems) in Core as they're used across worlds
- World-specific manifestations in their respective mods

### 3. Hierarchy
- Physical/Mental/Temporal/Enhancement categories for metals
- Consistent structure across both world mods
- Progressive complexity (basic → advanced)

### 4. Consistency
- Same structure patterns across both world mods
- Matching folder names where concepts overlap
- Predictable file locations

### 5. Discoverability
- Easy to find where any def should be located
- Clear naming conventions
- Logical progression from general to specific