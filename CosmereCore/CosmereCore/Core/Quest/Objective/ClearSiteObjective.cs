using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Completes when the site the previous stage created has no hostile threat left. Reads
///     the site off the QuestPart_ArrivedAtSite that TravelToSiteObjective added, so the two
///     must appear in that order in the def.
/// </summary>
public class ClearSiteObjective : QuestObjective {
    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        QuestPart_ArrivedAtSite? arrival = quest.GetFirstPartOfType<QuestPart_ArrivedAtSite>();
        if (arrival?.site == null) {
            throw new QuestBuildFailure("ClearSiteObjective found no preceding TravelToSiteObjective");
        }

        QuestPart_SiteCleared cleared = new QuestPart_SiteCleared {
            quest = quest,
            site = arrival.site,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(cleared);
    }
}
