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

# Available options
dotnet run --project tools/Cosmere.Tools.Cli generate --help
# -f, --force     Force generation ignoring changes
# --dry-run       Show what would be generated
# -d, --delete    Delete existing generated files first
# -v, --verbose   Show detailed output

# Legacy Node.js method (deprecated - will be removed)
cd .scripts
npm install
npm start -- -d -f
```

### Building C# Projects
```bash
# Build entire solution
dotnet build Cosmere.sln

# Build specific project
dotnet build CosmereScadrial/CosmereScadrial/Cosmere.Scadrial.csproj

# Build tools CLI
dotnet build tools/Cosmere.Tools.Cli/Cosmere.Tools.Cli.csproj
```

## Architecture Overview

### Modular Structure
The project follows a strict dependency hierarchy:

**Foundation Layer:**
- `CosmereFoundation` - Shared C# utilities, base classes, UI components, and extension methods (no game content)

**Core Layer:**
- `CosmereCore` - Shared Investiture system, stats, traits, needs, and base game mechanics
- `CosmereResources` - All metals, alloys, gems, and godmetals used across the Cosmere

**Content Modules (Shard Worlds):**
- `CosmereScadrial` - Allomancy, Feruchemy, Hemalurgy, vial systems, Mistborn genes
- `CosmereRoshar` - Surgebinding, spren bonding, stormlight, Ideals, Radiant Orders
- Future: Nalthis (Awakening), Sel (Elantrians)

Each content module depends on Foundation → Core → Resources but modules are independent of each other.

### Key Patterns

**Mod Classes:** Each module has a `Mod.cs` inheriting from `CosmereMod` (Foundation) which provides:
- Centralized settings management
- Logging infrastructure
- Profiling capabilities
- Quickstart scenario support

**Settings Architecture:** Multi-layer settings system:
- `CosmereModSettings` (base class in Foundation)
- Module-specific settings (e.g., `ScadrialModSettings`)
- Automatic UI generation and persistence

**DefModExtensions:** Custom data attached to game definitions:
- `Connection.cs` - Investiture connections between entities  
- `RadiantOrder.cs` - Radiant Order data for Roshar
- `MetalsLinked.cs` / `GemsLinked.cs` - Resource linking system

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