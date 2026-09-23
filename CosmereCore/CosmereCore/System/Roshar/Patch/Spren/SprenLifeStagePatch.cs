using Concord;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch]
public abstract class SprenLifeStagePatch : LifeStageWorker_HumanlikeAdult {
    [Inject(At.Head, nameof(Notify_LifeStageStarted))]
    private Control BeforeNotify_LifeStageStarted(Pawn pawn) {
        return pawn.def.HasComp(typeof(SprenBond)) ? Control.Cancel : Control.Continue;
    }
}
