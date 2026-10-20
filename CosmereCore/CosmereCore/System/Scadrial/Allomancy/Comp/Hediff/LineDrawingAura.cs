using Cosmere.Core.Ability;
using Cosmere.Core.Comp.Map;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;
using static Cosmere.Core.Mod;
using LineRenderer = Cosmere.Core.Comp.Map.LineRenderer;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Hediff;

public abstract class LineDrawingAuraProperties : HediffCompProperties {
    public virtual float Radius { get; set; } = 15;

    public virtual Color LineColor { get; set; }

    public virtual Material LineMaterial =>
        MaterialPool.MatFrom(
            GenDraw.OneSidedLineOpaqueTexPath,
            ShaderDatabase.TransparentPostLight,
            LineColor
        );
}

public abstract class LineDrawingAura : HediffComp {
    protected Material? cachedLineMaterial;

    protected virtual LineDrawingAuraProperties Props => (LineDrawingAuraProperties)props;

    protected AllomanticHediff Parent => (AllomanticHediff)parent;

    protected MetallicArtsMetalDef Metal => Parent.Metal;

    protected float Radius => Props.Radius * Parent.Severity;

    protected bool AtLeastBurning {
        get {
            foreach (IAbility<Allomancer, IHediff<Allomancer>> sa in Parent.SourceAbilities) {
                if (sa is AllomancyAbility a && a.AtLeastBurning) return true;
            }

            return false;
        }
    }

    protected abstract IEnumerable<Verse.Thing> GetThingsToDrawInCell(IntVec3 cell, Map map);

    protected abstract LineToRender GetLineToRender(Verse.Thing thing);

    protected float FadeFor(float distance) {
        return LineFade.For(Radius, distance);
    }

    public override void CompPostPostRemoved() {
        base.CompPostPostRemoved();
        LineRenderer.Remove(this);
        CircleRenderer.Remove(this);
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (!AtLeastBurning) {
            LineRenderer.Clear(this);
            return;
        }

        if (!Mod.AlwaysShowAllomanticAuras && !Find.Selector.IsSelected(Parent.pawn)) {
            LineRenderer.Clear(this);
            return;
        }

        if (DebugMode) {
            CircleRenderer.Add(
                this,
                new CircleToRender(
                    Parent.pawn,
                    Radius,
                    cachedLineMaterial ??= Props.LineMaterial
                )
            );
        }

        if (!parent.pawn.IsHashIntervalTick(20, delta)) {
            return;
        }

        LineRenderer.Clear(this);

        // computed once for the whole sweep, not per cell - it's one burn, same answer every time
        bool cloudy = Coppercloud.AnyOn(Parent.pawn.Map);
        float senseStrength = cloudy ? Coppercloud.BurnStrengthOf(Parent.pawn, Metal) : 0f;

        foreach (IntVec3 cell in Parent.pawn.GetCellsAround(Radius)) {
            IEnumerable<Verse.Thing> thingsToDrawInCell = GetThingsToDrawInCell(cell, Parent.pawn.Map);
            foreach (Verse.Thing thing in thingsToDrawInCell) {
                if (cloudy && Coppercloud.Hides(thing, senseStrength)) continue;

                LineRenderer.Add(this, GetLineToRender(thing));
            }
        }
    }
}
