namespace Cosmere.Core.Quest.Reward;

/// <summary>
///     Something the player receives on quest completion. Resolved at completion time, never
///     at offer time, so the offer letter cannot leak a rolled result.
/// </summary>
public abstract class QuestReward {
    /// <summary>
    ///     Restricts this reward to one branch of a ChoiceObjective, named by that option's
    ///     key. Untagged rewards are given on every branch.
    /// </summary>
    public string? afterChoice;

    public abstract void Give(QuestBuildContext ctx);

    /// <summary>Player-facing summary for the quest tab. Must not reveal a rolled outcome.</summary>
    public abstract string Describe();

    public virtual string? ConfigError() {
        return null;
    }
}
