using System;
using System.Collections.Generic;
using System.Linq;
using CosmereCore.Util;
using CosmereScadrial.Allomancy.Hediff;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Allomancy.Comp.Hediff;

public class PhysicalExternalAuraProperties : HediffCompProperties {
    public Color lineColor = new Color(0.3f, 0.6f, 1f, 1f);
    public float radius = 15f;

    public PhysicalExternalAuraProperties() {
        compClass = typeof(PhysicalExternalAura);
    }

    public Material lineMaterial =>
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, lineColor);
}

public class PhysicalExternalAura : HediffComp {
    private static readonly HashSet<Verse.Thing> MetalCache = new HashSet<Verse.Thing>();

    public Dictionary<Verse.Thing, float> thingsToDraw { get; } = new Dictionary<Verse.Thing, float>();

    public new PhysicalExternalAuraProperties props => (PhysicalExternalAuraProperties)base.props;

    private new AllomanticHediff parent => (AllomanticHediff)base.parent;

    private bool isAtLeastPassive => parent.Severity >= 0.5f;

    private void GetThingsToDraw() {
        Map? map = parent.pawn.Map;
        IntVec3 center = parent.pawn.Position;
        float radius = props.radius * parent.Severity;
        if (map == null || !center.IsValid || !Find.Selector.IsSelected(parent.pawn)) {
            return;
        }

        thingsToDraw.Clear();
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(
                     center,
                     Mathf.Round(Math.Min(GenRadial.MaxRadialPatternRadius, radius)),
                     false
                 )) {
            foreach (Verse.Thing? thing in cell.GetThingList(map)
                         .Where(thing =>
                             MetalDetector.IsCapableOfHavingMetal(thing.def) && !thingsToDraw.ContainsKey(thing)
                         )) {
                thingsToDraw[thing] = MetalDetector.GetMetal(thing);
            }
        }
    }

    public override void CompPostMake() {
        base.CompPostMake();
        GetThingsToDraw();
    }

    public override void CompPostTick(ref float severityAdjustment) {
        if (!isAtLeastPassive) {
            thingsToDraw.Clear();
            return;
        }

        if (!base.parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) return;

        GetThingsToDraw();
    }
}