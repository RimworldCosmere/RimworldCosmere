using System.Collections.Generic;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest.Reward;

/// <summary>
///     Picks one nested reward by weight. The roll happens in Give, which the builder calls
///     at completion, so the offer letter cannot leak the result and reloading before
///     accepting changes nothing.
/// </summary>
public class RolledReward : QuestReward {
    public List<RolledRewardBranch>? branches;

    public override void Give(QuestBuildContext ctx) {
        if (branches == null || branches.Count == 0) return;

        List<RewardTableEntry> entries = new List<RewardTableEntry>();
        for (int i = 0; i < branches.Count; i++) {
            entries.Add(new RewardTableEntry { key = branches[i].key, weight = branches[i].weight });
        }

        string? picked = RewardTable.Roll(entries, ctx.rewardSeed);
        if (picked == null) {
            Logger.Error($"RolledReward on {ctx.def?.defName} rolled nothing.");
            return;
        }

        for (int i = 0; i < branches.Count; i++) {
            if (branches[i].key != picked) continue;
            branches[i].reward?.Give(ctx);
            Logger.Info($"RolledReward on {ctx.def?.defName} resolved to '{picked}'.");
            return;
        }
    }

    /// <summary>
    ///     Deliberately vague. Naming the branches here would put the whole table in the
    ///     offer letter, which is the leak this design exists to prevent.
    /// </summary>
    public override string Describe() {
        return "CC_Quest_Reward_Rolled".Translate().Resolve();
    }

    public override string? ConfigError() {
        if (branches == null || branches.Count == 0) return "RolledReward has no branches.";

        List<RewardTableEntry> entries = new List<RewardTableEntry>();
        for (int i = 0; i < branches.Count; i++) {
            if (branches[i].reward == null) return $"RolledReward branch '{branches[i].key}' has no reward.";
            entries.Add(new RewardTableEntry { key = branches[i].key, weight = branches[i].weight });
        }

        if (!RewardTable.IsValid(entries)) {
            return $"RolledReward weights must sum to exactly 100, got {RewardTable.TotalWeight(entries)}.";
        }

        return null;
    }
}
