namespace Cosmere.Core.ScenarioPart.Trigger;

public class EventOccurredTrigger : ProgressionTrigger {
    public string eventKey = string.Empty;

    /// <summary>
    ///     Inverts it: fires only while the named event has NOT happened. Lets an arc tell the
    ///     difference between a campaign that lived through the previous one and a campaign that
    ///     started here, which is the whole difference between recapping and repeating.
    /// </summary>
    public bool negate;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return comp.HasEventFired(eventKey) != negate;
    }
}
