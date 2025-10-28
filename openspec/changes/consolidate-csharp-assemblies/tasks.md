# Implementation Tasks

## Pre-Migration (Phase 0)

- [ ] 0.1 Create `pre-consolidation-backup` branch from current state
- [ ] 0.2 Run full test suite and document passing state
- [ ] 0.3 Document performance baselines (load time, memory usage, frame rate)
- [ ] 0.4 Create migration tracking issue in GitHub
- [ ] 0.5 Review and approve this consolidation proposal
- [ ] 0.6 Communicate migration timeline to team

## Phase 1: Foundation Integration

### 1.1 Foundation Code Migration
- [ ] 1.1.1 Create branch `consolidation/phase-1-foundation`
- [ ] 1.1.2 Move all files from `CosmereFoundation/CosmereFoundation/*` to `CosmereCore/CosmereCore/`
- [ ] 1.1.3 Update all namespace declarations from `Cosmere.Foundation.*` to `Cosmere.*`
- [ ] 1.1.4 Update all `using Cosmere.Foundation.*` statements across entire codebase
- [ ] 1.1.5 Remove `<ProjectReference>` to CosmereFoundation from CosmereCore.csproj

### 1.2 Foundation Project Cleanup
- [ ] 1.2.1 Remove CosmereFoundation project from Cosmere.sln
- [ ] 1.2.2 Update remaining projects to remove Foundation references
- [ ] 1.2.3 Verify no references to Cosmere.Foundation.dll remain

### 1.3 Phase 1 Validation
- [ ] 1.3.1 Build CosmereCore successfully
- [ ] 1.3.2 Build entire solution successfully
- [ ] 1.3.3 Launch game and verify it loads without errors
- [ ] 1.3.4 Test UI components (settings, windows, widgets)
- [ ] 1.3.5 Verify logging system works
- [ ] 1.3.6 Run automated tests
- [ ] 1.3.7 Commit Phase 1 changes with clear message
- [ ] 1.3.8 Create PR for Phase 1 review

## Phase 2: Resources Integration

### 2.1 Resources Code Migration
- [ ] 2.1.1 Create branch `consolidation/phase-2-resources`
- [ ] 2.1.2 Move all files from `CosmereResources/CosmereResources/*` to `CosmereCore/CosmereCore/`
- [ ] 2.1.3 Relocate `MetalDef.cs` to `CosmereCore/CosmereCore/Def/MetalDef.cs`
- [ ] 2.1.4 Relocate `GemDef.cs` to `CosmereCore/CosmereCore/Def/GemDef.cs`
- [ ] 2.1.5 Relocate DefOf files to `CosmereCore/CosmereCore/DefOf/`
- [ ] 2.1.6 Update all namespace declarations from `Cosmere.Resources.*` to `Cosmere.Def.*` or `Cosmere.*`
- [ ] 2.1.7 Update all `using Cosmere.Resources.*` statements across codebase

### 2.2 Resources XML Migration
- [ ] 2.2.1 Move `CosmereResources/Defs/Things/Metals/*` to `CosmereCore/Defs/Resources/Metals/`
- [ ] 2.2.2 Move `CosmereResources/Defs/Things/Gems/*` to `CosmereCore/Defs/Resources/Gems/`
- [ ] 2.2.3 Update any def references in code if needed
- [ ] 2.2.4 Move resource-related recipes to CosmereCore

### 2.3 Resources Project Cleanup
- [ ] 2.3.1 Remove CosmereResources project from Cosmere.sln
- [ ] 2.3.2 Update remaining projects to remove Resources references
- [ ] 2.3.3 Verify no references to Cosmere.Resources.dll remain

### 2.4 Phase 2 Validation
- [ ] 2.4.1 Build solution successfully
- [ ] 2.4.2 Launch game and load a save
- [ ] 2.4.3 Verify all metals load correctly (check def list)
- [ ] 2.4.4 Verify all gems load correctly
- [ ] 2.4.5 Test alloy crafting recipes
- [ ] 2.4.6 Test gem cutting recipes
- [ ] 2.4.7 Run automated tests
- [ ] 2.4.8 Commit Phase 2 changes
- [ ] 2.4.9 Create PR for Phase 2 review

