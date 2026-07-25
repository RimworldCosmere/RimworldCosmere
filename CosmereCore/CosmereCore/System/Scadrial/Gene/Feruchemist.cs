using System;
using Cosmere.Core.Savant;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Hediff;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

public class Feruchemist : Metalborn {
    public const float MaxSeverity = 20f;
    public const float MaxTransferPerSecond = 5f;

    /// TickStoreOrTap runs once per real second, so this is per second - the old
    /// name said rare tick and the readout converted as if it were, which is how
    /// the reading came out four times under what the metalmind actually moved.
    public static readonly float AmountPerSecond = MaxTransferPerSecond / MaxSeverity;
    private HediffDef? cachedCompoundHediffDef;

    private List<IMetalmindSource>? cachedMetalminds;
    private HediffDef? cachedPermanentHediffDef;
    private HediffDef? cachedSavantHediffDef;
    private int cachedSavantStage;
    private HediffDef? cachedStoreHediffDef;

    private HediffDef? cachedTapHediffDef;
    private int metalmindsLastCachedTick = -1;
    private float savantDecayOffset;

    public List<IMetalmindSource> metalminds {
        get {
            int now = Find.TickManager.TicksGame;
            if (cachedMetalminds != null && metalmindsLastCachedTick == now) return cachedMetalminds;

            cachedMetalminds = [];
            List<Verse.Thing> items = pawn.inventory.innerContainer.InnerListForReading;
            for (int i = 0; i < items.Count; i++) {
                Metalmind? comp = items[i].TryGetComp<Metalmind>();
                if (comp != null && comp.Metal == metal) cachedMetalminds.Add(comp);
            }

            ImplantedMetalminds? implantHediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_ImplantedMetalminds) as
                    ImplantedMetalminds;
            if (implantHediff != null) {
                for (int i = 0; i < implantHediff.metalminds.Count; i++) {
                    ImplantedMetalmindData data = implantHediff.metalminds[i];
                    if (data.Metal == metal) cachedMetalminds.Add(data);
                }
            }

            metalmindsLastCachedTick = now;
            return cachedMetalminds;
        }
    }

    private float actualMax {
        get {
            float total = 0f;
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                total += mms[i].MaxAmount;
            }

            return total;
        }
    }

    private float actualValue {
        get {
            float total = 0f;
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                total += mms[i].StoredAmount;
            }

            return total;
        }
    }

    public override float InitialResourceMax => 100f;
    public override float Max => 100f;

    public override float Value {
        get => actualMax <= 0f ? 0f : actualValue / actualMax * Max;
        set {
            if (actualMax <= 0f) return;
            float targetTotal = value / Max * actualMax;
            float delta = targetTotal - actualValue;
            List<IMetalmindSource> mms = metalminds;
            if (delta > 0f) {
                for (int i = 0; i < mms.Count && delta > 0f; i++) {
                    if (!mms[i].CanStore) continue;
                    float add = Mathf.Min(mms[i].MaxAmount - mms[i].StoredAmount, delta);
                    if (add > 0f) { mms[i].AddStored(add); delta -= add; }
                }
            }
            else if (delta < 0f) {
                delta = -delta;
                for (int i = 0; i < mms.Count && delta > 0f; i++) {
                    if (!mms[i].CanTap) continue;
                    float remove = Mathf.Min(mms[i].StoredAmount, delta);
                    if (remove > 0f) { mms[i].ConsumeStored(remove); delta -= remove; }
                }
            }
        }
    }

    private HediffDef? tapHediffDef =>
        cachedTapHediffDef ??= DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Tap" + metal.defName);

    private Hediff? tapHediff => pawn.health.hediffSet.GetFirstHediffOfDef(tapHediffDef);

    private HediffDef? storeHediffDef =>
        cachedStoreHediffDef ??=
            DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Store" + metal.defName);

    private Hediff? storeHediff => pawn.health.hediffSet.GetFirstHediffOfDef(storeHediffDef);

    private HediffDef? compoundHediffDef =>
        cachedCompoundHediffDef ??=
            DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Compound" + metal.defName);

    private Hediff? compoundHediff => pawn.health.hediffSet.GetFirstHediffOfDef(compoundHediffDef);

    private HediffDef? savantHediffDef =>
        cachedSavantHediffDef ??= ScadrialSavantUtility.GetFeruchemicalSavantHediffDef(metal);

    private HediffDef? permanentHediffDef =>
        cachedPermanentHediffDef ??= ScadrialSavantUtility.GetFeruchemicalPermanentHediffDef(metal);

    public bool canTap {
        get {
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].CanTap) return true;
            }

            return false;
        }
    }

    public bool isTapping => tapHediffDef != null && pawn.health.hediffSet.HasHediff(tapHediffDef);

    public bool canStore {
        get {
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].CanStore) return true;
            }

            return false;
        }
    }

    public bool isStoring => storeHediffDef != null && pawn.health.hediffSet.HasHediff(storeHediffDef);
    public bool isCompounding => compoundHediffDef != null && pawn.health.hediffSet.HasHediff(compoundHediffDef);

    /// Charge moved per real second at the current dial setting, negative while
    /// tapping. Zero when the dial sits in its dead band or the direction is shut.
    public float TransferRatePerSecond {
        get {
            float rate = dialRatePerSecond;
            // Compounding runs alongside the dial rather than replacing it, so the
            // readout has to carry both or it understates what is happening.
            if (compoundHediff is Scadrial.Feruchemy.Hediff.Compound compound) {
                rate += compound.StorePerSecond;
            }

            return rate;
        }
    }

    private float dialRatePerSecond {
        get {
            float severity = effectiveSeverity;
            if (severity <= 0f) return 0f;

            float perSecond = AmountPerSecond * severity;

            if (targetValue < 50f) return canTap ? -perSecond : 0f;
            return canStore ? perSecond : 0f;
        }
    }

    private float effectiveSeverity {
        get {
            float delta = targetValue - 50f;
            if (Mathf.Abs(delta) < 2f) return 0f;

            float exponent = 2.5f;
            float maxSeverity = MaxSeverity - 1f;

            float normalized = Mathf.Abs(delta) / 50f;
            float baseSeverity = 1f + Mathf.Pow(normalized, exponent) * maxSeverity;

            if (delta > 0f) {
                float penalty = SavantUtility.GetFeruchemyStorePenaltyMultiplier(cachedSavantStage);
                baseSeverity *= penalty;
            }

            return baseSeverity;
        }
    }


    private void TryRemoveHediffByDef(HediffDef? def) {
        if (def == null) return;
        Hediff? hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def);
        if (hediff != null) pawn.health.RemoveHediff(hediff);
    }

    public override void Reset() {
        targetValue = 50f;
        TryRemoveHediffByDef(storeHediffDef);
        TryRemoveHediffByDef(tapHediffDef);
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.SyncFullFeruchemistTrait(pawn);
        MetalbornUtility.SyncFeruchemistTrait(pawn);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;

        TickCopper();
        if (!canTap && isTapping && !isCompounding) Reset();
        if (!canStore && isStoring && !isCompounding) Reset();
        TickSeverityHediffs();
        TickStoreOrTap();
        TickXPGain(delta);
        UpdateFeruchemicalSavantProgression(delta);
    }

    private void TickCopper() {
        if (metal != MetallicArtsMetalDefOf.Copper) return;
        List<IMetalmindSource> mms = metalminds;
        for (int i = 0; i < mms.Count; i++) {
            if (mms[i] is Metalmind mm) mm.SyncInjectedThoughts(pawn);
        }
    }

    private void TickSeverityHediffs() {
        if (effectiveSeverity <= 0f) return;
        if (targetValue < 50 && canTap) {
            TryRemoveHediffByDef(storeHediffDef);
            pawn.health.GetOrAddHediff(tapHediffDef).Severity = effectiveSeverity;
        }
        else if (targetValue > 50 && canStore) {
            TryRemoveHediffByDef(tapHediffDef);
            pawn.health.GetOrAddHediff(storeHediffDef).Severity = effectiveSeverity;
        }
    }

    private void TickStoreOrTap() {
        if (isStoring) {
            Hediff? sh = storeHediff;
            if (sh != null) AddToStore(AmountPerSecond * sh.Severity);
        }
        else if (isTapping) {
            Hediff? th = tapHediff;
            if (th != null) RemoveFromStore(AmountPerSecond * th.Severity);
        }
    }

    private void TickXPGain(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (isCompounding) {
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower)
                .Learn(10 * ScadrialMetallurgyConstants.FeruchemyXPPerTick * GenTicks.TickLongInterval);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower)
                .Learn(10 * ScadrialMetallurgyConstants.FeruchemyXPPerTick * GenTicks.TickLongInterval);
        }
        else if (isTapping || isStoring) {
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower)
                .Learn(
                    Mathf.Lerp(1, 2, effectiveSeverity) * ScadrialMetallurgyConstants.FeruchemyXPPerTick * GenTicks.TickLongInterval
                );
        }
    }

    public bool AddToStore(float amount) {
        List<IMetalmindSource> mms = metalminds;
        for (int i = 0; i < mms.Count; i++) {
            if (!mms[i].CanStore) continue;
            mms[i].AddStored(amount);
            return true;
        }

        return false;
    }

    public bool RemoveFromStore(float amount) {
        List<IMetalmindSource> mms = metalminds;
        for (int i = 0; i < mms.Count; i++) {
            if (!mms[i].CanTap) continue;
            mms[i].ConsumeStored(amount);
            return true;
        }

        return false;
    }

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        return [];
    }

    private void UpdateFeruchemicalSavantProgression(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (!SavantUtility.CanBeSavant(metal)) return;

        int previousStage = cachedSavantStage;
        RecordDef storingRecord = RecordDefOf.GetTimeSpentStoringForMetal(metal);
        RecordDef tappingRecord = RecordDefOf.GetTimeSpentTappingForMetal(metal);
        float ticks = pawn.records.GetValue(storingRecord) + pawn.records.GetValue(tappingRecord) - savantDecayOffset;
        int newStage = SavantUtility.GetFeruchemicalStage(ticks);

        if (newStage == 1 && !isTapping && !isStoring) {
            float decayAmount = ticks *
                                SavantUtility.Stage1DecayPerDayFraction /
                                GenDate.TicksPerDay *
                                GenTicks.TickLongInterval;
            savantDecayOffset += decayAmount;
            ticks -= decayAmount;
            newStage = SavantUtility.GetFeruchemicalStage(ticks);
        }

        cachedSavantStage = newStage;

        SavantUtility.UpdateSavantHediffState(pawn, previousStage, newStage, savantHediffDef, permanentHediffDef);

        if (newStage > previousStage && newStage > 0) {
            SavantUtility.SendSavantLetter(pawn, newStage, "Feruchemy", metal.LabelCap);
        }
    }
}