using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.PawnCardSize))]
public static class CharacterCardUtilitySizePatch {
    public static void Postfix(Pawn pawn, ref Vector2 __result) {
        __result = GetViewSize(pawn, __result);
    }

    private static Vector2 GetViewSize(Pawn p, Vector2 result) {
        const float rowHeight = 27f;

        if (p.skills?.skills?.Count == null) return result;

        int hiddenCount = SkillVisibilityPatch.GetHiddenSkillCount(p);
        int visibleExtra = p.skills.skills.Count - 12 - hiddenCount;

        result.y += visibleExtra * rowHeight;

        return result;
    }
}
