using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.Core.Extension;

public static class ThingExtension {
    public static bool IsCapableOfHavingMetal(this Thing thing) {
        return MetalDetector.IsCapableOfHavingMetal(thing.def);
    }

    public static float GetMetalMass(this Thing thing) {
        return MetalDetector.GetMetal(thing);
    }

    public static float GetInvestiture(this Thing thing) {
        return thing.GetStatValue(StatDefOf.Cosmere_Investiture);
    }
}