using CosmereResources.Def;
using Verse;

namespace CosmereResources.Extension;

public static class ThingExtension {
    public static bool IsCutGemOfType(this Thing thing, GemDef gemDef) {
        return thing.def.Equals(ThingDefOf.CutGem) && thing.Stuff.Equals(gemDef.Item);
    }
}