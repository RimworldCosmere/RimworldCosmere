using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(
    typeof(Pawn_RelationsTracker),
    nameof(Pawn_RelationsTracker.RemoveDirectRelation),
    typeof(PawnRelationDef),
    typeof(Pawn)
)]
public static class RelationshipLossTrackingPatch {
    private static readonly AccessTools.FieldRef<Pawn_RelationsTracker, Pawn> PawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(Pawn_RelationsTracker), "pawn");

    private static void Prefix(Pawn_RelationsTracker __instance, PawnRelationDef def, Pawn otherPawn) {
        Pawn pawn = PawnRef(__instance);
        if (pawn == null || pawn.Dead) return;

        if (!IsCloseRelation(def)) return;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        surgebinder.OnRelationshipLost();
    }

    private static bool IsCloseRelation(PawnRelationDef def) {
        return def == PawnRelationDefOf.Spouse ||
               def == PawnRelationDefOf.Fiance ||
               def == PawnRelationDefOf.Lover ||
               def == PawnRelationDefOf.Parent ||
               def == PawnRelationDefOf.Child ||
               def == PawnRelationDefOf.Bond;
    }
}