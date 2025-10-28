# Design Document: C# Assembly Consolidation

## Context

The RimWorld: Cosmere mod currently uses 5 separate C# assemblies in a strict dependency hierarchy. This was originally designed to maintain modularity, but creates performance overhead (5 assembly loads, restricted JIT optimization) and development friction (complex debugging, public API constraints).

### Current State
- **5 assemblies**: Foundation (71 files), Core (72 files), Resources (20 files), Scadrial (117 files), Roshar (142 files)
- **Dependency chain**: Foundation → Core → Resources → {Scadrial, Roshar}
- **422 total C# files** across 5 projects
- **Multiple namespaces**: `Cosmere.Foundation.*`, `Cosmere.Core.*`, `Cosmere.Resources.*`, `Cosmere.Scadrial.*`, `Cosmere.Roshar.*`

### RimWorld Context
- RimWorld core game: Single assembly
- Royalty DLC: Just XML, no assembly
- Ideology DLC: Just XML, no assembly
- Biotech DLC: Just XML, no assembly
- **Pattern**: Each major content addition is just an XML mod. The DLC is baked into the core game DLL

## Goals / Non-Goals

### Goals
1. **Performance**: Reduce mod load time by 60%, enable better JIT optimization
2. **Development Experience**: Single compile target, easier debugging
3. **Maintainability**: Eliminate version mismatch issues, simplify build
4. **RimWorld Alignment**: Follow established DLC patterns
5. **Preserve Modularity**: Use namespaces and folder structure for logical separation

### Non-Goals
1. **Not consolidating XML/Assets**: World-specific content stays in separate mod directories
2. **Not removing logical separation**: Magic systems remain distinct via namespaces
3. **Not changing gameplay**: Zero gameplay impact
4. **Not merging Steam Workshop mods**: CosmereScadrial and CosmereRoshar remain separate mods

## Decisions

### Decision 1: Single Assembly with Namespace Organization

**Chosen Approach**: Consolidate all C# into `Cosmere.Core.dll` with namespace-based organization

**Rationale**:
- RimWorld and all DLCs use this pattern
- Better JIT optimization within single assembly
- Eliminates assembly load overhead
- Namespace organization maintains clear boundaries

**Alternatives Considered**:
1. **Keep current 5 assemblies**: Rejected - performance overhead, debugging complexity
2. **Partial consolidation (3 assemblies)**: Rejected - still has most downsides with less benefit
3. **Merge everything including XML**: Rejected - loses Steam Workshop mod identity

### Decision 2: New Namespace Structure

**Chosen Approach**:
```csharp
// Core systems (no prefix)
namespace Cosmere.Investiture { }
namespace Cosmere.UI { }
namespace Cosmere.Extension { }

// World-specific magic under Cosmere.System.{World}
namespace Cosmere.System.Roshar.Surgebinding { }
namespace Cosmere.System.Scadrial.Allomancy { }
```

**Rationale**:
- Eliminates confusing Foundation/Core/Resources prefixes
- Clear separation of shared vs world-specific code
- Matches directory structure
- Future-proof for new worlds (Nalthis, Sel, etc.)

**Alternatives Considered**:
1. **Keep separate identity namespaces**: Rejected - perpetuates confusion
2. **Flat namespace (all `Cosmere.*`)**: Rejected - loses organization for 422 files
3. **World at top level (`Cosmere.Roshar.*`)**: Rejected - doesn't show shared core clearly

### Decision 3: Phased Migration Strategy

**Chosen Approach**: 6 sequential phases with validation checkpoints

**Phase Order**:
1. Foundation → Core (utilities, UI)
2. Resources → Core (metals, gems)
3. Scadrial C# → Core/System/Scadrial
4. Roshar C# → Core/System/Roshar
5. Build system updates
6. Cleanup and documentation

**Rationale**:
- Foundation first: No dependencies, establishes pattern
- Resources second: Shared by both worlds
- Worlds last: Depend on Core systems
- Build updates after code moves: Ensures code compiles first
- Allows pause/rollback at any phase

