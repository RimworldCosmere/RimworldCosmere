using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Framework.Patch;

[HarmonyPatch(typeof(Verse.Thing))]
public static class DebugSpawnPatch {
    [HarmonyPatch(nameof(Verse.Thing.Notify_DebugSpawned))]
    [HarmonyPrefix]
    public static void PrefixNotify_DebugSpawned(Verse.Thing __instance) {
        if (!__instance.def.CanHaveFaction) return;

        __instance.SetFactionDirect(Find.Selector.SelectedPawns.FirstOrDefault()?.Faction ?? Faction.OfPlayer);
    }
}