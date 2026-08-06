using System.Collections.Generic;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using RimWorld;
using Verse;

namespace Cosmere.Core.Util;

/// <summary>
///     Reading and setting the world a save is on.
/// </summary>
/// <remarks>
///     Deliberately has no cached variant. The shard equivalent used to cache and never
///     invalidate, so three MapComponents latched a stale answer and one of them disagreed with
///     itself. Reading the component is cheap; do not add a cache back.
/// </remarks>
public static class WorldUtility {
    public static CosmereWorld? component => Verse.Current.Game?.GetComponent<CosmereWorld>();

    /// <summary>The save's world, or null before one has been chosen.</summary>
    public static CosmereWorldDef? Primary => component?.primary;

    /// <summary>Every world whose mod is loaded, in author-declared order.</summary>
    public static List<CosmereWorldDef> All {
        get {
            List<CosmereWorldDef> worlds = [..DefDatabase<CosmereWorldDef>.AllDefsListForReading];
            worlds.SortBy(w => w.listOrder, w => w.defName);
            return worlds;
        }
    }

    public static void Set(CosmereWorldDef? world) {
        CosmereWorld? comp = component;
        if (comp == null) {
            Logger.Warning("WorldUtility: no CosmereWorld component, cannot set the world.");
            return;
        }

        comp.primary = world;
        Logger.Important(
            $"World set to {world?.defName ?? "none"} " +
            $"(scenario '{Verse.Current.Game?.Scenario?.name ?? "none"}')."
        );
    }

    /// <summary>Whether <paramref name="world" /> is the one this save runs on.</summary>
    public static bool IsPrimary(CosmereWorldDef? world) {
        return world != null && Primary == world;
    }

    /// <summary>
    ///     Whether content belonging to <paramref name="world" /> should be running. True on a
    ///     cross-world save for every loaded world, which is what lets those scenarios reach
    ///     every system.
    /// </summary>
    public static bool IsActive(CosmereWorldDef? world) {
        if (world == null) return false;

        CosmereWorldDef? current = Primary;
        if (current == null) return true;

        return current == world || current.crossWorld;
    }

    /// <summary>
    ///     Whether a phenomenon should occur: you must be on its world, and at least one Shard
    ///     it belongs to must exist.
    /// </summary>
    /// <remarks>
    ///     Both halves matter and they answer different questions. Highstorms do not happen on
    ///     Scadrial because they are Roshar's; they do not happen at all without Honor, because
    ///     they are Honor's. Mists are Preservation's, on Scadrial. Gating on only one of the two
    ///     gets a save where Honor is switched off but the storms still roll in.
    /// </remarks>
    public static bool IsActive(CosmereWorldDef? world, params ShardDef[] anyOf) {
        return IsActive(world) && ShardUtility.AreAnyEnabled(anyOf);
    }

