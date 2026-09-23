using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Places thingDef on the site map when its stage begins, then completes. Fires once on
///     Enable rather than polling, but still extends QuestPart_CosmereActivable rather than
///     QuestPartActivable directly, because a missing map, an unreachable cell, or a failed
///     placement must fail the quest through failSignal rather than complete into a
///     CarryHomeObjective stage that can never be satisfied.
/// </summary>
public class QuestPart_SpawnThing : QuestPart_CosmereActivable {
    public int count = 1;
    public Site? site;
    public ThingDef? thingDef;

    protected override void Enable(SignalArgs receivedArgs) {
        base.Enable(receivedArgs);

        ThingDef? def = thingDef;
        if (def == null) {
            Log.Error("QuestPart_SpawnThing has no thingDef to spawn. Failing the quest.");
            Fail();
            return;
        }

        Site? currentSite = site;
        if (currentSite == null || currentSite.Destroyed || !currentSite.HasMap) {
            Log.Error($"QuestPart_SpawnThing could not place {def.defName}: site has no map. Failing the quest.");
            Fail();
            return;
        }

        Map map = currentSite.Map;

        if (!CellFinder.TryRandomClosewalkCellNear(map.Center, map, 10, out IntVec3 cell)) {
            Log.Error($"QuestPart_SpawnThing found no walkable cell near the site map center to place {def.defName}. Failing the quest.");
            Fail();
            return;
        }

        Verse.Thing thing = ThingMaker.MakeThing(def);
        thing.stackCount = count;

        if (!GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near)) {
            Log.Error($"QuestPart_SpawnThing failed to place {def.defName} on site map. Failing the quest.");
            Fail();
            return;
        }

        Complete();
    }

    /// <summary>Never actually evaluated - Enable() completes or fails synchronously, so state is already Disabled before any QuestPartTick could poll. Unreachable by construction.</summary>
    protected override bool IsSatisfied() {
        return true;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref thingDef, "thingDef");
        Scribe_Values.Look(ref count, "count", 1);
        Scribe_References.Look(ref site, "site");
    }
}
