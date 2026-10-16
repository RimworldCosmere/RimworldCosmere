using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Trigger;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.ScenarioPart;

public class PawnIdealTrigger : IProgressionTrigger {
    public int minIdeal;
    public string pawnName = string.Empty;

    public bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = GameComponent_ScenarioProgression.FindPawnByName(pawnName);
        if (pawn == null) return false;

        Surgebinder? surgebinder =
            pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        return surgebinder != null && surgebinder.CurrentIdeal >= minIdeal;
    }
}
