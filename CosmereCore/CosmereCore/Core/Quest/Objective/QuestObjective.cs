namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     One stage's goal. Produces the vanilla QuestParts that implement it, so the builder
///     stays the only place that knows how a Quest is assembled.
/// </summary>
public abstract class QuestObjective {
    /// <summary>
    ///     Add whatever QuestParts implement this objective to the quest. inSignal is the
    ///     signal that starts this stage; outSignal is what the objective raises on success.
    /// </summary>
    public abstract void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx);

    public virtual string? ConfigError() {
        return null;
    }
}
