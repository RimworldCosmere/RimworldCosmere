using Cosmere.Core.Comp.Thing;
using Verse;

namespace Cosmere.Core.Util;

public static class ThingUtility {
    public static bool ShouldDrop(Verse.Thing thing) {
        if (thing is not ThingWithComps thingWithComps) return true;
        if (thingWithComps.holdingOwner.Owner.ParentHolder is Pawn { Dead: true }) return true;

        return !thingWithComps.HasComp<PreventDropOnDowned>() ||
               !thingWithComps.GetComp<PreventDropOnDowned>().preventDrop;
    }
}