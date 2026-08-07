using System;

namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     Which cells hold enough ash to swallow what is on them. Burial has hysteresis, so this
///     remembers the last answer per cell rather than recomputing from depth alone.
/// </summary>
public class AshBuriedCells {
    private readonly bool[] buried;

    public AshBuriedCells(int cellCount) {
        buried = new bool[cellCount];
    }

    /// <summary>
    ///     Rebuilds the set off a saved array. A length that does not match the map is dropped
    ///     rather than indexed - the sweep recomputes the whole grid within 64 ticks.
    /// </summary>
    public static AshBuriedCells Restore(bool[]? saved, int cellCount) {
        AshBuriedCells cells = new AshBuriedCells(cellCount);
        if (saved == null || saved.Length != cellCount) return cells;

        for (int i = 0; i < cellCount; i++) {
            cells.Set(i, saved[i]);
        }

        return cells;
    }

    public int Count { get; private set; }

    public bool Any => Count > 0;

    public bool[] Raw => buried;

    public bool IsBuried(int index) {
        if (index < 0 || index >= buried.Length) return false;
        return buried[index];
    }

    public bool Set(int index, bool value) {
        if (index < 0 || index >= buried.Length) return false;
        if (buried[index] == value) return false;
        buried[index] = value;
        Count += value ? 1 : -1;
        return true;
    }

    /// <summary>Unburies everything at once, for the dev action that empties the grid outright.</summary>
    public void Clear() {
        if (Count == 0) return;

        Array.Clear(buried, 0, buried.Length);
        Count = 0;
    }
}
