using CosmereCore.Defs;
using Verse;

namespace CosmereCore.Utils
{
    public static class ShardUtility
    {
        public static GameComponent_CosmereShards Shards => Current.Game.GetComponent<GameComponent_CosmereShards>();

        public static bool IsEnabled(string shard)
        {
            return Shards.IsEnabled(shard);
        }

        public static bool IsEnabled(ShardDef shard)
        {
            return Shards.IsEnabled(shard);
        }

        public static void Enable(string shard)
        {
            Shards.EnableShard(shard);
        }

        public static void Enable(ShardDef shard)
        {
            Shards.EnableShard(shard);
        }
    }
}