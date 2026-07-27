using Verse;

namespace Cosmere.Core.Framework;

public static class InvestitureHealExclusionRegistry {
    private static readonly List<string> excludedPrefixes = [];

    public static void Register(string prefix) {
        excludedPrefixes.Add(prefix);
    }

    public static bool IsExcluded(Verse.Hediff hediff) {
        string defName = hediff.def.defName;
        for (int i = 0; i < excludedPrefixes.Count; i++) {
            if (defName.StartsWith(excludedPrefixes[i])) return true;
        }

        return false;
    }
}
