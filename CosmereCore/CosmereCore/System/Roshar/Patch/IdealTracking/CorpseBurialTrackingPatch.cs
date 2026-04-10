using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Building_Grave), nameof(Building_Grave.TryAcceptThing))]
public static class CorpseBurialTrackingPatch {
    private static void Postfix(Building_Grave __instance, Verse.Thing thing, bool __result) {
        if (!__result) return;
        if (thing is not Corpse) return;
        if (__instance.Map == null) return;

        List<Pawn> colonists = __instance.Map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            Verse.AI.Job job = colonist.CurJob;
            if (job == null) continue;
            if (job.def != RimWorld.JobDefOf.HaulToContainer) continue;
            if (job.targetB.Thing != __instance) continue;

            Surgebinder? surgebinder = colonist.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) continue;

            colonist.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_DeadHonored, 1);
            break;
        }
    }
}
