using System;
using CosmereCore.Need;
using CosmereResources;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Feruchemy.Hediff;

public abstract class Nicrosil : Verse.Hediff {
    protected readonly bool isStoring;

    protected readonly bool isTapping;

    protected Nicrosil(bool tapping, bool storing) {
        isTapping = tapping;
        isStoring = storing;
    }

    protected Investiture? investiture => pawn?.needs?.TryGetNeed<Investiture>();
    protected Feruchemist? nicrosil => pawn.genes.GetFeruchemicGeneForMetal(MetalDefOf.Nicrosil);

    protected float changePerTick => Feruchemist.AmountPerRareTick * Severity * (isTapping ? -1 : 1);

    // There's a little bit of a race condition here that i'm not 100% sure how to fix.
    // Continuing to store/tap nicrosil will slowly increase how much investiture you have 
    private bool shouldResetNicrosil {
        get {
            float nextCurLevel = Mathf.Clamp(investiture!.CurLevel - changePerTick, 0, investiture.MaxLevel);
            if (isTapping) {
                if (!nicrosil!.canTap || nextCurLevel >= investiture.MaxLevel) return true;
            }

            if (isStoring) {
                if (!nicrosil!.canStore || nextCurLevel <= 0) return true;
            }

            return false;
        }
    }

    public override void PostMake() {
        base.PostMake();

        if (investiture == null) throw new Exception("Nicrosil can't be on a pawn that doesnt have investiture");
        if (nicrosil == null) throw new Exception("Nicrosil can't be on a pawn that doesnt have the Nicrosil gene");

        if (shouldResetNicrosil) nicrosil.Reset();
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        investiture!.CurLevel = Mathf.Clamp(investiture.CurLevel - changePerTick, 0, investiture.MaxLevel);
        if (shouldResetNicrosil) {
            nicrosil!.Reset();
        }
    }
}