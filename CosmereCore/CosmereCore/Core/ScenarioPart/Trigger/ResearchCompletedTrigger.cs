using Verse;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class ResearchCompletedTrigger : IProgressionTrigger {
    public string research = string.Empty;

    public bool IsMet(GameComponent_ScenarioProgression comp) {
        ResearchProjectDef? def = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(research);
        return def != null && def.IsFinished;
    }
}
