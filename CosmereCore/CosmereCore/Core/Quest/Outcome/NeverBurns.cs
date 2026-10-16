namespace Cosmere.Core.Quest.Outcome;

/// <summary>
///     Does nothing, deliberately. A capstone that genuinely cannot fail declares this so
///     the def states intent, rather than omitting onFailure and leaving a reader to guess
///     whether that was an oversight. CosmereQuestDef.ConfigErrors rejects a capstone with
///     no onFailure for exactly that reason.
/// </summary>
public class NeverBurns : QuestOutcome {
    public override void Resolve(QuestBuildContext ctx) { }
}
