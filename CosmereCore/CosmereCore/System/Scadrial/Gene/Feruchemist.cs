using Cosmere.Core.Savant;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Hediff;
using Cosmere.System.Scadrial.Gizmo;
using Cosmere.System.Scadrial.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

public class Feruchemist : Metalborn {
    public static readonly float AmountPerRareTick = 1 / 18f;
    private new FeruchemicGeneCommand? gizmo => (FeruchemicGeneCommand)base.gizmo;

    private List<IMetalmindSource>? cachedMetalminds;
    private int metalmindsLastCachedTick = -1;

    private HediffDef? cachedTapHediffDef;
    private HediffDef? cachedStoreHediffDef;
    private HediffDef? cachedCompoundHediffDef;
    private int cachedSavantStage;
    private float savantDecayOffset;
    private HediffDef? cachedSavantHediffDef;
    private HediffDef? cachedPermanentHediffDef;

    public List<IMetalmindSource> metalminds {
        get {
            int now = Find.TickManager.TicksGame;
            if (cachedMetalminds != null && metalmindsLastCachedTick == now) return cachedMetalminds;

            cachedMetalminds = [];
            List<Verse.Thing> items = pawn.inventory.innerContainer.InnerListForReading;
            for (int i = 0; i < items.Count; i++) {
                Metalmind? comp = items[i].TryGetComp<Metalmind>();
                if (comp != null && comp.metal == metal) cachedMetalminds.Add(comp);
            }

            ImplantedMetalminds? implantHediff =
                pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_ImplantedMetalminds) as ImplantedMetalminds;
            if (implantHediff != null) {
                for (int i = 0; i < implantHediff.metalminds.Count; i++) {
                    ImplantedMetalmindData data = implantHediff.metalminds[i];
                    if (data.metal == metal) cachedMetalminds.Add(data);
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
                total += mms[i].maxAmount;
            }
            return total;
        }
    }

