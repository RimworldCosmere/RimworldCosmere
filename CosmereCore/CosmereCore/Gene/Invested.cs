using System;
using RimWorld;
using Verse;

namespace Cosmere.Core.Gene;

public abstract class Invested : Gene_Resource {
    internal bool gizmoShrunk = true;

    public override float InitialResourceMax => 1f;
    public override float MinLevelForAlert => .15f;
    public override float MaxLevelOffset => .1f;

    public override float Max => throw new NotImplementedException();
    public override float Value => throw new NotImplementedException();

    public override float ValuePercent => Max > 0 ? Value / Max : 0;

    public override int ValueForDisplay => PostProcessValue(Value);
    public override int MaxForDisplay => PostProcessValue(Max);

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref gizmoShrunk, "gizmoShrunk");
    }

    public override void Reset() {
        targetValue = 0.5f;
    }

    protected virtual void PostAddOrRemove() { }

    public override void PostAdd() {
        base.PostAdd();
        PostAddOrRemove();
    }

    public override void PostRemove() {
        base.PostRemove();
        PostAddOrRemove();
    }
}