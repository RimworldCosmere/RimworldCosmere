using Cosmere.Core;
using Cosmere.Core.Savant;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.Thing;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Gene;

public class Allomancer : Metalborn {
    public const float MaxMetalAmount = 1f;
    public const int MaxRequestedVialStock = 20;
    private const float SleepDecayAmountPerRareInterval = .0025f;

    private RecordDef? cachedMetalBurntRecord;
    private HediffDef? cachedPermanentHediffDef;
    private HediffDef? cachedSavantHediffDef;
    private int cachedSavantStage;
    private HediffDef? cachedWithdrawalHediffDef;
    private float currentReserve;
    public int RequestedVialStock = 3;
    private float savantDecayOffset;
    private float? timeDilationFactor;

    public bool ShouldConsumeVialNow {
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
            if (!timeDilationFactor.HasValue) return UpkeepTicks;
            return Mathf.Max(1, Mathf.RoundToInt(UpkeepTicks / timeDilationFactor.Value));
        }
    }

    public override float Max => Mathf.Max(1, MaxMetalAmount * Mathf.Log(skill.Level + 1, 2f));

    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);

    private RecordDef metalBurntRecord => cachedMetalBurntRecord ??= RecordDefOf.GetMetalBurnRecordForMetal(metal);

    private HediffDef? savantHediffDef =>
        cachedSavantHediffDef ??= ScadrialSavantUtility.GetAllomanticSavantHediffDef(metal);

    private HediffDef? withdrawalHediffDef =>
        cachedWithdrawalHediffDef ??= ScadrialSavantUtility.GetAllomanticWithdrawalHediffDef(metal);

    private HediffDef? permanentHediffDef =>
        cachedPermanentHediffDef ??= ScadrialSavantUtility.GetAllomanticPermanentHediffDef(metal);

    public override float Value {
        get => currentReserve;
        set => currentReserve = value;
    }

    public float GetMetalNeededForBreathEquivalentUnits(float requiredBreathEquivalentUnits) {
        return requiredBreathEquivalentUnits / ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalUnit;
    }

    public bool TryBurnMetalForInvestiture(float requiredBreathEquivalentUnits) {
        // CanLowerReserve takes breath equivalent units and converts them itself.
        // Handing it the already-converted metal made every burn demand more than
        // three times the reserve it actually spends.
        if (!CanLowerReserve(requiredBreathEquivalentUnits)) return false;

        float metalNeeded = GetMetalNeededForBreathEquivalentUnits(requiredBreathEquivalentUnits);
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
                .Learn(sources.Count * ScadrialMetallurgyConstants.AllomancyXPPerTick * GenTicks.TickLongInterval);
        }

        UpdateSavantProgression(delta);
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

    private void UpdateSavantProgression(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (!SavantUtility.CanBeSavant(metal)) return;

        int previousStage = cachedSavantStage;
        RecordDef recordDef = RecordDefOf.GetTimeSpentBurningForMetal(metal);
        float ticks = pawn.records.GetValue(recordDef) - savantDecayOffset;
        int newStage = SavantUtility.GetAllomanticStage(ticks);

        if (newStage == 1 && !Burning) {
            float decayAmount = ticks *
                                SavantUtility.Stage1DecayPerDayFraction /
                                GenDate.TicksPerDay *
                                GenTicks.TickLongInterval;
            savantDecayOffset += decayAmount;
            ticks -= decayAmount;
            newStage = SavantUtility.GetAllomanticStage(ticks);
        }

        cachedSavantStage = newStage;

        SavantUtility.UpdateSavantHediffState(pawn, previousStage, newStage, savantHediffDef, permanentHediffDef);

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
                existing.Severity -= SavantUtility.WithdrawalSeverityLossPerDay /
                                     GenDate.TicksPerDay *
                                     GenTicks.TickLongInterval;
                if (existing.Severity <= 0.01f) pawn.health.RemoveHediff(existing);
            }
        } else {
            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(withdrawalHediffDef);
            if (existing == null) {
                Hediff hediff = HediffMaker.MakeHediff(withdrawalHediffDef, pawn);
                hediff.Severity = 0.05f;
                pawn.health.AddHediff(hediff);
            } else {
                existing.Severity += SavantUtility.WithdrawalSeverityGainPerDay /
                                     GenDate.TicksPerDay *
                                     GenTicks.TickLongInterval;
            }
        }
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.SyncMistbornTrait(pawn);
        MetalbornUtility.SyncAllomancerTrait(pawn);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref RequestedVialStock, "RequestedVialStock", 3);
        Scribe_Values.Look(ref currentReserve, "currentReserve");
        Scribe_Values.Look(ref savantDecayOffset, "savantDecayOffset");
        Scribe_Collections.Look(ref sources, "sources", LookMode.Deep);
    }

    public AcceptanceReport CanBurn(float requiredBreathEquivalentUnits) {
        if (!CanLowerReserve(requiredBreathEquivalentUnits) && !pawn.HasVial(metal)) {
            return "CS_CannotBurn".Translate(pawn.Named("PAWN"), metal.Named("METAL"));
        }

        return AcceptanceReport.WasAccepted;
    }

    public override bool CanLowerReserve(float breathEquivalentUnits) {
        return Value >= GetMetalNeededForBreathEquivalentUnits(breathEquivalentUnits);
    }

    /// <summary>
    ///     What this metal is holding, and a way to let any one of them go.
    /// </summary>
    /// <remarks>
    ///     The roster was a save file and nothing else - the koloss could say who owned it, but the
    ///     Allomancer had no way to see what they were carrying or what it was costing them. Living
    ///     on the metal's own gene means zinc and brass each answer for their own, which is also
    ///     how the upkeep is billed.
    /// </remarks>
    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        Comp.Game.KolossRoster? roster = Comp.Game.KolossRoster.Current;
        List<Pawn> held = roster?.HeldOnMetal(pawn, metal) ?? [];
        if (held.Count == 0) yield break;

        yield return new Command_Action {
            defaultLabel = "CS_KolossRoster_Label".Translate(held.Count.Named("COUNT")),
            defaultDesc = "CS_KolossRoster_Desc".Translate(
                metal.LabelCap.Named("METAL"),
                held.Select(one => one.LabelShortCap).ToCommaList().Named("HELD")
            ),
            icon = def.Icon,
            action = () => Find.WindowStack.Add(new UI.Dialog_KolossRoster(pawn, metal)),
        };

        foreach (Verse.Gizmo order in UI.KolossOrders.For(pawn)) yield return order;
    }

    public static string ThresholdDisplayLabel(Allomancer gene) {
        if (gene.targetValue <= 0f) return "CS_NeverConsumeVial".Translate();
        return "CS_ConsumeVialBelow".Translate(gene.PostProcessValue(gene.targetValue).Named("PERCENT"));
    }
}
