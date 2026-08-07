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
}
