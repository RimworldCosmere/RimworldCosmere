using System;

namespace Cosmere.Core.Quest;

/// <summary>
///     Thrown by an objective, reward or outcome that cannot resolve. CosmereQuestBuilder
///     catches it, discards the half-built quest and returns false, so a partially built
///     quest never reaches Find.QuestManager.
/// </summary>
public class QuestBuildFailure : Exception {
    public QuestBuildFailure(string reason) : base(reason) { }
}
