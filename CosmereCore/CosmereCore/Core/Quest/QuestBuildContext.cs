using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     Everything a prereq, objective, reward or outcome needs about the quest it belongs to.
///     Passed by the builder so those workers never reach into global state themselves.
/// </summary>
public class QuestBuildContext {
    public CosmereQuestDef def = null!;

    /// <summary>The map the quest targets. Never null for a quest that reached the builder.</summary>
    public Verse.Map map = null!;

    /// <summary>The quest being built or resolved.</summary>
    public RimWorld.Quest quest = null!;

    /// <summary>Seed for any rolled reward. Assigned once at completion so a reload cannot re-roll.</summary>
    public int rewardSeed;

    /// <summary>
    ///     The subject pawn for a pawn-scoped quest. Null when def.subject is Colony. Nothing
    ///     in the Scadrial arc sets this; the Roshar arc sets it on every capstone.
    /// </summary>
    public Pawn? subject;
}
