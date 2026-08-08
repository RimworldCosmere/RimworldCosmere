namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     When a vent's ash flow has earned another lump of metal, and which metal it throws. Pure
///     and Verse-free so the cadence and the pick can be tuned and tested without a live vent.
/// </summary>
public static class AshMetalThrow {
    /// <summary>
    ///     Adds this tick's share of a throw to the carried remainder and returns the whole
    ///     throws now due, keeping the rest. Same shape as <see cref="AshPlume.Bank"/>, unit of 1
    ///     throw instead of a millimetre multiple.
    /// </summary>
    public static int Bank(ref float remainder, float throwsPerDay, float dayFraction) {
        remainder += throwsPerDay * dayFraction;
        if (remainder < 1f) return 0;

        int whole = (int)remainder;
        remainder -= whole;
        return whole;
    }

    /// <summary>
    ///     Which metal throw number <paramref name="throwOrdinal"/> drops, stable for a given
    ///     seed. Same murmur3 finalizer as <see cref="AshDepthMath.SettleDelayDays"/>: seed takes
    ///     the cell-index role, throwOrdinal the salt.
    /// </summary>
    public static int MetalIndex(int throwOrdinal, int seed, int metalCount) {
        uint hash = (uint)seed * 2654435761u;
        hash ^= (uint)throwOrdinal;
        hash ^= hash >> 15;
        hash *= 2246822519u;
        hash ^= hash >> 13;

        return (int)(hash % (uint)metalCount);
    }
}
