using Cosmere.Framework.Comp.Thing;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.Patch;

// @TODO Replace this with a transpiler.
// Transpiler should return null in the Thing case, if the thing is ApparelWithStorage
[HarmonyPatch(typeof(HaulAIUtility))]
public static class PatchHaulToStorage {
    [HarmonyPatch(nameof(HaulAIUtility.HaulToStorageJob))]
    [HarmonyPostfix]
    public static void PostfixHaulToStorageJob(Pawn p, Verse.Thing t, bool forced, ref Job? __result) {
        if (__result == null || !__result.def.Equals(RimWorld.JobDefOf.HaulToContainer)) return;

        if (__result.GetTarget(TargetIndex.B).Thing.HasComp<InnerStorage>()) return;
        __result = null;
    }
}