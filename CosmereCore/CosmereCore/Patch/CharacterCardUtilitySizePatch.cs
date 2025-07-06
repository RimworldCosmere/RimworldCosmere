using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereCore.Patch;

[HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.PawnCardSize))]
public static class CharacterCardUtilitySizePatch {
    public static void Postfix(Pawn pawn, ref Vector2 __result) {
        __result = GetViewSize(pawn, __result);
    }

    private static Vector2 GetViewSize(Pawn p, Vector2 result) {
        const float rowHeight = 27f;

        if (p.skills?.skills?.Count == null) return result;

        int skillCount = p.skills.skills.Count - 12;

        result.y += skillCount * rowHeight;

        return result;
    }
}