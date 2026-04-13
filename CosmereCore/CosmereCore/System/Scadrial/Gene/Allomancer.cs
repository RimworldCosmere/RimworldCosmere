using Cosmere.Core;
using Cosmere.Core.Investiture;
using Cosmere.Core.Savant;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Thing;
using Cosmere.System.Scadrial.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Gene;

public class Allomancer : Metalborn {
    public const float MaxMetalAmount = 1f;
    private const float SleepDecayAmountPerRareInterval = .0025f;

    private RecordDef? cachedMetalBurntRecord;
    private float currentReserve;
    public int requestedVialStock = 3;
    private float? timeDilationFactor;
    private int cachedSavantStage;
    private float savantDecayOffset;
    private HediffDef? cachedSavantHediffDef;
    private HediffDef? cachedWithdrawalHediffDef;
    private HediffDef? cachedPermanentHediffDef;

    public bool shouldConsumeVialNow {
        get {
            if (metal.IsOneOf(MetalDefOf.Duralumin, MetalDefOf.Nicrosil)) return false;
            if (pawn.IsAsleep()) return false;
            if (pawn.IsShieldedAgainstInvestiture()) return false;
            if (
                pawn.CurJobDef?.defName == RimWorld.JobDefOf.Ingest.defName &&
                pawn.CurJob.targetA.Thing is AllomanticVial
            ) {
                return false;
            }

            return Value < (double)targetValue;
        }
    }

    public bool Burning => BurnRate > 0f;

    public float BurnRate {
        get {
            float total = 0f;
            for (int i = 0; i < sources.Count; i++) {
                total += sources[i].Rate;
            }
            return total;
        }
    }

