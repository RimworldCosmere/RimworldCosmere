using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(Pawn_IdeoTracker), "get_Ideo")]
public static class PatchPawnIdeoTrackerIdeo {
    private static void Postfix(Pawn_IdeoTracker __instance, ref Ideo __result) {
        Pawn? pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        if (pawn?.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Agnostic")) == true) {
            __result = null;
        }
    }
}

// 🚫 Patch 2: Block ideoligion assignment
[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo))]
public static class PatchPawnIdeoTrackerSetIdeo {
    private static bool Prefix(Pawn_IdeoTracker __instance, Ideo ideo) {
        Pawn? pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

        return pawn?.genes?.HasActiveGene(DefDatabase<GeneDef>.GetNamed("Cosmere_Skaa_Agnostic")) !=
               true; // Skip setting ideoligion
    }
}