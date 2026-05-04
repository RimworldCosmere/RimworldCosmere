namespace Cosmere.Core.ScenarioPart.Trigger;

public abstract class ProgressionTrigger {
    public abstract bool IsMet(GameComponent_ScenarioProgression comp);
}