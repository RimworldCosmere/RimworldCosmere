using Cosmere.System.Roshar.Surgebinding.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Thought;

public class BondsmithCallingMoodWorker : ThoughtWorker {
    protected override ThoughtState CurrentStateInternal(Pawn p) {
        List<Verse.Hediff> hediffs = p.health?.hediffSet?.hediffs ?? [];
        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is BondsmithCalling) return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}
