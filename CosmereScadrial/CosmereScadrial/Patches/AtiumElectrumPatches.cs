using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches;

[HarmonyPatch]
public static class AtiumElectrumPatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static bool PrefixVerbShoot(Verb_MeleeAttack __instance, ref bool __result) {
        return Patch(__instance, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Verb_Shoot), "TryCastShot")]
    public static bool PrefixVerbShoot(Verb_Shoot __instance, ref bool __result) {
        return Patch(__instance, ref __result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Verb_CastAbility), "TryCastShot")]
    public static bool PrefixVerbCastAbility(Verb_CastAbility __instance, ref bool __result) {
        return Patch(__instance, ref __result);
    }

    private static bool Patch(Verb instance, ref bool result) {
        if (!instance.CurrentTarget.HasThing || instance.CurrentTarget.Thing is not Pawn target) return true;
        if (instance.caster is not Pawn casterPawn) return true;
        if (casterPawn.Equals(target)) return true;

        bool casterHasAtium = casterPawn.health?.hediffSet?.HasHediff(HediffDefOf.Cosmere_Hediff_AtiumBuff) ?? false;
        bool targetHasAtium = target.health?.hediffSet?.HasHediff(HediffDefOf.Cosmere_Hediff_AtiumBuff) ?? false;
        bool targetHasElectrum =
            target.health?.hediffSet?.HasHediff(HediffDefOf.Cosmere_Hediff_ElectrumBuff) ?? false;

        if (!casterHasAtium && !targetHasAtium && !targetHasElectrum) return true;

        if (casterHasAtium) {
            if (targetHasElectrum && targetHasAtium) {
                result = false;
            } else if (targetHasAtium) {
                result = Rand.Chance(0.5f);
            } else if (targetHasElectrum) {
                result = Rand.Chance(0.25f);
            } else {
                result = true;
            }
        } else {
            result = !targetHasAtium && Rand.Chance(0.25f);
        }

        return result;
    }
}