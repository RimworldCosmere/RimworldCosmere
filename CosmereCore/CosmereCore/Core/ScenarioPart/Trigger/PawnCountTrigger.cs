using Verse;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class PawnCountTrigger : IProgressionTrigger {
    public int minCount;

    public bool IsMet(GameComponent_ScenarioProgression comp) {
        Map? map = Find.CurrentMap;
        return map != null && map.mapPawns.FreeColonistsCount >= minCount;
    }
}
