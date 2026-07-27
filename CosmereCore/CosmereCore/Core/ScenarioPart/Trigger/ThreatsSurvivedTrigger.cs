using Verse;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class ThreatsSurvivedTrigger : ProgressionTrigger {
    public int minThreats;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return (Find.StoryWatcher?.statsRecord?.numThreatBigs ?? 0) >= minThreats;
    }
}
