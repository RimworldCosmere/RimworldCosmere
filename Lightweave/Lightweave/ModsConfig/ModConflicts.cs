using Verse;

namespace Cosmere.Lightweave.ModsConfig;

internal static class ModConflicts {
    public static int CountFor(ModMetaData mod) {
        if (mod?.Dependencies == null) {
            return 0;
        }
        int count = 0;
        foreach (ModRequirement req in mod.Dependencies) {
            if (!req.IsSatisfied) {
                count++;
            }
        }
        return count;
    }

    public static bool HasConflict(ModMetaData mod) {
        return CountFor(mod) > 0;
    }
}
