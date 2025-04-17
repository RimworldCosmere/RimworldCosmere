using CosmereCore.Defs;
using CosmereCore.GameComponent;
using Verse;

namespace CosmereCore.Utils {
    public static class ShardUtility {
        public static Shards shards => Current.Game.GetComponent<Shards>();

        public static bool IsEnabled(string shard) {
            return shards.IsEnabled(shard);
        }

        public static bool IsEnabled(ShardDef shard) {
            return shards.IsEnabled(shard);
        }

        public static void Enable(string shard) {
            shards.EnableShard(shard);
        }

        public static void Enable(ShardDef shard) {
            shards.EnableShard(shard);
        }
    }
}