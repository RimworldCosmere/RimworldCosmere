using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(RimWorld.PawnFlyer))]
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
        if (triggeringAbility.Equals(CosmereRosharDefs.Cosmere_Roshar_LashingUpward)) {
            __state.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 75));
        }
    }
}