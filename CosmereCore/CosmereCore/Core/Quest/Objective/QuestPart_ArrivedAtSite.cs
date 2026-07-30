using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Completes once the player has generated a map at the target site, which is what
///     arriving with a caravan does. Fails if the site is gone, because another mod or a
///     world event removed it.
/// </summary>
public class QuestPart_ArrivedAtSite : QuestPart_CosmereActivable {
    public Site? site;

    protected override bool IsSatisfied() {
        if (site == null || site.Destroyed) {
            Fail();
            return false;
        }

        return site.HasMap;
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
