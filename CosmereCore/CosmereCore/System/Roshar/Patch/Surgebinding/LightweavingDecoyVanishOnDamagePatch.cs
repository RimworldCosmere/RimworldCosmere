using HarmonyLib;
using RimWorld;
using Verse;
using Cosmere.System.Roshar.Surgebinding.Ability.Illumination;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
public static class LightweavingDecoyVanishOnDamagePatch {
    [HarmonyPrefix]
    public static bool Prefix(Pawn __instance, ref DamageInfo dinfo, out bool absorbed) {
        absorbed = false;
        if (!DecoyHediff.IsDecoy(__instance)) return true;

        absorbed = true;
        if (__instance.Spawned) {
            FleckMaker.Static(__instance.Position, __instance.Map, FleckDefOf.PsycastAreaEffect);
            __instance.DeSpawn();
        }

        LightweavingDecoyRegistry.Remove(__instance);
        if (!__instance.Destroyed) {
            __instance.Discard(true);
        }

        return false;
    }
}
