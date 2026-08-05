using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Completes once most of the target mineable is out of the site's map. For a dig whose goal
///     is the ground itself rather than a quota carried home.
///     <para>
///         The bar is a share of what actually generated, snapshotted the first time the player
///         is standing on the map, because how much ore a field produces varies with the terrain
///         it grew through and a fixed number would be unreachable on a bad roll.
///     </para>
/// </summary>
public class QuestPart_SiteMinedOut : QuestPart_CosmereActivable {
    public float fraction = 0.85f;
    public ThingDef? mineable;
    public Site? site;

    /// <summary>What was standing when the player first arrived. Negative until then.</summary>
    private int initialCount = -1;

    protected override bool IsSatisfied() {
        ThingDef? target = mineable;
        Site? current = site;
        if (target == null || current == null || current.Destroyed) return false;

        // No map means the player has not arrived yet, or is away. Either way the ground has
        // not been emptied, and an absent map must never read as "nothing left".
        Verse.Map? map = current.Map;
        if (map == null) return false;

        int left = map.listerThings.ThingsOfDef(target).Count;
        if (initialCount < 0) initialCount = left;

        return left <= Threshold();
    }

    /// <summary>How many cells may still be standing when the dig counts as finished.</summary>
    private int Threshold() {
        return initialCount <= 0 ? 0 : Mathf.FloorToInt(initialCount * (1f - fraction));
    }

    public override string? ExtraInspectString(ISelectable target) {
        if (State != QuestPartState.Enabled || mineable == null) return null;

        Verse.Map? map = site == null || site.Destroyed ? null : site.Map;
        if (map == null) return null;

        int left = map.listerThings.ThingsOfDef(mineable).Count;
        int threshold = Threshold();
        return "CC_Quest_MinedOut_Remaining".Translate(
            Mathf.Max(left - threshold, 0).Named("COUNT")
        ).Resolve();
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
        Scribe_Values.Look(ref fraction, "fraction", 0.85f);
        Scribe_Values.Look(ref initialCount, "initialCount", -1);
    }
}
