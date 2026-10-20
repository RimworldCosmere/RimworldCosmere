namespace Cosmere.Core.ScenarioPart.Trigger;

public interface IProgressionTrigger {
    bool IsMet(GameComponent_ScenarioProgression comp);
}
