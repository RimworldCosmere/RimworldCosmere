using Cosmere.System.Roshar.Surgebinding.Ability.Tension;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.MaxHitPoints), MethodType.Getter)]
public static class HardenMaxHpPatch {
    private static void Postfix(Verse.Thing __instance, ref int __result) {
        if (__instance is not Building building) return;
        if (!Harden.TryGetHardenMultiplier(building, out float multiplier)) return;

        __result = Mathf.RoundToInt(__result * (1f + multiplier));
    }
}