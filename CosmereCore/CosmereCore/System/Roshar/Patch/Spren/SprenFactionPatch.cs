using HarmonyLib;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
public static class SprenFactionPatch {
    [HarmonyPrefix]
    public static bool Prefix(Pawn __instance) {
        if (__instance is Cosmere.System.Roshar.Thing.Pawn.Animal.Spren && __instance.Faction != null) {
            return false;
        }

        return true;
    }
}
