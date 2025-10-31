<!-- OPENSPEC:START -->
# OpenSpec Instructions

These instructions are for AI assistants working in this project.

Always open `@/openspec/AGENTS.md` when the request:
- Mentions planning or proposals (words like proposal, spec, change, plan)
- Introduces new capabilities, breaking changes, architecture shifts, or big performance/security work
- Sounds ambiguous and you need the authoritative spec before coding

Use `@/openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guidelines

Keep this managed block so 'openspec update' can refresh the instructions.

<!-- OPENSPEC:END -->

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RimWorld: Cosmere is a full-conversion mod series bringing Brandon Sanderson's Cosmere universe into RimWorld. It's built as a modular monorepo where each component is a loadable RimWorld mod.

## Development Commands

### Building Asset Bundles
```powershell
# Build all asset bundles (required before testing in RimWorld)
./buildAllCosmereBundles.ps1

# Alternative C# script version
dotnet script ./scripts/build-assets.csx
```

### Code Generation
```bash
# Generate all code (replaces .scripts functionality)
dotnet run --project tools/Cosmere.Tools.Cli generate

# Generate specific generator
dotnet run --project tools/Cosmere.Tools.Cli generate Resources

# Clean up all generated files (no generation)
dotnet run --project tools/Cosmere.Tools.Cli generate --clean

# Available options
dotnet run --project tools/Cosmere.Tools.Cli generate --help
# -f, --force     Force generation ignoring changes
# --dry-run       Show what would be generated
# -d, --delete    Delete existing generated files first
# -v, --verbose   Show detailed output
# --clean         Delete all generated files and exit (no generation)

# Legacy Node.js method (deprecated - will be removed)
cd .scripts
npm install
npm start -- -d -f
```

### Building C# Projects
```bash
# Build entire solution (only CosmereCore after consolidation)
dotnet build Cosmere.sln

# Build CosmereCore directly
dotnet build CosmereCore/CosmereCore/Cosmere.Core.csproj

