namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     What a roofed vent does instead of drifting. The plume's whole mass goes into the room, so
///     room size is the dial: a closet buries in days and a hall takes a season. Pure and
///     Verse-free - it takes a cell count, never a Room.
/// </summary>
public static class AshRoomFill {
    /// <summary>
    ///     Widest room a vent will fill. Sat at the plume's own footprint so a room misread as
    ///     enclosed can never turn a bounded local write into a map scan.
    /// </summary>
    public static int MaxCells => AshPlume.CellCount;

    /// <summary>
    ///     Whether this room takes the plume rather than letting it out. Anything wider is a
    ///     cavern, and a vent in one goes on behaving as it does under open sky.
    /// </summary>
    public static bool HoldsThePlume(int cellCount, bool psychologicallyOutdoors) {
        if (psychologicallyOutdoors) return false;
        if (cellCount <= 0) return false;

        return cellCount <= MaxCells;
    }

    /// <summary>
    ///     One cell's share of a mouth's output. The mass is the same the plume would have spread
    ///     over open ground; only the ground it lands on has shrunk.
    /// </summary>
    public static float PerCellMm(float mouthMm, int cellCount) {
        if (cellCount <= 0) return 0f;

        return mouthMm * AshPlume.TotalWeight / cellCount;
    }
}
