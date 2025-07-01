using CosmereScadrial.Comp.Thing;
using Verse;

namespace CosmereScadrial.Util;

public static class ThingUtility {
    public static bool ShouldDrop(ThingWithComps thing) {
        return !thing.HasComp<PreventDropOnDowned>();
    }
}