## Phase 3: Scadrial C# Integration

### 3.1 Scadrial Directory Setup
- [ ] 3.1.1 Create branch `consolidation/phase-3-scadrial`
- [ ] 3.1.2 Create folder structure `CosmereCore/CosmereCore/System/Scadrial/`
- [ ] 3.1.3 Create subfolders: `Allomancy/`, `Feruchemy/`, `Hemalurgy/`

### 3.2 Scadrial Code Migration
- [ ] 3.2.1 Move all files from `CosmereScadrial/CosmereScadrial/*` to `CosmereCore/CosmereCore/System/Scadrial/`
- [ ] 3.2.2 Maintain sub-folder structure within Scadrial
- [ ] 3.2.3 Update namespace: `Cosmere.Scadrial.Allomancy` → `Cosmere.System.Scadrial.Allomancy`
- [ ] 3.2.4 Update namespace: `Cosmere.Scadrial.Feruchemy` → `Cosmere.System.Scadrial.Feruchemy`
- [ ] 3.2.5 Update namespace: `Cosmere.Scadrial.Hemalurgy` → `Cosmere.System.Scadrial.Hemalurgy`
- [ ] 3.2.6 Update all using statements referencing Scadrial
- [ ] 3.2.7 Update CosmereCore.csproj to include new files

### 3.3 Scadrial Mod Cleanup
- [ ] 3.3.1 Remove C# source from CosmereScadrial directory
- [ ] 3.3.2 Clear `CosmereScadrial/Assemblies/` folder
- [ ] 3.3.3 Update `CosmereScadrial/About/About.xml` to depend on CosmereCore
- [ ] 3.3.4 Verify XML/Assets remain in CosmereScadrial
- [ ] 3.3.5 Remove Cosmere.Scadrial.csproj from solution

### 3.4 Phase 3 Validation
- [ ] 3.4.1 Build solution successfully
- [ ] 3.4.2 Launch game with CosmereCore and CosmereScadrial
- [ ] 3.4.3 Test Allomancy abilities (burning metals)
- [ ] 3.4.4 Test Feruchemy metalminds (storing/tapping)
- [ ] 3.4.5 Test Hemalurgy spikes
- [ ] 3.4.6 Verify Mistborn gene functions correctly
- [ ] 3.4.7 Test vial consumption
- [ ] 3.4.8 Run automated tests
- [ ] 3.4.9 Commit Phase 3 changes
- [ ] 3.4.10 Create PR for Phase 3 review

## Phase 4: Roshar C# Integration

### 4.1 Roshar Directory Setup
- [ ] 4.1.1 Create branch `consolidation/phase-4-roshar`
- [ ] 4.1.2 Create folder structure `CosmereCore/CosmereCore/System/Roshar/`
- [ ] 4.1.3 Create subfolders: `Spren/`, `Fabrials/`, `Surgebinding/`, `RadiantOrders/`, `Shardplate/`, `Shardblade/`

### 4.2 Roshar Code Migration
- [ ] 4.2.1 Move all files from `CosmereRoshar/CosmereRoshar/*` to `CosmereCore/CosmereCore/System/Roshar/`
- [ ] 4.2.2 Maintain sub-folder structure within Roshar
- [ ] 4.2.3 Update namespace: `Cosmere.Roshar.Spren` → `Cosmere.System.Roshar.Spren`
- [ ] 4.2.4 Update namespace: `Cosmere.Roshar.Surgebinding` → `Cosmere.System.Roshar.Surgebinding`
- [ ] 4.2.5 Update namespace: `Cosmere.Roshar.Fabrials` → `Cosmere.System.Roshar.Fabrials`
- [ ] 4.2.6 Update all other Roshar namespaces to new pattern
- [ ] 4.2.7 Update all using statements referencing Roshar
- [ ] 4.2.8 Update CosmereCore.csproj to include new files

