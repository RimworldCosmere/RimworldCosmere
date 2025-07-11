using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patch;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(Verse.PawnGenerator), nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn))]
public static class PawnGenerator {
    private static bool Prefix(PawnGenerationRequest request, ref XenotypeDef __result) {
        // Allow forced xenotype to override
        if (request.ForcedXenotype != null) return true;

        string? scenarioName = Find.Scenario?.name;
        ScenarioDef? def = DefDatabase<ScenarioDef>.AllDefsListForReading.FirstOrDefault(x => x.label == scenarioName);
        if (def == ScenarioDefOf.Cosmere_Scadrial_PreCatacendre) {
            __result = new[] {
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Skaa,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble,
            }.RandomElement();

            return false;
        }

        if (def == ScenarioDefOf.Cosmere_Scadrial_PostCatacendre) {
            __result = new[] {
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Scadrian,
            }.RandomElement();

            return false;
        }

        return true; // Fallback to vanilla
    }
}