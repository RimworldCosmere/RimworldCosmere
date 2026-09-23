using System;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Cosmere.Core.ShardConnection;
using Cosmere.Core.Util;
using RimWorks.Pickle;
using RimWorld;
using Verse;
using InvestitureHolder = Cosmere.Core.Comp.Thing.InvestitureHolder;

namespace Cosmere.Pickle.Lookup;

/// <summary>Lookups every Cosmere step class shares: pawns by the name a feature file
/// uses, Cosmere defs by name, and an assertion that reports what it found.</summary>
public static class CosmereLookup {
    /// <summary>Finds a player pawn by nickname, the same match the built-in
    /// <c>a colonist {string} exists</c> step makes.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name, as the feature file writes it.</param>
    /// <returns>The living player pawn with that nickname.</returns>
    public static Pawn RequirePawn(PickleContext ctx, string nickname) {
        Pawn? pawn = PawnsFinder.AllMaps_FreeColonists
            .FirstOrDefault(p => string.Equals(p.Name?.ToStringShort, nickname, StringComparison.OrdinalIgnoreCase));

        if (pawn == null) {
            ctx.Require(false, $"no pawn nicknamed '{nickname}'. player pawns present: {DescribeColonists()}");
        }

        return pawn!;
    }

    /// <summary>Finds a def by name, failing with the names that are loaded.</summary>
    /// <typeparam name="T">The def type to search.</typeparam>
    /// <param name="defName">The def name.</param>
    /// <returns>The def.</returns>
    public static T RequireDef<T>(string defName)
        where T : Verse.Def {
        T? def = DefDatabase<T>.GetNamedSilentFail(defName);
        if (def != null) {
            return def;
        }

        throw new InvalidOperationException(
            $"no {typeof(T).Name} named '{defName}'. loaded: {DescribeAll<T>()}");
    }

    /// <summary>Asserts a condition, adding what was actually found to the message.
    /// The description is built only when the assertion fails.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="condition">The condition that must hold.</param>
    /// <param name="wanted">What the step expected, phrased as "x should be y".</param>
    /// <param name="describeActual">Builds the description of what was found.</param>
    public static void AssertThat(PickleContext ctx, bool condition, string wanted, Func<string> describeActual) {
        ctx.Assert(condition, condition ? wanted : $"{wanted}; {describeActual()}");
    }

    /// <summary>Lists every loaded def name of a type, for a failure message.</summary>
    /// <typeparam name="T">The def type to list.</typeparam>
    /// <returns>The def names, comma separated.</returns>
    public static string DescribeAll<T>()
        where T : Verse.Def {
        List<string> names = [.. DefDatabase<T>.AllDefsListForReading
            .Select(d => d.defName)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)];

