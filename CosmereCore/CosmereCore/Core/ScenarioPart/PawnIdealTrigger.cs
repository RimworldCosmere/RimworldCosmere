using Verse;

namespace Cosmere.Core.ScenarioPart;

public class PawnIdealTrigger : ProgressionTrigger {
    public string pawnName = "";
    public int minIdeal;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) return false;

        Cosmere.System.Roshar.Gene.Surgebinder? surgebinder =
            pawn.genes?.GetFirstGeneOfType<Cosmere.System.Roshar.Gene.Surgebinder>();
        return surgebinder != null && surgebinder.currentIdeal >= minIdeal;
    }
}
