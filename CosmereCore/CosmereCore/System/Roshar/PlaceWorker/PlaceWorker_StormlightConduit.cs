using System.Collections.Generic;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.PlaceWorker;

public class PlaceWorker_StormlightConduit : Verse.PlaceWorker {
    public override AcceptanceReport AllowsPlacing(
        BuildableDef checkingDef,
        IntVec3 loc,
        Rot4 rot,
        Map map,
        Verse.Thing? thingToIgnore = null,
        Verse.Thing? thing = null
    ) {
        List<Verse.Thing> things = loc.GetThingList(map);
        for (int i = 0; i < things.Count; i++) {
            Verse.Thing existing = things[i];
            if (existing == thingToIgnore) continue;
            if (existing.TryGetComp<StormlightConduit>() == null) continue;

            if (!GenConstruct.CanReplace(checkingDef, existing.def)) {
                return false;
            }
        }

        return true;
    }
}
