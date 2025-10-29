using HarmonyLib;
using RimWorld;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(Verse.Thing))]
public static class DebugSpawnPatch {
    [HarmonyPatch(nameof(Verse.Thing.Notify_DebugSpawned))]
    [HarmonyPostfix]
    public static void PostfixNotify_DebugSpawned(Verse.Thing __instance) {
        if (__instance.def.IsOneOf(
                ThingDefOf.Cosmere_Roshar_Thing_Broam,
                ThingDefOf.Cosmere_Roshar_Thing_Mark,
                ThingDefOf.Cosmere_Roshar_Thing_Chip
            ) &&
            __instance.Stuff == Resources.ThingDefOf.CutGem) {
            __instance.SetStuffDirect(GenStuff.RandomStuffFor(Resources.ThingDefOf.CutGem));
        }
    }
}