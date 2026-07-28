using System.Reflection;
using Concord;
using Cosmere.Core.Settings;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.Core.Patch;

public static class FactionGeneratorPatch {
    private static readonly FieldInfo? PlayerFactionFactionDef = typeof(ScenPart_PlayerFaction).GetField(
        "factionDef",
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
    );

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

        string scenarioName = scenario.name ?? string.Empty;
        bool isScadrialScenario = scenarioName.StartsWith("Mistborn:") ||
                                  scenarioName.Contains("Scadrial");
        bool isRosharScenario = scenarioName.StartsWith("Stormlight:") ||
                                scenarioName.StartsWith("Roshar:");
        bool isCombinedCosmereScenario = scenarioName.StartsWith("Cosmere:");

        if (!isScadrialScenario && !isRosharScenario && !isCombinedCosmereScenario) {
            return true;
        }

        string defName = faction.defName ?? string.Empty;
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

        return isRosharFaction;
    }
}

[Patch(typeof(FactionGenerator))]
public static class FactionGeneratorConfigurableFactionsPatch {
    // ConfigurableFactions is a yield-return iterator. PatchBody.Declared (the default) attaches to
    // the method as written, which hands back the IEnumerable this replaces.
    [Inject(At.Return, "get_" + nameof(FactionGenerator.ConfigurableFactions))]
    private static void AfterConfigurableFactions(ControlHandle<IEnumerable<FactionDef>> ch) {
        Scenario scenario = Find.Scenario;
        string scenarioName = scenario?.name ?? "(null)";
        Logger.Verbose($"FactionGeneratorPatch: Filtering factions for scenario '{scenarioName}'");
        ch.ReturnValue = FilterFactions(ch.ReturnValue);
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

[Patch(typeof(FactionGenerator))]
public static class FactionGeneratorCreateFactionPatch {
    [Inject(
        At.Head,
        nameof(FactionGenerator.CreateFactionAndAddToManager),
        parameterTypes: [typeof(PlanetLayer), typeof(FactionDef)]
    )]
    private static Control BeforeCreateFactionAndAddToManager(FactionDef facDef) {
        return FactionGeneratorPatch.IsFactionAllowedForScenario(facDef) ? Control.Continue : Control.Cancel;
    }
}
