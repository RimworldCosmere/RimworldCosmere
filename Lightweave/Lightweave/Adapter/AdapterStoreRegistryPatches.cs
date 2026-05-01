using HarmonyLib;
using Verse.Profile;

namespace Cosmere.Lightweave.Adapter;

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.Destroy))]
public static class AdapterStoreRegistryPatches {
    private static void Postfix(Verse.Thing __instance) {
        AdapterStoreRegistry.ReleaseAllFor(__instance.thingIDNumber);
    }
}

[HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
public static class AdapterStoreRegistryClearOnLoadPatch {
    private static void Prefix() {
        AdapterStoreRegistry.ClearAll();
    }
}