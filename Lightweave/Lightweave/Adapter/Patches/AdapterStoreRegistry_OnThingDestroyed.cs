using HarmonyLib;

namespace Cosmere.Lightweave.Adapter.Patches;

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.Destroy))]
public static class AdapterStoreRegistry_OnThingDestroyed {
    private static void Postfix(Verse.Thing __instance) {
        AdapterStoreRegistry.ReleaseAllFor(__instance.thingIDNumber);
    }
}
