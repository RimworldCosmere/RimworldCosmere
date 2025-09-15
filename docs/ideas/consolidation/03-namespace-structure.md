# Namespace Structure

## Core Principles

1. **Eliminate separate identity namespaces** (no more Foundation, Core, Resources prefixes)
2. **Everything under unified `Cosmere` namespace**
3. **Magic systems clearly separated by world**
4. **Logical grouping by functionality**

## Namespace Hierarchy

```csharp
// Core Systems (merged from Foundation, Core, Resources)
namespace Cosmere.Ability { }              // Abstract ability system
namespace Cosmere.Investiture { }          // Investiture mechanics
namespace Cosmere.Entity { }               // Shards, splinters, etc.
namespace Cosmere.Extension { }            // Extension methods
namespace Cosmere.UI { }                   // UI components
namespace Cosmere.Utility { }              // General utilities
namespace Cosmere.Settings { }             // Mod settings
namespace Cosmere.Patch { }                // Harmony patches
namespace Cosmere.Comp { }                 // Components
namespace Cosmere.Def { }                  // Custom def types
namespace Cosmere.DefOf { }                // Generated DefOf classes
namespace Cosmere.DefModExtension { }      // Def mod extensions
namespace Cosmere.Gene { }                 // Gene-related code
namespace Cosmere.Gizmo { }                // Custom gizmos
namespace Cosmere.Hediff { }               // Core hediffs
namespace Cosmere.Job { }                  // Core jobs
namespace Cosmere.Need { }                 // Need_Investiture, etc.
namespace Cosmere.Shader { }               // Shader utilities
namespace Cosmere.Stat { }                 // Stat parts
namespace Cosmere.Thing { }                // Core things

// World-Specific Magic Systems
namespace Cosmere.Magic.Roshar.Spren { }
namespace Cosmere.Magic.Roshar.Fabrials { }
namespace Cosmere.Magic.Roshar.Surgebinding { }
namespace Cosmere.Magic.Roshar.RadiantOrders { }
namespace Cosmere.Magic.Roshar.Shardplate { }
namespace Cosmere.Magic.Roshar.Shardblade { }

namespace Cosmere.Magic.Scadrial.Allomancy { }
namespace Cosmere.Magic.Scadrial.Feruchemy { }
namespace Cosmere.Magic.Scadrial.Hemalurgy { }

// Future Worlds (examples)
namespace Cosmere.Magic.Nalthis.Awakening { }
namespace Cosmere.Magic.Sel.AonDor { }
namespace Cosmere.Magic.Taldain.SandMastery { }
```

## Migration Examples

### Before Consolidation
```csharp
using Cosmere.Foundation.Extension;
using Cosmere.Foundation.UI;
using Cosmere.Core.Investiture;
using Cosmere.Core.Entity;
using Cosmere.Resources.Metal;
using Cosmere.Resources.Gem;
using Cosmere.Roshar.Surgebinding;
using Cosmere.Scadrial.Allomancy;
```

### After Consolidation
```csharp
using Cosmere.Extension;
using Cosmere.UI;
using Cosmere.Investiture;
using Cosmere.Entity;
using Cosmere.Def;  // MetalDef, GemDef now here
using Cosmere.Magic.Roshar.Surgebinding;
using Cosmere.Magic.Scadrial.Allomancy;
```

## Namespace Rules

### DO:
- Keep world-specific code under `Cosmere.Magic.{World}`
- Use sub-namespaces for logical grouping within systems
- Maintain consistent naming patterns across worlds
- Place shared utilities in root `Cosmere.*` namespaces

### DON'T:
- Cross-reference between `Cosmere.Magic.Roshar` and `Cosmere.Magic.Scadrial` directly without mod checks
- Create world-specific code outside of `Cosmere.Magic.{World}`
- Mix concerns within a single namespace
- Create deeply nested namespaces (max 4 levels)

## Cross-System Integration Pattern

```csharp
// ✅ GOOD: Through Core systems
namespace Cosmere.Magic.Roshar.Surgebinding {
    public class SurgeAbility : Cosmere.Ability.AbstractAbility {
        // Uses core investiture system
        public override void ConsumeInvestiture(float amount) {
            base.ConsumeInvestiture(amount);
        }
    }
}

// ✅ ACCEPTABLE: Cross-world reference with mod checks
namespace Cosmere.Magic.Roshar.Surgebinding {
    public void InteractWithAllomancy() {
        // Always check if the other mod is loaded first
        if (ModsConfig.IsActive("aequa.cosmere.scadrial")) {
            var allomancyPower = GetAllomancyPower(); // Safe to reference
        }
    }
}

// ❌ BAD: Direct cross-world reference without mod checks
namespace Cosmere.Magic.Roshar.Surgebinding {
    public void UseWith(Cosmere.Magic.Scadrial.Allomancy.Power power) {
        // Will crash if Scadrial mod is not loaded
    }
}
```

## Mod Dependency Safety

When referencing cross-world systems, always use one of these patterns:

### Pattern 1: ModsConfig Check
```csharp
if (ModsConfig.IsActive("aequa.cosmere.scadrial")) {
    // Safe to use Scadrial classes here
}
```

### Pattern 2: Assembly Loading Check
```csharp
if (LoadedModManager.RunningMods.Any(m => m.PackageId == "aequa.cosmere.scadrial")) {
    // Safe to use Scadrial classes here
}
```

### Pattern 3: Try-Catch for Type Loading
```csharp
try {
    var scadrialType = Type.GetType("Cosmere.Magic.Scadrial.SomeClass");
    if (scadrialType != null) {
        // Safe to use reflection-based access
    }
} catch (Exception) {
    // Scadrial mod not available
}
```

## Benefits of This Structure

1. **Cleaner IntelliSense**: All core functionality under `Cosmere.*`
2. **Clear Boundaries**: Magic systems obviously separated
3. **Easy Navigation**: Logical grouping makes finding code simple
4. **Future-Proof**: New worlds follow same pattern
5. **Reduced Confusion**: No more Foundation vs Core vs Resources ambiguity