        return names.Count == 0 ? "(none)" : string.Join(", ", names);
    }

    /// <summary>The Shards component, which only exists inside a running game.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <returns>The component.</returns>
    public static Shards RequireShards(PickleContext ctx) {
        Shards? shards = ShardUtility.shards;

        ctx.Require(
            shards != null,
            "no game is loaded, so this cosmere has no Shards yet. tag the feature " +
            "@quickstart:<Name> or load a save fixture before a shard step runs");

        return shards!;
    }

    /// <summary>Finds a thing at a cell and hands back the Investiture it holds.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="defName">The thing def to look for.</param>
    /// <param name="x">The cell's x.</param>
    /// <param name="z">The cell's z.</param>
    /// <returns>The thing's holder comp.</returns>
    public static InvestitureHolder RequireThingHolder(PickleContext ctx, string defName, int x, int z) {
        Map? map = Find.CurrentMap;
        ctx.Require(map != null, "no current map; tag the feature with @quickstart: or load a save first");

        ThingDef def = RequireDef<ThingDef>(defName);
        IntVec3 cell = new IntVec3(x, 0, z);
        ctx.Require(
            cell.InBounds(map),
            $"cell ({x}, {z}) is outside the map, which is {map!.Size.x} by {map.Size.z}");

        Verse.Thing? thing = cell.GetThingList(map).FirstOrDefault(t => t.def == def);
        ctx.Require(thing != null, $"no {defName} at ({x}, {z}); the cell holds: {DescribeCell(map, cell)}");

        InvestitureHolder? holder = thing!.TryGetComp<InvestitureHolder>();
        ctx.Require(
            holder != null,
            $"the {defName} at ({x}, {z}) has no InvestitureHolder comp, so it cannot hold investiture at all");

        return holder!;
    }

    /// <summary>How close two banked figures have to be to count as equal. Flat breaks on a
    /// large reserve and relative breaks near zero, so this takes whichever is looser.</summary>
    /// <param name="expected">The figure a step asked for.</param>
    /// <returns>The slack allowed either side of it.</returns>
    public static float Tolerance(float expected) {
        return Math.Max(0.01f, Math.Abs(expected) * 0.001f);
    }

    /// <summary>Whether two banked figures are equal within <see cref="Tolerance" />.</summary>
    /// <param name="actual">The figure read back.</param>
    /// <param name="expected">The figure a step asked for.</param>
    /// <returns>True when they match.</returns>
    public static bool IsNear(float actual, float expected) {
        return Math.Abs(actual - expected) <= Tolerance(expected);
    }

    /// <summary>Lists the Shards holding this cosmere, for a failure message.</summary>
    /// <param name="shards">The Shards component.</param>
    /// <returns>The enabled Shard names, sorted.</returns>
    public static string DescribeShards(Shards shards) {
        return shards.enabledShards.Count == 0
            ? "no shard holds this cosmere"
            : $"shards on: {string.Join(", ", shards.enabledShards.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))}";
    }

    /// <summary>Describes what a pawn reads toward one Shard, part by part.</summary>
    /// <param name="pawn">The pawn to read.</param>
    /// <param name="shard">The Shard to read against.</param>
    /// <returns>The total and every part it was built from.</returns>
    // Three of the four parts are recomputed per read, so the parts say why a total is wrong.
    public static string DescribeConnection(Pawn pawn, ShardDef shard) {
        ConnectionBreakdown parts = ConnectionUtility.BreakdownFor(pawn, shard);

        return $"{pawn.Name?.ToStringShort ?? pawn.LabelShort} reads {parts.Total} toward " +
            $"{shard.defName} ({ConnectionMath.TierOf(parts.Total)}): ancestry={parts.Ancestry} " +
            $"residence={parts.Residence} investiture={parts.Investiture} earned={parts.Earned} " +
            $"held={parts.Held} harmony={parts.Harmony}";
    }

    /// <summary>Describes what Investiture a holder carries.</summary>
    /// <param name="holder">The holder comp.</param>
    /// <returns>Its own reserve, the stack total, and its decay.</returns>
    // self is what a step sets and reads back; total adds the stack and anything held inside.
    public static string DescribeHolder(InvestitureHolder holder) {
        return $"self={holder.currentInvestitureSelf:0.###} max={holder.maxInvestitureSelf:0.###} " +
            $"total={holder.currentInvestiture:0.###} of {holder.maxInvestiture:0.###} " +
            $"stack={holder.parent.stackCount} decay={holder.drainRate:0.####} per rare tick";
    }

    /// <summary>Lists what sits in a cell, for a failure message.</summary>
    /// <param name="map">The map the cell is on.</param>
    /// <param name="cell">The cell to read.</param>
    /// <returns>The thing def names in it.</returns>
    public static string DescribeCell(Map map, IntVec3 cell) {
        List<string> labels = [.. cell.GetThingList(map).Select(t => t.def.defName)];
        return labels.Count == 0 ? "(nothing)" : string.Join(", ", labels);
    }

    /// <summary>Lists the abilities a pawn holds right now, for a failure message.</summary>
    /// <param name="pawn">The pawn to read.</param>
    /// <returns>The ability def names, sorted.</returns>
    public static string DescribeHeldAbilities(Pawn pawn) {
        List<Ability>? held = pawn.abilities?.AllAbilitiesForReading;

        return held == null || held.Count == 0
            ? "the pawn holds no abilities"
            : $"the pawn holds: {string.Join(", ", held.Select(a => a.def.defName).OrderBy(n => n, StringComparer.OrdinalIgnoreCase))}";
    }

    private static string DescribeColonists() {
        List<string> names = [.. PawnsFinder.AllMaps_FreeColonists
            .Select(p => p.Name?.ToStringShort ?? p.LabelShort)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)];

        return names.Count == 0 ? "(none)" : string.Join(", ", names);
    }
}
