using Cosmere.Core;
using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Action;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.ScenarioPart;

public class AdvanceIdealAction : ProgressionAction {
    public string pawnName = string.Empty;
    public int targetIdeal = -1;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Log.Warn($"ScenarioProgression: Pawn '{pawnName}' not found for AdvanceIdeal");
            return;
        }

        Surgebinder? surgebinder =
            pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            Log.Warn($"ScenarioProgression: Pawn '{pawnName}' has no Surgebinder gene");
            return;
        }

        int target = targetIdeal >= 0 ? targetIdeal : surgebinder.CurrentIdeal + 1;
        if (target > 4) target = 4;

        surgebinder.CurrentIdeal = target;
    }
}
