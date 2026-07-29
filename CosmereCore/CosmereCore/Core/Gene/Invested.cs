using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Investiture;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Gene;

public abstract class Invested : Gene_Resource {
    protected List<DrainSource> sources = [];

    public List<DrainSource> Sources => sources;

    // What the reserve is losing per second right now. Ability upkeep is already
    // charged once a second, so it reads straight; the holder's passive decay is
    // charged once per rare tick and has to be divided back down, or it reports as a
    // number four seconds wide. The holder stops decaying at the floor, and so does
    // this - a reserve sitting at empty is not still draining.
    public float DrainPerSecond {
        get {
            float rate = 0f;
            for (int i = 0; i < sources.Count; i++) {
                rate += sources[i].Rate;
            }

            if (Value > 1f) rate += investitureHolder.drainRate / (GenTicks.TickRareInterval / 60f);

            return rate;
        }
    }

    public virtual float MinimumAmount => 0;

    public virtual string InvestitureLabel => string.Empty;

    public virtual float MaxInvestitureLevel => -1f;

    public override float InitialResourceMax => 1f;

    public override float MinLevelForAlert => .15f;

    public override float MaxLevelOffset => .1f;

    public abstract override float Max { get; }

    public abstract override float Value { get; set; }

    public override float ValuePercent => Max > 0 ? Value / Max : 0;

    public override int ValueForDisplay => PostProcessValue(Value);

    public override int MaxForDisplay => PostProcessValue(Max);

    public virtual List<AbilityDef> Abilities => def.abilities;

    protected Need.Investiture investiture => pawn.needs.TryGetNeed<Need.Investiture>();

    protected InvestitureHolder investitureHolder => pawn.TryGetComp<InvestitureHolder>();

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
        sources.Remove(source);
        if (source.Rate > 0) {
            sources.Add(source);
        }
    }

    public void RemoveFromReserve(float amount) {
        Value = Mathf.Max(MinimumAmount, Value - amount);
    }

    public void AddToReserve(float amount) {
        Value = Mathf.Min(Max, Value + amount);
    }

    public void SetReserve(float amount) {
        Value = Mathf.Clamp(amount, MinimumAmount, Max);
    }

    public void WipeReserve() {
        SetReserve(0);
    }

    public void FillReserve() {
        SetReserve(Max);
    }
}
