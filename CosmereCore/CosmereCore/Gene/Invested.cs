using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Investiture;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Gene;

public abstract class Invested : Gene_Resource {
    internal bool gizmoShrunk = true;
    protected List<DrainSource> sources = [];
    public List<DrainSource> Sources => sources;

    public virtual float minimumAmount => 0;
    public override float InitialResourceMax => 1f;
    public override float MinLevelForAlert => .15f;
    public override float MaxLevelOffset => .1f;

    public override float Max => throw new NotImplementedException();
    public override float Value => throw new NotImplementedException();

    public override float ValuePercent => Max > 0 ? Value / Max : 0;

    public override int ValueForDisplay => PostProcessValue(Value);
    public override int MaxForDisplay => PostProcessValue(Max);

    protected Need.Investiture investiture => pawn.needs.TryGetNeed<Need.Investiture>();
    protected InvestitureHolder investitureHolder => pawn.TryGetComp<InvestitureHolder>();

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
        investitureHolder.maxInvestitureSelf = float.PositiveInfinity;
    }

    public override void PostRemove() {
        base.PostRemove();
        PostAddOrRemove();
        investitureHolder.currentInvestitureSelf = 1;
        investitureHolder.maxInvestitureSelf = 1;
    }

    public virtual bool CanLowerReserve(float breathEquivalentUnits) {
        return Value >= breathEquivalentUnits;
    }

    public void UpdateDrainSource(DrainSource source) {
        if (source.Rate <= 0f || sources.Contains(source)) {
            sources.Remove(source);
        }

        if (source.Rate > 0) {
            sources.Add(source);
        }
    }

    public void RemoveFromReserve(float amount) {
        Value = Mathf.Max(minimumAmount, Value - amount);
    }

    public void AddToReserve(float amount) {
        Value = Mathf.Min(Max, Value + amount);
    }

    public void SetReserve(float amount) {
        Value = Mathf.Clamp(amount, minimumAmount, Max);
    }

    public float GetReservePercent() {
        return Mathf.Approximately(Max, 0) ? 0 : Value / Max;
    }

    public void WipeReserve() {
        SetReserve(0);
    }

    public void FillReserve() {
        SetReserve(Max);
    }
}