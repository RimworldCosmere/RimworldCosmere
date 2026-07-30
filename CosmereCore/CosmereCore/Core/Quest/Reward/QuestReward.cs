namespace Cosmere.Core.Quest.Reward;

/// <summary>
///     Something the player receives on quest completion. Resolved at completion time, never
///     at offer time, so the offer letter cannot leak a rolled result.
/// </summary>
public abstract class QuestReward {
    public abstract void Give(QuestBuildContext ctx);

    /// <summary>Player-facing summary for the quest tab. Must not reveal a rolled outcome.</summary>
    public abstract string Describe();

    public virtual string? ConfigError() {
        return null;
    }
}
