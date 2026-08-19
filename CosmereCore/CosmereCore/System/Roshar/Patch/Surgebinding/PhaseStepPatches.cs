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

/// <summary>
///     Lets a lashed pawn path as though walls and doors were not there.
///     <para>
///         Deliberately At.Head rather than At.Around. Reachability.CanReach calls itself once
///         for the PassAllDestroyable* modes, and an Around handler's original.Invoke re-enters
///         the handler instead of the copied body, so the mode never downgrades and the call
///         recurses until the stack goes. It took out GenStep_Outpost on quest site maps, which
///         is one of the few callers that asks for PassAllDestroyableThings.
///     </para>
/// </summary>
[Patch]
public abstract class FlyingReachabilityDirectPatch : Reachability {
    /// <summary>
    ///     Set during our own re-entrant call so the nested injection steps aside for the real body.
    ///     Vanilla's own PassAllDestroyableThings self-call lands here too and just passes through.
    /// </summary>
    [ThreadStatic]
    private static bool passingThrough;

    protected FlyingReachabilityDirectPatch(Verse.Map map) : base(map) { }

    [Inject(
        At.Head,
        nameof(CanReach),
        parameterTypes: [typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms)]
    )]
    private Control BeforeCanReach(
        IntVec3 start,
        LocalTargetInfo dest,
        PathEndMode peMode,
        TraverseParms traverseParams,
        ControlHandle<bool> ch
    ) {
        if (passingThrough) return Control.Continue;

        Pawn? pawn = traverseParams.pawn;
        if (pawn == null) return Control.Continue;
        if (!BasicLashing.FlyingPawns.Contains(pawn)) return Control.Continue;

        traverseParams.mode = TraverseMode.PassAllDestroyableThings;
        traverseParams.canBashDoors = true;
        traverseParams.canBashFences = true;

        passingThrough = true;
        try {
            ch.ReturnValue = CanReach(start, dest, peMode, traverseParams);
        } finally {
            passingThrough = false;
        }

        return Control.Cancel;
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
