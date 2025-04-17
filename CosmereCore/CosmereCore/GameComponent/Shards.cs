using System.Collections.Generic;
using CosmereCore.Defs;
using Verse;

namespace CosmereCore.GameComponent {
    public class Shards : Verse.GameComponent {
        public HashSet<ShardDef> enabledShardDefs = new HashSet<ShardDef>();

        public Shards(Game game) { }

        public override void ExposeData() {
            Scribe_Collections.Look(ref enabledShardDefs, "enabledShardDefs", LookMode.Def);
        }

        public void EnableShard(string defName, bool allowConflicts = false) {
            var shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
            if (shard == null) return;

            EnableShard(shard, allowConflicts);
        }

        public void EnableShard(ShardDef shard, bool allowConflicts = false) {
            if (!allowConflicts) {
                foreach (var conflict in shard.mutuallyExclusiveWith) {
                    enabledShardDefs.Remove(conflict);
                }
            }

            enabledShardDefs.Add(shard);
        }

        public bool IsEnabled(string defName) {
            var shard = DefDatabase<ShardDef>.GetNamedSilentFail(defName);
            return shard != null && IsEnabled(shard);
        }

        public bool IsEnabled(ShardDef shard) {
            return enabledShardDefs.Contains(shard);
        }
    }
}