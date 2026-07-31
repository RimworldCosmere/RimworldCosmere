using RimWorld;
using RimWorld.Planet;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Places thingDef on the site map when its stage begins, then completes. Fires once on
///     Enable rather than polling, so it extends QuestPartActivable directly rather than the
///     polling base - there is nothing to wait on, the site map is already guaranteed to exist
///     by the preceding QuestPart_ArrivedAtSite completing before this signal fires.
/// </summary>
public class QuestPart_SpawnThing : QuestPartActivable {
    public int count = 1;
    public Site? site;
    public ThingDef? thingDef;

    protected override void Enable(SignalArgs receivedArgs) {
        base.Enable(receivedArgs);

        ThingDef? def = thingDef;
        if (def == null) {
            Logger.Error("QuestPart_SpawnThing has no thingDef to spawn. Completing without spawning.");
            Complete();
            return;
        }

        Site? currentSite = site;
        if (currentSite == null || currentSite.Destroyed || !currentSite.HasMap) {
            Logger.Error(
                $"QuestPart_SpawnThing could not place {def.defName}: site has no map. Completing anyway so the quest does not stall permanently."
            );
            Complete();
            return;
        }

        Map map = currentSite.Map;
        Verse.Thing thing = ThingMaker.MakeThing(def);
        thing.stackCount = count;

        IntVec3 cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 10);
        if (!GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near)) {
            Logger.Error($"QuestPart_SpawnThing failed to place {def.defName} on site map. Completing anyway so the quest does not stall permanently.");
        }

        Complete();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref thingDef, "thingDef");
        Scribe_Values.Look(ref count, "count", 1);
        Scribe_References.Look(ref site, "site");
    }
}
