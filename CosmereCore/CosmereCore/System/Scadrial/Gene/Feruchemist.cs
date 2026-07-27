using System;
using Cosmere.Core.Savant;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Hediff;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

// Which stored pool the tap controls act on.
public enum FeruchemyChannel {
    Ordinary,
    Compounded,
}

public class Feruchemist : Metalborn {
    // The dial's neutral point. Below it the pawn taps, above it they store.
    public const float IdleTarget = 50f;
    public const float MaxSeverity = 20f;
    public const float MaxTransferPerSecond = 10f;

    // TickStoreOrTap runs once per real second, so this is per second - the old
    // name said rare tick and the readout converted as if it were, which is how
    // the reading came out four times under what the metalmind actually moved.
    public static readonly float AmountPerSecond = MaxTransferPerSecond / MaxSeverity;

    private List<IMetalmindSource>? cachedMetalminds;
    private HediffDef? cachedPermanentHediffDef;
    private HediffDef? cachedSavantHediffDef;
    private int cachedSavantStage;
    private HediffDef? cachedStoreHediffDef;

    private HediffDef? cachedTapHediffDef;
    private int metalmindsLastCachedTick = -1;
    private HediffDef? cachedTapCompoundedHediffDef;
    private float savantDecayOffset;

    // The compounded pool has its own dial: below idle it taps, above idle it
    // compounds, burning allomantic reserve to fill itself.
    public float compoundedTargetValue = IdleTarget;

    // Every metalmind, which is what a pawn carrying a dozen wants by default.
    public const string TargetAll = "";

    // Worn and carried metalminds only. Safe to draw on, since none of them can
    // be compounded and so none of them can be burned away.
    public const string TargetExternal = "group:external";

    // Implanted metalminds only, which is everything compounding can reach.
    public const string TargetInternal = "group:internal";

    // Which metalmind the dials act on: one of the group tokens above, or a
    // single metalmind's SourceId.
    public string targetMetalmindId = TargetAll;

    // Whether the dial is pointed at the compounded pool. Held here rather than in
    // the panel so gameplay can switch it off - a metal that stops paying out has
    // to be able to end the mode, not just park the dial under it.
    public bool compounding;

    // How hard the dial is calling for compounding, nought to one.
    public float CompoundFraction =>
        Mathf.Clamp01((compoundedTargetValue - IdleTarget) / (100f - IdleTarget));

    public bool canTapAny => canTap || canTapCompounded;

