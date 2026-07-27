namespace Cosmere.Core.ScenarioPart.Trigger;

public class EventOccurredTrigger : ProgressionTrigger {
    public string eventKey = string.Empty;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return comp.HasEventFired(eventKey);
    }
}
