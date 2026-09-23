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

    public static bool AreAnyEnabled(params string[] shardIds) {
        Shards? s = shards;
        if (s == null) return false;
        for (int i = 0; i < shardIds.Length; i++) {
            if (s.IsEnabled(shardIds[i])) return true;
        }

        return false;
    }

    public static bool AreAnyEnabled(params ShardDef[] shardDefs) {
        Shards? s = shards;
        if (s == null) return false;
        for (int i = 0; i < shardDefs.Length; i++) {
            if (s.IsEnabled(shardDefs[i])) return true;
        }

        return false;
    }

    public static bool AreAllEnabled(params string[] shardIds) {
        Shards? s = shards;
        if (s == null) return false;
        for (int i = 0; i < shardIds.Length; i++) {
            if (!s.IsEnabled(shardIds[i])) return false;
        }

        return true;
    }

    public static bool AreAllEnabled(params ShardDef[] shardDefs) {
        Shards? s = shards;
        if (s == null) return false;
        for (int i = 0; i < shardDefs.Length; i++) {
            if (!s.IsEnabled(shardDefs[i])) return false;
        }

        return true;
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
