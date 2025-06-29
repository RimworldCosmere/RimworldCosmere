using System;
using System.Collections.Generic;
using System.Linq;
using CosmereCore.Util;
using CosmereScadrial.Allomancy.Hediff;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Allomancy.Comp.Hediff;

public class BronzeAuraProperties : HediffCompProperties {
    public float radius = 15f;

    public BronzeAuraProperties() {
        compClass = typeof(BronzeAura);
    }

    public Color lineColor => MetallicArtsMetalDefOf.Bronze.color;

    public Material lineMaterial =>
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, lineColor);
}

public class BronzeAura : HediffComp {
    public Dictionary<Verse.Thing, float> thingsToDraw { get; } = new Dictionary<Verse.Thing, float>();

    public new BronzeAuraProperties props => (BronzeAuraProperties)base.props;

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
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(center,
                     (float)Math.Round(Math.Min(GenRadial.MaxRadialPatternRadius, radius)), false)) {
            foreach (Verse.Thing? thing in cell.GetThingList(map)
                         .Where(thing =>
                             InvestitureDetector.HasInvestiture(thing) && !thingsToDraw.ContainsKey(thing))) {
                thingsToDraw[thing] = InvestitureDetector.GetInvestiture(thing);
            }
        }
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