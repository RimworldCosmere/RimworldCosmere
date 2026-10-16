using Verse;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class ColonyWealthTrigger : IProgressionTrigger {
    public float minWealth;

    public bool IsMet(GameComponent_ScenarioProgression comp) {
        Map? map = Find.CurrentMap;
        return map != null && map.wealthWatcher.WealthTotal >= minWealth;
    }
}
