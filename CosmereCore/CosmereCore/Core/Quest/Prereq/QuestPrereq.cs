namespace Cosmere.Core.Quest.Prereq;

/// <summary>
///     A condition that must hold before a quest may be offered. Evaluated against a
///     QuestWorldState so most prereqs stay Verse-free, but the base allows Verse access for
///     the pawn-scoped cases the Roshar arc needs.
/// </summary>
public abstract class QuestPrereq {
    public abstract bool IsMet(QuestWorldState state);

    /// <summary>Override to reject a def at load time. Return null when valid.</summary>
    /// <returns>A message naming what is wrong with the def, or null when it is valid. The base never rejects anything.</returns>
    public virtual string? ConfigError() {
        return null;
    }
}
