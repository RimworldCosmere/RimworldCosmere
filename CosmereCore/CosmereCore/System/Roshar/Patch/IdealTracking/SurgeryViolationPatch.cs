using Cosmere.System.Roshar;
using Cosmere.System.Roshar.Surgebinding;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Recipe_Surgery), "CheckSurgeryFail")]
public static class SurgeryViolationPatch {
    private static void Postfix(bool __result, Pawn surgeon, Pawn patient) {
        if (!__result) return;
        if (surgeon == null || patient == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(surgeon, RadiantOrderDefOf.Truthwatcher)) {
            ViolationUtility.ApplyViolation(surgeon, 0.3f, "botching a surgery");
        }
    }
}
