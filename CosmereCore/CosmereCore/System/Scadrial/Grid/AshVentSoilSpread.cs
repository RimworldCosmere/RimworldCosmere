namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     How far a vent has fed the ground around it, and how fast that front moves. Pure and
///     Verse-free so the rate can be tested; the comp banks the radius and walks the disc.
/// </summary>
public static class AshVentSoilSpread {
    /// <summary>Cells the front reaches before it stops. A mod setting overrides it.</summary>
    public const float DefaultReachCells = 8f;

    /// <summary>Zero still feeds the mouth itself, which is ground the vent physically holds.</summary>
    public const float MinReachCells = 0f;

    /// <summary>
    ///     Ceiling on the setting. At 16 one vent walks a 34x34 rect a cycle, which six of them
    ///     can still afford on one tick in 64; the whole ash sweep walks 977 cells every tick.
    /// </summary>
    public const float MaxReachCells = 16f;

    /// <summary>
    ///     Days the front takes to advance one cell. Twelve puts 8 new cells on the outer ring in
    ///     a day and 60 in a quadrum, and carries the clearing out to the default 8 in a year.
    /// </summary>
    public const float DaysPerCell = 8f;

    /// <summary>
    ///     Where the front stands after this cycle. The vent always holds its own clearing, so a
    ///     fresh one starts there rather than at nothing, and a lowered setting pulls it back in.
    /// </summary>
    public static float Advance(float front, float clearRadius, float reach, float days) {
        if (reach < 0f) reach = 0f;

        float floor = clearRadius < reach ? clearRadius : reach;
        if (front < floor) front = floor;

        front += days / DaysPerCell;
        return front > reach ? reach : front;
    }

    /// <summary>Whether the front covers a cell this far out. Squared, to keep the walk off Sqrt.</summary>
    public static bool Reaches(int distanceSquared, float front) {
        return distanceSquared <= front * front;
    }
}
