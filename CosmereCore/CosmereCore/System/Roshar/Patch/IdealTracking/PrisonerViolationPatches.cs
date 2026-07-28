using Concord;
using Cosmere.System.Roshar.Surgebinding;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch(typeof(ExecutionUtility))]
public static class ExecutionViolationPatch {
    [Inject(At.Return, nameof(ExecutionUtility.DoExecutionByCut))]
    private static void AfterDoExecutionByCut(Pawn executioner, Pawn victim) {
        if (executioner == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(executioner, RadiantOrderDefOf.Edgedancer)) {
            ViolationUtility.ApplyViolation(executioner, 0.3f, "executing a prisoner");
        }
    }
}

[Patch]
public abstract class OrganHarvestViolationPatch : Recipe_RemoveBodyPart {
    [Inject(At.Return, nameof(ApplyOnPawn))]
    private void AfterApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer) {
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

[Patch(typeof(GenGuest))]
public static class EnslavementViolationPatch {
    [Inject(At.Return, nameof(GenGuest.EnslavePrisoner))]
    private static void AfterEnslavePrisoner(Pawn warden, Pawn prisoner) {
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

[Patch]
public abstract class ArrestViolationPatch : Pawn_GuestTracker {
    protected ArrestViolationPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(CapturedBy))]
    private void AfterCapturedBy(Faction by, Pawn byPawn) {
        if (byPawn == null) return;
        if (by != Faction.OfPlayer) return;

        if (ViolationUtility.IsSurgebinderOfOrder(byPawn, RadiantOrderDefOf.Willshaper)) {
            ViolationUtility.ApplyViolation(byPawn, 0.3f, "arresting someone");
        }
    }
}
