using System.Collections.Generic;
using Cosmere.Core.Quest.Reward;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Gives every reward on the quest's def when inSignal fires. Reconstructs a
///     QuestBuildContext at fire time because the builder's context is a build-time object
///     that does not survive a save. rewardSeed is persisted rather than re-rolled, so a
///     reload cannot re-roll an already-resolved RolledReward.
/// </summary>
public class QuestPart_CosmereReward : QuestPart {
    public CosmereQuestDef def = null!;
    public string? inSignal;
    public Verse.Map map = null!;
    public int rewardSeed;
    public Pawn? subject;

    public override void Notify_QuestSignalReceived(Signal signal) {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != inSignal) return;

        List<QuestReward>? rewards = def.rewards;
        if (rewards == null || rewards.Count == 0) return;

        QuestBuildContext ctx = new QuestBuildContext {
            def = def,
            map = map,
            quest = quest,
            subject = subject,
            rewardSeed = rewardSeed,
        };

        int given = 0;
        for (int i = 0; i < rewards.Count; i++) {
            if (!QuestBranch.Matches(rewards[i].afterChoice, quest)) continue;

            rewards[i].Give(ctx);
            given++;
        }

        Logger.Info($"{def.defName}: gave {given} reward(s) on quest completion.");
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
