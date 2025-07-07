using System.Collections.Generic;
using System.Linq;
using CosmereCore.Need;
using CosmereScadrial.Allomancy.Ability;
using CosmereScadrial.Def;
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

    private List<(AllomanticAbilityDef def, float amount)> sources = [];
    private float? timeDilationFactor;
    public List<(AllomanticAbilityDef def, float amount)> Sources => sources;
    public float BurnRate => sources.Sum(s => s.amount);
    public bool Burning => BurnRate > 0f;

    public override float Max => MaxMetalAmount * Mathf.Log(skill.Level + 1, 2); // log base 2
    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);
    private int burnTickRate => Mathf.RoundToInt(GenTicks.TickRareInterval / timeDilationFactor!.Value);

    public override float Value {
        get => currentReserve;
        set => currentReserve = value;
    }

    public bool shouldConsumeVialNow => Value < (double)targetValue;

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

        if (pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) {
            RemoveFromReserve(SleepDecayAmountPerRareInterval);
        }

        if (pawn.IsHashIntervalTick(burnTickRate, delta)) {
            BurnTickInterval();
        }

        if (shouldConsumeVialNow && pawn.HasVial(metal)) {
            pawn.TryConsumeVial(metal);
        }
    }

    public void BurnTickInterval() {
        float totalRate = sources.Sum(s => s.amount);
        if (totalRate <= 0) return;

        if (TryBurnMetalForInvestiture(totalRate)) return;

        RemoveAllSources();
        Logger.Info($"{pawn.NameFullColored} can't burn {metal} any more. Removing all burn sources for {metal}.");
    }

    private void RemoveAllSources() {
        foreach ((AllomanticAbilityDef def, float amount) source in sources) {
            pawn.GetAllomanticAbility(source.def).UpdateStatus(BurningStatus.Off);
        }

        sources.Clear();
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleMistbornTrait(pawn);
        MetalbornUtility.HandleAllomancerTrait(pawn);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref requestedVialStock, "RequestedVialStock", 3);
        Scribe_Values.Look(ref currentReserve, "currentReserve");
        Scribe_Collections.Look(ref sources, "sources", LookMode.Reference);
    }

    public AcceptanceReport CanBurn(float requiredBreathEquivalentUnits) {
        float amountToBurn = GetMetalNeededForBreathEquivalentUnits(requiredBreathEquivalentUnits);
        if (CanLowerReserve(amountToBurn) && !pawn.HasVial(metal)) {
            return "CS_CannotBurn".Translate(pawn.Named("PAWN"), metal.Named("METAL"));
        }

        return AcceptanceReport.WasAccepted;
    }

    public void UpdateBurnSource(float rate, AllomanticAbilityDef def) {
        if (rate <= 0) {
            sources.Add((def, rate));
        } else {
            sources.Remove((def, rate));
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
        return Value / Max;
    }

    public void WipeReserve() {
        SetReserve(0);
    }
}