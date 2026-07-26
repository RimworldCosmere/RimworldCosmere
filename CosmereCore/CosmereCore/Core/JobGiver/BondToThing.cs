using Cosmere.Core.Comp.Game;
using Verse;
using Verse.AI;

namespace Cosmere.Core.JobGiver;

public class BondToThing : ThinkNode_JobGiver {
    protected override Job? TryGiveJob(Pawn pawn) {
        // Skip if busy or incapable
        if (pawn.Drafted || pawn.InMentalState || !pawn.Spawned) return null;
        if (pawn.CurJobDef == JobDefOf.Cosmere_BondToThing) return null;

        // Lookup owned pawns
        List<Pawn> pawns = pawn.Map.listerThings.AllThings
            .Where(t => t is Pawn)
            .Cast<Pawn>()
            .Where(t => {
                if (t.playerSettings?.Master != null && t.playerSettings.Master != pawn) return false;
                Connection? conn = pawn.GetConnection(t);
                if (conn == null) return false;
                if (conn.ObjectOne.Equals(pawn) && !conn.CanBondObjectTwo) return false;
                if (conn.ObjectTwo.Equals(pawn) && !conn.CanBondObjectOne) return false;

                return conn.Value < 1f;
            }
            )
            .ToList();

        if (pawns.NullOrEmpty()) return null;

        // Pick nearestT
        Verse.Thing target = GenClosest.ClosestThing_Global(pawn.Position, pawns, 999f);

        if (target == null || !pawn.CanReach(target, PathEndMode.Touch, Danger.None)) return null;

        return JobMaker.MakeJob(JobDefOf.Cosmere_BondToThing, pawn, target);
    }
}