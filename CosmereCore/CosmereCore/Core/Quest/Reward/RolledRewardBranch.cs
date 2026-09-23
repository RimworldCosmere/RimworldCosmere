namespace Cosmere.Core.Quest.Reward;

/// <summary>One weighted branch of a RolledReward.</summary>
public class RolledRewardBranch {
    public string? key;
    public QuestReward? reward;
    public int weight;
}
