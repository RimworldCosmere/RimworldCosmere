using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(PawnFlyer))]
public static class PawnFlyerPatches {
    [HarmonyPatch("RespawnPawn")]
    [HarmonyPrefix]
    private static void Prefix(ThingOwner<Verse.Thing> innerContainer, out Pawn? __state) {
        if (innerContainer.InnerListForReading.Count <= 0) {
            __state = null;
            return;
        }

        __state = innerContainer.InnerListForReading[0] as Pawn;
    }

    [HarmonyPatch("RespawnPawn")]
    [HarmonyPostfix]
    private static void Postfix(AbilityDef? triggeringAbility, Pawn? __state) {
        if (__state == null || triggeringAbility == null) return;
        if (triggeringAbility.Equals(CosmereRosharDefs.WhtwlLashingUpward)) {
            __state.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 75));
        }
    }
}