using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Gene;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(Verse.PawnGenerator), nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn))]
public static class ScadrialXenotypePatch {
    // A postfix rather than a prefix: skipping the original stopped whatever else
    // it does on the way to a return value, which left vanilla generation to fail
    // on a factionless pawn. Overriding its answer is enough.
    [HarmonyPriority(Priority.Low)]
    private static void Postfix(PawnGenerationRequest request, ref XenotypeDef __result) {
        if (request.ForcedXenotype != null) return;

        // A xenotype only means anything to a humanlike with a gene tracker.
        // Forcing one onto whatever else a map generator asks for - Anomaly's
        // monolith among them - breaks generation further down.
        if (request.KindDef?.RaceProps?.Humanlike != true) return;


        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Preservation, ShardDefOf.Harmony)) return;

        bool preCatacendre = ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Preservation);

        if (preCatacendre) {
            __result = new[] {
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Skaa,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble,
            }.RandomElement();
            return;
        }

        __result = new[] {
            XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
            XenotypeDefOf.Cosmere_Scadrial_Xenotype_Scadrian,
        }.RandomElement();
    }
}