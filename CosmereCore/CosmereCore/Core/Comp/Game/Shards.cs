using Cosmere.Core.Def;
using Cosmere.Core.Entity;
using Verse;

namespace Cosmere.Core.Comp.Game;

public class Shards : GameComponent {
    public HashSet<Shard> enabledShards = [];
    private List<ShardDef> savedShardDefs = [];

    public Shards(Verse.Game game) { }

    public override void ExposeData() {
        if (Scribe.mode == LoadSaveMode.Saving) {
            savedShardDefs = enabledShards.Select(s => s.def).ToList();
        }

        Scribe_Collections.Look(ref savedShardDefs, "enabledShardDefs", LookMode.Def);
        savedShardDefs ??= [];

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            enabledShards = [];
            for (int i = 0; i < savedShardDefs.Count; i++) {
                if (savedShardDefs[i] != null) {
                    enabledShards.Add(new Shard(savedShardDefs[i]));
                }
            }
        }
    }

    public void EnableShard(string defName, bool allowConflicts = false) {
        ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
        if (shard == null) return;

        EnableShard(shard, allowConflicts);
    }

    public void EnableShard(ShardDef shard, bool allowConflicts = false) {
        if (!allowConflicts) {
            foreach (ShardDef conflict in shard.mutuallyExclusiveWith) {
                DisableShard(conflict);
            }
        }

        enabledShards.Add(new Shard(shard));
    }

    public void DisableShard(string defName) {
        ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
        if (shard == null) return;
        DisableShard(shard);
    }

    public void DisableShard(ShardDef shard) {
        enabledShards.RemoveWhere(x => x.def.defName == shard.defName);
    }

    public bool IsEnabled(string defName) {
        ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
        return shard != null && IsEnabled(shard);
    }

    public bool IsEnabled(ShardDef shard) {
        return enabledShards.FirstOrDefault(x => x.def.defName == shard.defName) != null;
    }
}
