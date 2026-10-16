namespace Cosmere.Core.Quest.Outcome;

/// <summary>What happens to persistent state when a quest fails.</summary>
public abstract class QuestOutcome {
    public abstract void Resolve(QuestBuildContext ctx);

    public virtual string? ConfigError() {
        return null;
    }
}
