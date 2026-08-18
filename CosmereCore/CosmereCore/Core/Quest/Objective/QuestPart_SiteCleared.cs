using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Completes when no hostile threat remains on the site map. Only polls while the map
///     exists, so abandoning the site pauses the objective rather than completing it.
/// </summary>
public class QuestPart_SiteCleared : QuestPart_CosmereActivable {
    public Site? site;

    /// <summary>
    ///     countDormantPawnsAsHostile stays true so a sleeping garrison still blocks completion;
    ///     canBeFogged stays false so an undiscovered threat does not block forever.
    /// </summary>
    protected override bool IsSatisfied() {
        if (site == null || site.Destroyed) {
            Fail();
            return false;
        }

        if (!site.HasMap) return false;

        return !GenHostility.AnyHostileActiveThreatTo(site.Map, Faction.OfPlayer, countDormantPawnsAsHostile: true);
    }

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets {
        get {
            foreach (GlobalTargetInfo target in base.QuestLookTargets) {
                yield return target;
            }

            if (site != null) yield return site;
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref site, "site");
    }
}
