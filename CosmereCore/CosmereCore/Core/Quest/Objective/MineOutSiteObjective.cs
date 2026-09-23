using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Completes when the site the previous stage created has been stripped of a mineable.
///     Reads the site off the QuestPart_ArrivedAtSite that TravelToSiteObjective added, so the
///     two must appear in that order in the def.
///     <para>
///         Only meaningful on a persistent site: a site whose map is regenerated on each visit
///         puts its ore back, so the objective could never be finished.
///     </para>
/// </summary>
public class MineOutSiteObjective : QuestObjective {
    public ThingDef? mineable;

    /// <summary>
    ///     How much of what generated has to come out before the dig counts as done. Short of
    ///     1.0 on purpose: the last few cells of a large field are usually one awkward vein
    ///     behind a wall, and hunting them down is not what the quest is about.
    /// </summary>
    public float fraction = 0.85f;

    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        QuestPart_ArrivedAtSite? arrival = quest.GetFirstPartOfType<QuestPart_ArrivedAtSite>();
        if (arrival?.site == null) {
            throw new QuestBuildFailure("MineOutSiteObjective found no preceding TravelToSiteObjective");
        }

        QuestPart_SiteMinedOut minedOut = new QuestPart_SiteMinedOut {
            quest = quest,
            site = arrival.site,
            mineable = mineable,
            fraction = fraction,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(minedOut);
    }

    public override string? ConfigError() {
        if (mineable == null) return "MineOutSiteObjective has no mineable.";
        if (fraction <= 0f || fraction > 1f) return "MineOutSiteObjective fraction must be above 0 and at most 1.";
        return null;
    }
}
