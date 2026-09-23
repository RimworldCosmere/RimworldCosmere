using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.WorkGiver;

/// <summary>
///     Ash clearing rides the vanilla snow-clearing area, so the designator players already know
///     covers ash too and there is no second piece of UI to find.
/// </summary>
public class ClearAsh : WorkGiver_Scanner {
    /// <summary>Below this a cell is a dusting and not worth a trip.</summary>
    public const int WorthClearingMm = AshDepthMath.DustingMm;

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    /// <summary>
    ///     The removal zone plus the home area. Players expect the home area to keep itself
    ///     swept the way it does for filth, without designating a second zone on top of it.
    /// </summary>
    public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn) {
        Area removal = pawn.Map.areaManager.SnowOrSandClear;
        foreach (IntVec3 cell in removal.ActiveCells) {
            yield return cell;
        }

        foreach (IntVec3 cell in pawn.Map.areaManager.Home.ActiveCells) {
            if (!removal[cell]) yield return cell;
        }
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false) {
        if (pawn.Map.areaManager.SnowOrSandClear.TrueCount == 0 &&
            pawn.Map.areaManager.Home.TrueCount == 0) {
            return true;
        }

        return pawn.Map.GetComponent<AshDepthTracker>()?.HasAnyAsh != true;
    }

    public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false) {
        AshDepthTracker? tracker = pawn.Map.GetComponent<AshDepthTracker>();
        if (tracker == null) return false;
        if (tracker.Grid.GetDepthMm(pawn.Map.cellIndices.CellToIndex(c)) < WorthClearingMm) return false;

        return pawn.CanReserve(c, 1, -1, null, forced);
    }

    public override Verse.AI.Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false) {
        return JobMaker.MakeJob(JobDefOf_Ash.Cosmere_Scadrial_Job_ClearAsh, c);
    }
}

[DefOf]
public static class JobDefOf_Ash {
    public static JobDef Cosmere_Scadrial_Job_ClearAsh = null!;

    static JobDefOf_Ash() {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_Ash));
    }
}
