using Verse;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class PawnAliveTrigger : IProgressionTrigger {
    public bool alive = true;
    public string pawnName = string.Empty;

    public bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = GameComponent_ScenarioProgression.FindPawnByName(pawnName);
        bool isAlive = pawn != null && !pawn.Dead;
        return alive == isAlive;
    }
}
