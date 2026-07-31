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

    /// <summary>
    ///     Removes the site from the world map if the quest ends before the player ever
    ///     reached it. TravelToSiteObjective spawns the site at offer time, before the player
    ///     accepts, so a declined, failed, or expired quest would otherwise leave it sitting on
    ///     the map forever. A site the player did reach (site.HasMap) is left alone - it
    ///     follows the normal MapParent.ShouldRemoveMapNow lifecycle instead.
    /// </summary>
    public override void Cleanup() {
        base.Cleanup();
        if (site != null && !site.Destroyed && !site.HasMap) site.Destroy();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref site, "site");
    }
}
