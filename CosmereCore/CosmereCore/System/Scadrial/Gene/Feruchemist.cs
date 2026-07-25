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

/// Which stored pool the tap controls act on.
public enum FeruchemyChannel {
    Ordinary,
    Compounded,
}

public class Feruchemist : Metalborn {
    /// The dial's neutral point. Below it the pawn taps, above it they store.
    public const float IdleTarget = 50f;
    public const float MaxSeverity = 20f;
    public const float MaxTransferPerSecond = 10f;

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
    private HediffDef? cachedTapCompoundedHediffDef;
    private float savantDecayOffset;

    /// Which pool the tap controls draw from. Compounded charge pays out through
    /// its own hediff ladder rather than a bigger severity number.
    public FeruchemyChannel channel = FeruchemyChannel.Ordinary;

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

    private float actualCompounded {
        get {
            float total = 0f;
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                total += mms[i].CompoundedAmount;
            }

            return total;
        }
    }

    /// Room left across every metalmind this pawn can actually compound into.
    public float CompoundedFreeSpace {
        get {
            float total = 0f;
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].CanStoreCompounded) total += mms[i].FreeSpace;
            }

            return total;
        }
    }

    public override float InitialResourceMax => 100f;
    public override float Max => 100f;

    public override float Value {
        get => actualMax <= 0f ? 0f : (actualValue + actualCompounded) / actualMax * Max;
        set {
            if (actualMax <= 0f) return;
            float targetTotal = value / Max * actualMax;
            float delta = targetTotal - (actualValue + actualCompounded);
            List<IMetalmindSource> mms = metalminds;
            if (delta > 0f) {
                for (int i = 0; i < mms.Count && delta > 0f; i++) {
                    if (!mms[i].CanStore) continue;
                    float add = Mathf.Min(mms[i].FreeSpace, delta);
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

                // Setting the resource directly has to be able to reach zero, so it
                // falls through to compounded charge once ordinary runs out. The
                // gameplay tap path deliberately does not - that stays explicit.
                for (int i = 0; i < mms.Count && delta > 0f; i++) {
                    if (!mms[i].CanTapCompounded) continue;
                    float remove = Mathf.Min(mms[i].CompoundedAmount, delta);
                    if (remove > 0f) { mms[i].ConsumeCompounded(remove); delta -= remove; }
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

    private HediffDef? tapCompoundedHediffDef =>
        cachedTapCompoundedHediffDef ??=
            DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_TapCompounded" + metal.defName);

    private Verse.Hediff? tapCompoundedHediff =>
        tapCompoundedHediffDef == null ? null : pawn.health.hediffSet.GetFirstHediffOfDef(tapCompoundedHediffDef);

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

    public bool canTapCompounded {
        get {
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].CanTapCompounded) return true;
            }

            return false;
        }
    }

    public bool canStoreCompounded {
        get {
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].CanStoreCompounded) return true;
            }

            return false;
        }
    }

    public bool isTapping =>
        (tapHediffDef != null && pawn.health.hediffSet.HasHediff(tapHediffDef)) ||
        (tapCompoundedHediffDef != null && pawn.health.hediffSet.HasHediff(tapCompoundedHediffDef));

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

            if (targetValue < IdleTarget) return canTap ? -perSecond : 0f;
            return canStore ? perSecond : 0f;
        }
    }

    private float effectiveSeverity {
        get {
            float delta = targetValue - IdleTarget;
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
        channel = FeruchemyChannel.Ordinary;
        TryRemoveHediffByDef(tapCompoundedHediffDef);
        targetValue = IdleTarget;
        TryRemoveHediffByDef(storeHediffDef);
        TryRemoveHediffByDef(tapHediffDef);
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.SyncFullFeruchemistTrait(pawn);
        MetalbornUtility.SyncFeruchemistTrait(pawn);
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref targetValue, "targetValue", IdleTarget);
        Scribe_Values.Look(ref savantDecayOffset, "savantDecayOffset");
        Scribe_Values.Look(ref channel, "channel");
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;

        TickCopper();
        bool tapAvailable = channel == FeruchemyChannel.Compounded ? canTapCompounded : canTap;
        if (!tapAvailable && isTapping && !isCompounding) Reset();
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
        if (targetValue < IdleTarget && (channel == FeruchemyChannel.Compounded ? canTapCompounded : canTap)) {
            TryRemoveHediffByDef(storeHediffDef);

            bool compounded = channel == FeruchemyChannel.Compounded;
            HediffDef? active = compounded ? tapCompoundedHediffDef : tapHediffDef;
            TryRemoveHediffByDef(compounded ? tapHediffDef : tapCompoundedHediffDef);
            if (active != null) pawn.health.GetOrAddHediff(active).Severity = effectiveSeverity;
        }
        else if (targetValue > IdleTarget && canStore) {
            TryRemoveHediffByDef(tapHediffDef);
            pawn.health.GetOrAddHediff(storeHediffDef).Severity = effectiveSeverity;
        }
    }

    private void TickStoreOrTap() {
        // Reads the curve rather than the hediff's severity. Compounded tapping
        // amplifies through its own stage ladder, so if the drain read severity
        // back it would move ten times the charge as well.
        float severity = effectiveSeverity;
        if (severity <= 0f) return;

        float amount = AmountPerSecond * severity;
        if (isStoring) {
            AddToStore(amount);
        }
        else if (isTapping) {
            if (channel == FeruchemyChannel.Compounded) RemoveCompoundedFromStore(amount);
            else RemoveFromStore(amount);
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
        return Distribute(amount, static m => m.CanStore, static (m, a) => m.AddStored(a), static m => m.FreeSpace);
    }

    public bool AddCompoundedToStore(float amount) {
        return Distribute(
            amount,
            static m => m.CanStoreCompounded,
            static (m, a) => m.AddCompounded(a),
            static m => m.FreeSpace
        );
    }

    public bool RemoveCompoundedFromStore(float amount) {
        return Distribute(
            amount,
            static m => m.CanTapCompounded,
            static (m, a) => m.ConsumeCompounded(a),
            static m => m.CompoundedAmount
        );
    }

    /// Carries the remainder across sources. Dumping the full amount into the first
    /// eligible metalmind let its clamp discard the overflow silently.
    private bool Distribute(
        float amount,
        Func<IMetalmindSource, bool> eligible,
        Action<IMetalmindSource, float> apply,
        Func<IMetalmindSource, float> room
    ) {
        if (amount <= 0f) return false;

        bool moved = false;
        float remaining = amount;
        List<IMetalmindSource> mms = metalminds;
        for (int i = 0; i < mms.Count && remaining > 0f; i++) {
            if (!eligible(mms[i])) continue;

            float take = Mathf.Min(room(mms[i]), remaining);
            if (take <= 0f) continue;

            apply(mms[i], take);
            remaining -= take;
            moved = true;
        }

        return moved;
    }

    public bool RemoveFromStore(float amount) {
        return Distribute(amount, static m => m.CanTap, static (m, a) => m.ConsumeStored(a), static m => m.StoredAmount);
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