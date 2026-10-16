using Verse;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class ThreatsSurvivedTrigger : IProgressionTrigger {
    public int minThreats;

    public bool IsMet(GameComponent_ScenarioProgression comp) {
        return (Find.StoryWatcher?.statsRecord?.numThreatBigs ?? 0) >= minThreats;
    }
}
