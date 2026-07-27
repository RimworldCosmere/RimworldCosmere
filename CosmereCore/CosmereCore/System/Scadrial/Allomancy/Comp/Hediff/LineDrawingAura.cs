using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Map;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;
using static Cosmere.Core.Mod;
using LineRenderer = Cosmere.Core.Comp.Map.LineRenderer;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Hediff;

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

    protected bool atLeastBurning {
        get {
            foreach (IAbility<Allomancer, IHediff<Allomancer>> sa in parent.SourceAbilities) {
                if (sa is AllomancyAbility a && a.atLeastBurning) return true;
            }

            return false;
        }
    }

    protected abstract IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map);

    protected abstract LineToRender GetLineToRender(Verse.Thing thing);

    public override void CompPostPostRemoved() {
        base.CompPostPostRemoved();
        LineRenderer.Remove(this);
        CircleRenderer.Remove(this);
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (!atLeastBurning) {
            LineRenderer.Clear(this);
            return;
        }

        if (!Mod.alwaysShowAllomanticAuras && !Find.Selector.IsSelected(parent.pawn)) {
            LineRenderer.Clear(this);
            return;
        }

        if (debugMode) {
            CircleRenderer.Add(
                this,
                new CircleToRender(
                    parent.pawn,
                    radius,
                    cachedLineMaterial ??= props.lineMaterial
                )
            );
        }

        if (!base.parent.pawn.IsHashIntervalTick(20, delta)) {
            return;
        }

        LineRenderer.Clear(this);
        foreach (IntVec3 cell in parent.pawn.GetCellsAround(radius)) {
            IEnumerable<Verse.Thing> thingsToDrawInCell = GetThingsToDrawInCell(cell, parent.pawn.Map);
            foreach (Verse.Thing thing in thingsToDrawInCell) {
                LineRenderer.Add(this, GetLineToRender(thing));
            }
        }
    }
}
