using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class ResearchCompletedTrigger : ProgressionTrigger {
    public string research = "";

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        ResearchProjectDef? def = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(research);
        return def != null && def.IsFinished;
    }
}
