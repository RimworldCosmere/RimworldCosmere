namespace Cosmere.Core.ScenarioPart;

public abstract class ProgressionTrigger {
    public abstract bool IsMet(GameComponent_ScenarioProgression comp);
}
