using Cosmere.Core.Def;
using Cosmere.Core.Entity;
using Verse;

namespace Cosmere.Core.Comp.Game;

public class Shards : GameComponent {
    public HashSet<Shard> enabledShards = [];

    public Shards(Verse.Game game) { }

    public override void ExposeData() {
        Scribe_Collections.Look(ref enabledShards, "enabledShardDefs", LookMode.Def);
    }

    public void EnableShard(string defName, bool allowConflicts = false) {
        ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
        if (shard == null) return;

        EnableShard(shard, allowConflicts);
    }

    public void EnableShard(ShardDef shard, bool allowConflicts = false) {
        if (!allowConflicts) {
            foreach (ShardDef? conflict in shard.mutuallyExclusiveWith) {
                enabledShards.RemoveWhere(x => x.def.defName == conflict.defName);
            }
        }

        enabledShards.Add(new Shard(shard));
    }

    public bool IsEnabled(string defName) {
        ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
        return shard != null && IsEnabled(shard);
    }

    public bool IsEnabled(ShardDef shard) {
        return enabledShards.FirstOrDefault(x => x.def.defName == shard.defName) != null;
    }
}