**Alternatives Considered**:
1. **Big bang migration**: Rejected - too risky, hard to debug
2. **Worlds before Resources**: Rejected - breaks dependency order
3. **Parallel migration**: Rejected - increases conflict complexity

### Decision 4: Maintain Separate Mod Directories for XML/Assets

**Chosen Approach**:
- `CosmereCore/` - All C# code + core XML/Assets
- `CosmereScadrial/` - Scadrial XML & Assets only (no C#)
- `CosmereRoshar/` - Roshar XML & Assets only (no C#)

**Rationale**:
- Preserves Steam Workshop mod identity
- Users can still choose which worlds to install
- Asset bundles remain world-specific
- Follows RimWorld DLC pattern exactly

**Alternatives Considered**:
1. **Everything in CosmereCore**: Rejected - loses mod choice, monolithic
2. **Duplicate C# in each mod**: Rejected - maintenance nightmare
3. **Keep 5 separate mods**: Rejected - back to square one

### Decision 5: Cross-World Integration Pattern

**Chosen Approach**: Always check if other world mods are active before referencing

```csharp
namespace Cosmere.System.Roshar {
    public class Windrunner {
        public void InteractWithAllomancy() {
            if (!ModsConfig.IsActive("cosmere.scadrial")) return;
            // Safe to reference Scadrial types here
        }
    }
}
```

**Rationale**:
- Prevents crashes when only one world mod installed
- Maintains optional dependencies
- Clear pattern for future cross-world features

**Alternatives Considered**:
1. **Reflection-based access**: Rejected - performance overhead, complexity
2. **Require all mods always**: Rejected - removes user choice
3. **Separate integration assembly**: Rejected - adds complexity back

## Technical Approach

### Directory Structure After Consolidation

```
CosmereCore/
├── CosmereCore/               # All C# source
│   ├── Ability/               # Core ability system
│   ├── Investiture/           # Investiture mechanics
│   ├── UI/                    # UI components (from Foundation)
│   ├── Extension/             # Extension methods (from Foundation)
│   ├── Def/                   # MetalDef, GemDef (from Resources)
│   ├── DefOf/                 # Generated DefOf classes
│   └── System/
│       ├── Roshar/
│       │   ├── Surgebinding/
│       │   ├── Spren/
│       │   └── Fabrials/
│       └── Scadrial/
│           ├── Allomancy/
│           ├── Feruchemy/
│           └── Hemalurgy/
├── Assemblies/
│   └── Cosmere.Core.dll       # Single output
└── Defs/                      # Core shared defs only

CosmereRoshar/
├── Assemblies/                # Empty (uses CosmereCore.dll)
├── Assets/                    # Roshar textures, shaders
└── Defs/                      # Roshar-specific XML

CosmereScadrial/
├── Assemblies/                # Empty (uses CosmereCore.dll)
├── Assets/                    # Scadrial textures, shaders
└── Defs/                      # Scadrial-specific XML
```

### Build System Changes

**Current**:
```bash
dotnet build CosmereFoundation.csproj
dotnet build CosmereCore.csproj        # depends on Foundation
dotnet build CosmereResources.csproj   # depends on Core
dotnet build CosmereScadrial.csproj    # depends on Resources
dotnet build CosmereRoshar.csproj      # depends on Resources
```

**After**:
```bash
dotnet build CosmereCore.csproj        # Single build
```

### Code Generation Updates

Update `tools/Cosmere.Tools.Cli` to:
- Generate DefOf files into `CosmereCore/CosmereCore/DefOf/`
- Update search paths for new structure
- Handle world-specific def generation

### Migration Script Approach

For each phase, automated script can:
1. Move files to new locations
2. Update namespace declarations
3. Update using statements
4. Update project references
5. Validate compilation

## Risks / Trade-offs

### Risk 1: Build Time Increase
**Impact**: Medium - Single assembly takes longer to compile
**Mitigation**:
- Incremental compilation enabled
- Build caching configured
- Parallel build settings optimized
**Monitoring**: Track build times before/after each phase

### Risk 2: IDE Performance Degradation
**Impact**: Low - Large projects can slow IntelliSense
**Mitigation**:
- Modern IDEs handle large projects well
- Namespace organization aids navigation
- Solution filters can limit scope if needed
**Monitoring**: Developer feedback on IDE responsiveness

### Risk 3: Increased Coupling
**Impact**: Medium - Easier to create tight coupling without project boundaries
**Mitigation**:
- Strict code review for cross-world references
- Namespace conventions enforce separation
- ModsConfig checks required for cross-world features
**Monitoring**: Architecture reviews in PRs

### Risk 4: Migration Complexity
**Impact**: High - 422 files to move/update
**Mitigation**:
- Phased approach with validation
- Automated scripts where possible
- Comprehensive backup branch
- Can pause/rollback at any phase
**Monitoring**: Validation checklist at each phase

### Risk 5: Merge Conflicts During Migration
**Impact**: Medium - Feature freeze during migration reduces conflicts
**Mitigation**:
- Minimize parallel development during migration
- Use feature branches
- Clear communication on migration status
- Fast migration pace (1-2 weeks per phase)
**Monitoring**: Git conflict frequency

## Migration Plan

### Pre-Migration Checklist
- [ ] Create `pre-consolidation-backup` branch
- [ ] Ensure all tests pass
- [ ] Document current performance baselines
- [ ] Communicate migration start to team
- [ ] Create migration tracking issue

### Phase 1: Foundation Integration (Week 1)
- Move 71 files from Foundation to Core
- Update namespaces: `Cosmere.Foundation.*` → `Cosmere.*`
- Remove Foundation project references
- **Validation**: Core builds, game loads, UI works

### Phase 2: Resources Integration (Week 2)
- Move 20 files from Resources to Core
- Move metal/gem definitions to Core/Defs/Resources/
- Update code generation paths
- **Validation**: Metals load, alloy recipes work, gems cut

### Phase 3: Scadrial C# Integration (Week 3-4)
- Move 117 files to Core/System/Scadrial/
- Update namespaces to `Cosmere.System.Scadrial.*`
- Clear Scadrial/Assemblies/ folder
- **Validation**: Allomancy works, Feruchemy works, genes work

### Phase 4: Roshar C# Integration (Week 5-6)
- Move 142 files to Core/System/Roshar/
- Update namespaces to `Cosmere.System.Roshar.*`
- Clear Roshar/Assemblies/ folder
- **Validation**: Surgebinding works, spren spawn, fabrials work

### Phase 5: Build System Updates (Week 7)
- Update Cosmere.sln
- Update all .csproj files
- Update build scripts
- Update CI/CD workflows
- Update code generation tool
- **Validation**: CI/CD builds, releases work, asset bundles build

### Phase 6: Cleanup (Week 8)
- Remove old project files
- Update all documentation
- Update Steam Workshop descriptions
- Performance benchmarking
- **Validation**: All tests pass, performance improved

### Rollback Procedure
If critical issues arise:
1. Stop migration at current phase
2. Review issue severity
3. If blocking: `git reset --hard` to `pre-consolidation-backup`
4. If minor: Fix issue and continue
5. Document issue and mitigation

## Open Questions

1. **Should we create automated migration scripts for namespace updates?**
   - Recommendation: Yes, for at least namespace and using statement updates
   - Reduces human error, speeds migration

2. **What performance testing should we do at each phase?**
   - Recommendation: Load time, memory usage, frame rate sampling
   - Compare against baseline from pre-migration

3. **Should we feature-freeze during migration?**
   - Recommendation: Soft freeze - critical bugs only
   - Reduces merge conflicts, faster migration

4. **How do we handle hotfixes during migration?**
   - Recommendation: Apply to backup branch, cherry-pick to main
   - Maintain parallel fix capability

5. **Should we update code generation tool first or after?**
   - Recommendation: After Phase 2 (Resources)
   - Allows manual verification of pattern before automation
