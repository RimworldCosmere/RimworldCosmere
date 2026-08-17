using Cosmere.Core;
using Cosmere.Core.Need;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Nicrosil : HediffWithComps {
    private Investiture? investiture => pawn?.needs?.TryGetNeed<Investiture>();

    private Feruchemist? nicrosil => pawn.genes?.GetFeruchemicGeneForMetal(MetalDefOf.Nicrosil);

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
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (investiture == null) return;

        Feruchemist? gene = nicrosil;
        if (gene == null) return;

        // Draining zeroes the shared accumulator, so a second hediff reading it this tick finds nothing.
        float moved = gene.DrainChargeMoved();
        if (Mathf.Approximately(moved, 0f)) return;

        investiture.CurLevel = Mathf.Clamp(
            investiture.CurLevel - moved * ScadrialMetallurgyConstants.NicrosilBeuPerCharge,
            0f,
            investiture.MaxLevel
        );
    }
}
