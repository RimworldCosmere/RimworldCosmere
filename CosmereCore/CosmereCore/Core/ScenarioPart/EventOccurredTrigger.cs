namespace Cosmere.Core.ScenarioPart;

public class EventOccurredTrigger : ProgressionTrigger {
    public string eventKey = "";

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return comp.HasEventFired(eventKey);
    }
}