    /// <summary>The world a xenotype is born to, for the ancestry floor. Null when it is nobody's.</summary>
    public static CosmereWorldDef? WorldForXenotype(XenotypeDef? xenotype) {
        if (xenotype == null) return null;

        List<CosmereWorldDef> worlds = DefDatabase<CosmereWorldDef>.AllDefsListForReading;
        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) continue;
            if (worlds[i].xenotypes.Contains(xenotype)) return worlds[i];
        }

        return null;
    }

    /// <summary>
    ///     The world a faction belongs to. Null for vanilla and DLC factions, which belong to no
    ///     shardworld.
    /// </summary>
    public static CosmereWorldDef? WorldForFaction(FactionDef? faction) {
        return WorldForDefName(faction?.defName);
    }

    /// <summary>
    ///     The world a def belongs to, read out of its own defName. Null when it names none.
    /// </summary>
    /// <remarks>
    ///     Everything we ship is named Cosmere_&lt;World&gt;_&lt;Kind&gt;_*, and the world's own
    ///     defName is that middle token, so this needs no per-def bookkeeping and a new shardworld
    ///     costs no code. A hand-kept list would fail silently: miss one entry and that def
    ///     disappears from its own world with nothing logged. The convention is covered by tests
    ///     instead.
    ///     <para>
    ///         Naming a def without a world token is meaningful rather than a mistake - it says
    ///         the def belongs to every world.
    ///     </para>
    /// </remarks>
    public static CosmereWorldDef? WorldForDefName(string? defName) {
        if (string.IsNullOrEmpty(defName)) return null;

        List<CosmereWorldDef> worlds = DefDatabase<CosmereWorldDef>.AllDefsListForReading;
        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) continue;
            if (defName!.Contains(worlds[i].defName)) return worlds[i];
        }

        return null;
    }

    /// <summary>
    ///     The Shards a pawn born to this save gets an ancestry floor for. A cross-world save
    ///     gathers from every loaded world rather than listing them, so it does not dangle when
    ///     a shard mod is absent.
    /// </summary>
    public static List<ShardDef> AncestryShards(CosmereWorldDef? world) {
        if (world == null) return [];
        if (!world.crossWorld) return world.DefaultShardsFor(null);

        List<ShardDef> all = [];
        List<CosmereWorldDef> worlds = DefDatabase<CosmereWorldDef>.AllDefsListForReading;
        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) continue;
            for (int j = 0; j < worlds[i].nativeShards.Count; j++) {
                if (!all.Contains(worlds[i].nativeShards[j])) all.Add(worlds[i].nativeShards[j]);
            }
        }

        return all;
    }

    /// <summary>
    ///     What a world switches on at game start. A cross-world save takes each other world's
    ///     own default set, which keeps every set internally conflict-free.
    /// </summary>
    public static List<ShardDef> StartingShards(CosmereWorldDef? world, EraDef? era) {
        if (world == null) return [];
        if (!world.crossWorld) return world.DefaultShardsFor(era);

        List<ShardDef> all = [];
        List<CosmereWorldDef> worlds = DefDatabase<CosmereWorldDef>.AllDefsListForReading;
        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) continue;
            List<ShardDef> defaults = worlds[i].DefaultShardsFor(era);
            for (int j = 0; j < defaults.Count; j++) {
                if (!all.Contains(defaults[j])) all.Add(defaults[j]);
            }
        }

        return all;
    }

    /// <summary>
    ///     Works out which world the running scenario is set on and commits it. Must be called
    ///     before WorldGenerator.GenerateWorld - a WorldGenStep that reads the world during
    ///     generation, such as Scadrial's Ashmounts, sees null otherwise.
    /// </summary>
    /// <remarks>
    ///     The page path commits when the player picks. The two page-less paths - Quickstarter
    ///     and VanillaQuicktest - both set their Shards *after* generating the world, so they
    ///     have to call this themselves at the right point.
    /// </remarks>
    public static CosmereWorldDef? SeedFromScenario() {
        List<CosmereWorldDef> worlds = All;
        if (worlds.Count == 0) return null;

        // Falling back to the sentinel, never to worlds[0]. A scenario that names no Shards -
        // vanilla's own Crashlanded, say, which the quick-test path uses - belongs to no
        // particular world, and picking the first in list order silently made it Scadrial.
        CosmereWorldDef? inferred = InferFromScenario(worlds) ?? Sentinel(worlds) ?? worlds[0];
        Set(inferred);
        return inferred;
    }

    private static CosmereWorldDef? Sentinel(List<CosmereWorldDef> worlds) {
        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) return worlds[i];
        }

        return null;
    }

    /// <summary>
    ///     The world that permits every Shard the scenario declares. A set spanning two worlds
    ///     matches only the cross-world sentinel, which is what those scenarios want.
    /// </summary>
    public static CosmereWorldDef? InferFromScenario(List<CosmereWorldDef>? worlds = null) {
        worlds ??= All;

        List<string>? declared = ScenarioDefUtility.CurrentShards?.shards;
        if (declared is not { Count: > 0 }) return null;

        CosmereWorldDef? sentinel = null;

        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) {
                sentinel = worlds[i];
                continue;
            }

            bool all = true;
            for (int j = 0; j < declared.Count; j++) {
                if (!Permits(worlds[i], declared[j])) {
                    all = false;
                    break;
                }
            }

            if (all) return worlds[i];
        }

        return sentinel;
    }

    private static bool Permits(CosmereWorldDef world, string shardName) {
        for (int i = 0; i < world.nativeShards.Count; i++) {
            if (world.nativeShards[i].defName == shardName) return true;
        }

        return false;
    }
}
