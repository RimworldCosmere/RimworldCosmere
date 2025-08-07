using Verse;

namespace Cosmere.Foundation.Extension;

public static class DefExtension {
    public static bool IsOneOf(this Def def, params Def[] defs) {
        return defs.Any(def.Equals);
    }

    public static IEnumerable<string> ParseDefName(this Def def) {
        return def.defName.Split('_');
    }
}