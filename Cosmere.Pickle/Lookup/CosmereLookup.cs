using System;
using RimWorks.Pickle;
using RimWorld;
using Verse;

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

    private static string DescribeColonists() {
        List<string> names = [.. PawnsFinder.AllMaps_FreeColonists
            .Select(p => p.Name?.ToStringShort ?? p.LabelShort)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)];

        return names.Count == 0 ? "(none)" : string.Join(", ", names);
    }
}
