using System;
using Concord;
using Cosmere.System.Roshar.Surgebinding.Ability.Gravitation;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[Patch]
public abstract class FlyingMoveCostPatch : Pawn_PathFollower {
    protected FlyingMoveCostPatch(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Return, "CostToMoveIntoCell", parameterTypes: [typeof(Pawn), typeof(IntVec3)])]
    private static void AfterCostToMoveIntoCell(Pawn pawn, IntVec3 c, ControlHandle<float> ch) {
        if (pawn == null) return;
        if (!BasicLashing.FlyingPawns.Contains(pawn)) return;

        ch.ReturnValue = c.x != pawn.Position.x && c.z != pawn.Position.z
            ? pawn.TicksPerMoveDiagonal
            : pawn.TicksPerMoveCardinal;
    }
}

[Patch]
public abstract class FlyingBlocksPawnPatch : Verse.Thing {
    [Inject(At.Return, nameof(BlocksPawn))]
    private void AfterBlocksPawn(Pawn p, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        if (p == null) return;
        if (BasicLashing.FlyingPawns.Contains(p)) {
            ch.ReturnValue = false;
        }
    }
}

[Patch(typeof(ReachabilityUtility))]
public static class FlyingReachabilityPatch {
    [Inject(
        At.Head,
        nameof(ReachabilityUtility.CanReach),
        parameterTypes: [
            typeof(Pawn),
            typeof(LocalTargetInfo),
            typeof(PathEndMode),
            typeof(Danger),
            typeof(bool),
            typeof(bool),
            typeof(TraverseMode),
        ]
    )]
    private static Control BeforeCanReach(Pawn pawn, ControlHandle<bool> ch) {
        if (pawn == null || !pawn.Spawned) return Control.Continue;
        if (!BasicLashing.FlyingPawns.Contains(pawn)) return Control.Continue;

        ch.ReturnValue = true;
        return Control.Cancel;
    }
}

[Patch]
public abstract class FlyingReachabilityDirectPatch : Reachability {
    [ThreadStatic]
    private static bool patching;

    protected FlyingReachabilityDirectPatch(Verse.Map map) : base(map) { }

    [Inject(
        At.Around,
        nameof(CanReach),
        parameterTypes: [typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms)]
    )]
    private bool AroundCanReach(
        IntVec3 start,
        LocalTargetInfo dest,
        PathEndMode peMode,
        TraverseParms traverseParams,
        Operation<IntVec3, LocalTargetInfo, PathEndMode, TraverseParms, bool> original
    ) {
        if (patching) return original.Invoke(start, dest, peMode, traverseParams);

        Pawn? pawn = traverseParams.pawn;
        if (pawn == null) return original.Invoke(start, dest, peMode, traverseParams);
        if (!BasicLashing.FlyingPawns.Contains(pawn)) return original.Invoke(start, dest, peMode, traverseParams);

        traverseParams.mode = TraverseMode.PassAllDestroyableThings;
        traverseParams.canBashDoors = true;
        traverseParams.canBashFences = true;

        patching = true;
        try {
            return original.Invoke(start, dest, peMode, traverseParams);
        } finally {
            patching = false;
        }
    }
}

[Patch]
public abstract class FlyingBuildingCostPatch : Building {
    [Inject(At.Return, nameof(PathWalkCostFor))]
    private void AfterPathWalkCostFor(Pawn p, ControlHandle<ushort> ch) {
        if (p == null) return;
        if (!BasicLashing.FlyingPawns.Contains(p)) return;

        ch.ReturnValue = 0;
    }
}
