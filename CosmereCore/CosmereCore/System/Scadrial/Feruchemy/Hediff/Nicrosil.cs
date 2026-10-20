using Cosmere.Core;
using Cosmere.Core.Need;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

/// <summary>
///     The gene applies the Investiture mirror, since it ticks even without this hediff alive. This
///     class only handles the readout and its preconditions.
/// </summary>
public class Nicrosil : HediffWithComps {
    private Investiture? Investiture => pawn?.needs?.TryGetNeed<Investiture>();

    private Feruchemist? FeruchemicGene => pawn.genes?.GetFeruchemicGeneForMetal(MetalDefOf.Nicrosil);

    public override void PostMake() {
        base.PostMake();

        if (Investiture == null) {
            Log.Error("CS_Error_MissingRequirement".Translate("Nicrosil", "investiture"));
            pawn.health.RemoveHediff(this);
            return;
        }

        if (FeruchemicGene == null) {
            Log.Error("CS_Error_MissingRequirement".Translate("Nicrosil", "the Nicrosil gene"));
            pawn.health.RemoveHediff(this);
        }
    }
}
