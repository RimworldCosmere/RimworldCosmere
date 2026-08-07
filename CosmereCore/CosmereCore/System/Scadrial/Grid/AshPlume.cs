using System;

namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     How much of a vent's output lands on a cell. Pure and Verse-free so the shape can be
///     tested; the comp only walks the offsets and multiplies.
/// </summary>
public static class AshPlume {
    /// <summary>Cells from the vent the plume reaches. Bounds the per-vent write.</summary>
    public const int RadiusCells = 12;

    /// <summary>Softens the inverse square so the vent cell is not absurdly deeper than its neighbour.</summary>
    private const float Softening = 2f;

    /// <summary>
    ///     1 at the vent, falling to 0 at the radius. Skew runs 0 to 1 and leans the plume along
    ///     the heading, which is what gives a map a downwind side instead of a tidy circle.
    /// </summary>
    public static float Weight(int offsetX, int offsetZ, float headingX, float headingZ, float skew) {
        float distanceSquared = offsetX * offsetX + offsetZ * offsetZ;
        if (distanceSquared > RadiusCells * RadiusCells) return 0f;

        float falloff = Softening / (Softening + distanceSquared);

        float distance = (float)Math.Sqrt(distanceSquared);
        if (distance < 0.001f) return falloff;

        // Dot of the offset direction against the heading: 1 straight downwind, -1 straight up.
        float alignment = (offsetX * headingX + offsetZ * headingZ) / distance;
        float lean = 1f + skew * alignment;
        if (lean < 0f) lean = 0f;

        float weight = falloff * lean;
        return weight > 1f ? 1f : weight;
    }

    /// <summary>
    ///     Adds to a cell's carried remainder and returns the whole units to deposit, keeping the
    ///     rest. Depositing without banking loses everything under one unit, which starves the
    ///     plume's tail completely.
    /// </summary>
    public static int Bank(ref float remainder, float millimetres, int unitMm) {
        remainder += millimetres;
        if (remainder < unitMm) return 0;

        int deposit = (int)(remainder / unitMm) * unitMm;
        remainder -= deposit;
        return deposit;
    }

    /// <summary>
    ///     Rebuilds a saved bank, discarding one whose length no longer matches the offset list.
    ///     A stale length would index past the array inside the tick loop; losing fractions will not.
    /// </summary>
    public static float[]? RestoreBank(List<float>? saved, int cellCount) {
        if (saved == null || saved.Count != cellCount) return null;

        return saved.ToArray();
    }
}
