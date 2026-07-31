using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Spawns a thing on the site map when its stage begins, so a later CarryHomeObjective has
///     something to carry. Must appear after a TravelToSiteObjective in the def.
/// </summary>
public class SpawnThingObjective : QuestObjective {
    public int count = 1;
    public ThingDef? thingDef;

    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        QuestPart_ArrivedAtSite? arrival = quest.GetFirstPartOfType<QuestPart_ArrivedAtSite>();
        if (arrival?.site == null) {
            throw new QuestBuildFailure("SpawnThingObjective found no preceding TravelToSiteObjective");
        }

        QuestPart_SpawnThing spawn = new QuestPart_SpawnThing {
            quest = quest,
            site = arrival.site,
            thingDef = thingDef,
            count = count,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(spawn);
    }

    public override string? ConfigError() {
        if (thingDef == null) return "SpawnThingObjective has no thingDef.";
        if (count < 1) return "SpawnThingObjective count must be at least 1.";
        return null;
    }
}
