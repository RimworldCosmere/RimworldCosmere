using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

/// <summary>
///     Sweeps ash off a 3x3 footprint and leaves it as a haulable pile. Clearing never deletes
///     ash - the colony moves it, which is what makes the endgame a logistics problem rather
///     than a chore that quietly resets.
/// </summary>
public class ClearAsh : Verse.AI.JobDriver {
    private const float WorkPerMetre = 320f;
    private const int BrushRadius = 1;

    /// <summary>Millimetres of swept ash that make one haulable unit.</summary>
    private const int MmPerAshItem = 900;

    private float workDone;

    private AshDepthTracker? Tracker => Map.GetComponent<AshDepthTracker>();

    private float TotalNeededWork {
        get {
            AshDepthTracker? tracker = Tracker;
            if (tracker == null) return 1f;

            int total = 0;
            foreach (IntVec3 cell in Brush()) {
                total += tracker.Grid.GetDepthMm(Map.cellIndices.CellToIndex(cell));
            }

            return Mathf.Max(1f, total / 1000f * WorkPerMetre);
        }
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);

        Toil clear = ToilMaker.MakeToil("MakeNewToils");
        clear.tickIntervalAction = _ => {
            workDone += clear.actor.GetStatValue(RimWorld.StatDefOf.GeneralLaborSpeed);
            if (workDone < TotalNeededWork) return;

            SweepBrush();
            ReadyForNextToil();
        };

        clear.defaultCompleteMode = ToilCompleteMode.Never;
        clear.WithEffect(EffecterDefOf.ClearSnow, TargetIndex.A);
        clear.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
        clear.WithProgressBar(TargetIndex.A, () => workDone / TotalNeededWork, true);
        clear.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        yield return clear;
    }

    private void SweepBrush() {
        AshDepthTracker? tracker = Tracker;
        if (tracker == null) return;

        AshGrid grid = tracker.Grid;
        AshBuriedCells buried = tracker.Buried;
        CellIndices indices = Map.cellIndices;
        int swept = 0;

        foreach (IntVec3 cell in Brush()) {
            int index = indices.CellToIndex(cell);
            swept += grid.RemoveDepthMm(index, AshGrid.MaxDepthMm);

            // mark dirty now - left to the sweep, swept ground still reads as deep ash for 64 ticks.
            buried.Set(index, AshDepthMath.IsBuried(grid.GetDepthMm(index), buried.IsBuried(index)));
        }

        tracker.NotifyAshChanged();

        int piles = swept / MmPerAshItem;
        if (piles <= 0) return;

        Verse.Thing ash = ThingMaker.MakeThing(ThingDefOf_Ash.Cosmere_Scadrial_Thing_Ash);
        ash.stackCount = piles;
        GenPlace.TryPlaceThing(ash, TargetLocA, Map, ThingPlaceMode.Near);
    }

    /// <summary>Spelled out rather than using CellRect, so the footprint is provably 3x3.</summary>
    private IEnumerable<IntVec3> Brush() {
        for (int dx = -BrushRadius; dx <= BrushRadius; dx++) {
            for (int dz = -BrushRadius; dz <= BrushRadius; dz++) {
                IntVec3 cell = TargetLocA + new IntVec3(dx, 0, dz);
                if (cell.InBounds(Map)) yield return cell;
            }
        }
    }
}

[DefOf]
public static class ThingDefOf_Ash {
    public static ThingDef Cosmere_Scadrial_Thing_Ash = null!;

    static ThingDefOf_Ash() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_Ash));
    }
}
