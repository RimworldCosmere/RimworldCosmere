using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Cosmere.Lightweave.ModsConfig;

internal static class ModConflicts {
    public static int CountFor(ModMetaData mod) {
        if (mod == null || !mod.Active) {
            return 0;
        }
        int count = 0;
        if (mod.Dependencies != null) {
            foreach (ModRequirement req in mod.Dependencies) {
                if (!req.IsSatisfied) {
                    count++;
                }
            }
        }
        count += CountLoadOrderViolations(mod);
        return count;
    }

    public static bool HasConflict(ModMetaData mod) {
        return CountFor(mod) > 0;
    }

    private static int CountLoadOrderViolations(ModMetaData mod) {
        List<ModMetaData> active = Verse.ModsConfig.ActiveModsInLoadOrder.ToList();
        int myIdx = active.FindIndex(m => m.SamePackageId(mod.PackageId, false));
        if (myIdx < 0) {
            return 0;
        }
        int violations = 0;
        if (mod.LoadBefore != null) {
            foreach (string pid in mod.LoadBefore) {
                int otherIdx = active.FindIndex(m => m.SamePackageId(pid, false));
                if (otherIdx >= 0 && otherIdx < myIdx) {
                    violations++;
                }
            }
        }
        if (mod.ForceLoadBefore != null) {
            foreach (string pid in mod.ForceLoadBefore) {
                int otherIdx = active.FindIndex(m => m.SamePackageId(pid, false));
                if (otherIdx >= 0 && otherIdx < myIdx) {
                    violations++;
                }
            }
        }
        if (mod.LoadAfter != null) {
            foreach (string pid in mod.LoadAfter) {
                int otherIdx = active.FindIndex(m => m.SamePackageId(pid, false));
                if (otherIdx >= 0 && otherIdx > myIdx) {
                    violations++;
                }
            }
        }
        if (mod.ForceLoadAfter != null) {
            foreach (string pid in mod.ForceLoadAfter) {
                int otherIdx = active.FindIndex(m => m.SamePackageId(pid, false));
                if (otherIdx >= 0 && otherIdx > myIdx) {
                    violations++;
                }
            }
        }
        return violations;
    }
}
