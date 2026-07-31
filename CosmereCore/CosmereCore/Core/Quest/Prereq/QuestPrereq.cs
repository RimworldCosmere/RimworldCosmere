namespace Cosmere.Core.Quest.Prereq;

/// <summary>
///     A condition that must hold before a quest may be offered. Evaluated against a
///     QuestWorldState so most prereqs stay Verse-free, but the base allows Verse access for
///     the pawn-scoped cases the Roshar arc needs.
/// </summary>
public abstract class QuestPrereq {
    public abstract bool IsMet(QuestWorldState state);

    /// <summary>Override to reject a def at load time. Return null when valid.</summary>
    public virtual string? ConfigError() {
        return null;
    }
}
