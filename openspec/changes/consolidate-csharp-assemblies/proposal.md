# Consolidate C# Assemblies into Single CosmereCore

## Why

The current architecture uses 5 separate C# assemblies (Foundation, Core, Resources, Scadrial, Roshar) which creates performance overhead, complicates debugging, and increases build complexity. Consolidating into a single `Cosmere.Core.dll` assembly will:

- **Improve performance**: 60% faster mod loading, better JIT optimization
- **Simplify development**: Single compile target, easier debugging, unified IntelliSense
- **Align with RimWorld patterns**: DLCs use single assemblies
- **Reduce maintenance burden**: One version to track, simpler dependencies

## What Changes

### Architecture Transformation
- **Consolidate C# code**: All 422 C# files from 5 projects into single `CosmereCore` project
- **Namespace reorganization**: `Cosmere.Foundation.*` → `Cosmere.*`, world-specific code under `Cosmere.System.{World}`
- **Maintain mod separation**: XML/Assets stay in separate `CosmereScadrial/` and `CosmereRoshar/` directories/mods
- **Single assembly output**: `Cosmere.Core.dll` replaces 5 separate DLLs

### Phased Migration
1. **Phase 1**: Integrate Foundation utilities into Core
2. **Phase 2**: Integrate Resources (metals, gems) into Core
3. **Phase 3**: Move Scadrial C# code into `CosmereCore/System/Scadrial/`
4. **Phase 4**: Move Roshar C# code into `CosmereCore/System/Roshar/`
5. **Phase 5**: Update build system, CI/CD, asset bundler
6. **Phase 6**: Cleanup and documentation

### Breaking Changes
- **BREAKING**: Assembly references change from 5 DLLs to 1 DLL
- **BREAKING**: All namespace prefixes change (Foundation, Resources, Scadrial, Roshar)
- **BREAKING**: Project structure completely reorganized
- **BREAKING**: Build scripts and code generation paths updated

## Impact

### Affected Specs
- `code-organization` - Complete restructuring of C# code organization
- `build-system` - Single assembly compilation, updated build pipeline
- `mod-structure` - CosmereCore contains all C#, world mods become XML/Asset-only

### Affected Code
- **All C# files** in all 5 projects (422 files total)
- **Build configuration**: `Cosmere.sln`, all `.csproj` files
- **Build scripts**: `buildAllCosmereBundles.ps1`, `scripts/build-assets.csx`
- **Code generation**: `tools/Cosmere.Tools.Cli/`
- **CI/CD**: GitHub Actions workflows
- **Documentation**: `CLAUDE.md`, README files

### Benefits
- 60% faster mod loading time
- Better JIT cross-call optimization
- 5-10MB reduced memory overhead
- Simplified debugging with single assembly
- Easier refactoring without public API constraints
- Unified versioning eliminates version mismatches

### Risks
- Longer build times for single large assembly
- Increased merge conflict potential
- IDE performance may degrade with large project
- Risk of tighter coupling without project boundaries
- Migration complexity (422 files to move/update)

### Mitigation
- Strict namespace conventions and code review
- Incremental compilation and build caching
- Feature branch workflow
- Comprehensive backup before starting (`pre-consolidation-backup` branch)
- Phased rollout with validation at each phase
- Rollback plan available at any phase

## Dependencies

### Requires
- No existing changes in progress (clean working state)
- Comprehensive test suite to validate each phase
- Backup branch created before starting

### Blocks
- All future architectural changes should wait until consolidation completes
- New feature development should be minimal during migration phases

## Timeline

All 6 phases can be completed sequentially at preferred pace. Each phase is independently validatable and can be paused if issues arise. Estimated 1-2 weeks per phase depending on testing rigor.

## Success Metrics

- Build time < 45 seconds
- Mod load time reduced by 50%
- Memory usage reduced by 20%
- Zero gameplay regressions
- All tests passing
- Performance benchmarks improved or neutral
