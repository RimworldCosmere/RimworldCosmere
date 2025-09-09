# Cosmere Mod Consolidation Overview

## Executive Summary

Consolidate all C# code from 5 separate projects (Foundation, Core, Resources, Scadrial, Roshar) into a single `CosmereCore` assembly while maintaining XML/Asset separation by world.

## Current Architecture

**Current C# Distribution:**
- CosmereFoundation: 71 files (shared utilities, UI, base classes)
- CosmereCore: 72 files (investiture system, stats, traits)
- CosmereResources: 20 files (metals, alloys, gems)
- CosmereScadrial: 117 files (Allomancy, Feruchemy, Hemalurgy)
- CosmereRoshar: 142 files (Surgebinding, spren, stormlight)
- **Total: 422 C# files across 5 assemblies**

**Current Dependency Chain:**
```
Foundation → Core → Resources → {Scadrial, Roshar}
```

## Proposed Architecture

**Single Assembly:**
- All C# code consolidated into `CosmereCore.dll`
- XML/Assets remain in separate mod directories
- Logical separation maintained through namespaces

**Mod Structure:**
- `CosmereCore/` - All C# code + shared XML/Assets
- `CosmereScadrial/` - Scadrial-specific XML/Assets only
- `CosmereRoshar/` - Roshar-specific XML/Assets only

## Key Benefits

1. **Performance**: Single assembly load, better JIT optimization
2. **Development**: Simplified debugging, faster iteration
3. **Maintenance**: Unified versioning, reduced complexity
4. **RimWorld Alignment**: Follows DLC architecture pattern

## Key Risks

1. **Build Times**: Larger assembly takes longer to compile
2. **Merge Conflicts**: More developers in same project
3. **IDE Performance**: Large project may slow IntelliSense

## Recommendation

**Proceed with consolidation** - Benefits significantly outweigh costs, especially for performance and maintainability.