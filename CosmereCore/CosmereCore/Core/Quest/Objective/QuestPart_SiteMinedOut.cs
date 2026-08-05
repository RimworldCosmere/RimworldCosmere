using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Completes once nothing of the target mineable is left standing on the site's map. For a
///     dig whose goal is the ground itself rather than a quota carried home.
/// </summary>
public class QuestPart_SiteMinedOut : QuestPart_CosmereActivable {
    public ThingDef? mineable;
    public Site? site;

    protected override bool IsSatisfied() {
        ThingDef? target = mineable;
        Site? current = site;
        if (target == null || current == null || current.Destroyed) return false;

        // No map means the player has not arrived yet, or is away. Either way the ground has
        // not been emptied, and an absent map must never read as "nothing left".
        Verse.Map? map = current.Map;
        if (map == null) return false;

        return map.listerThings.ThingsOfDef(target).Count == 0;
    }

    public override string? ExtraInspectString(ISelectable target) {
        if (State != QuestPartState.Enabled || mineable == null) return null;

        Verse.Map? map = site == null || site.Destroyed ? null : site.Map;
        if (map == null) return null;

        int left = map.listerThings.ThingsOfDef(mineable).Count;
        return "CC_Quest_MinedOut_Remaining".Translate(left.Named("COUNT")).Resolve();
    }

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets {
        get {
            foreach (GlobalTargetInfo target in base.QuestLookTargets) {
                yield return target;
            }

            if (site != null && !site.Destroyed) yield return site;
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref site, "site");
        Scribe_Defs.Look(ref mineable, "mineable");
    }
}
