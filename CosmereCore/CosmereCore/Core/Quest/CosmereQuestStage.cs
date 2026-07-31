using Cosmere.Core.Quest.Objective;

namespace Cosmere.Core.Quest;

/// <summary>One stage of a quest: a signal key and the objective that completes it.</summary>
public class CosmereQuestStage {
    public string? key;
    public QuestObjective? objective;
}
