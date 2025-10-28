# Project Context

## Purpose
RimWorld: Cosmere is a full-conversion mod series bringing Brandon Sanderson's Cosmere universe into RimWorld. The project aims to faithfully recreate the magic systems (Investiture), lore, and world-building from the Cosmere novels as playable RimWorld content. Built as a modular monorepo where each component is a separate loadable RimWorld mod.

## Tech Stack

### Core Technologies
- **C# / .NET Framework 4.8** - Primary language (RimWorld requirement)
- **Unity 2022.3.35f1** - Asset creation and AssetBundle building
- **Harmony 2.3.6** - Runtime patching library for modding RimWorld
- **Krafs.Rimworld.Ref** - NuGet package providing RimWorld API references

### Build Tools
- **PowerShell 7** - Asset bundle build scripts
- **dotnet script** - C# scripting for build automation
- **Node.js v24** - Legacy code generation (being migrated to .NET tools)
- **.NET SDK 9.0** - Development tooling (Cosmere.Tools.Cli)

### Development Tools
- **Nullable reference types** - Enabled throughout for better null safety
- **semantic-release** - Automated versioning and releases
- **GitHub Actions** - CI/CD pipeline
- **Steam Workshop** - Distribution platform

## Project Conventions

### Code Style
- **Nullable Reference Types**: Enabled project-wide, use `?` for nullable types
- **Naming Conventions**:
  - PascalCase for public members, classes, methods
  - camelCase for private fields (with `_` prefix: `_fieldName`)
  - UPPER_CASE for constants
- **Logging**: Use Cosmere.Foundation `Logger` with levels: `Verbose()`, `Info()`, `Important()`, `Warning()`, `Error()`
  - Do NOT add redundant prefixes like `[ClassName Debug]` - logging already includes file context
- **File Organization**: Group by feature/system, not by type (e.g., `Surgebinding/` not `Controllers/`, `Models/`)
- **Generated Files**: Never manually edit `*.generated.cs` or `*.generated.xml` files

### Architecture Patterns

#### Modular Dependency Hierarchy
Strict layered architecture enforced:
```
Foundation Layer:
  └─ CosmereFoundation (utilities, UI, base classes)
      ↓
Core Layer:
  ├─ CosmereCore (Investiture system, stats, traits)
  └─ CosmereResources (metals, alloys, gems)
      ↓
Content Modules (independent of each other):
  ├─ CosmereScadrial (Allomancy, Feruchemy, Hemalurgy)
  ├─ CosmereRoshar (Surgebinding, spren, Radiants)
  └─ Future: Nalthis, Sel, etc.
```

#### Key Architectural Components

**Mod Classes**: Each module has a `Mod.cs` inheriting from `CosmereMod`:
- Centralized settings management
- Logging infrastructure with profiling
- Quickstart scenario support

**Settings System**: Multi-layer hierarchy:
- Base: `CosmereModSettings` (Foundation)
- Module-specific: `ScadrialModSettings`, `RosharModSettings`
- Automatic UI generation and persistence

**DefModExtensions**: Attach custom data to RimWorld definitions:
- `Connection.cs` - Investiture connections between entities
- `RadiantOrder.cs` - Radiant Order data
- `MetalsLinked.cs` / `GemsLinked.cs` - Resource linking

**Threading Patterns**:
- Unity objects (Mesh, ParticleSystem) MUST be created on main thread
- Use `InitializeOnMainThread()` pattern to defer Unity object creation
- Background threads for terrain scanning and heavy computation

#### Asset Management
- Assets in `ModName/Assets/` directories
- Built into platform-specific AssetBundles
- Hash-based incremental building (only rebuilds changed assets)
- AssetBuilder project as sibling directory (not in repo)

### Testing Strategy
- Local testing via symlinked mod directories to RimWorld's Mods folder
- Build asset bundles before testing: `./buildAllCosmereBundles.ps1`
- Game logs location: `C:\Users\aequa\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios`
- Alpha branch for experimental work
- Beta branch for broader testing
- Main branch for production releases

### Git Workflow

**Branch Strategy**:
- `alpha` - Unstable/experimental development
- `beta` - Stable work ready for broader testing
- `main` - Production releases (auto-published to Steam Workshop)

