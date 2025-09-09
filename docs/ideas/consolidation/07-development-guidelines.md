# Development Guidelines

## Code Organization Rules

### Namespace Conventions

#### DO ✅
```csharp
// Core functionality
namespace Cosmere.Investiture { }
namespace Cosmere.Entity { }
namespace Cosmere.UI { }

// World-specific magic
namespace Cosmere.Magic.Roshar.Surgebinding { }
namespace Cosmere.Magic.Scadrial.Allomancy { }

// Sub-systems with clear hierarchy
namespace Cosmere.Magic.Roshar.Spren.Controller { }
namespace Cosmere.Magic.Scadrial.Allomancy.Metal { }
```

#### DON'T ❌
```csharp
// Don't use old prefixes
namespace Cosmere.Foundation.UI { }  // Wrong!
namespace Cosmere.Core.Investiture { } // Wrong!

// Don't cross-contaminate worlds
namespace Cosmere.Magic.Roshar.Allomancy { } // Wrong world!

// Don't create overly deep nesting
namespace Cosmere.Magic.Roshar.Surgebinding.Abilities.Physical.Gravitation.Effects { } // Too deep!
```

### File Organization

#### Folder Structure Must Match Namespace
```
✅ CORRECT:
File: CosmereCore/Magic/Roshar/Spren/SprenController.cs
Namespace: Cosmere.Magic.Roshar.Spren

❌ WRONG:
File: CosmereCore/Spren/SprenController.cs
Namespace: Cosmere.Magic.Roshar.Spren
```

#### One Class Per File
```csharp
// ✅ SprenController.cs
namespace Cosmere.Magic.Roshar.Spren {
    public class SprenController { }
}

// ❌ Don't combine multiple classes
namespace Cosmere.Magic.Roshar.Spren {
    public class SprenController { }
    public class SprenManager { }  // Should be in SprenManager.cs
}

// Exception: CompProperties can share with their Comp
namespace Cosmere.Comp.Thing {
    public class InvestitureHolder : ThingComp { }
    public class InvestitureHolderProperties : CompProperties { } // OK
}
```

## Cross-System Integration

### Through Core Systems Only

#### ✅ GOOD - Through Investiture System
```csharp
namespace Cosmere.Magic.Roshar.Surgebinding {
    public class StormlightAbility : Cosmere.Ability.AbstractAbility {
        protected override void ConsumeInvestiture(float amount) {
            // Uses core investiture system
            base.ConsumeInvestiture(amount);
        }
    }
}

namespace Cosmere.Magic.Scadrial.Allomancy {
    public class AllomanticPower : Cosmere.Ability.AbstractAbility {
        protected override void ConsumeInvestiture(float amount) {
            // Also uses core investiture system
            base.ConsumeInvestiture(amount);
        }
    }
}
```

#### ❌ BAD - Direct Cross-World Reference
```csharp
namespace Cosmere.Magic.Roshar.Surgebinding {
    using Cosmere.Magic.Scadrial.Allomancy; // Never do this!
    
    public class Windrunner {
        public void CombineWith(AllomanticPower power) { // Wrong!
            // Don't directly reference other magic systems
        }
    }
}
```

### Worldhopper Pattern
```csharp
// ✅ CORRECT - Worldhoppers use core interfaces
namespace Cosmere.Entity {
    public interface IWorldhopper {
        List<IMagicSystem> AccessibleSystems { get; }
    }
}

namespace Cosmere.Magic.Roshar {
    public class RosharMagicSystem : IMagicSystem { }
}

namespace Cosmere.Magic.Scadrial {
    public class ScadrialMagicSystem : IMagicSystem { }
}
```

## Naming Conventions

### Classes and Methods
```csharp
// ✅ GOOD
public class SprenController { }           // PascalCase
public void CaptureLesserSpren() { }      // PascalCase
private void updateSprenPosition() { }     // camelCase for private

// ❌ BAD
public class spren_controller { }          // Wrong case
public class SprenControllerManager { }    // Redundant suffix
```

