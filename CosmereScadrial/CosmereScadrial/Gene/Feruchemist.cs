using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Extension;
using CosmereScadrial.Feruchemy.Comp.Thing;
using CosmereScadrial.Gizmo;
using CosmereScadrial.Util;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gene;

public class Feruchemist : Metalborn {
    public static readonly float AmountPerRareTick = 1 / 18f;
    private new FeruchemicGeneCommand? gizmo => (FeruchemicGeneCommand)base.gizmo;

    public List<Metalmind> metalminds => pawn.inventory.innerContainer.InnerListForReading
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
        DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Tap" + metal.LabelCap);

    private Hediff tapHediff => pawn.health.hediffSet.GetFirstHediffOfDef(tapHediffDef);

    private HediffDef storeHediffDef =>
        DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Store" + metal.LabelCap);

    private Hediff storeHediff => pawn.health.hediffSet.GetFirstHediffOfDef(storeHediffDef);

    public bool canTap => metalminds.Any(x => x.canTap);
    public bool canStore => metalminds.Any(x => x.canStore);

    private float effectiveSeverity {
        get {
            float delta = targetValue - 50f;
            if (Mathf.Abs(delta) < 1f) return 0f; // Deadzone from 49–51

            float exponent = 2.5f;
            float maxSeverity = 19f;
            float normalized = Mathf.Abs(delta) / 50f; // [0, 1]
            return 1 + Mathf.Pow(normalized, exponent) * maxSeverity; // [1, 20]
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
        if (!canTap && pawn.health.hediffSet.HasHediff(tapHediffDef) && !pawn.IsCompounding(metal)) Reset();
        // If we can't store anymore, but we are storing (and we aren't compounding), stop storing
        if (!canStore && pawn.health.hediffSet.HasHediff(storeHediffDef) && !pawn.IsCompounding(metal)) Reset();

        if (effectiveSeverity > 0f) {
            if (targetValue < 50 && canStore) {
                if (pawn.health.hediffSet.HasHediff(tapHediffDef)) pawn.health.RemoveHediff(tapHediff);

                pawn.health.GetOrAddHediff(storeHediffDef).Severity = effectiveSeverity;
            } else if (targetValue > 50 && canTap) {
                if (pawn.health.hediffSet.HasHediff(storeHediffDef)) pawn.health.RemoveHediff(storeHediff);

                pawn.health.GetOrAddHediff(tapHediffDef).Severity = effectiveSeverity;
            }
        }

        if (pawn.health.hediffSet.HasHediff(storeHediffDef)) {
            AddToStore(AmountPerRareTick * storeHediff.Severity);
        } else if (pawn.health.hediffSet.HasHediff(tapHediffDef)) {
            RemoveFromStore(AmountPerRareTick * tapHediff.Severity);
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