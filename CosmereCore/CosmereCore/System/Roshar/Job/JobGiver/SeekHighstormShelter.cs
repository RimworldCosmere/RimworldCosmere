using Cosmere.System.Roshar.Utility;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Job.JobGiver;

public class SeekHighstormShelter : ThinkNode_JobGiver {
    protected override Verse.AI.Job TryGiveJob(Pawn pawn) {
        if (pawn.Map == null || !pawn.Spawned) return null;
        if (StormlightUtilities.IsHighstormImmune(pawn)) return null;
        if (StormShelterManager.IsInsideShelter(pawn.Position)) return null;

        IntVec3 shelterCell = StormShelterManager.FindNearestShelterCell(
            pawn.Position,
            pawn.Map,
            TraverseParms.For(pawn, Danger.Deadly)
        );

        if (!shelterCell.IsValid) return null;

        return JobMaker.MakeJob(
            JobDefOf.Cosmere_Roshar_GoToHighstormShelter,
            shelterCell
        );
    }
}
