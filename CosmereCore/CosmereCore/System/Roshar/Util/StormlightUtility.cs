using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Util;

public static class StormlightUtility {
    public static float Remap(float value, float vMin, float vMax, float tMin, float tMax) {
        return (value - vMin) / (vMax - vMin) * (tMax - tMin) + tMin;
    }

    public static bool IsThingCutGemstone(Verse.Thing thing) {
        return thing.def.Equals(Core.ThingDefOf.CutGem);
    }

    public static bool IsHighstormImmune(Pawn pawn) {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return false;

        if (surgebinder.radiantOrderDef.defName == "Windrunner" && surgebinder.CurrentIdeal >= 1) return true;
        if (surgebinder.radiantOrderDef.defName == "Bondsmith" && surgebinder.godsprenName == "Stormfather")
            return true;

        return false;
    }
}
