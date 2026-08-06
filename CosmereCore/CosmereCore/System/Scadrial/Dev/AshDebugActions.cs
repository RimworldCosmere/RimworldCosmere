using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using LudeonTK;
using Verse;

namespace Cosmere.System.Scadrial.Dev;

/// <summary>Dev-only shortcuts so ash can be judged without waiting days for it to pile up.</summary>
public static class AshDebugActions {
    [DebugAction("Cosmere", "Ash: +250mm everywhere", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void AddAshShallow() {
        AddAsh(250);
    }

    [DebugAction("Cosmere", "Ash: +1200mm everywhere", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void AddAshDeep() {
        AddAsh(1200);
    }

    [DebugAction("Cosmere", "Ash: clear all", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ClearAsh() {
        Verse.Map map = Find.CurrentMap;
        AshDepthTracker? tracker = map?.GetComponent<AshDepthTracker>();
        if (tracker == null) return;

        tracker.Grid.Clear();
        map!.mapDrawer.RegenerateEverythingNow();
        map.GetComponent<AshOverlayDrawer>()?.SetDirty();
    }

    [DebugAction("Cosmere", "Ash: severity to max", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void MaxSeverity() {
        Find.CurrentMap?.GetComponent<AshDepthTracker>()?.SetSeverityNow(1f);
    }

    [DebugAction("Cosmere", "Ash: severity to baseline", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void BaselineSeverity() {
        Find.CurrentMap?.GetComponent<AshDepthTracker>()?.SetSeverityNow(AshDepthTracker.BaselineSeverity);
    }

    private static void AddAsh(int millimetres) {
        Verse.Map map = Find.CurrentMap;
        AshDepthTracker? tracker = map?.GetComponent<AshDepthTracker>();
        if (tracker == null) return;

        AshGrid grid = tracker.Grid;
        CellIndices indices = map!.cellIndices;
        for (int i = 0; i < indices.NumGridCells; i++) {
            if (grid.CanHaveAsh(indices.IndexToCell(i))) grid.AddDepthMm(i, millimetres);
        }

        map.mapDrawer.RegenerateEverythingNow();
        map.GetComponent<AshOverlayDrawer>()?.SetDirty();
    }
}
