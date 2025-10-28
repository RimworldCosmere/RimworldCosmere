using Verse;

namespace Cosmere.Extension;

public static class DefExtension {
    public static bool IsOneOf(this Verse.Def def, params Verse.Def[] defs) {
        return defs.Any(def.Equals);
    }

    public static IEnumerable<string> ParseDefName(this Verse.Def def) {
        return def.defName.Split('_');
    }
}