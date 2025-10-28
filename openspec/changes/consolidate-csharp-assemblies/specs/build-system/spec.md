# Build System Spec Delta

## ADDED Requirements

### Requirement: Single Assembly Compilation
The build system SHALL compile all C# code into a single `Cosmere.Core.dll` assembly.

#### Scenario: CosmereCore project build
- **WHEN** `dotnet build CosmereCore.csproj` is executed
- **THEN** it SHALL produce `Cosmere.Core.dll` containing all Cosmere C# code

#### Scenario: World mod assembly folders
- **WHEN** CosmereScadrial or CosmereRoshar mods are built
- **THEN** their `Assemblies/` folders SHALL remain empty (no C# compilation)

#### Scenario: Solution-level build
- **WHEN** `dotnet build Cosmere.sln` is executed
- **THEN** it SHALL only compile the CosmereCore project

### Requirement: Build Performance Optimization
The consolidated build SHALL use incremental compilation and caching to minimize build times.

#### Scenario: Incremental compilation
- **WHEN** only a subset of files are changed
- **THEN** the build SHALL only recompile changed files and their dependents

#### Scenario: Build time target
- **WHEN** a full clean build is performed
- **THEN** it SHALL complete in under 45 seconds

#### Scenario: Parallel build configuration
- **WHEN** the solution is built
- **THEN** parallel build settings SHALL be optimized for the single-project structure

### Requirement: Code Generation Path Updates
Code generation tools SHALL output to paths matching the consolidated structure.

#### Scenario: DefOf generation target
- **WHEN** DefOf classes are generated
- **THEN** they SHALL be output to `CosmereCore/CosmereCore/DefOf/`

#### Scenario: World-specific def generation
- **WHEN** XML definitions are generated for a world
- **THEN** they SHALL be output to the world mod's Defs folder

#### Scenario: Generation tool configuration
- **WHEN** `dotnet run --project tools/Cosmere.Tools.Cli generate` is executed
- **THEN** it SHALL use updated search and output paths

## REMOVED Requirements

### Requirement: Multi-Project Dependency Chain
**Reason**: Single assembly eliminates project-to-project dependencies

**Migration**: Remove all `<ProjectReference>` elements except for tool projects

### Requirement: Assembly Version Coordination
**Reason**: Single assembly means single version number

**Migration**: Version management unified in CosmereCore.csproj only

### Requirement: Separate Assembly Output Directories
**Reason**: Only one assembly is produced

**Migration**: All output goes to `CosmereCore/Assemblies/Cosmere.Core.dll`

## MODIFIED Requirements

### Requirement: Solution Structure
The Visual Studio solution SHALL contain the CosmereCore C# project plus tool projects.

#### Scenario: Solution project list
- **WHEN** `Cosmere.sln` is opened
- **THEN** it SHALL contain: `Cosmere.Core.csproj`, `Cosmere.Tools.csproj`, `Cosmere.Tools.Cli.csproj`

#### Scenario: Removed projects
- **WHEN** the solution is inspected
- **THEN** it SHALL NOT contain Cosmere.Foundation, Cosmere.Resources, Cosmere.Scadrial, or Cosmere.Roshar projects

#### Scenario: Build configuration
- **WHEN** solution configurations are checked
- **THEN** Debug and Release configurations SHALL only build CosmereCore and tools

### Requirement: Asset Bundle Build Process
Asset bundle building SHALL continue to target world-specific mod directories.

#### Scenario: Roshar asset bundle build
- **WHEN** `buildAllCosmereBundles.ps1` builds Roshar assets
- **THEN** bundles SHALL be output to `CosmereRoshar/AssetBundles/`

#### Scenario: Scadrial asset bundle build
- **WHEN** `buildAllCosmereBundles.ps1` builds Scadrial assets
- **THEN** bundles SHALL be output to `CosmereScadrial/AssetBundles/`

#### Scenario: Core asset bundle build
- **WHEN** core shared assets are built
- **THEN** bundles SHALL be output to `CosmereCore/AssetBundles/`

### Requirement: CI/CD Pipeline
GitHub Actions workflows SHALL build the single assembly and publish to Steam Workshop.

#### Scenario: Build workflow
- **WHEN** CI/CD runs on main branch
- **THEN** it SHALL execute `dotnet build Cosmere.sln` once

#### Scenario: Release workflow
- **WHEN** a release is created
- **THEN** it SHALL package `Cosmere.Core.dll` with appropriate world mod directories

#### Scenario: Prerelease workflow
- **WHEN** alpha or beta branches build
- **THEN** they SHALL use the same single-assembly build process
