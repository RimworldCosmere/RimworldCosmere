using HarmonyLib;
using RimWorld;
using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.System.Roshar.Patch.World;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(Verse.PawnGenerator), nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn))]
public static class RosharXenotypePatch {
    [HarmonyPriority(Priority.Low)]
    private static bool Prefix(PawnGenerationRequest request, ref XenotypeDef __result) {
        if (request.ForcedXenotype != null) return true;

        // A xenotype only means anything to a humanlike with a gene tracker.
        // Forcing one onto whatever else a map generator asks for - Anomaly's
        // monolith among them - breaks generation further down.
        if (request.KindDef?.RaceProps?.Humanlike != true) return true;


        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Honor, ShardDefOf.Cultivation, ShardDefOf.Odium)) return true;

        // 70% darkeyes, 30% lighteyes - reflecting Rosharan demographics
        __result = Rand.Value < 0.7f
            ? XenotypeDefOf.Cosmere_Roshar_Xenotype_Darkeyes
            : XenotypeDefOf.Cosmere_Roshar_Xenotype_Lighteyes;

        return false;
    }
}