using System.Collections.Generic;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gene;

public class Allomancer : Metalborn {
    public const float MinMetalAmount = 0;
    public const float MaxMetalAmount = 1f;
    private const float SleepDecayAmountPerRareInterval = .0025f;

    private float currentReserve;

    public int requestedVialStock = 3;

    private List<(AllomanticAbilityDef def, float amount)> sources = [];
    private float? timeDilationFactor;

    public override float Max => MaxMetalAmount * Mathf.Log(skill.Level + 1, 2); // log base 2
    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);
    private int burnTickRate => Mathf.RoundToInt(GenTicks.TickRareInterval / timeDilationFactor!.Value);

    public override float Value {
        get => currentReserve;
        set => currentReserve = value;
    }

    public bool shouldConsumeVialNow => Value < (double)targetValue;

    public override void Reset() {
        targetValue = 0.05f;
    }

    private void UpdateTimeDilationFactor() {
        timeDilationFactor = pawn.GetStatValue(CosmereCore.StatDefOf.Cosmere_Time_Dilation_Factor);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);
        if (!timeDilationFactor.HasValue) UpdateTimeDilationFactor();

        if (pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) {
            RemoveFromReserve(SleepDecayAmountPerRareInterval);
        }

        if (pawn.IsHashIntervalTick(burnTickRate, delta)) {
            BurnTickInterval(delta);
        }

        if (shouldConsumeVialNow && pawn.HasVial(metal)) {
            pawn.TryConsumeVial(metal);
        }
    }

    protected void BurnTickInterval(int delta) { }

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleMistbornTrait(pawn);
        MetalbornUtility.HandleAllomancerTrait(pawn);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref requestedVialStock, "RequestedVialStock", 3);
        Scribe_Values.Look(ref currentReserve, "currentReserve");
    }

    public bool CanLowerReserve(float amount) {
        return Value >= amount;
    }

    public void RemoveFromReserve(float amount) {
        Value = Mathf.Max(MinMetalAmount, Value - amount);
    }

    public void AddToReserve(float amount) {
        Value = Mathf.Min(Max, Value + amount);
    }

    public void SetReserve(float amount) {
        Value = Mathf.Clamp(amount, MinMetalAmount, Max);
    }

    public float GetReservePercent() {
        return Value / Max;
    }

    public void WipeReserve() {
        SetReserve(0);
    }
}