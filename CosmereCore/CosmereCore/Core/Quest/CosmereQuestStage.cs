using Cosmere.Core.Quest.Objective;

namespace Cosmere.Core.Quest;

/// <summary>One stage of a quest: a signal key and the objective that completes it.</summary>
public class CosmereQuestStage {
    /// <summary>
    ///     Restricts this stage to one branch of an earlier ChoiceObjective, named by that
    ///     option's key. On any other branch the stage passes straight through.
    /// </summary>
    public string? afterChoice;

    /// <summary>
    ///     Language key for the line shown in the Quests tab when this stage starts. Vanilla
    ///     keeps each line once its stage has begun, so they read as a growing record.
    /// </summary>
    public string? descriptionKey;

    public string? key;
    public QuestObjective? objective;
}
