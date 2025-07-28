using Cosmere.Core.Investiture;
using Cosmere.Core.Need;
using Cosmere.Resources;
using Cosmere.Scadrial.Def;
using Cosmere.Scadrial.Thing;
using Cosmere.Scadrial.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Framework.Logger;

namespace Cosmere.Scadrial.Gene;

public class Allomancer : Metalborn {
    public const float MaxMetalAmount = 1f;
    private const float SleepDecayAmountPerRareInterval = .0025f;

    private RecordDef? cachedMetalBurntRecord;
    private float currentReserve;
    public int requestedVialStock = 3;
    private float? timeDilationFactor;

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
    public float BurnRate => sources.Sum(s => s.Rate);
    private int burnTickRate => Mathf.RoundToInt(GenTicks.TickRareInterval / timeDilationFactor!.Value);
    public override float Max => Mathf.Max(1, MaxMetalAmount * Mathf.Log(skill.Level + 1, 2f));
    private SkillRecord skill => pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower);

    private RecordDef metalBurntRecord => cachedMetalBurntRecord ??= RecordDefOf.GetMetalBurnRecordForMetal(metal);

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
        Investiture need = pawn.needs.TryGetNeed<Investiture>();
        need.CurLevel += requiredBreathEquivalentUnits;
        RemoveFromReserve(metalNeeded);
        need.CurLevel -= requiredBreathEquivalentUnits;

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
    }

    public void BurnTickInterval() {
        if (BurnRate <= 0) return;

        pawn.TryConsumeVialIfNeeded(metal);

        if (TryBurnMetalForInvestiture(BurnRate)) return;

        RemoveAllSources();
        Logger.Info($"{pawn.NameFullColored} can't burn {metal} any more. Removing all burn sources for {metal}.");
    }

    private void RemoveAllSources() {
        foreach (DrainSource source in sources.ToList()) {
            pawn.GetAllomanticAbility((AllomanticAbilityDef)source.Def)?.UpdateStatus(Core.Ability.Active.Off);
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
        if (!CanLowerReserve(amountToBurn) && !pawn.HasVial(metal)) {
            return "CS_CannotBurn".Translate(pawn.Named("PAWN"), metal.Named("METAL"));
        }

        return AcceptanceReport.WasAccepted;
    }

    public override bool CanLowerReserve(float breathEquivalentUnits) {
        return Value >= GetMetalNeededForBreathEquivalentUnits(breathEquivalentUnits);
    }
}