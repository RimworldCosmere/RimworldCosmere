using Cosmere.System.Roshar.Thing.Pawn.Animal;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
public static class SprenFactionPatch {
    [HarmonyPrefix]
    public static bool Prefix(Pawn __instance) {
        if (__instance is Spren && __instance.Faction != null) {
            return false;
        }

        return true;
    }
}
