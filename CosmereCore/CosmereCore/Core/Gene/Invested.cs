using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Investiture;
using Cosmere.Core.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Gene;

public abstract class Invested : Gene_Resource {
    protected List<DrainSource> sources = [];

    public List<DrainSource> Sources => sources;

    /// <summary>
    ///     Gets the tick interval every invested system is charged upkeep on. One shared setting
    ///     rather than a constant per system.
    /// </summary>
    public static int UpkeepTicks =>
        UpkeepRate.TicksFor(Mod.GetModSettings<CoreModSettings>().upkeepCadence);

    /// <summary>
    ///     Gets how much reserve is lost per second right now. Upkeep and the holder's passive decay
    ///     run on different cadences, both rescaled to seconds, and this floors at zero like the holder.
    /// </summary>
    public float DrainPerSecond {
        get {
            float rate = 0f;
            for (int i = 0; i < sources.Count; i++) {
                rate += sources[i].Rate;
            }

            rate = UpkeepRate.PerSecond(rate, UpkeepTicks);

            if (Value > 1f) rate += InvestitureHolder.drainRate / (GenTicks.TickRareInterval / 60f);

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

    protected Need.Investiture Investiture => pawn.needs.TryGetNeed<Need.Investiture>();

    protected InvestitureHolder InvestitureHolder => pawn.TryGetComp<InvestitureHolder>();

    public override void Reset() {
        targetValue = 0.5f;
    }

    protected virtual void PostAddOrRemove() { }

    public override void PostAdd() {
        base.PostAdd();
        PostAddOrRemove();
        InvestitureHolder.maxInvestitureSelf = float.PositiveInfinity;
    }

    public override void PostRemove() {
        base.PostRemove();
        PostAddOrRemove();
        InvestitureHolder.CurrentInvestitureSelf = 1;
        InvestitureHolder.maxInvestitureSelf = 1;
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
