using HarmonyLib;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Adapter;

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.Destroy))]
public static class AdapterStoreRegistryPatches
{
    private static void Postfix(Verse.Thing __instance)
    {
        AdapterStoreRegistry.ReleaseAllFor(__instance.thingIDNumber);
    }
}
