using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Verse;

namespace Cosmere.Core.Util;

public static class ShardUtility {
    public static Shards? shards => Current.Game?.GetComponent<Shards>();

    public static bool IsEnabled(ShardDef shard) {
        return AreAllEnabled(shard);
    }

    public static bool CachedAreAnyEnabled(ref bool? cache, params ShardDef[] shardDefs) {
        if (cache.HasValue) return cache.Value;
        try {
            bool result = AreAnyEnabled(shardDefs);
            cache = result;
            return result;
        } catch (Exception ex) {
            Logger.Verbose($"Shard check failed during init: {ex.Message}");
            return false;
        }
    }

    public static bool AreAnyEnabled(params string[] shardIds) {
        Shards? s = shards;
        return s != null && shardIds.Any(shardId => s.IsEnabled(shardId));
    }

    public static bool AreAnyEnabled(params ShardDef[] shardDefs) {
        Shards? s = shards;
        return s != null && shardDefs.Any(shard => s.IsEnabled(shard));
    }

    public static bool AreAllEnabled(params string[] shardIds) {
        Shards? s = shards;
        return s != null && shardIds.All(shardId => s.IsEnabled(shardId));
    }

    public static bool AreAllEnabled(params ShardDef[] shardDefs) {
        Shards? s = shards;
        return s != null && shardDefs.All(shard => s.IsEnabled(shard));
    }

    public static void Enable(string shard) {
        shards?.EnableShard(shard);
    }

    public static void Enable(ShardDef shard) {
        shards?.EnableShard(shard);
    }

    public static void Disable(string shard) {
        shards?.DisableShard(shard);
    }

    public static void Disable(ShardDef shard) {
        shards?.DisableShard(shard);
    }
}