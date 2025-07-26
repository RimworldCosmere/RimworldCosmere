using HarmonyLib;
using Verse;
using Verse.AI;

namespace Cosmere.Roshar.Patches;

// @TODO Move to CosmereRoshar/Patches/PawnPathFollowerPatches.cs
[HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell")]
[HarmonyPatch([typeof(Pawn), typeof(IntVec3)])]
public static class PatchPawnMovement {
    private static void Postfix(Pawn? pawn, IntVec3 c, ref float __result) {
        if (pawn?.health?.hediffSet == null) return;
        if (pawn.health.hediffSet.HasHediff(Defs.Cosmere_Roshar_Surge_Abrasion)) {
            __result = c.x != pawn.Position.x && c.z != pawn.Position.z
                ? pawn.TicksPerMoveDiagonal
                : pawn.TicksPerMoveCardinal;
        }
    }
}