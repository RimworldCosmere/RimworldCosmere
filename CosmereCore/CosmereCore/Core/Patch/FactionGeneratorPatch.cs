using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Cosmere.Core.Settings;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Patch;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class FactionGeneratorPatch {
    private static readonly FieldInfo? PlayerFactionFactionDef =
        AccessTools.Field(typeof(ScenPart_PlayerFaction), "factionDef");

    private static string? GetPlayerFactionDefName(Scenario scenario) {
        foreach (ScenPart part in scenario.AllParts) {
            if (part is ScenPart_PlayerFaction playerPart) {
                FactionDef? def = PlayerFactionFactionDef?.GetValue(playerPart) as FactionDef;
                return def?.defName;
            }
        }

        return null;
    }

    public static bool IsFactionAllowedForScenario(FactionDef faction) {
        Scenario scenario = Find.Scenario;
        if (scenario == null) {
            return true;
        }

        string scenarioName = scenario.name ?? "";
        bool isScadrialScenario = scenarioName.StartsWith("Mistborn:") ||
                                  scenarioName.Contains("Scadrial");
        bool isRosharScenario = scenarioName.StartsWith("Stormlight:") ||
                                scenarioName.StartsWith("Roshar:");
        bool isCombinedCosmereScenario = scenarioName.StartsWith("Cosmere:");

        if (!isScadrialScenario && !isRosharScenario && !isCombinedCosmereScenario) {
            return true;
        }

        string defName = faction.defName ?? "";
        bool isScadrialFaction = defName.Contains("Scadrial");
        bool isRosharFaction = defName.Contains("Roshar");
        bool isCosmereFaction = isScadrialFaction || isRosharFaction;
        CoreModSettings settings = Mod.GetModSettings<CoreModSettings>();

        if (settings.disableEmpireInCosmereScenarios && defName == "Empire") {
            return false;
        }

        if (settings.disableOdysseyFactionsInCosmereScenarios &&
            (defName == "MechanoidHive" || defName == "InsectGeneline")) {
            return false;
        }

        string? playerFactionDefName = GetPlayerFactionDefName(scenario);
        if (defName == "Cosmere_Scadrial_Faction_FinalEmpireNPC" &&
            playerFactionDefName == "Cosmere_Scadrial_Faction_FinalEmpire") {
            return false;
        }

        if (isCombinedCosmereScenario) {
            return isCosmereFaction;
        }

        if (isScadrialScenario) {
            return isScadrialFaction;
        }

        if (isRosharScenario) {
            return isRosharFaction;
        }

        return false;
    }
}

[HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.ConfigurableFactions), MethodType.Getter)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class FactionGeneratorConfigurableFactionsPatch {
    private static void Postfix(ref IEnumerable<FactionDef> __result) {
        Scenario scenario = Find.Scenario;
        string scenarioName = scenario?.name ?? "(null)";
        Logger.Verbose($"FactionGeneratorPatch: Filtering factions for scenario '{scenarioName}'");
        __result = FilterFactions(__result);
    }

    private static IEnumerable<FactionDef> FilterFactions(IEnumerable<FactionDef> factions) {
        foreach (FactionDef faction in factions) {
            bool allowed = FactionGeneratorPatch.IsFactionAllowedForScenario(faction);
            Logger.Verbose($"FactionGeneratorPatch: {faction.defName} -> {(allowed ? "allowed" : "filtered")}");
            if (allowed) {
                yield return faction;
            }
        }
    }
}

[HarmonyPatch(
    typeof(FactionGenerator),
    nameof(FactionGenerator.CreateFactionAndAddToManager),
    typeof(PlanetLayer),
    typeof(FactionDef)
)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class FactionGeneratorCreateFactionPatch {
    private static bool Prefix(FactionDef facDef) {
        return FactionGeneratorPatch.IsFactionAllowedForScenario(facDef);
    }
}