    private float actualValue {
        get {
            float total = 0f;
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                total += mms[i].storedAmount;
            }
            return total;
        }
    }

    public override float InitialResourceMax => 100f;
    public override float Max => 100f;
    public override float Value => actualMax <= 0f ? 0f : actualValue / actualMax * Max;

    private HediffDef? tapHediffDef =>
        cachedTapHediffDef ??= DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Tap" + metal.defName);

    private Hediff? tapHediff => pawn.health.hediffSet.GetFirstHediffOfDef(tapHediffDef);

    private HediffDef? storeHediffDef =>
        cachedStoreHediffDef ??= DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Store" + metal.defName);

    private Hediff? storeHediff => pawn.health.hediffSet.GetFirstHediffOfDef(storeHediffDef);

    private HediffDef? compoundHediffDef =>
        cachedCompoundHediffDef ??= DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Compound" + metal.defName);

    private Hediff? compoundHediff => pawn.health.hediffSet.GetFirstHediffOfDef(compoundHediffDef);

    private HediffDef? savantHediffDef =>
        cachedSavantHediffDef ??= SavantUtility.GetFeruchemicalSavantHediffDef(metal);

    private HediffDef? permanentHediffDef =>
        cachedPermanentHediffDef ??= SavantUtility.GetFeruchemicalPermanentHediffDef(metal);

    public bool canTap {
        get {
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].canTap) return true;
            }
            return false;
        }
    }

    public bool isTapping => tapHediffDef != null && pawn.health.hediffSet.HasHediff(tapHediffDef);

    public bool canStore {
        get {
            List<IMetalmindSource> mms = metalminds;
            for (int i = 0; i < mms.Count; i++) {
                if (mms[i].canStore) return true;
            }
            return false;
        }
    }

    public bool isStoring => storeHediffDef != null && pawn.health.hediffSet.HasHediff(storeHediffDef);
    public bool isCompounding => compoundHediffDef != null && pawn.health.hediffSet.HasHediff(compoundHediffDef);

    private float effectiveSeverity {
        get {
            float delta = targetValue - 50f;
            if (Mathf.Abs(delta) < 2f) return 0f;

            float exponent = 2.5f;
            float maxSeverity = 19f;

            float normalized = Mathf.Abs(delta) / 50f;
            float baseSeverity = 1f + Mathf.Pow(normalized, exponent) * maxSeverity;

            if (delta > 0f) {
                float penalty = SavantUtility.GetFeruchemyStorePenaltyMultiplier(cachedSavantStage);
                baseSeverity *= penalty;
            }

            return baseSeverity;
        }
    }


    public override void Reset() {
        targetValue = 50f;
        if (gizmo != null) gizmo.targetValuePercent = .5f;
        if (storeHediffDef != null && pawn.health.hediffSet.HasHediff(storeHediffDef)) {
            Hediff? sh = storeHediff;
            if (sh != null) pawn.health.RemoveHediff(sh);
        }
        if (tapHediffDef != null && pawn.health.hediffSet.HasHediff(tapHediffDef)) {
            Hediff? th = tapHediff;
            if (th != null) pawn.health.RemoveHediff(th);
        }
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleFullFeruchemistTrait(pawn);
        MetalbornUtility.HandleFeruchemistTrait(pawn);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;

        if (!canTap && isTapping && !isCompounding) Reset();
        if (!canStore && isStoring && !isCompounding) Reset();

        if (effectiveSeverity > 0f) {
            if (targetValue < 50 && canTap) {
                if (storeHediffDef != null && pawn.health.hediffSet.HasHediff(storeHediffDef)) {
                    Hediff? sh = storeHediff;
                    if (sh != null) pawn.health.RemoveHediff(sh);
                }

                pawn.health.GetOrAddHediff(tapHediffDef).Severity = effectiveSeverity;
            } else if (targetValue > 50 && canStore) {
                if (tapHediffDef != null && pawn.health.hediffSet.HasHediff(tapHediffDef)) {
                    Hediff? th = tapHediff;
                    if (th != null) pawn.health.RemoveHediff(th);
                }

                pawn.health.GetOrAddHediff(storeHediffDef).Severity = effectiveSeverity;
            }
        }

        if (isStoring) {
            Verse.Hediff? sh = storeHediff;
            if (sh != null) AddToStore(AmountPerRareTick * sh.Severity);
        } else if (isTapping) {
            Verse.Hediff? th = tapHediff;
            if (th != null) RemoveFromStore(AmountPerRareTick * th.Severity);
        }

        if (pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) {
            if (isCompounding) {
                pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower)
                    .Learn(10 * Constants.FeruchemyXPPerTick * GenTicks.TickLongInterval);
                pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_AllomanticPower)
                    .Learn(10 * Constants.FeruchemyXPPerTick * GenTicks.TickLongInterval);
            } else if (isTapping || isStoring) {
                pawn.skills.GetSkill(SkillDefOf.Cosmere_Scadrial_Skill_FeruchemicPower)
                    .Learn(
                        Mathf.Lerp(1, 2, effectiveSeverity) * Constants.FeruchemyXPPerTick * GenTicks.TickLongInterval
                    );
            }
        }

        CheckFeruchemicalSavantProgression(delta);
    }

    public bool AddToStore(float amount) {
        List<IMetalmindSource> mms = metalminds;
        for (int i = 0; i < mms.Count; i++) {
            if (!mms[i].canStore) continue;
            mms[i].AddStored(amount);
            return true;
        }
        return false;
    }

    public bool RemoveFromStore(float amount) {
        List<IMetalmindSource> mms = metalminds;
        for (int i = 0; i < mms.Count; i++) {
            if (!mms[i].canTap) continue;
            mms[i].ConsumeStored(amount);
            return true;
        }
        return false;
    }

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        return [];
    }

    private void CheckFeruchemicalSavantProgression(int delta) {
        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) return;
        if (!SavantUtility.CanBeSavant(metal)) return;

        int previousStage = cachedSavantStage;
        RecordDef storingRecord = RecordDefOf.GetTimeSpentStoringForMetal(metal);
        RecordDef tappingRecord = RecordDefOf.GetTimeSpentTappingForMetal(metal);
        float ticks = pawn.records.GetValue(storingRecord) + pawn.records.GetValue(tappingRecord) - savantDecayOffset;
        int newStage = SavantUtility.GetFeruchemicalStage(ticks);

        if (newStage == 1 && !isTapping && !isStoring) {
            float decayAmount = ticks * SavantUtility.Stage1DecayPerDayFraction / GenDate.TicksPerDay * GenTicks.TickLongInterval;
            savantDecayOffset += decayAmount;
            ticks -= decayAmount;
            newStage = SavantUtility.GetFeruchemicalStage(ticks);
        }

        cachedSavantStage = newStage;

        if (newStage > 0 && savantHediffDef != null) {
            Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(savantHediffDef);
            if (existing == null) {
                Hediff hediff = HediffMaker.MakeHediff(savantHediffDef, pawn);
                hediff.Severity = SavantUtility.SeverityForStage(newStage);
                pawn.health.AddHediff(hediff);
            } else {
                existing.Severity = SavantUtility.SeverityForStage(newStage);
            }
        } else if (newStage == 0 && savantHediffDef != null) {
            Hediff? existing = pawn.health.hediffSet.GetFirstHediffOfDef(savantHediffDef);
            if (existing != null) pawn.health.RemoveHediff(existing);
        }

        if (newStage >= 3 && previousStage < 3 && permanentHediffDef != null) {
            if (!pawn.health.hediffSet.HasHediff(permanentHediffDef)) {
                pawn.health.AddHediff(HediffMaker.MakeHediff(permanentHediffDef, pawn));
            }
        }

        if (newStage > previousStage && newStage > 0) {
            SavantUtility.SendSavantLetter(pawn, newStage, "Feruchemy", metal.LabelCap);
        }
    }
}
