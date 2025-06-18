using System.Collections.Generic;
using CosmereCore.Defs;
using Verse;

namespace CosmereCore.Comps.Game;

public class Shards : GameComponent {
    public HashSet<ShardDef> enabledShardDefs = new HashSet<ShardDef>();

    public Shards(Verse.Game game) { }

    public override void ExposeData() {
        Scribe_Collections.Look(ref enabledShardDefs, "enabledShardDefs", LookMode.Def);
    }

    public void EnableShard(string defName, bool allowConflicts = false) {
        ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
        if (shard == null) return;

        EnableShard(shard, allowConflicts);
    }

    public void EnableShard(ShardDef shard, bool allowConflicts = false) {
        if (!allowConflicts) {
            foreach (ShardDef? conflict in shard.mutuallyExclusiveWith) {
                enabledShardDefs.Remove(conflict);
            }
        }

        enabledShardDefs.Add(shard);
    }

    public bool IsEnabled(string defName) {
        ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
        return shard != null && IsEnabled(shard);
    }

    public bool IsEnabled(ShardDef shard) {
        return enabledShardDefs.Contains(shard);
    }
}