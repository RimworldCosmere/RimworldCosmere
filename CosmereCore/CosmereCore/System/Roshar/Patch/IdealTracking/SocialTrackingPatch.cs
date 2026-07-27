using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
public static class SocialTrackingPatch {
    private static readonly AccessTools.FieldRef<Pawn_InteractionsTracker, Pawn> PawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(Pawn_InteractionsTracker), "pawn");

    private static void Prefix(Pawn_InteractionsTracker __instance, Pawn recipient, out int __state) {
        Pawn initiator = PawnRef(__instance);
        __state = -999;

        Surgebinder? surgebinder = initiator.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;
        if (recipient?.relations == null) return;

        __state = recipient.relations.OpinionOf(initiator);
    }

    private static void Postfix(Pawn_InteractionsTracker __instance, Pawn recipient, bool __result, int __state) {
        if (!__result) return;
        if (__state == -999) return;

        Pawn initiator = PawnRef(__instance);
        Surgebinder? surgebinder = initiator.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;
        if (recipient?.relations == null) return;

        int newOpinion = recipient.relations.OpinionOf(initiator);
        if (newOpinion > __state) {
            initiator.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_SocialHealing, 1);
        }

        if (__state < 20 && newOpinion >= 20) {
            initiator.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_FriendshipsFormed, 1);
        }
    }
}
