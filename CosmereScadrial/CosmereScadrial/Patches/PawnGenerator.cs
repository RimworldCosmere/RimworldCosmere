using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereScadrial.Patches {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [HarmonyPatch(typeof(Verse.PawnGenerator), nameof(Verse.PawnGenerator.GetXenotypeForGeneratedPawn))]
    public static class PawnGenerator {
        private static bool Prefix(PawnGenerationRequest request, ref XenotypeDef __result) {
            // Allow forced xenotype to override
            if (request.ForcedXenotype != null) {
                return true;
            }

            var scenarioName = Find.Scenario?.name;
            var defName = DefDatabase<ScenarioDef>.AllDefsListForReading.First(x => x.label == scenarioName)?.defName;
            if (defName != null && !defName.StartsWith("Cosmere_Scadrial_")) return true;

            if (defName == "Cosmere_Scadrial_PreCatacendre") {
                __result = DefDatabase<XenotypeDef>.GetNamed(new[] {
                    "Cosmere_Terris",
                    "Cosmere_Skaa",
                    "Cosmere_ScadrialNoble",
                }.RandomElement());

                return false;
            }

            if (defName == "Cosmere_Scadrial_PostCatacendre") {
                __result = DefDatabase<XenotypeDef>.GetNamed(new[] {
                    "Cosmere_Terris",
                    "Cosmere_Scadrian",
                }.RandomElement());

                return false;
            }

            return true; // Fallback to vanilla
        }
    }
}