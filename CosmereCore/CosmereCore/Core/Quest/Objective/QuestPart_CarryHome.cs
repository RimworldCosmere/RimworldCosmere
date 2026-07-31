using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Completes when the required thing is present on any player home map. The First Bead's
///     final stage, where the reward has to survive the trip rather than teleport home.
/// </summary>
public class QuestPart_CarryHome : QuestPart_CosmereActivable {
    public int requiredCount = 1;
    public ThingDef? requiredThing;

    protected override bool IsSatisfied() {
        ThingDef? thing = requiredThing;
        if (thing == null) return false;

        List<Verse.Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            Verse.Map map = maps[i];
            if (!map.IsPlayerHome) continue;
            if (map.resourceCounter.GetCount(thing) >= requiredCount) return true;
        }

        return false;
    }

    public override string? ExtraInspectString(ISelectable target) {
        if (State != QuestPartState.Enabled || requiredThing == null) return null;
        return "CC_Quest_CarryHome_Remaining".Translate(
            requiredThing.label.Named("THING"),
            requiredCount.Named("COUNT")
        ).Resolve();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref requiredThing, "requiredThing");
        Scribe_Values.Look(ref requiredCount, "requiredCount", 1);
    }
}
