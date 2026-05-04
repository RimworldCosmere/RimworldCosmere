using Verse;
using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Trigger;
using Cosmere.System.Roshar.Gene;

namespace Cosmere.System.Roshar.ScenarioPart;

public class PawnIdealTrigger : ProgressionTrigger {
    public int minIdeal;
    public string pawnName = "";

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) return false;

        Surgebinder? surgebinder =
            pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        return surgebinder != null && surgebinder.CurrentIdeal >= minIdeal;
    }
}
