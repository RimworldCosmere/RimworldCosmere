using System.Collections.Generic;
using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Comp.Thing;
using Cosmere.System.Scadrial.Grid;
using LudeonTK;
using RimWorld;
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

        // The regenerate below happens now. The sweep that would unbury these does not run paused.
        tracker.Buried.Clear();
        map!.mapDrawer.RegenerateEverythingNow();
        map.GetComponent<AshOverlayDrawer>()?.SetDirty();
    }

    /// <summary>
    ///     A vent throws every second day on its own, which is half an hour of watching. Nothing
    ///     here is skipped - it is the same ThrowOnce the plume calls, landing rules and all.
    /// </summary>
    [DebugAction("Cosmere", "Ash: throw metal from every vent", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ThrowMetal() {
        AshDepthTracker? tracker = Find.CurrentMap?.GetComponent<AshDepthTracker>();
        if (tracker == null) return;

        IReadOnlyList<CompAshVent> vents = tracker.Vents;
        int landed = 0;
        for (int i = 0; i < vents.Count; i++) {
            if (vents[i].ThrowOnce()) landed++;
        }

        Messages.Message($"{landed} of {vents.Count} vents landed metal.", MessageTypeDefOf.NeutralEvent, false);
    }

    [DebugAction("Cosmere", "Ash: severity to max", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void MaxSeverity() {
        Find.CurrentMap?.GetComponent<AshDepthTracker>()?.SetSeverityNow(1f);
    }

    [DebugAction("Cosmere", "Ash: severity to baseline", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void BaselineSeverity() {
        Find.CurrentMap?.GetComponent<AshDepthTracker>()?.SetSeverityNow(AshDepthTracker.BaselineSeverity);
    }

    [DebugAction("Cosmere", "Ash: settle terrain now", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void SettleTerrain() {
        Verse.Map map = Find.CurrentMap;
        AshDepthTracker? tracker = map?.GetComponent<AshDepthTracker>();
        if (tracker == null) return;

        // The live sweep rations itself to eight changes a tick, which is minutes of watching on
        // a full map. This runs the same pass with the ration off.
        int swapped = tracker.RunTerrainSweepNow();
        Messages.Message($"{swapped} cells standing as deep ash.", MessageTypeDefOf.NeutralEvent, false);
    }

    /// <summary>
    ///     Standing in gas long enough to reach a late stage is most of a day per stage, so the
    ///     stages get read here instead. Severity 0 removes it the way receding to zero would.
    /// </summary>
    [DebugAction(
        "Cosmere",
        "Ash: set ash lung",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    private static void SetAshLung(Pawn pawn) {
        HediffDef? def = DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_AshLung");
        if (def == null) {
            Messages.Message("Cosmere_Scadrial_Hediff_AshLung is not loaded.", MessageTypeDefOf.RejectInput, false);

            return;
        }

        List<DebugMenuOption> options = [];
        foreach (float severity in new[] { 0f, 0.15f, 0.35f, 0.6f, 0.85f, 1f }) {
            float value = severity;
            options.Add(new DebugMenuOption(
                value.ToStringPercent(),
                DebugMenuOptionMode.Action,
                () => {
                    pawn.health.hediffSet.TryGetHediff(def, out Hediff? lung);
                    if (value <= 0f) {
                        if (lung != null) pawn.health.RemoveHediff(lung);

                        return;
                    }

                    lung ??= pawn.health.GetOrAddHediff(def);
                    lung.Severity = value;
                }
            ));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
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

        // The regenerate below draws off the set, and the sweep that fills it does not run paused.
        tracker.Buried.RefreshFromDepth(grid.GetDepthMm);
        map.mapDrawer.RegenerateEverythingNow();
        map.GetComponent<AshOverlayDrawer>()?.SetDirty();
    }
}
