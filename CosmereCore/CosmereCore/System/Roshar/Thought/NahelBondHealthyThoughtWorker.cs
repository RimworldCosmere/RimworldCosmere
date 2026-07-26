using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thought;

public class NahelBondHealthyThoughtWorker : ThoughtWorker {
    protected override ThoughtState CurrentStateInternal(Pawn p) {
        Surgebinder? surgebinder = p.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return ThoughtState.Inactive;

        ILoadReferenceable? bondTarget = surgebinder.GetBondTarget();
        if (bondTarget == null) return ThoughtState.Inactive;

        float connection = SpiritWeb.Instance?.GetConnectionValue(p, bondTarget) ?? 0f;
        if (connection >= 0.7f) return ThoughtState.ActiveAtStage(0);

        return ThoughtState.Inactive;
    }
}