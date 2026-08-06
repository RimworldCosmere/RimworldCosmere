using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.Def;

/// <summary>Which Shards a world starts with in one era.</summary>
public class EraShardSet {
    public EraDef? era;
    public List<ShardDef> shards = [];
}

/// <summary>
///     One Cosmere world. A save is set on exactly one of these, and it decides the active
///     Shards, the ancestry floor pawns are born with, which ambient content fires, and which
///     factions generate.
/// </summary>
public class CosmereWorldDef : Verse.Def {
    /// <summary>
    ///     Everything this world may ever have enabled, including Shards that conflict with each
    ///     other. Scadrial permits Ruin, Preservation and Harmony even though Harmony excludes
    ///     the other two.
    /// </summary>
    public List<ShardDef> nativeShards = [];

    /// <summary>
    ///     What actually gets switched on, per era. Kept separate from
    ///     <see cref="nativeShards" /> because enabling a conflicting set in sequence leaves
    ///     only the last one standing - Ruin, Preservation, Harmony collapses to Harmony alone.
    /// </summary>
    public List<EraShardSet> defaultShardsByEra = [];

    /// <summary>Used when the scenario names no era, or an era this world has no entry for.</summary>
    public List<ShardDef> fallbackShards = [];

    /// <summary>
    ///     Xenotypes born to this world. Read in reverse to find a pawn's ancestry floor. Not
    ///     usable forward - the forward direction carries spawn weights and an era split that a
    ///     flat list cannot hold.
    /// </summary>
    public List<XenotypeDef> xenotypes = [];

    /// <summary>
    ///     Names a climate profile for worldgen. Deliberately an opaque string: the profiles are
    ///     shard-side types and Core may not reference them.
    /// </summary>
    public string? climateProfile;

    /// <summary>
    ///     Which system skin colours this world's UI. Falls back to the defName, and the skin
    ///     registry falls back again to a neutral grey, so a world whose mod ships no skin still
    ///     renders rather than throwing.
    /// </summary>
    public string? skinId;

    /// <summary>The skin to theme this world with.</summary>
    public string SkinId => skinId ?? defName;

    /// <summary>
    ///     A world that stands for all of them. Takes its Shards from every other loaded world
    ///     rather than listing them, so it does not dangle when a shard mod is absent, and it
    ///     grants the ancestry floor for all of them so no system is out of reach.
    /// </summary>
    public bool crossWorld;

    public int listOrder;

    /// <summary>The Shards this world switches on for an era, falling back when it has no entry.</summary>
    public List<ShardDef> DefaultShardsFor(EraDef? era) {
        if (era != null) {
            for (int i = 0; i < defaultShardsByEra.Count; i++) {
                if (defaultShardsByEra[i].era == era) return defaultShardsByEra[i].shards;
            }
        }

        return fallbackShards;
    }

    public override IEnumerable<string> ConfigErrors() {
        foreach (string error in base.ConfigErrors()) {
            yield return error;
        }

        if (nativeShards.Count == 0 && !crossWorld) {
            yield return "nativeShards is empty, so this world can never enable anything.";
        }

        if (defaultShardsByEra.Count == 0 && fallbackShards.Count == 0 && !crossWorld) {
            yield return "neither defaultShardsByEra nor fallbackShards is set, so this world starts with no Shards.";
        }

        foreach (string error in CheckSet(fallbackShards, "fallbackShards")) {
            yield return error;
        }

        for (int i = 0; i < defaultShardsByEra.Count; i++) {
            EraShardSet set = defaultShardsByEra[i];
            string label = set.era == null ? $"defaultShardsByEra[{i}]" : $"defaultShardsByEra for {set.era.defName}";

            if (set.era == null) yield return $"{label} has no era.";

            foreach (string error in CheckSet(set.shards, label)) {
                yield return error;
            }
        }
    }

    /// <summary>
    ///     A default set has to be internally conflict-free. EnableShard disables everything in
    ///     mutuallyExclusiveWith before adding, so a conflicting set silently loses members at
    ///     load with nothing logged - which is exactly the bug this catches.
    /// </summary>
    private IEnumerable<string> CheckSet(List<ShardDef> shards, string label) {
        for (int i = 0; i < shards.Count; i++) {
            ShardDef shard = shards[i];

            if (!nativeShards.Contains(shard)) {
                yield return $"{label} names {shard.defName}, which is not in nativeShards.";
            }

            for (int j = i + 1; j < shards.Count; j++) {
                if (shard.mutuallyExclusiveWith.Contains(shards[j]) ||
                    shards[j].mutuallyExclusiveWith.Contains(shard)) {
                    yield return
                        $"{label} enables both {shard.defName} and {shards[j].defName}, which are " +
                        "mutually exclusive - enabling them in sequence would leave only one.";
                }
            }
        }
    }
}
