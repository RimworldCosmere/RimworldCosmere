using Cosmere.System.Roshar.Surgebinding.Ability.Gravitation;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawPos), MethodType.Getter)]
public static class BasicLashingFloatPatch {
    private const float FloatHeight = 0.5f;

    private static void Postfix(Pawn __instance, ref Vector3 __result) {
        if (!BasicLashing.FlyingPawns.Contains(__instance)) return;

        __result += new Vector3(0f, 0f, FloatHeight);
    }
}

[HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
public static class BasicLashingMeleeBlockPatch {
    private static bool Prefix(Verb_MeleeAttack __instance, ref bool __result) {
        if (__instance.CurrentTarget.Thing is not Pawn targetPawn) return true;
        if (!BasicLashing.FlyingPawns.Contains(targetPawn)) return true;

        Pawn? attacker = __instance.CasterPawn;
        if (attacker == null) return true;
        if (BasicLashing.FlyingPawns.Contains(attacker)) return true;

        __result = false;
        return false;
    }
}
