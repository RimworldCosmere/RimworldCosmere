namespace Cosmere.Core.Quest.Prereq;

/// <summary>Requires another capstone to be in a given state, usually Completed.</summary>
public class CapstoneStatePrereq : QuestPrereq {
    public string? questDefName;
    public CapstoneState requiredState = CapstoneState.Completed;

    public override bool IsMet(QuestWorldState state) {
        string? name = questDefName;
        return name != null && name.Length > 0 && state.StateOf(name) == requiredState;
    }

    public override string? ConfigError() {
        return string.IsNullOrEmpty(questDefName) ? "CapstoneStatePrereq has no questDefName." : null;
    }
}