    public List<IMetalmindSource> metalminds {
        get {
            int now = Find.TickManager.TicksGame;
            if (cachedMetalminds != null && metalmindsLastCachedTick == now) return cachedMetalminds;

            cachedMetalminds = [];
            List<Verse.Thing> items = pawn.inventory.innerContainer.InnerListForReading;
            for (int i = 0; i < items.Count; i++) {
                Metalmind? comp = items[i].TryGetComp<Metalmind>();
                if (comp != null && SameMetal(comp.Metal)) cachedMetalminds.Add(comp);
            }

            ImplantedMetalminds? implantHediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_ImplantedMetalminds) as
                    ImplantedMetalminds;
            if (implantHediff != null) {
                for (int i = 0; i < implantHediff.metalminds.Count; i++) {
                    ImplantedMetalmindData data = implantHediff.metalminds[i];
                    if (SameMetal(data.Metal)) cachedMetalminds.Add(data);
                }
            }

            metalmindsLastCachedTick = now;
            return cachedMetalminds;
        }
    }

    // The single metalmind the dials act on, or none while pointed at a group.
    public IMetalmindSource? SelectedSource {
        get {
            if (IsGroupTarget(targetMetalmindId)) return null;

            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].SourceId == targetMetalmindId) return mms[i];
            }

            return null;
        }
    }

    // Whether everything the dials can currently reach is implanted, which is the
    // condition for compounding. True for a single implant and for the internal
    // group alike.
    public bool TargetIsInternalOnly {
        get {
            if (SelectedSource is { IsImplanted: true }) return true;
            if (targetMetalmindId != TargetInternal) return false;

            // Burning consumes implants, so the group can empty out entirely. An
            // empty group is not something to compound into, and saying otherwise
            // leaves the panel stuck in a mode nothing can act on.
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].IsImplanted) return true;
            }

            return false;
        }
    }

    public static bool IsGroupTarget(string target) {
        return target is TargetAll or TargetExternal or TargetInternal;
    }

    // A target that no longer resolves falls through to every metalmind rather
    // than quietly moving nothing.
    private static bool MatchesTarget(IMetalmindSource source, string target) {
        return target switch {
            TargetAll => true,
            TargetInternal => source.IsImplanted,
            TargetExternal => !source.IsImplanted,
            _ => source.SourceId == target,
        };
    }

    // A metal is declared twice - once as MetalDef and again as the richer
    // MetallicArtsMetalDef - so the two live in separate databases and are never
    // the same object. Metalminds resolve the plain one while the gene holds the
    // arts one, which made every implant invisible to its own gene.
    private bool SameMetal(Core.Def.MetalDef? candidate) {
        return candidate != null && candidate.defName == metal.defName;
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

    public float CompoundedAmount => actualCompounded;

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

    // Room in the one metalmind compounding is filling right now. Deposits go in
    // order, so this is the first that will accept charge.
    public float CurrentCompoundedFreeSpace {
        get {
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].CanStoreCompounded) return mms[i].FreeSpace;
            }

            return 0f;
        }
    }

    // Room left across every metalmind this pawn can actually compound into.
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
                    if (add > 0f) {
                        mms[i].AddStored(add);
                        delta -= add;
                    }
                }
            } else if (delta < 0f) {
                delta = -delta;
                for (int i = 0; i < mms.Count && delta > 0f; i++) {
                    if (!mms[i].CanTap) continue;
                    float remove = Mathf.Min(mms[i].StoredAmount, delta);
                    if (remove > 0f) {
                        mms[i].ConsumeStored(remove);
                        delta -= remove;
                    }
                }

                // Setting the resource directly has to be able to reach zero, so it
                // falls through to compounded charge once ordinary runs out. The
                // gameplay tap path deliberately does not - that stays explicit.
                bool drewCompounded = false;
                for (int i = 0; i < mms.Count && delta > 0f; i++) {
                    if (!mms[i].CanTapCompounded) continue;
                    float remove = Mathf.Min(mms[i].CompoundedAmount, delta);
                    if (remove > 0f) {
                        mms[i].ConsumeCompounded(remove);
                        delta -= remove;
                        drewCompounded = true;
                    }
                }

                if (drewCompounded) SweepBurnedOut();
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
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MatchesTarget(mms[i], target)) continue;
                if (mms[i].CanTap) return true;
            }

            return false;
        }
    }

    public bool canTapCompounded {
        get {
            List<IMetalmindSource> mms = metalminds;
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MatchesTarget(mms[i], target)) continue;
                if (mms[i].CanTapCompounded) return true;
            }

            return false;
        }
    }

    public bool canStoreCompounded {
        get {
            List<IMetalmindSource> mms = metalminds;
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MatchesTarget(mms[i], target)) continue;
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
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MatchesTarget(mms[i], target)) continue;
                if (mms[i].CanStore) return true;
            }

            return false;
        }
    }

    public bool isStoring => storeHediffDef != null && pawn.health.hediffSet.HasHediff(storeHediffDef);

    // The compounded dial is off its rest point in either direction. Filling that
    // pool is ordinary feruchemical storing; the amplification lives in the burn.
    public bool isCompounding => compoundedTargetValue != IdleTarget;

    // Compounding pours charge into a metalmind exactly as storing does, so it
    // counts as storing - mirroring isTapping, which already covers both the
    // ordinary and the compounded channel.
    public bool isStoringAny => isStoring || (compoundedTargetValue > IdleTarget && canStoreCompounded);

    // Charge moved per real second at the current dial setting, negative while
    // tapping. Zero when the dial sits in its dead band or the direction is shut.
    // What the compounded dial is moving, negative while tapping it and positive
    // while compounding into it.
    public float CompoundedRatePerSecond {
        get {
            float severity = SeverityForTarget(compoundedTargetValue);
            if (severity <= 0f) return 0f;

            if (compoundedTargetValue > IdleTarget) {
                return canStoreCompounded ? AmountPerSecond * severity : 0f;
            }

            return canTapCompounded ? -AmountPerSecond * severity : 0f;
        }
    }

    // Both dials move charge at once, so the readout carries the pair or it
    // understates what is happening.
    public float TransferRatePerSecond => dialRatePerSecond + CompoundedRatePerSecond;

    private float dialRatePerSecond {
        get {
            float severity = effectiveSeverity;
            if (severity <= 0f) return 0f;

            float perSecond = AmountPerSecond * severity;

            if (targetValue < IdleTarget) return canTap ? -perSecond : 0f;
            return canStore ? perSecond : 0f;
        }
    }

    private const float DeadBand = 2f;
    private const float CurveExponent = 2.5f;

    // The dial reads in whole units per second, so it steps in twentieths of a
    // severity point rather than anywhere along the curve.
    public const float RateQuantum = 0.05f;

    private static float SeverityQuantum => RateQuantum / AmountPerSecond;

    private float effectiveSeverity => SeverityForTarget(targetValue);

    private float SeverityForTarget(float target) {
        float delta = target - IdleTarget;
        if (Mathf.Abs(delta) < DeadBand) return 0f;

        float normalized = Mathf.Abs(delta) / 50f;
        float baseSeverity = 1f + Mathf.Pow(normalized, CurveExponent) * (MaxSeverity - 1f);

        if (delta > 0f) {
            baseSeverity *= SavantUtility.GetFeruchemyStorePenaltyMultiplier(cachedSavantStage);
        }

        return baseSeverity;
    }

    // Inverse of the curve above. Lives here because the storing side carries a
    // savant penalty, so the mapping depends on pawn state and cannot be a
    // second copy of the arithmetic somewhere in the UI.
    private float TargetForSeverity(float severity, bool storing) {
        float unpenalised = storing
            ? severity / SavantUtility.GetFeruchemyStorePenaltyMultiplier(cachedSavantStage)
            : severity;

        float normalized = Mathf.Clamp01((unpenalised - 1f) / (MaxSeverity - 1f));
        float delta = 50f * Mathf.Pow(normalized, 1f / CurveExponent);

        return IdleTarget + (storing ? delta : -delta);
    }

    // Nudges a raw dial position onto the nearest rate step.
    public float SnapTarget(float rawTarget) {
        if (Mathf.Abs(rawTarget - IdleTarget) < DeadBand) return IdleTarget;

        bool storing = rawTarget > IdleTarget;
        float snapped = Mathf.Round(SeverityForTarget(rawTarget) / SeverityQuantum) * SeverityQuantum;

        float floor = SeverityForTarget(IdleTarget + (storing ? DeadBand : -DeadBand));
        float ceiling = SeverityForTarget(storing ? 100f : 0f);
        snapped = Mathf.Clamp(snapped, Mathf.Ceil(floor / SeverityQuantum) * SeverityQuantum, ceiling);

        return Mathf.Clamp(TargetForSeverity(snapped, storing), 0f, 100f);
    }

    private void TryRemoveHediffByDef(HediffDef? def) {
        if (def == null) return;
        Hediff? hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def);
        if (hediff != null) pawn.health.RemoveHediff(hediff);
    }

    public override void Reset() {
        TryRemoveHediffByDef(tapCompoundedHediffDef);
        compoundedTargetValue = IdleTarget;
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
        Scribe_Values.Look(ref compoundedTargetValue, "compoundedTargetValue", IdleTarget);
        Scribe_Values.Look(ref targetMetalmindId, "targetMetalmindId", string.Empty);
        Scribe_Values.Look(ref compounding, "compounding");
        Scribe_Values.Look(ref savantDecayOffset, "savantDecayOffset");
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;

        TickCopper();

        // Compounding needs an implant to act on, and burning destroys them. Ending
        // the mode here rather than in the panel means it holds whether or not the
        // player happens to be looking at it.
        if (compounding && !TargetIsInternalOnly) {
            compounding = false;
            compoundedTargetValue = IdleTarget;
        }

        if (!canTapAny && isTapping && !isCompounding) Reset();
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
        // No early out on the stored dial: each pool decides for itself below, and
        // gating both on the stored one let compounded charge drain with no hediff
        // to show for it.
        float ordinary = SeverityForTarget(targetValue);
        if (targetValue < IdleTarget && canTap && ordinary > 0f) {
            TryRemoveHediffByDef(storeHediffDef);
            if (tapHediffDef != null) pawn.health.GetOrAddHediff(tapHediffDef).Severity = ordinary;
        } else if (targetValue > IdleTarget && canStore && ordinary > 0f) {
            TryRemoveHediffByDef(tapHediffDef);
            pawn.health.GetOrAddHediff(storeHediffDef).Severity = ordinary;
        } else {
            TryRemoveHediffByDef(tapHediffDef);
            TryRemoveHediffByDef(storeHediffDef);
        }

        // The compounded pool runs alongside rather than instead, so both ladders
        // can be lit at once and each pays out on its own terms.
        float compounded = SeverityForTarget(compoundedTargetValue);
        if (compoundedTargetValue < IdleTarget && canTapCompounded && compounded > 0f) {
            if (tapCompoundedHediffDef != null) {
                pawn.health.GetOrAddHediff(tapCompoundedHediffDef).Severity = compounded;
            }
        } else {
            TryRemoveHediffByDef(tapCompoundedHediffDef);
        }
    }

    private void TickStoreOrTap() {
        // Reads the curve rather than the hediff's severity. Compounded tapping
        // amplifies through its own stage ladder, so if the drain read severity
        // back it would move ten times the charge as well.
        float ordinary = SeverityForTarget(targetValue);
        if (ordinary > 0f) {
            if (targetValue > IdleTarget && canStore) AddToStore(AmountPerSecond * ordinary);
            else if (targetValue < IdleTarget && canTap) RemoveFromStore(AmountPerSecond * ordinary);
        }

        // The compounded pool fills at the ordinary storing rate and costs the same
        // attribute. What makes it compounding is the burn on the way out, which
        // pays ten times over and consumes the metalmind with it.
        float compounded = SeverityForTarget(compoundedTargetValue);
        if (compounded <= 0f) return;

        if (compoundedTargetValue > IdleTarget) {
            // Filling is ordinary storing. Nothing about the charge is special; the
            // burn on the way out is what compounds it.
            if (canStore) AddToStore(AmountPerSecond * compounded);
        } else if (canTapCompounded) {
            RemoveCompoundedFromStore(AmountPerSecond * compounded);
        }
    }

    private void TickXPGain(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        // Burning a metalmind is the allomantic half, so only the compounded tap
        // teaches allomancy. Filling the pool is ordinary feruchemy.
        if (compoundedTargetValue < IdleTarget && canTapCompounded) {
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower)
                .Learn(10 * ScadrialMetallurgyConstants.FeruchemyXPPerTick * GenTicks.TickLongInterval);
            pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower)
                .Learn(10 * ScadrialMetallurgyConstants.FeruchemyXPPerTick * GenTicks.TickLongInterval);
        } else if (isTapping || isStoringAny) {
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
        // Burning draws on everything the metalmind holds, so the room left to take
        // is its whole charge. Reading the compounded pool here left nothing to
        // take, which both stalled the burn and span the sweep below every tick.
        bool result = Distribute(
            amount,
            static m => m.CanTapCompounded,
            static (m, a) => m.ConsumeCompounded(a),
            static m => m.TotalStored
        );

        if (result) SweepBurnedOut();

        return result;
    }

    // A metalmind emptied of compounded charge has no capacity left, so it is gone.
    // The cache is dropped because the sweep can remove entries it holds.
    private void SweepBurnedOut() {
        MetalmindBurnout.Sweep(pawn);
        cachedMetalminds = null;
    }

    // Carries the remainder across sources. Dumping the full amount into the first
    // eligible metalmind let its clamp discard the overflow silently.
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

        // Resolved every pass rather than cached: a metalmind can burn out mid-tick,
        // and a target that no longer exists falls back to spreading the charge.
        string target = targetMetalmindId;
        for (int i = 0; i < mms.Count && remaining > 0f; i++) {
            if (!MatchesTarget(mms[i], target)) continue;
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

        if (newStage == 1 && !isTapping && !isStoringAny) {
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
