using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(Verse.Pawn_AgeTracker), "BiologicalTicksPerTick", MethodType.Getter)]
public static class AgelessBodyPatch {
    private static readonly FieldInfo PawnField =
        AccessTools.Field(typeof(Verse.Pawn_AgeTracker), "pawn");

    private static HediffDef? agelessDef;

    private static void Postfix(Verse.Pawn_AgeTracker __instance, ref float __result) {
        agelessDef ??= DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Roshar_Hediff_NW_BoonPassive_AgelessBody");
        if (agelessDef == null) return;
        Verse.Pawn? pawn = PawnField.GetValue(__instance) as Verse.Pawn;
        if (pawn?.health?.hediffSet?.HasHediff(agelessDef) == true)
            __result *= 0.5f;
    }
}
