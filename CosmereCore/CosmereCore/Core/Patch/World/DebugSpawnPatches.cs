using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(Verse.Thing))]
public static class DebugSpawnPatch {
    [HarmonyPatch(nameof(Verse.Thing.Notify_DebugSpawned))]
    [HarmonyPostfix]
    public static void PostfixNotify_DebugSpawned(Verse.Thing __instance) {
        if (__instance.def.CanHaveFaction) {
            __instance.SetFactionDirect(Find.Selector.SelectedPawns.FirstOrDefault()?.Faction ?? Faction.OfPlayer);
        }
    }
}
