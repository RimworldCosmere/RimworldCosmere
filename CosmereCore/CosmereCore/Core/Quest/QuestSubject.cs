namespace Cosmere.Core.Quest;

/// <summary>
///     Whether a quest belongs to the colony or to one specific pawn. Pawn-scoped quests
///     resolve a subject at offer time and thread it through every objective, reward and
///     outcome. Nothing in the Scadrial arc uses Pawn; the Roshar arc uses it heavily.
/// </summary>
public enum QuestSubject {
    Colony,
    Pawn,
}
