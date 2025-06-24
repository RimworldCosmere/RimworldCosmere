using System.Linq;
using Verse;

namespace CosmereFramework.Extensions;

public static class DefExtension {
    public static bool IsOneOf(this Def def, params Def[] defs) {
        return defs.Any(def.Equals);
    }
}