using HarmonyLib;
using RimWorld;
using Verse;

namespace YourModNamespace
{
    [HarmonyPatch(typeof(Pawn_IdeoTracker), "get_Ideo")]
    public static class Patch_PawnIdeoTracker_Ideo
    {
        private static void Postfix(Pawn_IdeoTracker __instance, ref Ideo __result)
        {
            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn?.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Agnostic")) == true)
                __result = null;
        }
    }

    // 🚫 Patch 2: Block ideoligion assignment
    [HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo))]
    public static class Patch_PawnIdeoTracker_SetIdeo
    {
        private static bool Prefix(Pawn_IdeoTracker __instance, Ideo ideo)
        {
            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            return pawn?.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Agnostic")) !=
                   true; // Skip setting ideoligion
        }
    }
}