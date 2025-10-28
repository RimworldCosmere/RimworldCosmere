using Cosmere.Def;
using Verse;

namespace Cosmere.Resources.Extension;

public static class ThingExtension {
    public static bool IsCutGemOfType(this Verse.Thing thing, GemDef gemDef) {
        return thing.def.Equals(ThingDefOf.CutGem) && thing.Stuff.Equals(gemDef.Item);
    }
}