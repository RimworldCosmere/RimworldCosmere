using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Feruchemy.Comp.Thing;
using CosmereScadrial.Gizmo;
using CosmereScadrial.Util;
using UnityEngine;
using Verse;
using Logger = CosmereFramework.Logger;

namespace CosmereScadrial.Gene;

public class Feruchemist : Metalborn {
    public static readonly float AmountPerRareTick = 1 / 18f;
    private new FeruchemicGeneCommand? gizmo => (FeruchemicGeneCommand)base.gizmo;

    public List<Metalmind> metalminds =>
        pawn.inventory.innerContainer
            .InnerListForReading
            .Where(t => t.HasComp<Metalmind>())
            .Select(t => t.TryGetComp<Metalmind>())
            .Where(c => c.metal == metal)
            .ToList() ??
        [];

    private float ActualMax => metalminds.Sum(m => m.maxAmount);
    private float ActualValue => metalminds.Sum(m => m.StoredAmount);

    public override float InitialResourceMax => 100f;
    public override float Max => 100f;
    public override float Value => ActualMax <= 0f ? 0f : ActualValue / ActualMax * Max;

    private HediffDef tapHediffDef =>
        DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Tap" + metal.defName);

    private Hediff tapHediff => pawn.health.hediffSet.GetFirstHediffOfDef(tapHediffDef);

    private HediffDef storeHediffDef =>
        DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Store" + metal.defName);

    private Hediff storeHediff => pawn.health.hediffSet.GetFirstHediffOfDef(storeHediffDef);

    private HediffDef compoundHediffDef =>
        DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Compound" + metal.defName);

    private Hediff compoundHediff => pawn.health.hediffSet.GetFirstHediffOfDef(compoundHediffDef);

    public bool canTap => metalminds.Any(x => x.canTap);
    public bool isTapping => pawn.health.hediffSet.HasHediff(tapHediffDef);
    public bool canStore => metalminds.Any(x => x.canStore);
    public bool isStoring => pawn.health.hediffSet.HasHediff(storeHediffDef);
    public bool isCompounding => pawn.health.hediffSet.HasHediff(compoundHediffDef);

    private float effectiveSeverity {
        get {
            float delta = targetValue - 50f;
            if (Mathf.Abs(delta) < 2f) return 0f; // Deadzone: 49–51

            float exponent = 2.5f;
            float maxSeverity = 19f;

            float normalized = Mathf.Abs(delta) / 50f; // [0,1]
            return 1f + Mathf.Pow(normalized, exponent) * maxSeverity; // [1,20]
        }
    }


    public override void Reset() {
        targetValue = 50f;
        if (gizmo != null) gizmo.targetValuePct = .5f;
        if (pawn.health.hediffSet.HasHediff(storeHediffDef)) pawn.health.RemoveHediff(storeHediff);
        if (pawn.health.hediffSet.HasHediff(tapHediffDef)) pawn.health.RemoveHediff(tapHediff);
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleFullFeruchemistTrait(pawn);
        MetalbornUtility.HandleFeruchemistTrait(pawn);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta)) return;

        // If we can't tap anymore, but we ARE tapping (and we aren't compounding), stop tapping
        if (!canTap && isTapping && !isCompounding) Reset();
        // If we can't store anymore, but we are storing (and we aren't compounding), stop storing
        if (!canStore && isStoring && !isCompounding) Reset();

        if (effectiveSeverity > 0f) {
            if (targetValue < 50 && canTap) {
                if (pawn.health.hediffSet.HasHediff(storeHediffDef)) pawn.health.RemoveHediff(storeHediff);

                pawn.health.GetOrAddHediff(tapHediffDef).Severity = effectiveSeverity;
            } else if (targetValue > 50 && canStore) {
                if (pawn.health.hediffSet.HasHediff(tapHediffDef)) pawn.health.RemoveHediff(tapHediff);

                pawn.health.GetOrAddHediff(storeHediffDef).Severity = effectiveSeverity;
            }
        }


        if (isStoring) {
            AddToStore(AmountPerRareTick * storeHediff.Severity);
        } else if (isTapping) {
            RemoveFromStore(AmountPerRareTick * tapHediff.Severity);
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
    }

    public bool AddToStore(float amount) {
        Metalmind? metalmind = metalminds.FirstOrDefault(x => x.canStore);
        if (metalmind == null) return false;

        metalmind.AddStored(amount);
        return true;
    }

    public bool RemoveFromStore(float amount) {
        Metalmind? metalmind = metalminds.FirstOrDefault(x => x.canTap);
        if (metalmind == null) return false;

        metalmind.ConsumeStored(amount);
        return true;
    }
}