namespace Cosmere.Core.Quest.Prereq;

/// <summary>
///     Requires a named hediff on the subject pawn. Every Roshar godspren capstone triggers off
///     a BondsmithCalling hediff, which is what makes those quests pawn-scoped.
/// </summary>
public class HediffPrereq : QuestPrereq {
    public string? hediffDefName;

    public override bool IsMet(QuestWorldState state) {
        if (state.subjectPawnId == 0) return false;

        return !string.IsNullOrEmpty(hediffDefName) && state.subjectHediffs.Contains(hediffDefName!);
    }

    public override string? ConfigError() {
        return string.IsNullOrEmpty(hediffDefName) ? "HediffPrereq has no hediffDefName." : null;
    }
}
