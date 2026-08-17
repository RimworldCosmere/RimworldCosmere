using System;
using Cosmere.Core.Def;
using Cosmere.Core.Need;
using Cosmere.Core.Savant;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Hediff;
using Cosmere.System.Scadrial.Feruchemy.Ledger;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Gene;

// Which stored pool the tap controls act on.
public enum FeruchemyChannel {
    Ordinary,
    Compounded,
}

public class Feruchemist : Metalborn {
    // The dial's neutral point. Below it the pawn taps, above it they store.
    public const float IdleTarget = 50f;
    public const float MaxSeverity = FeruchemyRate.MaxSeverity;

    private List<IMetalmindSource>? cachedMetalminds;
    private HediffDef? cachedPermanentHediffDef;
    private HediffDef? cachedSavantHediffDef;
    private int cachedSavantStage;
    private HediffDef? cachedStoreHediffDef;

    private HediffDef? cachedTapHediffDef;
    private int metalmindsLastCachedTick = -1;
    private HediffDef? cachedTapCompoundedHediffDef;
    private float savantDecayOffset;
    private readonly ChargeLedger chargeLedger = new ChargeLedger();

    /// The compounded pool has its own dial: below idle it taps, above idle it
    /// compounds, burning allomantic reserve to fill itself.
    public float compoundedTargetValue = IdleTarget;

    /// The target vocabulary lives with the matcher that reads it. These keep the gene's
    /// own callers spelling it the way they always have.
    public const string TargetAll = MetalmindDistribution.TargetAll;

    public const string TargetExternal = MetalmindDistribution.TargetExternal;

    public const string TargetInternal = MetalmindDistribution.TargetInternal;

    /// Which metalmind the dials act on: one of the group tokens above, or a
    /// single metalmind's SourceId.
    public string targetMetalmindId = TargetAll;

    /// Which tie duralumin's dial spends. A ledger with no capacity parks the dial.
    public DuraluminLedger targetLedger = DuraluminLedger.Residence;

    // Which Shard the Shard ledger acts on. Seeded once, then the player's to change.
    public string targetShardDefName = string.Empty;

    /// Duralumin moves whole points and each dial pays out a fraction of one a second, so an ask
    /// banks here until it is worth a point. A bank holds intent, never charge.
    private float ordinaryCarry;

    private float compoundedCarry;

    // What the banks were filled for. A dial pointed somewhere new banks nothing from the old tie.
    private string? bankedKey;

    /// Whether the dial is pointed at the compounded pool. Held here, not the panel,
    /// so a metal that stops paying out can end the mode instead of just parking it.
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

