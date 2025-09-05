using Cosmere.Core.Need;
using Cosmere.Resources;
using Cosmere.Scadrial.Gene;
using UnityEngine;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Scadrial.Feruchemy.Hediff;

public class Nicrosil : HediffWithComps {
    private bool isTapping => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_TapNicrosil);
    private bool isStoring => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_StoreNicrosil);
    private Investiture? investiture => pawn?.needs?.TryGetNeed<Investiture>();
    private Feruchemist? nicrosil => pawn.genes.GetFeruchemicGeneForMetal(MetalDefOf.Nicrosil);

    private float changePerTick => Feruchemist.AmountPerRareTick * Severity * (isTapping ? -1 : 1);

    // There's a little bit of a race condition here that i'm not 100% sure how to fix.
    // Continuing to store/tap nicrosil will slowly increase how much investiture you have 
    private bool shouldResetNicrosil {
        get {
            float nextCurLevel = Mathf.Clamp(investiture!.CurLevel - changePerTick, 0, investiture.MaxLevel);
            if (isTapping && nextCurLevel >= Investiture.MaxInvestiture) return true;

            return isStoring && nextCurLevel <= 0;
        }
    }

    public override void PostMake() {
        base.PostMake();

        if (investiture == null) {
            Logger.Error("CS_Error_MissingRequirement".Translate("Nicrosil", "investiture"));
            pawn.health.RemoveHediff(this);
            return;
        }

        if (nicrosil == null) {
            Logger.Error("CS_Error_MissingRequirement".Translate("Nicrosil", "the Nicrosil gene"));
            pawn.health.RemoveHediff(this);
            return;
        }

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