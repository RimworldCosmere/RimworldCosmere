using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Util;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.JobGiver;

public class SeekHighstormShelter : ThinkNode_JobGiver {
    protected override Verse.AI.Job? TryGiveJob(Pawn pawn) {
        if (pawn.Map == null || !pawn.Spawned) return null;
        if (StormlightUtility.IsHighstormImmune(pawn)) return null;
        if (StormShelterManager.IsInsideShelter(pawn.Position)) return null;

        IntVec3 shelterCell = StormShelterManager.FindNearestShelterCell(
            pawn.Position,
            pawn.Map,
            TraverseParms.For(pawn)
        );

        if (!shelterCell.IsValid) return null;

        return JobMaker.MakeJob(
            JobDefOf.Cosmere_Roshar_GoToHighstormShelter,
            shelterCell
        );
    }
}
