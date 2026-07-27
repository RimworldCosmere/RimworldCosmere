using System.Reflection;
using HarmonyLib;
using Verse;

namespace Cosmere.System.Roshar.Patch.Nightwatcher;

[HarmonyPatch(typeof(Pawn_AgeTracker), "BiologicalTicksPerTick", MethodType.Getter)]
public static class AgelessBodyPatch {
    private static readonly FieldInfo PawnField =
        AccessTools.Field(typeof(Pawn_AgeTracker), "pawn");

    private static void Postfix(Pawn_AgeTracker __instance, ref float __result) {
        HediffDef? agelessDef = HediffDefOf.Cosmere_Roshar_Hediff_NW_BoonPassive_AgelessBody;
        if (agelessDef == null) return;
        Pawn? pawn = PawnField.GetValue(__instance) as Pawn;
        if (pawn?.health?.hediffSet?.HasHediff(agelessDef) == true) {
            __result *= 0.5f;
        }
    }
}
