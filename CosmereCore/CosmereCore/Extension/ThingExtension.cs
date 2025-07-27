using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.Extension;

public static class ThingExtension {
    public static bool IsCapableOfHavingMetal(this Thing thing) {
        return MetalDetector.IsCapableOfHavingMetal(thing.def);
    }

    public static float GetMetalMass(this Thing thing) {
        return MetalDetector.GetMetal(thing);
    }

    public static InvestitureHolder GetInvestiture(this Thing thing) {
        return thing.TryGetComp<InvestitureHolder>();
    }
}