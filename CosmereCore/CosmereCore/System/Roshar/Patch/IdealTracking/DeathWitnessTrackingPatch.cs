using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class DeathWitnessTrackingPatch {
    private static void Prefix(Pawn __instance, out Map __state) {
        __state = __instance.MapHeld;
    }

    private static void Postfix(Pawn __instance, Map __state) {
        if (!__instance.RaceProps.Humanlike) return;
        if (!__instance.IsColonist) return;
        if (__state == null) return;

        List<Pawn> colonists = __state.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (colonist == __instance) continue;
            if (colonist.Dead) continue;

            Surgebinder? surgebinder = colonist.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) continue;

            surgebinder.OnWitnessedDeath(__instance);
        }
    }
}
