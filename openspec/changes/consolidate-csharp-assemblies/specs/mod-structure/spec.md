# Mod Structure Spec Delta

## ADDED Requirements

### Requirement: CosmereCore as C# Container
The CosmereCore mod SHALL contain all C# source code and compile to a single assembly.

#### Scenario: CosmereCore directory contents
- **WHEN** CosmereCore mod directory is inspected
- **THEN** it SHALL contain `CosmereCore/` folder with all C# source organized by namespace

#### Scenario: CosmereCore assembly output
- **WHEN** CosmereCore is built
- **THEN** it SHALL produce `Assemblies/Cosmere.Core.dll`

#### Scenario: CosmereCore XML definitions
- **WHEN** XML definitions are needed
- **THEN** CosmereCore SHALL only contain shared/core definitions in `Defs/`

### Requirement: World Mods as Content-Only
World-specific mods (CosmereScadrial, CosmereRoshar) SHALL contain only XML definitions and assets.

#### Scenario: World mod C# source
- **WHEN** CosmereScadrial or CosmereRoshar directories are inspected
- **THEN** they SHALL NOT contain any C# source code directories

#### Scenario: World mod assemblies folder
- **WHEN** world mod `Assemblies/` folder is checked
- **THEN** it SHALL be empty (no DLL files)

#### Scenario: World mod About.xml dependency
- **WHEN** world mod `About/About.xml` is read
- **THEN** it SHALL list CosmereCore as a dependency

### Requirement: Asset Organization by World
Unity assets (textures, shaders, audio) SHALL remain organized by their source world.

#### Scenario: Roshar-specific assets
- **WHEN** Shardplate textures or spren shaders are needed
- **THEN** they SHALL be located in `CosmereRoshar/Assets/`

#### Scenario: Scadrial-specific assets
- **WHEN** Mistcloak textures or vial icons are needed
- **THEN** they SHALL be located in `CosmereScadrial/Assets/`

#### Scenario: Shared core assets
- **WHEN** assets are used across all worlds
- **THEN** they SHALL be located in `CosmereCore/Assets/`

## REMOVED Requirements

### Requirement: World Mod C# Compilation
**Reason**: World mods no longer contain or compile C# code

**Migration**: Move all C# from CosmereScadrial and CosmereRoshar to CosmereCore/System/{World}/

### Requirement: Foundation Mod Existence
**Reason**: Foundation functionality merged into CosmereCore

**Migration**: All Foundation utilities, UI, and base classes moved to CosmereCore root namespaces

### Requirement: Resources Mod Existence
**Reason**: Resources (metals, gems) merged into CosmereCore

**Migration**: Metal and gem definitions moved to CosmereCore/Def/, XML defs to CosmereCore/Defs/Resources/

## MODIFIED Requirements

### Requirement: Mod Loading Order
RimWorld SHALL load CosmereCore first, then world-specific mods can load in any order.

#### Scenario: Dependency declaration
- **WHEN** world mods declare dependencies in About.xml
- **THEN** they SHALL list only CosmereCore (no Foundation, no Resources)

#### Scenario: Load order enforcement
- **WHEN** RimWorld loads mods
- **THEN** it SHALL load CosmereCore before any world mods

#### Scenario: Optional world mods
- **WHEN** a user installs only CosmereCore and CosmereRoshar
- **THEN** the game SHALL load successfully without CosmereScadrial

### Requirement: Steam Workshop Mod Identity
Steam Workshop SHALL maintain separate mod entries for CosmereCore, CosmereScadrial, and CosmereRoshar.

#### Scenario: Workshop mod descriptions
- **WHEN** users browse Steam Workshop
- **THEN** they SHALL see three distinct mods with updated descriptions

#### Scenario: Mod dependencies on Workshop
- **WHEN** users subscribe to CosmereRoshar
- **THEN** Steam SHALL automatically subscribe them to CosmereCore

#### Scenario: Workshop update process
- **WHEN** CI/CD publishes to Workshop
- **THEN** it SHALL update all three mod entries

### Requirement: Def File Organization
XML definition files SHALL be located in the mod directory appropriate to their scope.

#### Scenario: Core shared definitions
- **WHEN** definitions are used across all worlds (investiture, stats, needs)
- **THEN** they SHALL be in `CosmereCore/Defs/`

#### Scenario: Resource definitions
- **WHEN** metal or gem definitions are needed
- **THEN** they SHALL be in `CosmereCore/Defs/Resources/`

#### Scenario: Roshar-specific definitions
- **WHEN** Surgebinding abilities or Radiant Orders are defined
- **THEN** they SHALL be in `CosmereRoshar/Defs/`

#### Scenario: Scadrial-specific definitions
- **WHEN** Allomancy abilities or Mistborn genes are defined
- **THEN** they SHALL be in `CosmereScadrial/Defs/`

### Requirement: Asset Bundle Distribution
Asset bundles SHALL be built and distributed with their respective mods.

#### Scenario: Core asset bundles
- **WHEN** core shared assets are bundled
- **THEN** bundles SHALL be in `CosmereCore/AssetBundles/`

#### Scenario: Roshar asset bundles
- **WHEN** Roshar-specific assets are bundled
- **THEN** bundles SHALL be in `CosmereRoshar/AssetBundles/`

#### Scenario: Scadrial asset bundles
- **WHEN** Scadrial-specific assets are bundled
- **THEN** bundles SHALL be in `CosmereScadrial/AssetBundles/`

#### Scenario: Platform-specific bundles
- **WHEN** asset bundles are built for different platforms
- **THEN** each mod SHALL contain Windows, Mac, and Linux bundle variants
