using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thought;

public class SprenMoodWorker : ThoughtWorker {
    protected override ThoughtState CurrentStateInternal(Pawn p) {
        if (p.TryGetComp<SprenBond>() != null) return ThoughtState.ActiveAtStage(0);
        return ThoughtState.Inactive;
    }
}
