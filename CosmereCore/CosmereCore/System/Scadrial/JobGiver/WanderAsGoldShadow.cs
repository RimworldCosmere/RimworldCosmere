using Cosmere.System.Scadrial.Thing;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobGiver;

public class WanderAsGoldShadow : JobGiver_Wander {
    public WanderAsGoldShadow() {
        wanderRadius = 8f;
        ticksBetweenWandersRange = new IntRange(60, 180);
        locomotionUrgency = LocomotionUrgency.Walk;
        maxDanger = Danger.Deadly;
    }

    protected override IntVec3 GetWanderRoot(Pawn pawn) {
        return pawn is GoldShadow { owner.Spawned: true } shadow ? shadow.owner.Position : pawn.Position;
    }
}
