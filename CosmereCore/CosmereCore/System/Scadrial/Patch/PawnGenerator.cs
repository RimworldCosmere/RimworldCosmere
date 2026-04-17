using System.Diagnostics.CodeAnalysis;
using Cosmere.Core.Util;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(Verse.PawnGenerator), nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn))]
public static class PawnGenerator {
    [HarmonyPriority(Priority.Low)]
    private static bool Prefix(PawnGenerationRequest request, ref XenotypeDef __result) {
        if (request.ForcedXenotype != null) return true;

        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Preservation, ShardDefOf.Harmony)) return true;

        bool preCatacendre = ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Preservation);

        if (preCatacendre) {
            __result = new[] {
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Skaa,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble,
            }.RandomElement();
            return false;
        }

        __result = new[] {
            XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
            XenotypeDefOf.Cosmere_Scadrial_Xenotype_Scadrian,
        }.RandomElement();
        return false;
    }
}