    private int burnTickRate {
        get {
            if (!timeDilationFactor.HasValue) return GenTicks.TickRareInterval;
            return Mathf.RoundToInt(GenTicks.TickRareInterval / timeDilationFactor.Value);
        }
    }
    public override float Max => Mathf.Max(1, MaxMetalAmount * Mathf.Log(skill.Level + 1, 2f));
    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);

    private RecordDef metalBurntRecord => cachedMetalBurntRecord ??= RecordDefOf.GetMetalBurnRecordForMetal(metal);

    private HediffDef? savantHediffDef =>
        cachedSavantHediffDef ??= SavantUtility.GetAllomanticSavantHediffDef(metal);

    private HediffDef? withdrawalHediffDef =>
        cachedWithdrawalHediffDef ??= SavantUtility.GetAllomanticWithdrawalHediffDef(metal);

    private HediffDef? permanentHediffDef =>
        cachedPermanentHediffDef ??= SavantUtility.GetAllomanticPermanentHediffDef(metal);

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

        pawn.records.AddTo(metalBurntRecord, metalNeeded);

        return true;
    }

    public override void Reset() {
        targetValue = 0.05f;
    }

    private void UpdateTimeDilationFactor() {
        timeDilationFactor = pawn.GetStatValue(Core.StatDefOf.Cosmere_Time_Dilation_Factor);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);
        if (!timeDilationFactor.HasValue) UpdateTimeDilationFactor();

        if (pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta) && pawn.IsAsleep()) {
            RemoveFromReserve(SleepDecayAmountPerRareInterval / Mathf.Max(0, Mathf.Log(skill.Level + 1, 2f)));
        }

        if (sources.Count > 0 && pawn.IsHashIntervalTick(burnTickRate, delta)) {
            BurnTickInterval();
        }

        if (sources.Count > 0 && pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) {
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower)
                .Learn(sources.Count * Constants.AllomancyXPPerTick * GenTicks.TickLongInterval);
        }

        CheckSavantProgression(delta);
    }

    public void BurnTickInterval() {
        if (BurnRate <= 0) return;

        pawn.TryConsumeVialIfNeeded(metal);

        if (TryBurnMetalForInvestiture(BurnRate)) return;

        RemoveAllSources();
        Logger.Info($"{pawn.NameFullColored} can't burn {metal} any more. Removing all burn sources for {metal}.");
    }

    private void RemoveAllSources() {
        for (int i = 0; i < sources.Count; i++) {
            pawn.GetAllomanticAbility((AllomanticAbilityDef)sources[i].Def)?.UpdateStatus(Core.Ability.Active.Off);
        }
        sources.Clear();
    }

    private void CheckSavantProgression(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (!SavantUtility.CanBeSavant(metal)) return;

        int previousStage = cachedSavantStage;
        RecordDef recordDef = RecordDefOf.GetTimeSpentBurningForMetal(metal);
        float ticks = pawn.records.GetValue(recordDef) - savantDecayOffset;
        int newStage = SavantUtility.GetAllomanticStage(ticks);

        if (newStage == 1 && !Burning) {
            float decayAmount = ticks * SavantUtility.Stage1DecayPerDayFraction / GenDate.TicksPerDay * GenTicks.TickLongInterval;
            savantDecayOffset += decayAmount;
            ticks -= decayAmount;
            newStage = SavantUtility.GetAllomanticStage(ticks);
        }

        cachedSavantStage = newStage;

        if (newStage > 0 && savantHediffDef != null) {
            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(savantHediffDef);
            if (existing == null) {
                Hediff hediff = HediffMaker.MakeHediff(savantHediffDef, pawn);
                hediff.Severity = SavantUtility.SeverityForStage(newStage);
                pawn.health.AddHediff(hediff);
            } else {
                existing.Severity = SavantUtility.SeverityForStage(newStage);
            }
        } else if (newStage == 0 && savantHediffDef != null) {
            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(savantHediffDef);
            if (existing != null) pawn.health.RemoveHediff(existing);
        }

        if (newStage >= 3 && previousStage < 3 && permanentHediffDef != null) {
            if (!pawn.health.hediffSet.HasHediff(permanentHediffDef)) {
                pawn.health.AddHediff(HediffMaker.MakeHediff(permanentHediffDef, pawn));
            }
        }

        if (newStage > previousStage && newStage > 0) {
            SavantUtility.SendSavantLetter(pawn, newStage, "Allomancy", metal.LabelCap);
        }

        UpdateWithdrawal();
    }

    private void UpdateWithdrawal() {
        if (cachedSavantStage < 2 || !SavantUtility.IsDependencyMetal(metal) || withdrawalHediffDef == null) {
            Hediff? existing = withdrawalHediffDef != null
                ? pawn.health.hediffSet.GetFirstHediffOfDef(withdrawalHediffDef)
                : null;
            if (existing != null) pawn.health.RemoveHediff(existing);
            return;
        }

        if (Burning) {
            Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(withdrawalHediffDef);
            if (existing != null) {
                existing.Severity -= SavantUtility.WithdrawalSeverityLossPerDay / GenDate.TicksPerDay * GenTicks.TickLongInterval;
                if (existing.Severity <= 0.01f) pawn.health.RemoveHediff(existing);
            }
        } else {
            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(withdrawalHediffDef);
            if (existing == null) {
                Hediff hediff = HediffMaker.MakeHediff(withdrawalHediffDef, pawn);
                hediff.Severity = 0.05f;
                pawn.health.AddHediff(hediff);
            } else {
                existing.Severity += SavantUtility.WithdrawalSeverityGainPerDay / GenDate.TicksPerDay * GenTicks.TickLongInterval;
            }
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
        Scribe_Values.Look(ref savantDecayOffset, "savantDecayOffset");
        Scribe_Collections.Look(ref sources, "sources", LookMode.Deep);
    }

    public AcceptanceReport CanBurn(float requiredBreathEquivalentUnits) {
        float amountToBurn = GetMetalNeededForBreathEquivalentUnits(requiredBreathEquivalentUnits);
        if (!CanLowerReserve(amountToBurn) && !pawn.HasVial(metal)) {
            return "CS_CannotBurn".Translate(pawn.Named("PAWN"), metal.Named("METAL"));
        }

        return AcceptanceReport.WasAccepted;
    }

    public override bool CanLowerReserve(float breathEquivalentUnits) {
        return Value >= GetMetalNeededForBreathEquivalentUnits(breathEquivalentUnits);
    }
}