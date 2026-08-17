using Cosmere.Core;
using Cosmere.Core.Need;
using Cosmere.System.Scadrial.Gene;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

// The gene applies the Investiture mirror, because it ticks whether or not a hediff is
// alive to see the charge move. What is left here is the readout and its preconditions.
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
        }
    }
}