### 4.3 Roshar Mod Cleanup
- [ ] 4.3.1 Remove C# source from CosmereRoshar directory
- [ ] 4.3.2 Clear `CosmereRoshar/Assemblies/` folder
- [ ] 4.3.3 Update `CosmereRoshar/About/About.xml` to depend only on CosmereCore
- [ ] 4.3.4 Verify XML/Assets remain in CosmereRoshar
- [ ] 4.3.5 Remove Cosmere.Roshar.csproj from solution

### 4.4 Phase 4 Validation
- [ ] 4.4.1 Build solution successfully
- [ ] 4.4.2 Launch game with CosmereCore and CosmereRoshar
- [ ] 4.4.3 Test Surgebinding abilities (all 10 surges)
- [ ] 4.4.4 Verify lesser spren spawn correctly
- [ ] 4.4.5 Test spren bond formation
- [ ] 4.4.6 Test fabrial functionality
- [ ] 4.4.7 Verify Radiant Order selection works
- [ ] 4.4.8 Test Shardblade summoning
- [ ] 4.4.9 Test Shardplate functionality
- [ ] 4.4.10 Run automated tests
- [ ] 4.4.11 Commit Phase 4 changes
- [ ] 4.4.12 Create PR for Phase 4 review

## Phase 5: Build System Updates

### 5.1 Solution File Updates
- [ ] 5.1.1 Create branch `consolidation/phase-5-build-system`
- [ ] 5.1.2 Update `Cosmere.sln` to remove old projects
- [ ] 5.1.3 Verify only CosmereCore and tools projects remain
- [ ] 5.1.4 Test solution builds in Visual Studio
- [ ] 5.1.5 Test solution builds with `dotnet build`

### 5.2 Project File Updates
- [ ] 5.2.1 Review CosmereCore.csproj for any needed optimizations
- [ ] 5.2.2 Enable incremental compilation settings
- [ ] 5.2.3 Configure parallel build settings
- [ ] 5.2.4 Update output paths if needed
- [ ] 5.2.5 Verify all new files are included in build

### 5.3 Build Script Updates
- [ ] 5.3.1 Update `buildAllCosmereBundles.ps1` for new structure
- [ ] 5.3.2 Update `scripts/build-assets.csx` if needed
- [ ] 5.3.3 Test asset bundle building for all mods
- [ ] 5.3.4 Verify bundles output to correct directories

### 5.4 Code Generation Updates
- [ ] 5.4.1 Update `tools/Cosmere.Tools.Cli` search paths
- [ ] 5.4.2 Update DefOf output paths to `CosmereCore/CosmereCore/DefOf/`
- [ ] 5.4.3 Test code generation: `dotnet run --project tools/Cosmere.Tools.Cli generate`
- [ ] 5.4.4 Verify generated files appear in correct locations
- [ ] 5.4.5 Update any generator templates if needed

### 5.5 CI/CD Pipeline Updates
- [ ] 5.5.1 Update GitHub Actions workflows in `.github/workflows/`
- [ ] 5.5.2 Update build steps to build single project
- [ ] 5.5.3 Update release packaging steps
- [ ] 5.5.4 Update Steam Workshop publishing configuration
- [ ] 5.5.5 Test CI/CD on feature branch
- [ ] 5.5.6 Verify artifacts contain correct structure

### 5.6 Phase 5 Validation
- [ ] 5.6.1 Clean build entire solution
- [ ] 5.6.2 Verify build time < 45 seconds
- [ ] 5.6.3 Test asset bundle generation
- [ ] 5.6.4 Test code generation
- [ ] 5.6.5 Trigger CI/CD build and verify success
- [ ] 5.6.6 Commit Phase 5 changes
- [ ] 5.6.7 Create PR for Phase 5 review

## Phase 6: Cleanup and Documentation

