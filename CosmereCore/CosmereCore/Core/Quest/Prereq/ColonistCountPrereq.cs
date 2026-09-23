namespace Cosmere.Core.Quest.Prereq;

/// <summary>Requires a minimum number of free colonists.</summary>
public class ColonistCountPrereq : QuestPrereq {
    public int minCount = 1;

    public override bool IsMet(QuestWorldState state) {
        return state.freeColonistCount >= minCount;
    }

    public override string? ConfigError() {
        return minCount < 1 ? "ColonistCountPrereq minCount must be at least 1." : null;
    }
}
