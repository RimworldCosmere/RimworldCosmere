using Verse;

namespace Cosmere.Core.ScenarioPart;

public class ColonyWealthTrigger : ProgressionTrigger {
    public float minWealth;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Map? map = Find.CurrentMap;
        return map != null && map.wealthWatcher.WealthTotal >= minWealth;
    }
}
