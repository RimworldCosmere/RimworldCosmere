using Cosmere.Framework.Comp.Thing;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace Cosmere.Framework.Patch;

// @TODO Replace this with a transpiler.
// Transpiler should return null in the Thing case, if the thing is ApparelWithStorage
[HarmonyPatch]
public static class PatchHaulToContainerJob {
    [HarmonyPatch(typeof(HaulAIUtility), nameof(HaulAIUtility.HaulToContainerJob))]
    [HarmonyPrefix]
    public static bool PrefixHaulToContainerJob(Pawn p, Verse.Thing t, Verse.Thing container, ref Job? __result) {
        if (!container.HasComp<InnerStorage>()) return true;

        __result = null;
        return false;
    }

    [HarmonyPatch(typeof(ThingOwnerUtility), nameof(ThingOwnerUtility.TryGetInnerInteractableThingOwner))]
    [HarmonyPrefix]
    public static bool PrefixTryGetInnerInteractableThingOwner(Verse.Thing thing, ref ThingOwner __result) {
        if (!thing.TryGetComp(out InnerStorage innerStorage)) return true;

        __result = innerStorage.innerContainer;
        return false;
    }
}