using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest.Outcome;

/// <summary>
///     Marks the capstone burned for the subject pawn only. Other colonists stay eligible.
///     Nothing in the Scadrial arc uses this; the Roshar Nightwatcher capstone does.
/// </summary>
public class BurnForPawn : QuestOutcome {
    public override void Resolve(QuestBuildContext ctx) {
        if (ctx.subject == null) {
            Logger.Warning($"BurnForPawn on {ctx.def?.defName} had no subject pawn. Nothing was burned.");
            return;
        }

        Current.Game?.GetComponent<CosmereQuestManager>()?.NotifyFailed(ctx.def.defName, ctx.subject);
    }
}
