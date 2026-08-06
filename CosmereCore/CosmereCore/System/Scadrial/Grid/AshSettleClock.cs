using Verse;

namespace Cosmere.System.Scadrial.Grid;

/// <summary>
///     How long each cell has left before its terrain changes hands, in either direction. Per
///     cell rather than global so the map turns over in patches over weeks, which is what deep
///     ash actually looks like - a shared timer reads as a wave crossing the map.
/// </summary>
public class AshSettleClock : IExposable {
    private readonly Verse.Map map;

    /// <summary>Absolute game day the cell may change, plus one. Zero means no clock is running.</summary>
    private ushort[]? due;

    public AshSettleClock(Verse.Map map) {
        this.map = map;
    }

    /// <summary>Cells counting down. Zero lets the array go.</summary>
    public int PendingCount { get; private set; }

    /// <summary>Starts the wait on first sight of the cell, then reports when it is up.</summary>
    public bool IsDue(int index, int today, bool settling) {
        due ??= new ushort[map.cellIndices.NumGridCells];

        if (due[index] == 0) {
            due[index] = (ushort)(today + AshDepthMath.SettleDelayDays(index, settling) + 1);
            PendingCount++;
            return false;
        }

        return today + 1 >= due[index];
    }

    /// <summary>
    ///     Called whenever the cell stops wanting to change. A drift that crested 1100mm and got
    ///     shovelled back down must not settle three weeks later at ankle depth, and a cell
    ///     reburied while it was waiting to revert has to stay ash.
    /// </summary>
    public void Cancel(int index) {
        if (due == null || due[index] == 0) return;

        due[index] = 0;
        PendingCount--;
        if (PendingCount == 0) due = null;
    }

    /// <summary>Dev shortcut, so verifying the swap does not mean skipping a month.</summary>
    public void ExpireAll() {
        due = null;
        PendingCount = 0;
    }

    public void ExposeData() {
        bool any = PendingCount > 0;
        Scribe_Values.Look(ref any, "ashSettleAny", false);

        if (!any) {
            if (Scribe.mode == LoadSaveMode.LoadingVars) {
                due = null;
                PendingCount = 0;
            }

            return;
        }

        MapExposeUtility.ExposeUshort(map, ReadCell, WriteCell, "ashSettleDue");

        if (Scribe.mode == LoadSaveMode.PostLoadInit) Recount();
    }

    private ushort ReadCell(IntVec3 cell) {
        return due == null ? (ushort)0 : due[map.cellIndices.CellToIndex(cell)];
    }

    private void WriteCell(IntVec3 cell, ushort value) {
        if (value == 0) return;

        due ??= new ushort[map.cellIndices.NumGridCells];
        due[map.cellIndices.CellToIndex(cell)] = value;
    }

    private void Recount() {
        PendingCount = 0;
        if (due == null) return;

        for (int i = 0; i < due.Length; i++) {
            if (due[i] != 0) PendingCount++;
        }

        if (PendingCount == 0) due = null;
    }
}
