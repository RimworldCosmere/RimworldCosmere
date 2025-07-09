using System.Collections.Generic;
using CosmereFramework.Comp.Map;
using CosmereFramework.Extension;
using CosmereScadrial.Allomancy.Hediff;
using CosmereScadrial.Def;
using UnityEngine;
using Verse;
using static CosmereFramework.CosmereFramework;
using LineRenderer = CosmereFramework.Comp.Map.LineRenderer;

namespace CosmereScadrial.Allomancy.Comp.Hediff;

public abstract class LineDrawingAuraProperties : HediffCompProperties {
    public virtual float radius { get; set; } = 15;
    public virtual Color lineColor { get; set; }

    public virtual Material lineMaterial =>
        MaterialPool.MatFrom(
            GenDraw.OneSidedLineOpaqueTexPath,
            ShaderDatabase.TransparentPostLight,
            lineColor
        );
}

public abstract class LineDrawingAura : HediffComp {
    protected Material? cachedLineMaterial;
    protected new virtual LineDrawingAuraProperties props => (LineDrawingAuraProperties)base.props;
    protected new AllomanticHediff parent => (AllomanticHediff)base.parent;
    protected MetallicArtsMetalDef metal => parent.metal;
    protected float radius => props.radius * parent.Severity;
    protected bool atLeastPassive => parent.sourceAbilities.Any(a => a.atLeastPassive);

    protected abstract IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map);
    protected abstract LineToRender GetLineToRender(Verse.Thing thing);


    public override void CompPostPostRemoved() {
        base.CompPostPostRemoved();
        LineRenderer.TryRemove(this);
        CircleRenderer.TryRemove(this);
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (!parent.sourceAbilities.Any(x => x.atLeastPassive) || !Find.Selector.IsSelected(parent.pawn)) {
            LineRenderer.TryClear(this);
            return;
        }

        if (debugMode) {
            CircleRenderer.TryAdd(
                this,
                new CircleToRender(
                    parent.pawn,
                    radius,
                    cachedLineMaterial ??= props.lineMaterial,
                    1f
                )
            );
        }

        if (!base.parent.pawn.IsHashIntervalTick(20, delta)) {
            return;
        }

        LineRenderer.TryClear(this);
        foreach (IntVec3 cell in parent.pawn.GetCellsAround(radius)) {
            IEnumerable<Verse.Thing> thingsToDrawInCell = GetThingsToDrawInCell(cell, parent.pawn.Map);
            foreach (Verse.Thing thing in thingsToDrawInCell) {
                LineRenderer.TryAdd(this, GetLineToRender(thing));
            }
        }
    }
}