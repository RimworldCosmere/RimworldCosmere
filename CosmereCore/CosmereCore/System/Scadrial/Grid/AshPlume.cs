using System;

namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     How much of a vent's output lands on a cell, and how much of it the vent keeps off its own
///     doorstep. Pure and Verse-free so the shape can be tested; the comp only walks the offsets.
/// </summary>
public static class AshPlume {
    /// <summary>Cells from the vent the plume reaches. Bounds the per-vent write.</summary>
    public const int RadiusCells = 12;

    /// <summary>Softens the inverse square so the vent cell is not absurdly deeper than its neighbour.</summary>
    private const float Softening = 2f;

    private static int cellCount = -1;
    private static float totalWeight = -1f;

    /// <summary>
    ///     Whether an offset falls inside the disc. The comp's cell list, the cell count and the
    ///     weights all ask this, so none of the three can drift away from the other two.
    /// </summary>
    public static bool InRange(int offsetX, int offsetZ) {
        return offsetX * offsetX + offsetZ * offsetZ <= RadiusCells * RadiusCells;
    }

    /// <summary>Cells one vent writes in a cycle. The bound a roofed vent has to stay inside too.</summary>
    public static int CellCount {
        get {
            if (cellCount < 0) Measure();

            return cellCount;
        }
    }

    /// <summary>
    ///     Every cell's weight added up, wind neutral. A vent's whole output in millimetre-cells is
    ///     its mouth rate times this, which is the mass a roofed vent has to put somewhere else.
    /// </summary>
    public static float TotalWeight {
        get {
            if (cellCount < 0) Measure();

            return totalWeight;
        }
    }

    /// <summary>
    ///     1 at the vent, thinning to about 0.014 on the boundary ring and 0 past it. Skew runs 0
    ///     to 1 and leans the plume along the heading, which gives a map a downwind side.
    /// </summary>
    public static float Weight(int offsetX, int offsetZ, float headingX, float headingZ, float skew) {
        if (!InRange(offsetX, offsetZ)) return 0f;

        float distanceSquared = offsetX * offsetX + offsetZ * offsetZ;
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
    ///     The deepest a cell this close to a vent is allowed to stay. 0 on the mouth itself,
    ///     rising to waist deep at the feather's edge and leaving anything past it alone.
    /// </summary>
    public static int AllowedDepthMm(int depthMm, float distance, float radius) {
        if (distance <= 0f) return 0;
        if (distance >= radius) return depthMm;

        int allowed = CeilingMm(distance, radius);
        return depthMm < allowed ? depthMm : allowed;
    }

    /// <summary>
    ///     Waist deep at the feather's edge, not the grid cap. The burial wash saturates its alpha
    ///     there and the movement bucket tops out there, so a higher ceiling moves nothing visible.
    /// </summary>
    private static int CeilingMm(float distance, float radius) {
        return (int)(distance / radius * AshDepthMath.WaistMm);
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

    /// <summary>Walks the disc once so neither the count nor the mass is paid for again.</summary>
    private static void Measure() {
        int cells = 0;
        float weight = 0f;

        for (int x = -RadiusCells; x <= RadiusCells; x++) {
            for (int z = -RadiusCells; z <= RadiusCells; z++) {
                if (!InRange(x, z)) continue;

                cells++;
                weight += Weight(x, z, 1f, 0f, 0f);
            }
        }

        totalWeight = weight;
        cellCount = cells;
    }
}
