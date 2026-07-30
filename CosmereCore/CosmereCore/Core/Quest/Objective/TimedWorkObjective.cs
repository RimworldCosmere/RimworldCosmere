using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Hold a site for a duration while reinforcements arrive. The Pits of Hathsin harvest
///     stage. The Roshar arc reuses this for the Urithiru assault and the Stoneward trial.
/// </summary>
public class TimedWorkObjective : QuestObjective {
    public IncidentDef? reinforcementIncident;
    public int reinforcementIntervalHours = 4;
    public int requiredDaysOnSite = 3;

    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        QuestPart_ArrivedAtSite? arrival = quest.GetFirstPartOfType<QuestPart_ArrivedAtSite>();
        if (arrival?.site == null) {
            throw new QuestBuildFailure("TimedWorkObjective found no preceding TravelToSiteObjective");
        }

        QuestPart_TimedWork work = new QuestPart_TimedWork {
            quest = quest,
            site = arrival.site,
            requiredTicksOnSite = requiredDaysOnSite * GenDate.TicksPerDay,
            reinforcementIntervalTicks = reinforcementIntervalHours * GenDate.TicksPerHour,
            reinforcementIncident = reinforcementIncident,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(work);
    }

    public override string? ConfigError() {
        if (requiredDaysOnSite < 1) return "TimedWorkObjective requiredDaysOnSite must be at least 1.";
        if (reinforcementIntervalHours < 0) {
            return "TimedWorkObjective reinforcementIntervalHours cannot be negative.";
        }

        return null;
    }
}
