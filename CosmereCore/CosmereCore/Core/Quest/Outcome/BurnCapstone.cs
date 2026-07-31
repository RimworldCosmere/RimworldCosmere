using Verse;

namespace Cosmere.Core.Quest.Outcome;

/// <summary>Marks the capstone burned for the whole campaign. It never re-offers.</summary>
public class BurnCapstone : QuestOutcome {
    public override void Resolve(QuestBuildContext ctx) {
        Current.Game?.GetComponent<CosmereQuestManager>()?.NotifyFailed(ctx.def.defName, null);
    }
}
