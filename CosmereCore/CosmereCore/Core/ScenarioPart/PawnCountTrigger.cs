using Verse;

namespace Cosmere.Core.ScenarioPart;

public class PawnCountTrigger : ProgressionTrigger {
    public int minCount;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Map? map = Find.CurrentMap;
        return map != null && map.mapPawns.FreeColonistsCount >= minCount;
    }
}