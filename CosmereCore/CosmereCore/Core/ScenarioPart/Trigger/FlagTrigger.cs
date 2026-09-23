using Cosmere.Core.Quest;

namespace Cosmere.Core.ScenarioPart.Trigger;

/// <summary>
///     Met once a quest has set a flag. The bridge from the quest system back into scenario
///     progression: a quest earns the flag, and a later story beat unlocks because of it.
/// </summary>
public class FlagTrigger : ProgressionTrigger {
    public string flag = string.Empty;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return flag.Length > 0 && QuestFlagStore.HasFlag(flag);
    }
}
