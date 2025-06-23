using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(Verse.PawnGenerator), nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn))]
public static class PawnGenerator {
    private static bool Prefix(PawnGenerationRequest request, ref XenotypeDef __result) {
        // Allow forced xenotype to override
        if (request.ForcedXenotype != null) {
            return true;
        }

        string? scenarioName = Find.Scenario?.name;
        string? defName = DefDatabase<ScenarioDef>.AllDefsListForReading.First(x => x.label == scenarioName)?.defName;
        if (defName != null && !defName.StartsWith("Cosmere_Scadrial_")) return true;

        if (defName == "Cosmere_Scadrial_PreCatacendre") {
            __result = DefDatabase<XenotypeDef>.GetNamed(new[] {
                "Cosmere_Scadrial_Xenotype_Terris",
                "Cosmere_Scadrial_Xenotype_Skaa",
                "Cosmere_Scadrial_Xenotype_Noble",
            }.RandomElement());

            return false;
        }

        if (defName == "Cosmere_Scadrial_PostCatacendre") {
            __result = DefDatabase<XenotypeDef>.GetNamed(new[] {
                "Cosmere_Scadrial_Xenotype_Terris",
                "Cosmere_Scadrial_Xenotype_Scadrian",
            }.RandomElement());

            return false;
        }

        return true; // Fallback to vanilla
    }
}