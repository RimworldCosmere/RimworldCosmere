# Code Organization Spec Delta

## ADDED Requirements

### Requirement: Unified Namespace Structure
The codebase SHALL use a unified namespace hierarchy under `Cosmere` without separate identity prefixes for Foundation, Core, or Resources.

#### Scenario: Core system namespaces
- **WHEN** core functionality is defined (investiture, UI, extensions, utilities)
- **THEN** it SHALL use direct `Cosmere.*` namespaces (e.g., `Cosmere.Investiture`, `Cosmere.UI`, `Cosmere.Extension`)

#### Scenario: World-specific magic system namespaces
- **WHEN** world-specific magic systems are defined (Surgebinding, Allomancy)
- **THEN** they SHALL use `Cosmere.System.{World}.*` namespaces (e.g., `Cosmere.System.Roshar.Surgebinding`, `Cosmere.System.Scadrial.Allomancy`)

#### Scenario: Sub-system organization
- **WHEN** magic systems have sub-systems (spren controller, fabrial types)
- **THEN** they SHALL be organized under their parent system namespace with maximum 4 levels of nesting

### Requirement: Directory Structure Mirrors Namespaces
The file system directory structure SHALL exactly mirror the namespace hierarchy.

#### Scenario: Core system file location
- **WHEN** a class is in namespace `Cosmere.Investiture`
- **THEN** the file SHALL be located at `CosmereCore/CosmereCore/Investiture/ClassName.cs`

#### Scenario: World-specific file location
- **WHEN** a class is in namespace `Cosmere.System.Roshar.Surgebinding`
- **THEN** the file SHALL be located at `CosmereCore/CosmereCore/System/Roshar/Surgebinding/ClassName.cs`

#### Scenario: Namespace-directory mismatch detection
- **WHEN** a file's directory path doesn't match its namespace
- **THEN** the mismatch SHALL be identified during code review

### Requirement: Cross-World Integration Safety
Cross-world references SHALL always check for mod activation before accessing world-specific types.

#### Scenario: Safe cross-world reference
- **WHEN** Roshar code needs to reference Scadrial functionality
- **THEN** it SHALL check `ModsConfig.IsActive("aequa.cosmere.scadrial")` before using Scadrial types

#### Scenario: Unsafe cross-world reference
- **WHEN** world-specific code directly references another world without mod checks
- **THEN** the code SHALL fail code review

#### Scenario: Core system integration
- **WHEN** world-specific code needs to interact with other worlds
- **THEN** it SHOULD use core system abstractions (e.g., `AbstractAbility`) rather than direct references

## REMOVED Requirements

### Requirement: Separate Assembly Namespaces
**Reason**: Consolidating into single assembly eliminates need for separate Foundation, Core, Resources namespace identities

**Migration**: All `Cosmere.Foundation.*` → `Cosmere.*`, `Cosmere.Resources.*` → `Cosmere.*` or `Cosmere.Def.*`

### Requirement: Public API Boundaries Between Modules
**Reason**: Single assembly means internal access between systems is now available

**Migration**: Classes can use `internal` for within-assembly access, no longer need `public` for cross-module use

## MODIFIED Requirements

### Requirement: Code File Organization
C# source files SHALL be organized by feature/system using namespace-based folder structure within the single `CosmereCore` project.

#### Scenario: Core shared code
- **WHEN** code is shared across all worlds (investiture, stats, UI)
- **THEN** it SHALL be located in root-level folders under `CosmereCore/CosmereCore/`

#### Scenario: World-specific code
- **WHEN** code is specific to a magic system or world
- **THEN** it SHALL be located under `CosmereCore/CosmereCore/System/{World}/`

#### Scenario: DefOf and generated files
- **WHEN** DefOf classes are generated
- **THEN** they SHALL be placed in `CosmereCore/CosmereCore/DefOf/` regardless of source world

### Requirement: Generated File Management
Generated files (`*.generated.cs`, `*.generated.xml`) SHALL be located based on their content scope, not source project.

#### Scenario: Core resource DefOf generation
- **WHEN** DefOf classes are generated for metals or gems
- **THEN** they SHALL be output to `CosmereCore/CosmereCore/DefOf/MetalDefOf.generated.cs` and `GemDefOf.generated.cs`

#### Scenario: World-specific def generation
- **WHEN** XML defs are generated for a specific world
- **THEN** they SHALL be output to the appropriate world mod's Defs folder (e.g., `CosmereRoshar/Defs/`)

#### Scenario: Code generation path updates
- **WHEN** code generation tools run
- **THEN** they SHALL use updated paths reflecting the consolidated structure
