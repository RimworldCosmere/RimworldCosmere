using Verse;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class PawnAliveTrigger : ProgressionTrigger {
    public bool alive = true;
    public string pawnName = string.Empty;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        bool isAlive = pawn != null && !pawn.Dead;
        return alive == isAlive;
    }
}
