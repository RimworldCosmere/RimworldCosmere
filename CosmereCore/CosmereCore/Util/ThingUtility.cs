using CosmereCore.Comp.Thing;
using Verse;

namespace CosmereCore.Util;

public static class ThingUtility {
    public static bool ShouldDrop(ThingWithComps thing) {
        if (thing.holdingOwner.Owner.ParentHolder is Pawn pawn) {
            if (pawn.Dead) return true;
        }

        return !thing.HasComp<PreventDropOnDowned>() || thing.GetComp<PreventDropOnDowned>().PreventDrop;
    }
}