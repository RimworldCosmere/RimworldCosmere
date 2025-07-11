using CosmereCore.Comp.Thing;
using Verse;

namespace CosmereCore.Util;

public static class ThingUtility {
    public static bool ShouldDrop(Thing thing) {
        if (thing is not ThingWithComps thingWithComps) return true;
        if (thingWithComps.holdingOwner.Owner.ParentHolder is Pawn { Dead: true }) return true;

        return !thingWithComps.HasComp<PreventDropOnDowned>() ||
               thingWithComps.GetComp<PreventDropOnDowned>().preventDrop;
    }
}