using Cosmere.System.Roshar.Surgebinding;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
public static class SkillViolationPatch {
    private static void Prefix(SkillRecord __instance, float xp, out int __state) {
        __state = __instance.Level;
    }

    private static void Postfix(SkillRecord __instance, float xp, int __state) {
        if (xp >= 0) return;

        int newLevel = __instance.Level;
        if (newLevel >= __state) return;

        Pawn pawn = __instance.Pawn;
        if (pawn == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(pawn, "Elsecaller")) {
            ViolationUtility.ApplyViolation(pawn, 0.3f, "losing a skill level");
        }
    }
}