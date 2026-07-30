using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     Tracks quest failure state so QuestOutcome workers can record burned capstones and
///     goodwill history. This is a stub: it exists only so BurnCapstone and BurnForPawn
///     (Task 14) have something to call and the build stays green. Task 15 fills in the
///     persisted dictionaries, BuildWorldState, PickWeighted, and everything else this
///     component actually needs to do. Do not add members here outside of Task 15.
/// </summary>
public class CosmereQuestManager : GameComponent {
    public CosmereQuestManager(Verse.Game game) { }

    public void NotifyFailed(string defName, Pawn? subject) { }
}
