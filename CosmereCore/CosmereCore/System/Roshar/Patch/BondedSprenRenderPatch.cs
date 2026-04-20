using System.Reflection;
using Cosmere.Core.Patch;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt))]
public static class BondedSprenBaseHeadOffsetPatch {
    private static FieldInfo? pawnField;

    static bool Prefix(PawnRenderer __instance, ref Vector3 __result) {
        pawnField ??= AccessTools.Field(typeof(PawnRenderer), "pawn");
        Verse.Pawn? pawn = pawnField?.GetValue(__instance) as Verse.Pawn;
        if (pawn?.story == null || pawn.ageTracker?.CurLifeStage == null) {
            __result = Vector3.zero;
            return false;
        }
        if (pawn.story.bodyType == null && pawn.RaceProps?.Humanlike == true) {
            pawn.story.bodyType = PawnBodyTypeFallbackPatch.FallbackBodyTypeFor(pawn);
        }
        if (pawn.story.bodyType == null) {
            __result = Vector3.zero;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(
    typeof(DynamicPawnRenderNodeSetup_Apparel),
    nameof(DynamicPawnRenderNodeSetup_Apparel.GetDynamicNodes)
)]
public static class BondedSprenApparelGetDynamicNodesPatch {
    static bool Prefix(Verse.Pawn pawn, ref IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> __result) {
        if (pawn?.story?.bodyType != null) return true;
        __result = [];
        return false;
    }
}
