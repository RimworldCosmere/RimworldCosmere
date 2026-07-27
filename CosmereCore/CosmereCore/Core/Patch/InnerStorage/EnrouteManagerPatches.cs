using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Patch.InnerStorage;

[HarmonyPatch]
public static class EnrouteManagerPatch {
    private static readonly MethodInfo GetOrAddTracker = AccessTools.Method(
        typeof(EnrouteManager),
        "GetOrAddTracker"
    );

    [HarmonyPatch(typeof(EnrouteManager), nameof(EnrouteManager.AddEnroute))]
    [HarmonyPrefix]
    public static bool PrefixAddEnroute(
        EnrouteManager __instance,
        IHaulEnroute container,
        Pawn pawn,
        ThingDef stuff,
        int count
    ) {
        if (container is not Comp.Thing.InnerStorage innerStorage) return true;

        ThingCountTracker tracker = (ThingCountTracker)GetOrAddTracker.Invoke(__instance, [innerStorage]);
        tracker.Add(pawn, stuff, count);

        pawn.MapHeld.events.Notify_HaulEnrouteAdded(innerStorage.ParentThing, pawn, stuff, count);
        return false;
    }

    [HarmonyPatch(typeof(ThingCountTracker), nameof(ThingCountTracker.ParentThing), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool PrefixGetParentThing(ThingCountTracker __instance, ref Verse.Thing __result) {
        if (__instance.parent is not Comp.Thing.InnerStorage innerStorage) return true;

        __result = innerStorage.ParentThing!;
        return false;
    }
}
