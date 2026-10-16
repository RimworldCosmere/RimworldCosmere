using Concord;
using Cosmere.System.Roshar.Gene;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class MentalBreakTrackingPatch : MentalState {
    [Inject(At.Return, nameof(RecoverFromState))]
    private void AfterRecoverFromState() {
        Pawn statePawn = pawn;
        if (statePawn == null || statePawn.Dead) return;

        Surgebinder? surgebinder = statePawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        surgebinder.OnMentalBreakSurvived();
    }
}
