using System.Collections.Generic;
using System.Linq;
using CosmereCore.Need;
using CosmereFramework.Extension;
using CosmereScadrial.Allomancy;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Extension;
using CosmereScadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = CosmereFramework.Logger;

namespace CosmereScadrial.Gene;

public class Allomancer : Metalborn {
    public const float MinMetalAmount = 0;
    public const float MaxMetalAmount = 1f;
    private const float SleepDecayAmountPerRareInterval = .0025f;
    private float currentReserve;
    public int requestedVialStock = 3;
    private List<AllomanticBurnSource> sources = [];
    private float? timeDilationFactor;
    public bool shouldConsumeVialNow => Value < (double)targetValue;
    public bool Burning => BurnRate > 0f;
    public float BurnRate => sources.Sum(s => s.Rate);
    private int burnTickRate => Mathf.RoundToInt(GenTicks.TickRareInterval / timeDilationFactor!.Value);
    public override float Max => MaxMetalAmount * Mathf.Log(skill.Level + 1, 2); // log base 2
    public List<AllomanticBurnSource> Sources => sources;
    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);

    public override float Value {
        get => currentReserve;
        set => currentReserve = value;
    }

    public float GetMetalNeededForBreathEquivalentUnits(float requiredBreathEquivalentUnits) {
        return requiredBreathEquivalentUnits / Constants.BreathEquivalentUnitsPerMetalUnit;
    }

    public bool TryBurnMetalForInvestiture(float requiredBreathEquivalentUnits) {
        float metalNeeded = GetMetalNeededForBreathEquivalentUnits(requiredBreathEquivalentUnits);
        if (!CanLowerReserve(metalNeeded)) return false;
        RemoveFromReserve(metalNeeded);

        Investiture need = pawn.needs.TryGetNeed<Investiture>();
        need.CurLevel = Mathf.Min(need.CurLevel + requiredBreathEquivalentUnits, need.MaxLevel);

        return true;
    }

    public override void Reset() {
        targetValue = 0.05f;
    }

    private void UpdateTimeDilationFactor() {
        timeDilationFactor = pawn.GetStatValue(CosmereCore.StatDefOf.Cosmere_Time_Dilation_Factor);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);
        if (!timeDilationFactor.HasValue) UpdateTimeDilationFactor();

        if (pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta) && pawn.IsAsleep()) {
            RemoveFromReserve(SleepDecayAmountPerRareInterval);
        }

        if (sources.Count > 0 && pawn.IsHashIntervalTick(burnTickRate, delta)) {
            BurnTickInterval();
        }

        if (shouldConsumeVialNow && pawn.HasVial(metal)) {
            pawn.TryConsumeVial(metal);
        }
    }

    private void BurnTickInterval() {
        if (BurnRate <= 0) return;

        if (TryBurnMetalForInvestiture(BurnRate)) return;

        RemoveAllSources();
        Logger.Info($"{pawn.NameFullColored} can't burn {metal} any more. Removing all burn sources for {metal}.");
    }

    private void RemoveAllSources() {
        foreach (AllomanticBurnSource source in sources.ToList()) {
            pawn.GetAllomanticAbility(source.Def).UpdateStatus(BurningStatus.Off);
            sources.Remove(source);
        }
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleMistbornTrait(pawn);
        MetalbornUtility.HandleAllomancerTrait(pawn);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref requestedVialStock, "RequestedVialStock", 3);
        Scribe_Values.Look(ref currentReserve, "currentReserve");
        Scribe_Collections.Look(ref sources, "sources", LookMode.Deep);
    }

    public AcceptanceReport CanBurn(float requiredBreathEquivalentUnits) {
        float amountToBurn = GetMetalNeededForBreathEquivalentUnits(requiredBreathEquivalentUnits);
        if (CanLowerReserve(amountToBurn) && !pawn.HasVial(metal)) {
            return "CS_CannotBurn".Translate(pawn.Named("PAWN"), metal.Named("METAL"));
        }

        return AcceptanceReport.WasAccepted;
    }

    public void UpdateBurnSource(AllomanticBurnSource source) {
        if (source.Rate > 0f) {
            if (!sources.Contains(source)) sources.Add(source);
        } else {
            sources.Remove(source);
        }
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
        return Max == 0 ? 0 : Value / Max;
    }

    public void WipeReserve() {
        SetReserve(0);
    }

    public void FillReserve() {
        SetReserve(Max);
    }
}