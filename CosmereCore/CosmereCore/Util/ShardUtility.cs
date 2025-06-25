using System.Linq;
using CosmereCore.Comp.Game;
using CosmereCore.Def;
using Verse;

namespace CosmereCore.Util;

public static class ShardUtility {
    public static Shards shards => Current.Game.GetComponent<Shards>();

    public static bool IsEnabled(ShardDef shard) {
        return AreAllEnabled(shard);
    }

    public static bool AreAnyEnabled(params string[] shardIds) {
        return shardIds.Any(shardId => shards.IsEnabled(shardId));
    }

    public static bool AreAnyEnabled(params ShardDef[] shardDefs) {
        return shardDefs.Any(shard => shards.IsEnabled(shard));
    }

    public static bool AreAllEnabled(params string[] shardIds) {
        return shardIds.All(shardId => shards.IsEnabled(shardId));
    }

    public static bool AreAllEnabled(params ShardDef[] shardDefs) {
        return shardDefs.All(shard => shards.IsEnabled(shard));
    }

    public static void Enable(string shard) {
        shards.EnableShard(shard);
    }

    public static void Enable(ShardDef shard) {
        shards.EnableShard(shard);
    }
}