    /// Whether everything the dials can currently reach is implanted, which is
    /// the condition for compounding. True for a single implant or the internal group.
    public bool TargetIsInternalOnly {
        get {
            if (SelectedSource is { IsImplanted: true }) return true;
            if (targetMetalmindId != TargetInternal) return false;

            // burning can empty the group entirely; an empty group is not something to compound into.
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].IsImplanted) return true;
            }

            return false;
        }
    }

    public static bool IsGroupTarget(string target) {
        return MetalmindDistribution.IsGroupTarget(target);
    }

    /// A metal is declared twice, as MetalDef and as the richer MetallicArtsMetalDef,
    /// so the two are never the same object. Compare by defName, not reference.
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

    /// Room in the one metalmind compounding is filling right now. Deposits go in
    /// order, so this is the first that will accept charge.
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

                // setting Value directly must reach zero, so it falls through to compounded once ordinary runs out; the gameplay tap path deliberately does not.
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

    /// Nicrosil's charge is the pawn's own Investiture rather than a notional attribute, so
    /// the pawn sits at the far end of every transfer this gene makes.
    private bool storesInvestiture => metal == MetallicArtsMetalDefOf.Nicrosil;

    private Investiture? investitureNeed => storesInvestiture ? pawn.needs?.TryGetNeed<Investiture>() : null;

    /// Duralumin's charge is the pawn's own Connection, counted in points, so it puts the pawn at
    /// the far end of every transfer the same way nicrosil does.
    private bool storesConnection => metal == MetallicArtsMetalDefOf.Duralumin;

    private IConnectionLedger? cachedLedger;
    private string? cachedLedgerKey;
    private bool ledgerResolved;
    private DuraluminLedger resolvedFor;
    private string resolvedForShard = string.Empty;

    /// The ledger the dial names, or none for a ledger with no capacity and for a Shard this save
    /// does not have. Cached, because the dock reads it every frame through the capacity readout.
    public IConnectionLedger? SelectedLedger {
        get {
            if (!storesConnection) return null;
            ResolveLedger();

            return cachedLedger;
        }
    }

    // The name a transfer is filed under on the metalmind, or none when the dial reaches nothing.
    private string? transferKey {
        get {
            if (!storesConnection) return null;
            ResolveLedger();

            return cachedLedgerKey;
        }
    }

    private void ResolveLedger() {
        if (ledgerResolved && resolvedFor == targetLedger && resolvedForShard == targetShardDefName) return;

        ledgerResolved = true;
        resolvedFor = targetLedger;
        resolvedForShard = targetShardDefName;
        cachedLedger = null;
        cachedLedgerKey = null;
        if (ConnectionBudget.Capacity(targetLedger) <= 0) return;

        switch (targetLedger) {
            case DuraluminLedger.Residence:
                cachedLedger = new ResidenceLedger();
                break;
            case DuraluminLedger.Bonds:
                cachedLedger = new BondLedger();
                break;
            case DuraluminLedger.Shard:
                ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(targetShardDefName);
                if (shard == null) return;

                cachedLedger = new ShardLedger(shard);
                break;
            default:
                return;
        }

        cachedLedgerKey = ConnectionKey.For(targetLedger, targetShardDefName);
    }

    /// Picked once and scribed, so charge banked against one Shard is only ever given back to that
    /// Shard. The player retargets it from the selector; this is only the opening guess.
    private void EnsureShardTarget() {
        if (!storesConnection || !string.IsNullOrEmpty(targetShardDefName)) return;

        CosmereWorldDef? world = WorldUtility.Primary;
        if (world == null) return;

        for (int i = 0; i < world.nativeShards.Count; i++) {
            if (!ShardUtility.IsEnabled(world.nativeShards[i])) continue;

            targetShardDefName = world.nativeShards[i].defName;

            return;
        }
    }

    /// Why duralumin's dial is parked, for the selector to show, or none when it can move. Social
    /// has no capacity at all; a Shard the save never enabled has nothing to act on.
    public string? LedgerBlockedReason {
        get {
            if (!storesConnection || SelectedLedger != null) return null;

            return ConnectionBudget.Capacity(targetLedger) <= 0
                ? "CS_Duralumin_LedgerHasNoCapacity".Translate().Resolve()
                : "CS_Duralumin_NoShardSelected".Translate().Resolve();
        }
    }

    // Charge the metalminds this dial reaches already hold under the key it names.
    private float storedForLedger {
        get {
            string? key = transferKey;
            if (key == null) return 0f;

            float total = 0f;
            List<IMetalmindSource> mms = metalminds;
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MetalmindDistribution.MatchesTarget(mms[i], target)) continue;
                total += mms[i].StoredFor(key);
            }

            return total;
        }
    }

    private int connectionBudgetTick = -1;
    private DuraluminLedger connectionBudgetLedger;
    private string connectionBudgetShard = string.Empty;
    private float connectionStorable;
    private float connectionTappable;

    /// Both bounds come off one read of the ledger, memoized per tick. The dock's capacity readout
    /// asks four times a frame, and the Bonds ledger walks the pawn's whole SpiritWeb each time.
    private void RefreshConnectionBudget() {
        int now = Find.TickManager.TicksGame;
        if (connectionBudgetTick == now &&
            connectionBudgetLedger == targetLedger &&
            connectionBudgetShard == targetShardDefName) {
            return;
        }

        connectionBudgetTick = now;
        connectionBudgetLedger = targetLedger;
        connectionBudgetShard = targetShardDefName;

        IConnectionLedger? ledger = SelectedLedger;
        if (ledger == null) {
            connectionStorable = 0f;
            connectionTappable = 0f;

            return;
        }

        float stored = storedForLedger;
        connectionStorable = ConnectionBudget.Storable(ledger.CurrentPoints(pawn), stored, targetLedger);
        connectionTappable = ConnectionBudget.Tappable(ledger.HeadroomPoints(pawn), stored, targetLedger);
    }

    /// Neither nicrosil nor duralumin gets a stage ladder for a compounded burn to amplify
    /// through, so the burn pays out faster instead of larger: the same charge, a tenth of the time.
    private float CompoundedBurnRate(float perSecond) {
        return storesInvestiture || storesConnection
            ? perSecond * CompoundedTap.EffectMultiplier
            : perSecond;
    }

    /// Charge the pawn still has to give; every other metal stores something notional, so only
    /// nicrosil's owner and duralumin's ledger bound it.
    private float storableFromPawn {
        get {
            if (storesConnection) {
                RefreshConnectionBudget();

                return connectionStorable;
            }

            Investiture? need = investitureNeed;

            return need == null
                ? float.PositiveInfinity
                : InvestitureBudget.Storable(need.CurLevel, ScadrialMetallurgyConstants.NicrosilBeuPerCharge);
        }
    }

    // The mirror of that floor: charge the pawn has room to take back.
    private float tappableToPawn {
        get {
            if (storesConnection) {
                RefreshConnectionBudget();

                return connectionTappable;
            }

            Investiture? need = investitureNeed;

            return need == null
                ? float.PositiveInfinity
                : InvestitureBudget.Tappable(
                    need.CurLevel,
                    need.MaxLevel,
                    ScadrialMetallurgyConstants.NicrosilBeuPerCharge
                );
        }
    }

    public bool canTap {
        get {
            if (tappableToPawn <= 0f) return false;

            List<IMetalmindSource> mms = metalminds;
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MetalmindDistribution.MatchesTarget(mms[i], target)) continue;
                if (mms[i].CanTap) return true;
            }

            return false;
        }
    }

    public bool canTapCompounded {
        get {
            if (tappableToPawn <= 0f) return false;

            List<IMetalmindSource> mms = metalminds;
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MetalmindDistribution.MatchesTarget(mms[i], target)) continue;
                if (mms[i].CanTapCompounded) return true;
            }

            return false;
        }
    }

    public bool canStoreCompounded {
        get {
            if (storableFromPawn <= 0f) return false;

            List<IMetalmindSource> mms = metalminds;
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MetalmindDistribution.MatchesTarget(mms[i], target)) continue;
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
            if (storableFromPawn <= 0f) return false;

            List<IMetalmindSource> mms = metalminds;
            string target = targetMetalmindId;
            for (int i = 0; i < mms.Count; i++) {
                if (!MetalmindDistribution.MatchesTarget(mms[i], target)) continue;
                if (mms[i].CanStore) return true;
            }

            return false;
        }
    }

    public bool isStoring => storeHediffDef != null && pawn.health.hediffSet.HasHediff(storeHediffDef);

    /// The compounded dial is off its rest point in either direction. Filling that
    /// pool is ordinary feruchemical storing; the amplification lives in the burn.
    public bool isCompounding => compoundedTargetValue != IdleTarget;

    /// Compounding pours charge into a metalmind exactly as storing does, so it counts
    /// as storing too - mirroring isTapping, which already covers both channels.
    public bool isStoringAny => isStoring || (compoundedTargetValue > IdleTarget && canStoreCompounded);

    /// Per-metal pacing, applied to every path that moves charge so the dial readout
    /// and the actual drain cannot drift apart. One figure for both directions.
    public float RateMultiplier => metal.feruchemy?.rateMultiplier ?? 1f;

    /// Skill and strength buy duration, not magnitude: the ladder pays the same for
    /// everyone, but a master draws on it more slowly and runs longer on one metalmind.
    public float Efficiency {
        get {
            SkillRecord? skill = pawn.skills?.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower);

            return FeruchemyRate.Efficiency(
                pawn.GetStatValue(StatDefOf.Cosmere_Scadrial_Stat_FeruchemicPower),
                skill?.Level ?? 0,
                SavantUtility.GetFeruchemicalPowerMultiplier(cachedSavantStage)
            );
        }
    }

    /// What the compounded dial is moving: negative while tapping it, positive
    /// while compounding into it. Zero inside the dead band.
    public float CompoundedRatePerSecond {
        get {
            float severity = SeverityForTarget(compoundedTargetValue);
            if (severity <= 0f) return 0f;

            float perSecond = FeruchemyRate.PerSecond(severity, RateMultiplier, Efficiency);

            if (compoundedTargetValue > IdleTarget) {
                return canStoreCompounded ? perSecond : 0f;
            }

            return canTapCompounded ? -CompoundedBurnRate(perSecond) : 0f;
        }
    }

    /// Both dials move charge at once, so the readout carries the pair or it
    /// understates what is happening.
    public float TransferRatePerSecond => dialRatePerSecond + CompoundedRatePerSecond;

    private float dialRatePerSecond {
        get {
            float severity = effectiveSeverity;
            if (severity <= 0f) return 0f;

            float perSecond = FeruchemyRate.PerSecond(severity, RateMultiplier, Efficiency);

            if (targetValue < IdleTarget) return canTap ? -perSecond : 0f;
            return canStore ? perSecond : 0f;
        }
    }

    private const float DeadBand = 2f;
    private const float CurveExponent = 2.5f;

    /// One step per severity point, which is also one step per rung of the hediff
    /// ladder - the dial cannot land between two stages and pay for one it does not get.
    public const float RateQuantum = FeruchemyRate.AmountPerSecond;

    private static float SeverityQuantum => RateQuantum / FeruchemyRate.AmountPerSecond;

    private float effectiveSeverity => SeverityForTarget(targetValue);

    private float SeverityForTarget(float target) {
        float delta = target - IdleTarget;
        if (Mathf.Abs(delta) < DeadBand) return 0f;

        float normalized = Mathf.Abs(delta) / 50f;

        return 1f + Mathf.Pow(normalized, CurveExponent) * (MaxSeverity - 1f);
    }

    // Inverse of the curve above.
    private static float TargetForSeverity(float severity, bool storing) {
        float normalized = Mathf.Clamp01((severity - 1f) / (MaxSeverity - 1f));
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

        // clamp to the dead band's own severity: rounding up to the next whole step left the first rung of every ladder unreachable.
        snapped = Mathf.Clamp(snapped, floor, ceiling);

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
        ordinaryCarry = 0f;
        compoundedCarry = 0f;
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
        Scribe_Values.Look(ref targetLedger, "targetLedger", DuraluminLedger.Residence);
        Scribe_Values.Look(ref targetShardDefName, "targetShardDefName", string.Empty);
        Scribe_Values.Look(ref compounding, "compounding");
        Scribe_Values.Look(ref savantDecayOffset, "savantDecayOffset");
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;

        TickCopper();
        EnsureShardTarget();
        SyncConnectionBank();

        // compounding needs an implant to act on; end the mode here so it holds even when the player isnt looking at the panel.
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
        // no early out on the stored dial: gating both pools on it let compounded charge drain with no hediff to show for it.
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

        // the compounded pool runs alongside rather than instead, so both ladders can be lit at once.
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
        // reads the curve, not the hediff severity: compounded tapping amplifies severity tenfold, so reading it back would double-apply the multiplier.
        float efficiency = Efficiency;
        float rateMultiplier = RateMultiplier;

        // what the pawn can still trade this tick, so it cannot overshoot the budget TickInterval parks the dial on.
        float storable = storableFromPawn;
        float tappable = tappableToPawn;

        float ordinary = SeverityForTarget(targetValue);
        if (ordinary > 0f) {
            float perSecond = FeruchemyRate.PerSecond(ordinary, rateMultiplier, efficiency);
            if (targetValue > IdleTarget && canStore) {
                float moved = AddToStore(TransferAsk(ref ordinaryCarry, perSecond, storable, true));
                storable -= moved;
                chargeLedger.Stored(moved);
                SettleConnection(moved, true);
            } else if (targetValue < IdleTarget && canTap) {
                float moved = RemoveFromStore(TransferAsk(ref ordinaryCarry, perSecond, tappable, false));
                tappable -= moved;
                chargeLedger.Tapped(moved);
                SettleConnection(moved, false);
            }
        }

        // the compounded pool fills at the ordinary storing rate; the burn on the way out is what compounds it, paying ten times over and consuming the metalmind.
        float compounded = SeverityForTarget(compoundedTargetValue);
        if (compounded > 0f) {
            float compoundedPerSecond = FeruchemyRate.PerSecond(compounded, rateMultiplier, efficiency);

            if (compoundedTargetValue > IdleTarget) {
                if (canStore) {
                    float moved = AddToStore(TransferAsk(ref compoundedCarry, compoundedPerSecond, storable, true));
                    chargeLedger.Stored(moved);
                    SettleConnection(moved, true);
                }
            } else if (canTapCompounded) {
                float moved = RemoveCompoundedFromStore(
                    TransferAsk(ref compoundedCarry, CompoundedBurnRate(compoundedPerSecond), tappable, false)
                );
                chargeLedger.Tapped(moved);
                SettleConnection(moved, false);
            }
        }

        // drained from the ledger, not a hediff: compound-fill leaves the pawn without one, and the gene still has to tick.
        MirrorToInvestiture(chargeLedger.Drain());
    }

    // A dial pointed somewhere new banks nothing from where it used to point.
    private void SyncConnectionBank() {
        string? key = transferKey;
        if (bankedKey == key) return;

        bankedKey = key;
        ordinaryCarry = 0f;
        compoundedCarry = 0f;
    }

    // Every metal but duralumin passes its ask straight through; duralumin banks it to a whole point.
    private float TransferAsk(ref float carry, float perSecond, float budget, bool storing) {
        return storesConnection
            ? ConnectionCarry.Bank(ref carry, perSecond, budget, storing)
            : Mathf.Min(perSecond, budget);
    }

    /// Moves the pawn's Connection to match what the metalmind just moved, then hands back whichever
    /// side moved more. Settled per move: a netted tick hides two half-failed moves that cancel.
    private void SettleConnection(float chargeMoved, bool storing) {
        if (!storesConnection || chargeMoved <= 0f) return;

        IConnectionLedger? ledger = SelectedLedger;
        if (ledger == null) return;

        float signed = ledger.Move(pawn, ConnectionSettlement.Ask(chargeMoved, storing));
        ConnectionSettlement due = ConnectionSettlement.Resolve(chargeMoved, storing, signed);

        float unsettled = 0f;
        if (due.MetalmindCharge > 0f) {
            unsettled = due.MetalmindCharge - AddToStore(due.MetalmindCharge);
        } else if (due.MetalmindCharge < 0f) {
            unsettled = -due.MetalmindCharge - ReclaimFromStore(-due.MetalmindCharge);
        } else if (due.PawnPoints != 0f) {
            float back = ledger.Move(pawn, due.PawnPoints);
            unsettled = ConnectionBudget.ChargeForPoints(Mathf.Abs(due.PawnPoints - back));
        }

        if (unsettled <= 0f) return;

        Logger.Warning(
            $"Duralumin: {unsettled:F1} of a {chargeMoved:F1} charge transfer went unsettled for {pawn.LabelShort}"
        );
    }

    /// Nicrosil's charge is the pawn's own Investiture, so what the metalmind gains is
    /// exactly what the pawn gives up. Every other metal stores a notional attribute.
    private void MirrorToInvestiture(float chargeMoved) {
        if (chargeMoved == 0f) return;

        Investiture? need = investitureNeed;
        if (need == null) return;

        need.CurLevel = Mathf.Clamp(
            need.CurLevel - chargeMoved * ScadrialMetallurgyConstants.NicrosilBeuPerCharge,
            0f,
            need.MaxLevel
        );
    }

    private void TickXPGain(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;

        // burning is the allomantic half, so only the compounded tap teaches allomancy; filling the pool is ordinary feruchemy.
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

    public float AddToStore(float amount) {
        return Distribute(
            amount,
            static (m, _) => m.CanStore,
            static (m, k, a) => m.AddStored(a, k),
            static (m, _) => m.FreeSpace
        );
    }

    public float AddCompoundedToStore(float amount) {
        return Distribute(
            amount,
            static (m, _) => m.CanStoreCompounded,
            static (m, _, a) => m.AddCompounded(a),
            static (m, _) => m.FreeSpace
        );
    }

    public float RemoveCompoundedFromStore(float amount) {
        // burning draws on the whole charge, not just the compounded pool: reading only that left nothing to take and stalled the burn.
        float moved = Distribute(
            amount,
            static (m, _) => m.CanTapCompounded,
            static (m, k, a) => m.ConsumeCompounded(a, k),
            static (m, _) => m.TotalStored
        );

        if (moved > 0f) SweepBurnedOut();

        return moved;
    }

    /// A metalmind emptied of compounded charge has no capacity left, so it is gone.
    /// The cache is dropped because the sweep can remove entries it holds.
    private void SweepBurnedOut() {
        MetalmindBurnout.Sweep(pawn);
        cachedMetalminds = null;
    }

    private float Distribute(
        float amount,
        Func<IMetalmindSource, string?, bool> eligible,
        Func<IMetalmindSource, string?, float, float> apply,
        Func<IMetalmindSource, string?, float> room
    ) {
        // read fresh, not cached: a metalmind can burn out mid-tick and take the dial's chosen target with it.
        return MetalmindDistribution.Carry(metalminds, targetMetalmindId, transferKey, amount, eligible, apply, room);
    }

    public float RemoveFromStore(float amount) {
        return Distribute(
            amount,
            static (m, _) => m.CanTap,
            static (m, k, a) => m.ConsumeStored(a, k),
            static (m, _) => m.StoredAmount
        );
    }

    /// Takes a correction back out of the charge this key is recorded as holding, never out of
    /// another ledger's. Draining past the named key would mint one tie by destroying another.
    private float ReclaimFromStore(float amount) {
        string? key = transferKey;

        return key == null ? 0f : MetalmindDistribution.Reclaim(metalminds, targetMetalmindId, key, amount);
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
