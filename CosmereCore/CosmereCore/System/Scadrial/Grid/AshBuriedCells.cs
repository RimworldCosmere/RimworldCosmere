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
}
