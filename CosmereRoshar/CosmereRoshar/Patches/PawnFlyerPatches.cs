using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(RimWorld.PawnFlyer))]
public static class PawnFlyerPatches {
    private static readonly AccessTools.FieldRef<RimWorld.PawnFlyer, ThingOwner<Verse.Thing>> innerContainerRef =
        AccessTools.FieldRefAccess<ThingOwner<Verse.Thing>>(typeof(RimWorld.PawnFlyer), "innerContainer");

    private static readonly AccessTools.FieldRef<RimWorld.PawnFlyer, AbilityDef> triggeringAbilityRef =
        AccessTools.FieldRefAccess<AbilityDef>(typeof(RimWorld.PawnFlyer), "triggeringAbility");

    [HarmonyPatch("RespawnPawn")]
    [HarmonyPrefix]
    private static void Prefix(RimWorld.PawnFlyer __instance, out Pawn? __state) {
        if (innerContainerRef(__instance).InnerListForReading.Count <= 0) {
            __state = null;
            return;
        }

        __state = innerContainerRef(__instance).InnerListForReading[0] as Pawn;
    }

    [HarmonyPatch("RespawnPawn")]
    [HarmonyPostfix]
    private static void Postfix(RimWorld.PawnFlyer __instance, Pawn? __state) {
        AbilityDef? triggeringAbility = triggeringAbilityRef(__instance);
        if (__state == null || triggeringAbility == null) return;
        if (triggeringAbility.Equals(CosmereRosharDefs.Cosmere_Roshar_LashingUpward)) {
            __state.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 75));
        }
    }
}