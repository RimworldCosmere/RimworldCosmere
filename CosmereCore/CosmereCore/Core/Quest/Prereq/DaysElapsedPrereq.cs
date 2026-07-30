namespace Cosmere.Core.Quest.Prereq;

/// <summary>Requires a minimum number of campaign days elapsed.</summary>
public class DaysElapsedPrereq : QuestPrereq {
    public int minDays = 1;

    public override bool IsMet(QuestWorldState state) {
        return state.daysElapsed >= minDays;
    }

    public override string? ConfigError() {
        return minDays < 0 ? "DaysElapsedPrereq minDays cannot be negative." : null;
    }
}
