using System;
using Cosmere.System.Roshar.Surgebinding.Ability.Gravitation;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell")]
[HarmonyPatch([typeof(Pawn), typeof(IntVec3)])]
public static class FlyingMoveCostPatch {
    private static void Postfix(Pawn pawn, IntVec3 c, ref float __result) {
        if (pawn == null) return;
        if (!BasicLashing.FlyingPawns.Contains(pawn)) return;

        __result = c.x != pawn.Position.x && c.z != pawn.Position.z
            ? pawn.TicksPerMoveDiagonal
            : pawn.TicksPerMoveCardinal;
    }
}

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.BlocksPawn))]
public static class FlyingBlocksPawnPatch {
    private static void Postfix(Pawn p, ref bool __result) {
        if (!__result) return;
        if (p == null) return;
        if (BasicLashing.FlyingPawns.Contains(p)) {
            __result = false;
        }
    }
}

[HarmonyPatch(
    typeof(ReachabilityUtility),
    nameof(ReachabilityUtility.CanReach),
    typeof(Pawn),
    typeof(LocalTargetInfo),
    typeof(PathEndMode),
    typeof(Danger),
    typeof(bool),
    typeof(bool),
    typeof(TraverseMode)
)]
public static class FlyingReachabilityPatch {
    private static bool Prefix(Pawn pawn, ref bool __result) {
        if (pawn == null || !pawn.Spawned) return true;
        if (!BasicLashing.FlyingPawns.Contains(pawn)) return true;

        __result = true;
        return false;
    }
}

[HarmonyPatch(
    typeof(Reachability),
    nameof(Reachability.CanReach),
    typeof(IntVec3),
    typeof(LocalTargetInfo),
    typeof(PathEndMode),
    typeof(TraverseParms)
)]
public static class FlyingReachabilityDirectPatch {
    [ThreadStatic]
    private static bool patching;

    private static bool Prefix(ref TraverseParms traverseParams, ref bool __state) {
        __state = false;
        if (patching) return true;
        Pawn? pawn = traverseParams.pawn;
        if (pawn == null) return true;
        if (!BasicLashing.FlyingPawns.Contains(pawn)) return true;

        traverseParams.mode = TraverseMode.PassAllDestroyableThings;
        traverseParams.canBashDoors = true;
        traverseParams.canBashFences = true;
        patching = true;
        __state = true;
        return true;
    }

    private static void Finalizer(bool __state) {
        if (__state) patching = false;
    }
}

[HarmonyPatch(typeof(Building), nameof(Building.PathWalkCostFor))]
public static class FlyingBuildingCostPatch {
    private static void Postfix(Building __instance, Pawn p, ref ushort __result) {
        if (p == null) return;
        if (!BasicLashing.FlyingPawns.Contains(p)) return;

        __result = 0;
    }
}