using Verse;

namespace Cosmere.Core.ScenarioPart;

public class PawnAliveTrigger : ProgressionTrigger {
    public string pawnName = "";
    public bool alive = true;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        bool isAlive = pawn != null && !pawn.Dead;
        return alive == isAlive;
    }
}
