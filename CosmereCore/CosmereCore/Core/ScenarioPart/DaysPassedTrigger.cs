using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class DaysPassedTrigger : ProgressionTrigger {
    public int days;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return GenDate.DaysPassed >= days;
    }
}
