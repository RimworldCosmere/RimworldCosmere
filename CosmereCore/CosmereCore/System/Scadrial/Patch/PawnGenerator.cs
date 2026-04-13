using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[HarmonyPatch(typeof(Verse.PawnGenerator), nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn))]
public static class PawnGenerator {
    private static bool Prefix(PawnGenerationRequest request, ref XenotypeDef __result) {
        // Allow forced xenotype to override
        if (request.ForcedXenotype != null) return true;

        Scenario? scenario = Find.Scenario;
        if (scenario == null) return true;

        if (scenario.name == ScenarioDefOf.Cosmere_Scadrial_Scenario_PreCatacendre.label) {
            __result = new[] {
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Skaa,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble,
            }.RandomElement();

            return false;
        }

        if (scenario.name == ScenarioDefOf.Cosmere_Scadrial_Scenario_PostCatacendre.label) {
            __result = new[] {
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
                XenotypeDefOf.Cosmere_Scadrial_Xenotype_Scadrian,
            }.RandomElement();

            return false;
        }

        return true; // Fallback to vanilla
    }
}