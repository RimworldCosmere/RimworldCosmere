using System.Collections.Generic;

namespace Cosmere.Core.Quest.Prereq;

/// <summary>Requires named Shards to be enabled. All of them, not any.</summary>
public class ShardPrereq : QuestPrereq {
    public List<string>? shards;

    public override bool IsMet(QuestWorldState state) {
        if (shards == null || shards.Count == 0) return true;

        for (int i = 0; i < shards.Count; i++) {
            if (!state.enabledShards.Contains(shards[i])) return false;
        }

        return true;
    }

    public override string? ConfigError() {
        return shards == null || shards.Count == 0 ? "ShardPrereq lists no shards." : null;
    }
}
