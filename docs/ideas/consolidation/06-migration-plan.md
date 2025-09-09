# Migration Plan

## Overview

A phased approach to consolidate 5 separate C# projects into a single CosmereCore assembly while maintaining XML/Asset separation.

## Phase 1: Foundation Integration (Week 1)

### Steps
1. **Create backup branch**: `pre-consolidation-backup`
2. **Move CosmereFoundation code**:
   ```bash
   CosmereFoundation/CosmereFoundation/* → CosmereCore/CosmereCore/
   ```
3. **Update namespaces**:
   - `Cosmere.Foundation.*` → `Cosmere.*`
   - Remove Foundation prefix from all namespaces
4. **Update using statements** across all files
5. **Remove Foundation project references** from other projects
6. **Test build** and fix any compilation errors
7. **Run game** to verify basic functionality

### Validation
- [ ] CosmereCore builds successfully
- [ ] No Foundation references remain
- [ ] Game loads without errors
- [ ] Basic UI components work

## Phase 2: Resources Integration (Week 1-2)

### Steps
1. **Move CosmereResources code**:
   ```bash
   CosmereResources/CosmereResources/* → CosmereCore/CosmereCore/
   ```
2. **Relocate files**:
   - `MetalDef.cs` → `CosmereCore/Def/MetalDef.cs`
   - `GemDef.cs` → `CosmereCore/Def/GemDef.cs`
   - `MetalDefOf.cs` → `CosmereCore/DefOf/MetalDefOf.cs`
   - `GemDefOf.cs` → `CosmereCore/DefOf/GemDefOf.cs`
3. **Update namespaces**:
   - `Cosmere.Resources.*` → `Cosmere.*`
4. **Move XML definitions**:
   ```bash
   CosmereResources/Defs/Things/Metals/* → CosmereCore/Defs/Resources/Metals/
   CosmereResources/Defs/Things/Gems/* → CosmereCore/Defs/Resources/Gems/
   ```
5. **Update code generation** paths if needed
6. **Test resource loading**

### Validation
- [ ] All metals and gems load correctly
- [ ] Alloy recipes work
- [ ] Gem cutting works
- [ ] No Resources references remain

## Phase 3: Scadrial C# Integration (Week 2)

### Steps
1. **Create folder structure**:
   ```bash
   mkdir -p CosmereCore/CosmereCore/Magic/Scadrial/{Allomancy,Feruchemy,Hemalurgy}
   ```
2. **Move Scadrial C# code**:
   ```bash
   CosmereScadrial/CosmereScadrial/* → CosmereCore/CosmereCore/Magic/Scadrial/
   ```
3. **Update namespaces**:
   - `Cosmere.Scadrial.Allomancy` → `Cosmere.Magic.Scadrial.Allomancy`
   - `Cosmere.Scadrial.Feruchemy` → `Cosmere.Magic.Scadrial.Feruchemy`
   - `Cosmere.Scadrial.Hemalurgy` → `Cosmere.Magic.Scadrial.Hemalurgy`
4. **Keep XML/Assets** in CosmereScadrial mod
5. **Update CosmereScadrial/About/About.xml** to reference CosmereCore
6. **Clear CosmereScadrial/Assemblies/** folder

### Validation
- [ ] Allomancy abilities work
- [ ] Feruchemy metalminds work
- [ ] Hemalurgy spikes work
- [ ] Mistborn gene functions

## Phase 4: Roshar C# Integration (Week 2-3)

### Steps
1. **Create folder structure**:
   ```bash
   mkdir -p CosmereCore/CosmereCore/Magic/Roshar/{Spren,Fabrials,Surgebinding,RadiantOrders,Shardplate,Shardblade}
   ```
2. **Move Roshar C# code**:
   ```bash
   CosmereRoshar/CosmereRoshar/* → CosmereCore/CosmereCore/Magic/Roshar/
   ```
3. **Update namespaces**:
   - `Cosmere.Roshar.Spren` → `Cosmere.Magic.Roshar.Spren`
   - `Cosmere.Roshar.Surgebinding` → `Cosmere.Magic.Roshar.Surgebinding`
   - etc.
4. **Keep XML/Assets** in CosmereRoshar mod
5. **Update CosmereRoshar/About/About.xml** to reference only CosmereCore
6. **Clear CosmereRoshar/Assemblies/** folder

### Validation
- [ ] Surgebinding works
- [ ] Spren spawn correctly
- [ ] Fabrials function
- [ ] Radiant orders selectable

## Phase 5: Build System Updates (Week 3)

### Steps
1. **Update .csproj files**:
   - Remove old project references
   - Single CosmereCore.csproj
2. **Update CI/CD**:
   - Modify GitHub Actions for single assembly
   - Update build scripts
3. **Update asset bundler**:
   ```powershell
   # Update buildAllCosmereBundles.ps1
   # Update paths to new structure
   ```
4. **Update code generation**:
   ```bash
   # Update tools/Cosmere.Tools.Cli paths
   ```

### Validation
- [ ] CI/CD builds successfully
- [ ] Asset bundles generate correctly
- [ ] Code generation works
- [ ] Release process functions

## Phase 6: Cleanup (Week 3-4)

### Steps
1. **Remove old projects**:
   - Delete empty CosmereFoundation C# project
   - Delete empty CosmereResources C# project
   - Delete CosmereScadrial C# project files
   - Delete CosmereRoshar C# project files
2. **Update documentation**:
   - Update README files
   - Update CLAUDE.md
   - Update contributing guidelines
3. **Update Steam Workshop**:
   - Update mod descriptions
   - Update dependencies
4. **Final testing**:
   - Full playthrough test
   - Performance benchmarking
   - Memory profiling

### Validation
- [ ] No old project files remain
- [ ] Documentation is current
- [ ] Steam Workshop updated
- [ ] Performance improved or unchanged

## Rollback Plan

If issues arise at any phase:

1. **Immediate**: Revert to `pre-consolidation-backup` branch
2. **Partial**: Keep completed phases, pause migration
3. **Debug**: Use git bisect to find breaking changes
4. **Alternative**: Consider partial consolidation (e.g., keep Scadrial/Roshar separate)

## Success Metrics

- **Build time**: Target < 45 seconds
- **Load time**: Target 50% reduction
- **Memory usage**: Target 20% reduction
- **Zero gameplay regressions**
- **All tests passing**

## Timeline

- **Week 1**: Phase 1-2 (Foundation + Resources)
- **Week 2**: Phase 3-4 (World integrations)
- **Week 3**: Phase 5-6 (Build system + Cleanup)
- **Week 4**: Testing and optimization

## Risk Mitigation

1. **Create comprehensive tests** before starting
2. **Daily backups** during migration
3. **Incremental commits** with clear messages
4. **Parallel development branch** for urgent fixes
5. **Team communication** throughout process