**Commit Conventions**: Angular-style for semantic-release:
- `feat:` - New features (minor version bump)
- `fix:` - Bug fixes (patch version bump)
- `BREAKING CHANGE:` - Breaking changes (major version bump)
- `docs:`, `chore:`, `refactor:`, etc. - No version bump

**CI/CD**:
- Automated versioning via semantic-release
- GitHub releases with changelogs
- Automatic Steam Workshop publishing for main branch
- Prerelease builds for alpha/beta

## Domain Context

### Cosmere Universe (Brandon Sanderson)
The Cosmere is a shared universe across multiple novel series, each taking place on different "Shard Worlds":

- **Scadrial** (Mistborn series): Allomancy (burning metals), Feruchemy (storing attributes), Hemalurgy (stealing powers)
- **Roshar** (Stormlight Archive): Surgebinding (manipulating fundamental forces), spren bonds, Radiant Orders, Ideals
- **Nalthis** (Warbreaker): Awakening, Breath manipulation
- **Sel** (Elantris): Elantrian magic systems

**Investiture**: The magical energy system underlying all Cosmere magic. Entities can be "Invested" with magical abilities.

### RimWorld Integration
- **RimWorld** is a sci-fi colony simulation game with deep modding support
- Mods are loaded as directories in `RimWorld/Mods/`
- XML definitions for game content (items, buildings, creatures)
- C# assemblies for custom logic and game mechanics
- AssetBundles for custom Unity assets (textures, shaders, audio)

### Key Systems

**Lesser Spren (Roshar)**:
- Particle-based visual spren spawning system
- Categories: Nature (static terrain-based), Emotion (dynamic from pawns), Event (dynamic from game events)
- Spawns 30 seconds after map load to avoid performance issues
- `SprenConfig.cs` for enabling/disabling individual spren types during development

**Resource System**:
- Always check `CosmereResources/GemDefOf.generated.cs` for available gems
- Available gems: Amethyst, Diamond, Emerald, Garnet, Heliodor, Ruby, Sapphire, Smokestone, Topaz, Zircon
- **Do NOT use non-existent gems** (e.g., Aquamarine) - causes compilation errors

## Important Constraints

### Technical Constraints
- **Target Framework**: .NET Framework 4.8 (RimWorld requirement, not modern .NET)
- **Unity Version**: Must use 2022.3.35f1 exactly (AssetBundle compatibility)
- **Main Thread Requirement**: All Unity object creation must happen on main thread
- **RimWorld Compatibility**: Must work with current RimWorld version and follow modding conventions
- **Performance**: Large particle systems and background processing must be optimized for gameplay

### Development Constraints
- Generated files (`*.generated.cs`, `*.generated.xml`) must not be manually edited
- AssetBuilder must be installed as sibling directory
- Mod directories must be symlinked to RimWorld for testing
- Asset bundles must be rebuilt before testing changes in-game

### Design Constraints
- Lore accuracy to Brandon Sanderson's Cosmere universe
- Modules must remain independent (no cross-dependencies between content modules)
- Follow RimWorld's game balance and design philosophy

## External Dependencies

### RimWorld API
- **Krafs.Rimworld.Ref** NuGet package - Provides RimWorld game API references
- **Harmony 2.3.6** - Runtime method patching/hooking library
- RimWorld mod loading and lifecycle hooks

### Unity
- **Unity 2022.3.35f1** - Required for AssetBundle building
- ParticleSystem API for spren effects
- Shader support (custom shaders in `Assets/Materials/`)

### Build Pipeline
- **AssetBuilder** (sibling project) - Unity project for building AssetBundles
- Hash-based asset tracking system for incremental builds
- Platform-specific bundle outputs (Windows/Mac/Linux)

### Distribution
- **Steam Workshop** - Primary distribution platform
- GitHub Releases - Secondary distribution and version tracking
- semantic-release automation

### Code Generation
- **Current**: `dotnet run --project tools/Cosmere.Tools.Cli generate`
- **Legacy**: Node.js scripts in `.scripts/` (being phased out)
- Generates DefOf classes and XML definition files
