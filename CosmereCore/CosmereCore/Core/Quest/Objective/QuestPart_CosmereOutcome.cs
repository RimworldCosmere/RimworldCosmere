using Cosmere.Core.Quest.Outcome;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Resolves the quest's onFailure outcome when inSignal fires. Reconstructs a
///     QuestBuildContext at fire time for the same reason QuestPart_CosmereReward does, and
///     persists the same fields so the reconstructed context matches what the reward part
///     would have used had the quest succeeded instead.
/// </summary>
public class QuestPart_CosmereOutcome : QuestPart {
    public CosmereQuestDef def = null!;
    public string? inSignal;
    public Verse.Map map = null!;
    public int rewardSeed;
    public Pawn? subject;

    public override void Notify_QuestSignalReceived(Signal signal) {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != inSignal) return;

        QuestOutcome? outcome = def.onFailure;
        if (outcome == null) return;

        QuestBuildContext ctx = new QuestBuildContext {
            def = def,
            map = map,
            quest = quest,
            subject = subject,
            rewardSeed = rewardSeed,
        };

        outcome.Resolve(ctx);
        Log.Info($"{def.defName}: resolved failure outcome.");
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref def, "def");
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref map, "map");
        Scribe_Values.Look(ref rewardSeed, "rewardSeed");
        Scribe_References.Look(ref subject, "subject");
    }
}