### Constants and Fields
```csharp
public class SprenSystem {
    // ✅ GOOD
    private const float SprenSpawnRate = 0.5f;    // PascalCase for const
    private readonly int maxSprenCount = 100;      // camelCase for fields
    private float currentSpawnTimer;               // camelCase
    
    // ❌ BAD
    private const float SPREN_SPAWN_RATE = 0.5f;  // No SCREAMING_CASE
    private float CurrentSpawnTimer;               // No PascalCase for fields
}
```

## Using Statements

### Order and Grouping
```csharp
// System namespaces first
using System;
using System.Collections.Generic;
using System.Linq;

// Unity namespaces
using UnityEngine;

// RimWorld namespaces
using RimWorld;
using Verse;

// Cosmere namespaces
using Cosmere.Investiture;
using Cosmere.Magic.Roshar.Surgebinding;

// Aliases if needed
using SurgeAbility = Cosmere.Magic.Roshar.Surgebinding.Ability;
```

### Global Usings (in .csproj)
```xml
<ItemGroup>
    <Using Include="System.Linq"/>
    <Using Include="System.Collections.Generic"/>
    <Using Include="Cosmere.Extension"/>
</ItemGroup>
```

## Documentation Standards

### XML Documentation
```csharp
/// <summary>
/// Controls the spawning and behavior of lesser spren
/// </summary>
public class SprenController {
    /// <summary>
    /// Spawns a new spren at the specified location
    /// </summary>
    /// <param name="position">World position for spawn</param>
    /// <param name="type">Type of spren to spawn</param>
    /// <returns>The spawned spren instance, or null if spawn failed</returns>
    public Spren SpawnSpren(Vector3 position, SprenType type) { }
}
```

### Inline Comments
```csharp
// ✅ GOOD - Explains WHY
// Delay spawn to avoid performance spike during map load
DelayedActionScheduler.Schedule(() => SpawnSpren(), 30000);

// ❌ BAD - Explains WHAT (obvious from code)
// Increment counter by 1
counter++;
```

## Performance Guidelines

### Avoid LINQ in Hot Paths
```csharp
// ❌ BAD - LINQ in frequently called method
public void UpdateEveryTick() {
    var activeSpren = allSpren.Where(s => s.IsActive).ToList();
}

// ✅ GOOD - Manual iteration
public void UpdateEveryTick() {
    List<Spren> activeSpren = new();
    foreach (Spren spren in allSpren) {
        if (spren.IsActive) activeSpren.Add(spren);
    }
}
```

### Cache Expensive Operations
```csharp
public class RadiantOrder {
    private List<Pawn>? radiants;
    private int lastCacheUpdate = -1;
    
    public List<Pawn> GetRadiants() {
        int currentTick = Find.TickManager.TicksGame;
        if (radiants == null || currentTick - lastCacheUpdate > 250) {
            radiants = CalculateRadiants();
            lastCacheUpdate = currentTick;
        }
        return radiants;
    }
}
```

## Git Workflow

### Branch Naming
- `feature/surgebinding-adhesion`
- `bugfix/spren-spawn-crash`
- `refactor/consolidate-namespaces`

### Commit Messages
```bash
# ✅ GOOD
feat: Add Adhesion surge ability
fix: Prevent spren spawn during loading
refactor: Consolidate Foundation namespace into Core

# ❌ BAD
Updated stuff
Fix
Changes
```

### Pull Request Template
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Refactoring
- [ ] Documentation

## Testing
- [ ] Compiled successfully
- [ ] Tested in-game
- [ ] No new warnings

## World Affected
- [ ] Core (shared systems)
- [ ] Roshar
- [ ] Scadrial
- [ ] All worlds
```

## Common Pitfalls to Avoid

1. **Don't use `var` for explicit types** (per CLAUDE.md)
2. **Don't add section separator comments** like `// =========`
3. **Always use fully qualified names** when there's ambiguity (e.g., `Verse.Thing`)
4. **Don't create files unless necessary** - prefer editing existing ones
5. **Never cross-reference between magic worlds directly**
6. **Don't use deeply nested namespaces** (max 4 levels)
7. **Avoid LINQ in performance-critical code**