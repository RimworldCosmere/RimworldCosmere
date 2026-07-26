
using Verse;
namespace Cosmere.Core.ScenarioPart.Trigger;

public class ResearchCompletedTrigger : ProgressionTrigger {
    public string research = "";

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        ResearchProjectDef? def = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(research);
        return def != null && def.IsFinished;
    }
}