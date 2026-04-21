using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

public interface IAoEAbility {
    float AoERadius { get; }
}

[HarmonyPatch(typeof(Verb_CastAbility), "DrawHighlight")]
public static class AoERadiusPatch {
    private static void Postfix(Verb_CastAbility __instance, LocalTargetInfo target) {
        if (__instance.ability is not IAoEAbility aoeAbility) return;

        float radius = aoeAbility.AoERadius;
        if (radius <= 0f) return;

        IntVec3 cell = target.IsValid ? target.Cell : Verse.UI.MouseCell();
        GenDraw.DrawRadiusRing(cell, radius);
    }
}