### 6.1 File Cleanup
- [ ] 6.1.1 Create branch `consolidation/phase-6-cleanup`
- [ ] 6.1.2 Delete empty `CosmereFoundation/` directory
- [ ] 6.1.3 Delete empty `CosmereResources/` directory
- [ ] 6.1.4 Remove `CosmereScadrial/CosmereScadrial/` C# directory
- [ ] 6.1.5 Remove `CosmereRoshar/CosmereRoshar/` C# directory
- [ ] 6.1.6 Clean up any leftover `.csproj` or `.sln` files
- [ ] 6.1.7 Remove old build output directories

### 6.2 Documentation Updates
- [ ] 6.2.1 Update `README.md` in repository root
- [ ] 6.2.2 Update `CLAUDE.md` with new architecture
- [ ] 6.2.3 Update architecture documentation
- [ ] 6.2.4 Update contributing guidelines
- [ ] 6.2.5 Update development setup instructions
- [ ] 6.2.6 Document new namespace conventions
- [ ] 6.2.7 Update code generation documentation

### 6.3 Steam Workshop Updates
- [ ] 6.3.1 Update CosmereCore mod description
- [ ] 6.3.2 Update CosmereScadrial mod description
- [ ] 6.3.3 Update CosmereRoshar mod description
- [ ] 6.3.4 Update dependency declarations on Workshop
- [ ] 6.3.5 Update mod preview images if needed
- [ ] 6.3.6 Post changelog/announcement about consolidation

### 6.4 Performance Benchmarking
- [ ] 6.4.1 Measure mod load time (compare to baseline)
- [ ] 6.4.2 Measure memory usage (compare to baseline)
- [ ] 6.4.3 Measure build time
- [ ] 6.4.4 Profile frame rate in various scenarios
- [ ] 6.4.5 Document performance improvements
- [ ] 6.4.6 Verify performance targets met (60% load time improvement goal)

### 6.5 Final Testing
- [ ] 6.5.1 Full playthrough test (several in-game days)
- [ ] 6.5.2 Test with only CosmereCore + CosmereRoshar
- [ ] 6.5.3 Test with only CosmereCore + CosmereScadrial
- [ ] 6.5.4 Test with all mods enabled
- [ ] 6.5.5 Test save/load functionality
- [ ] 6.5.6 Verify no regression in gameplay features
- [ ] 6.5.7 Run full automated test suite
- [ ] 6.5.8 Test on clean RimWorld install

### 6.6 Phase 6 Completion
- [ ] 6.6.1 Commit Phase 6 changes
- [ ] 6.6.2 Create PR for Phase 6 review
- [ ] 6.6.3 Merge all phases to main branch
- [ ] 6.6.4 Tag release with new version
- [ ] 6.6.5 Publish to Steam Workshop
- [ ] 6.6.6 Archive consolidation documentation
- [ ] 6.6.7 Close migration tracking issue
- [ ] 6.6.8 Announce completion to community

## Post-Migration

- [ ] 7.1 Monitor community feedback for issues
- [ ] 7.2 Track load time improvements in telemetry
- [ ] 7.3 Document lessons learned
- [ ] 7.4 Update team workflows for new structure
- [ ] 7.5 Archive `pre-consolidation-backup` branch after stability confirmed

## Rollback Procedure (If Needed)

- [ ] R.1 Identify blocking issue
- [ ] R.2 Document issue severity and impact
- [ ] R.3 Determine if hotfix possible or rollback needed
- [ ] R.4 If rollback: `git reset --hard pre-consolidation-backup`
- [ ] R.5 Restore old build process
- [ ] R.6 Communicate rollback to team and community
- [ ] R.7 Analyze root cause of failure
- [ ] R.8 Update consolidation plan with fixes
- [ ] R.9 Schedule retry if appropriate

## Notes

- Each phase should be completed in sequence
- Create separate PRs for each phase for easier review
- Validate thoroughly at each phase before proceeding
- Can pause migration between phases if needed
- Keep `pre-consolidation-backup` branch until consolidation stable in production
- Estimated 1-2 weeks per phase depending on testing rigor
