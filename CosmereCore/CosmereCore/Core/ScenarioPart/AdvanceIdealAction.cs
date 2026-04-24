using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class AdvanceIdealAction : ProgressionAction {
    public string pawnName = "";
    public int targetIdeal = -1;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for AdvanceIdeal");
            return;
        }

        Surgebinder? surgebinder =
            pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' has no Surgebinder gene");
            return;
        }

        int target = targetIdeal >= 0 ? targetIdeal : surgebinder.currentIdeal + 1;
        if (target > 4) target = 4;

        surgebinder.currentIdeal = target;
    }
}