using RimWorld;

namespace Cosmere.Core.ScenarioPart.Trigger;

public class DaysPassedTrigger : ProgressionTrigger {
    public int days;

    /// <summary>
    ///     Counts from when this arc took over rather than from the start of the campaign. Any
    ///     arc reached by a handoff wants this: a campaign that arrives at day 75 has already
    ///     passed every absolute threshold, so the whole arc would fire in one tick.
    /// </summary>
    public bool sinceArcStart;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return (sinceArcStart ? comp.DaysInActiveArc : GenDate.DaysPassed) >= days;
    }
}
