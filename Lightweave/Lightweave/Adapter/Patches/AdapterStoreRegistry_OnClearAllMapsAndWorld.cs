using HarmonyLib;
using Verse.Profile;

namespace Cosmere.Lightweave.Adapter.Patches;

[HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
public static class AdapterStoreRegistry_OnClearAllMapsAndWorld {
    private static void Prefix() {
        AdapterStoreRegistry.ClearAll();
    }
}
