using System.Linq;
using Verse;

namespace Cosmere.Framework.Extension;

public static class DefExtension {
    public static bool IsOneOf(this Def def, params Def[] defs) {
        return defs.Any(def.Equals);
    }
}