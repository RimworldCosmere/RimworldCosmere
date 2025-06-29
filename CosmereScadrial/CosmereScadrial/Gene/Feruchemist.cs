using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Feruchemy.Comp.Thing;
using CosmereScadrial.Util;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Gene;

public class Feruchemist : Metalborn {
    private readonly float metalPerRareTick = .0333333f;

    public IEnumerable<Metalmind> metalminds => pawn.inventory.innerContainer.InnerListForReading
        .Where(t => t.HasComp<Metalmind>())
        .Select(t => t.TryGetComp<Metalmind>())
        .Where(c => c.metal == metal) ?? [];

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

    private float EffectiveSeverity {
        get {
            float delta = targetValue - 50f;
            if (Mathf.Abs(delta) < 1f) return 0f; // Deadzone from 49–51

            float exponent = 2.5f;
            float maxSeverity = 20f;
            float normalized = Mathf.Abs(delta) / 50f; // [0, 1]
            return Mathf.Pow(normalized, exponent) * maxSeverity; // [0, 20]
        }
    }

    public override void Reset() {
        targetValue = 50f;
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleFullFeruchemistTrait(pawn);
        MetalbornUtility.HandleFeruchemistTrait(pawn);
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta)) {
            Log.Warning($"Value={Value} Max={Max} targetValue={targetValue}");
        }

        bool canTap = metalminds.Any(x => x.canTap);
        bool canStore = metalminds.Any(x => x.canStore);

        if (!canTap && pawn.health.hediffSet.HasHediff(tapHediffDef)) {
            pawn.health.RemoveHediff(tapHediff);
            targetValue = 50;
        }

        if (!canStore && pawn.health.hediffSet.HasHediff(storeHediffDef)) {
            pawn.health.RemoveHediff(storeHediff);
            targetValue = 50;
        }


        if (EffectiveSeverity > 0f) {
            if (targetValue < 50 && canStore) {
                if (pawn.health.hediffSet.HasHediff(tapHediffDef)) {
                    pawn.health.RemoveHediff(tapHediff);
                }

                pawn.health.GetOrAddHediff(storeHediffDef).Severity = EffectiveSeverity;
            } else if (targetValue > 50 && canTap) {
                if (pawn.health.hediffSet.HasHediff(storeHediffDef)) {
                    pawn.health.RemoveHediff(storeHediff);
                }

                pawn.health.GetOrAddHediff(tapHediffDef).Severity = EffectiveSeverity;
            }
        }

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;
        if (pawn.health.hediffSet.HasHediff(storeHediffDef)) {
            metalminds.First(x => x.canStore).AddStored(metalPerRareTick * storeHediff.Severity);
        } else if (pawn.health.hediffSet.HasHediff(tapHediffDef)) {
            metalminds.First(x => x.canTap).ConsumeStored(metalPerRareTick * tapHediff.Severity);
        }
    }
}