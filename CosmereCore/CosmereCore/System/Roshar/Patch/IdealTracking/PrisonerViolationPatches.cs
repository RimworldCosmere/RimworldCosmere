using Cosmere.System.Roshar;
using Cosmere.System.Roshar.Surgebinding;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(ExecutionUtility), nameof(ExecutionUtility.DoExecutionByCut))]
public static class ExecutionViolationPatch {
    private static void Postfix(Pawn executioner, Pawn victim) {
        if (executioner == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(executioner, RadiantOrderDefOf.Edgedancer)) {
            ViolationUtility.ApplyViolation(executioner, 0.3f, "executing a prisoner");
        }
    }
}

[HarmonyPatch(typeof(Recipe_RemoveBodyPart), nameof(Recipe_RemoveBodyPart.ApplyOnPawn))]
public static class OrganHarvestViolationPatch {
    private static void Postfix(Pawn pawn, BodyPartRecord part, Pawn billDoer) {
        if (billDoer == null) return;
        if (pawn == null) return;
        if (part == null) return;
        if (pawn.Faction == billDoer.Faction) return;
        if (!MedicalRecipesUtility.IsClean(pawn, part)) return;

        List<Pawn>? colonists = billDoer.Map?.mapPawns?.FreeColonistsSpawned;
        if (colonists == null) return;

        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (ViolationUtility.IsSurgebinderOfOrder(colonist, RadiantOrderDefOf.Edgedancer)) {
                ViolationUtility.ApplyViolation(colonist, 0.6f, "harvesting a prisoner's organs");
            }
        }
    }
}

[HarmonyPatch(typeof(GenGuest), nameof(GenGuest.EnslavePrisoner))]
public static class EnslavementViolationPatch {
    private static void Postfix(Pawn warden, Pawn prisoner) {
        if (warden == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(warden, RadiantOrderDefOf.Willshaper)) {
            ViolationUtility.ApplyViolation(warden, 0.6f, "enslaving a prisoner");
        }

        List<Pawn>? colonists = warden.Map?.mapPawns?.FreeColonistsSpawned;
        if (colonists == null) return;

        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (colonist == warden) continue;

            if (ViolationUtility.IsSurgebinderOfOrder(colonist, RadiantOrderDefOf.Willshaper)) {
                ViolationUtility.ApplyViolation(colonist, 0.1f, "allowing enslavement of a prisoner");
            }
        }
    }
}

[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.CapturedBy))]
public static class ArrestViolationPatch {
    private static void Postfix(Pawn_GuestTracker __instance, Faction by, Pawn byPawn) {
        if (byPawn == null) return;
        if (by != Faction.OfPlayer) return;

        if (ViolationUtility.IsSurgebinderOfOrder(byPawn, RadiantOrderDefOf.Willshaper)) {
            ViolationUtility.ApplyViolation(byPawn, 0.3f, "arresting someone");
        }
    }
}