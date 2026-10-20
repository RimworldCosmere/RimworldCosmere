using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Requires the player to hold a site map for a duration, while reinforcements arrive on
///     a timer. Accrues time only while a colonist is actually present, so parking an empty
///     map does not count.
/// </summary>
public class QuestPart_TimedWork : QuestPart_CosmereActivable {
    private int nextReinforcementTick = -1;

    /// <summary>
    ///     Preferred raider. Null lets the storyteller pick - the fallback when the quest's
    ///     targetFaction is not present in this world.
    /// </summary>
    public Faction? reinforcementFaction;

    public IncidentDef? reinforcementIncident;
    public int reinforcementIntervalTicks = 15000;
    public int requiredTicksOnSite = 180000;
    public Site? site;
    public int ticksAccrued;

    protected override bool IsSatisfied() {
        Site? currentSite = site;
        if (currentSite == null || currentSite.Destroyed) {
            Fail();
            return false;
        }

        if (!currentSite.HasMap) return false;
        if (currentSite.Map.mapPawns.FreeColonistsSpawnedCount <= 0) return false;

        ticksAccrued += checkIntervalTicks;
        QueueReinforcements(currentSite);

        return ticksAccrued >= requiredTicksOnSite;
    }

    private void QueueReinforcements(Site currentSite) {
        IncidentDef? incident = reinforcementIncident;
        if (incident == null || reinforcementIntervalTicks <= 0) return;

        int now = Find.TickManager.TicksGame;
        if (nextReinforcementTick < 0) {
            nextReinforcementTick = now + reinforcementIntervalTicks;
            return;
        }

        if (now < nextReinforcementTick) return;
        nextReinforcementTick = now + reinforcementIntervalTicks;

        Find.Storyteller.incidentQueue.Add(
            incident,
            now,
            new IncidentParms { target = currentSite.Map, forced = true, faction = reinforcementFaction }
        );
    }

    public override string? ExtraInspectString(ISelectable target) {
        if (State != QuestPartState.Enabled) return null;
        int remaining = requiredTicksOnSite - ticksAccrued;
        if (remaining < 0) remaining = 0;
        return "CC_Quest_TimedWork_Remaining".Translate(remaining.ToStringTicksToPeriod().Named("TIME")).Resolve();
    }

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets {
        get {
            foreach (GlobalTargetInfo t in base.QuestLookTargets) {
                yield return t;
            }

            if (site != null) yield return site;
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref site, "site");
        Scribe_Defs.Look(ref reinforcementIncident, "reinforcementIncident");
        Scribe_Values.Look(ref requiredTicksOnSite, "requiredTicksOnSite", 180000);
        Scribe_Values.Look(ref reinforcementIntervalTicks, "reinforcementIntervalTicks", 15000);
        Scribe_Values.Look(ref ticksAccrued, "ticksAccrued");
        Scribe_Values.Look(ref nextReinforcementTick, "nextReinforcementTick", -1);
        Scribe_References.Look(ref reinforcementFaction, "reinforcementFaction");
    }
}
