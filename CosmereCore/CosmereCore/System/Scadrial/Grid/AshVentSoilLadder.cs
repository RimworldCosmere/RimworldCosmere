using System.Collections.Generic;

namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     The chain a vent walks the ground up, one rung at a time. Def names rather than defs, so
///     the rules hold without a game running; the comp resolves the pairs once and looks them up.
/// </summary>
public static class AshVentSoilLadder {
    /// <summary>Where the chain ends. Our own soil, richer than anything vanilla grows.</summary>
    public const string TopRung = "Cosmere_Scadrial_Terrain_VentSoil";

    /// <summary>Days a cell holds one rung. Sand to vent soil is four of them, so forty-eight.</summary>
    public const float DaysPerRung = 12f;

    /// <summary>
    ///     Steps the jitter is cut into. A rung split a thousand and twenty-four ways lands finer
    ///     than one sweep, and a power of two costs a mask rather than a modulo.
    /// </summary>
    private const int JitterSteps = 1024;

    private static readonly Dictionary<string, string> chain = new Dictionary<string, string> {
        { "SoftSand", "Sand" },
        { "Mud", "Sand" },
        { "Sand", "Gravel" },
        { "Gravel", "Soil" },
        { "Soil", "SoilRich" },
        { "MossyTerrain", "SoilRich" },
        { "MarshyTerrain", "SoilRich" },
        { "SoilRich", TopRung },
        { "Riverbank", TopRung },
    };

    /// <summary>Every rung, for a caller that has to resolve the names against a def database.</summary>
    public static IReadOnlyDictionary<string, string> Rungs => chain;

    /// <summary>
    ///     What this ground becomes next, or null when the vent has nothing to give it. Stone, ice,
    ///     water and terrain the mod has never heard of fall out here rather than default into it.
    /// </summary>
    public static string? NextRung(string? terrain) {
        if (terrain == null) return null;

        return chain.TryGetValue(terrain, out string? next) ? next : null;
    }

    /// <summary>
    ///     Days after the vent lit before its front covers a cell this far out. A cell's dwell runs
    ///     from there, so ground the vent has only just reached starts its first rung then.
    /// </summary>
    public static float WarmedAtDays(float distance, float clearRadius) {
        if (distance <= clearRadius) return 0f;

        return (distance - clearRadius) * AshVentSoilSpread.DaysPerCell;
    }

    /// <summary>
    ///     A cell's own place in the rung window. Without it a ring would come due on one tick and
    ///     swamp the sweep's ration, because the front and a rung both take twelve days.
    /// </summary>
    public static float JitterDays(int x, int z) {
        unchecked {
            int hash = (x * 73856093) ^ (z * 19349663);
            hash ^= hash >> 13;
            hash *= 1274126177;
            hash ^= hash >> 16;

            return (hash & (JitterSteps - 1)) * (DaysPerRung / JitterSteps);
        }
    }

    /// <summary>Whole rungs a cell has earned after dwelling this long, never fewer than none.</summary>
    public static int RungsClimbed(float dwellDays) {
        if (dwellDays < DaysPerRung) return 0;

        return (int)(dwellDays / DaysPerRung);
    }

    /// <summary>Whether a cell finishes a rung between two readings of the vent's clock.</summary>
    public static bool RungIsDue(
        int x, int z, float distance, float clearRadius, float daysBefore, float daysAfter
    ) {
        float warmed = WarmedAtDays(distance, clearRadius) + JitterDays(x, z);

        return RungsClimbed(daysAfter - warmed) > RungsClimbed(daysBefore - warmed);
    }
}
