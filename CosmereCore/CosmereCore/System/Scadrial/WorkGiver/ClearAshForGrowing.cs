using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.WorkGiver;

/// <summary>
///     The same clearing job, but owned by Growing rather than Cleaning and scoped to growing
///     zones. A farm silting up should be the grower's problem to notice, not something that
///     waits on whoever happens to hold the cleaning job.
/// </summary>
public class ClearAshForGrowing : ClearAsh {
    public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn) {
        List<Zone> zones = pawn.Map.zoneManager.AllZones;
        for (int i = 0; i < zones.Count; i++) {
            if (zones[i] is not Zone_Growing growing) continue;

            List<IntVec3> cells = growing.Cells;
            for (int c = 0; c < cells.Count; c++) {
                yield return cells[c];
            }
        }
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false) {
        if (pawn.Map.GetComponent<Comp.Map.AshDepthTracker>()?.HasAnyAsh != true) return true;

        List<Zone> zones = pawn.Map.zoneManager.AllZones;
        for (int i = 0; i < zones.Count; i++) {
            if (zones[i] is Zone_Growing) return false;
        }

        return true;
    }
}
