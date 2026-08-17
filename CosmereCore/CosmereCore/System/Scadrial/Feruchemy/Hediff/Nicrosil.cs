using Cosmere.Core;
using Cosmere.Core.Need;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Nicrosil : HediffWithComps {
    private bool isTapping => CompoundedTap.IsTap(def, HediffDefOf.Cosmere_Scadrial_Hediff_TapNicrosil);

    private bool isStoring => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_StoreNicrosil);

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
        }
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (investiture == null) return;

        Feruchemist? gene = nicrosil;
        if (gene == null) return;

        // Two hediffs can share this gene; only the one matching its net direction may drain it.
        bool matchesDirection = isStoring ? gene.TransferRatePerSecond > 0f : gene.TransferRatePerSecond < 0f;
        if (!matchesDirection) return;

        // Draining rather than reading stops a faster tick applying the same second twice.
        float moved = gene.DrainChargeMoved();
        if (Mathf.Approximately(moved, 0f)) return;

        investiture.CurLevel = Mathf.Clamp(
            investiture.CurLevel - moved * ScadrialMetallurgyConstants.NicrosilBeuPerCharge,
            0f,
            investiture.MaxLevel
        );
    }
}
