using Concord;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Radiant;

[Patch]
public abstract class PatchPawnMovement : Pawn_PathFollower {
    protected PatchPawnMovement(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Return, "CostToMoveIntoCell", parameterTypes: [typeof(Pawn), typeof(IntVec3)])]
    private static void AfterCostToMoveIntoCell(Pawn? pawn, IntVec3 c, ControlHandle<float> ch) {
        if (pawn?.health?.hediffSet == null) return;
        if (pawn.health.hediffSet.HasHediff(HediffDefOf.Cosmere_Roshar_Surge_Abrasion)) {
            ch.ReturnValue = c.x != pawn.Position.x && c.z != pawn.Position.z
                ? pawn.TicksPerMoveDiagonal
                : pawn.TicksPerMoveCardinal;
        }
    }
}
