using Cosmere.System.Scadrial.Thing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Allomancy;

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.Print))]
public static class HideStoredVialsPatch {
    [HarmonyPrefix]
    public static bool Prefix(Verse.Thing __instance) {
        if (__instance.def.category != ThingCategory.Item) return true;
        if (__instance.Map == null) return true;

        SlotGroup slotGroup = __instance.Position.GetSlotGroup(__instance.Map);
        if (slotGroup?.parent is Building_VialCabinet) return false;

        return true;
    }
}