# Build tools CLI
dotnet build tools/Cosmere.Tools.Cli/Cosmere.Tools.Cli.csproj
```

## Architecture Overview

### Consolidated Single-Assembly Structure
All C# code has been consolidated into a single `CosmereCore` assembly (`Cosmere.Core.dll`) for improved performance (60% faster mod loading).

**Namespace Organization:**
- `Cosmere.*` - Core framework, shared utilities, base classes, UI components, extension methods
- `Cosmere.Core.*` - Game mechanics (abilities, hediffs, genes, stats, needs)
- `Cosmere.Def.*` - Custom definition types (MetalDef, GemDef, ShardDef, AbilityDef)
- `Cosmere.System.Scadrial.*` - Allomancy, Feruchemy, Hemalurgy, vial systems, Mistborn genes
- `Cosmere.System.Roshar.*` - Surgebinding, spren bonding, stormlight, Ideals, Radiant Orders

**Mod Structure:**
- `CosmereCore` - Single C# assembly + shared XML defs, textures, assets
- `CosmereScadrial` - XML defs, textures, and assets only (no C# code)
- `CosmereRoshar` - XML defs, textures, and assets only (no C# code)

All world-specific C# code lives in `CosmereCore/System/{World}/` directories.

### Key Patterns

**Mod Classes:** Each world system has a `Mod.cs` in `CosmereCore/System/{World}/Mod.cs` inheriting from `CosmereMod` which provides:
- Centralized settings management
- Logging infrastructure
- Profiling capabilities
- Quickstart scenario support

**Settings Architecture:** Multi-layer settings system:
- `CosmereModSettings` (base class in `Cosmere.Settings`)
- World-specific settings in `Cosmere.System.{World}.Settings` (e.g., `ScadrialModSettings`, `RosharModSettings`)
- Automatic UI generation and persistence

**DefModExtensions:** Custom data attached to game definitions:
- `Connection.cs` - Investiture connections between entities
- `RadiantOrder.cs` - Radiant Order data for Roshar
- `MetalsLinked.cs` / `GemsLinked.cs` - Resource linking system

**Extension Methods:**
- Core extensions in `Cosmere.Extension` namespace (ThingExtension, PawnExtension, HediffExtension)
- World-specific extensions in `Cosmere.System.{World}.Extension` namespaces
- Global using statements configured in `CosmereCore.csproj` for automatic imports

### Unity Asset Pipeline

Assets are managed through Unity 2022.3.35f1:
- Assets stored in `ModName/Assets/` directories
- Built into AssetBundles via `AssetBuilder` sibling project
- Hash-based incremental building (only rebuilds changed assets)
- Platform-specific bundles (Windows/Mac/Linux)

## Development Environment

### Required Software
- .NET SDK 9.0 + .NET Framework 4.8
- Unity Hub with Unity 2022.3.35f1
- Node.js v24
- PowerShell 7

### Project Setup
1. Clone AssetBuilder as sibling directory (not inside this repo)
2. Install Node.js dependencies in `.scripts/`
3. Symlink mod directories to RimWorld's Mods folder for testing

### C# Project Configuration
- Target Framework: .NET Framework 4.8 (RimWorld requirement)
- Uses Krafs.Rimworld.Ref package for RimWorld API references
- Harmony 2.3.6 for runtime patching
- Nullable reference types enabled throughout

### File Generation
Some files are auto-generated and should not be manually edited:
- `*.generated.cs` files (DefOf classes, gene definitions)
- `*.generated.xml` files (def files for resources/traits)

## Testing and Release

### Branching Strategy
- `alpha` - Unstable/experimental work
- `beta` - Stable work ready for broader testing  
- `main` - Production releases (published to Steam)

### CI/CD
Uses semantic-release with Angular-style commit messages:
- Automated versioning and GitHub releases
- Steam Workshop publishing for main branch
- Prerelease builds for alpha/beta branches

### Local Testing
Link mod directories to RimWorld Mods folder and ensure asset bundles are built before launching RimWorld.

## Development Notes

### Lesser Spren System (CosmereRoshar)
The lesser spren particle system uses Unity ParticleSystem components to create visual spren effects throughout the game world. Key implementation details:

**Architecture:**
- `LesserSprenSpawner` (MapComponent) - Main system manager that coordinates spren spawning
- `SprenParticleSystem` - Individual particle system wrapper for each spren type
- `CellValidator` - Terrain analysis and cell validation logic  
- `SprenConfig` - Development configuration for enabling/disabling spren types

**Threading Considerations:**
- Unity objects (Mesh, ParticleSystem) MUST be created on main thread only
- Background thread handles terrain scanning and cell validation
- `InitializeOnMainThread()` pattern defers Unity object creation until main thread processes pending systems

**Spren Categories:**
- **Nature Spren** (static): Wavespren, Rockspren, Sandspren, etc. - spawn based on terrain type, cached after map initialization
- **Emotion Spren** (dynamic): Joyspren, Fearspren, etc. - spawn based on pawn mental states, updated every 250 ticks  
- **Event Spren** (dynamic): Flamespren, Deathspren, etc. - spawn based on game events (fires, corpses, births), updated every 250 ticks

**Key Files:**
- `CosmereRoshar/ParticleSystem/SprenConfig.cs` - Enable/disable individual spren types for development
- `CosmereRoshar/Comp/Map/LesserSprenSpawner.cs` - Main spawning system
- `CosmereRoshar/Map/CellValidator.cs` - Terrain detection and cell validation logic

**Development Tips:**
- Use `SprenConfig.EnabledSprenTypes` to test individual spren types
- System initializes 30 seconds after map load to avoid performance issues
- Use appropriate logging levels from Cosmere.Foundation Logger: `Verbose()`, `Info()`, `Important()`, `Warning()`, `Error()`
- Mesh errors indicate Unity objects created on wrong thread

## Available Resources

### Gems
Always check `CosmereCore/CosmereCore/GemDefOf.generated.cs` for available gems before using them in code. Available gems:
- Amethyst, Diamond, Emerald, Garnet, Heliodor, Ruby, Sapphire, Smokestone, Topaz, Zircon
- **Do NOT use non-existent gems** like Aquamarine - they will cause compilation errors

- Game logs are in: C:\Users\aequa\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios
- In the future, you dont need thigns like [Windspren Debug]  in logger statements. it already logs the file its coming from
- im on Unity 2022.3.35f1