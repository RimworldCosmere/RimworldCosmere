using System.Collections.Generic;
using Verse;

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
            Log.Error($"RolledReward on {ctx.def?.defName} rolled nothing.");
            return;
        }

        for (int i = 0; i < branches.Count; i++) {
            if (branches[i].key != picked) continue;
            branches[i].reward?.Give(ctx);
            Log.Info($"RolledReward on {ctx.def?.defName} resolved to '{picked}'.");
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
        List<string> keys = new List<string>();
        for (int i = 0; i < branches.Count; i++) {
            RolledRewardBranch branch = branches[i];
            string? key = branch.key;

            if (branch.reward == null) return $"RolledReward branch '{key}' has no reward.";
            if (key == null || key.Length == 0) return $"RolledReward branch {i} has no key.";
            if (branch.weight <= 0) return $"RolledReward branch '{key}' weight must be positive.";

            entries.Add(new RewardTableEntry { key = key, weight = branch.weight });
            keys.Add(key);
        }

        HashSet<string> seenKeys = new HashSet<string>();
        for (int i = 0; i < keys.Count; i++) {
            if (!seenKeys.Add(keys[i])) return $"RolledReward has duplicate branch key '{keys[i]}'.";
        }

        int total = RewardTable.TotalWeight(entries);
        if (total != 100) return $"RolledReward weights must sum to exactly 100, got {total}.";

        return null;
    }
}
