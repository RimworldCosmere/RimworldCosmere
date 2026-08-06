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

        // Mechanoid and Insect, not MechanoidHive and InsectGeneline - those are the labels.
        // The setting had never removed anything.
        if (settings.disableOdysseyFactionsInCosmereScenarios &&
            (defName == "Mechanoid" || defName == "Insect")) {
            return false;
        }

        string? playerFactionDefName = GetPlayerFactionDefName(scenario);
        if (defName == "Cosmere_Scadrial_Faction_FinalEmpireNPC" &&
            playerFactionDefName == "Cosmere_Scadrial_Faction_FinalEmpire") {
            return false;
        }

        // Hidden factions never reach the faction list or the world map, and vanilla assumes
        // they exist - Faction.OfAncients is null without one, which NREs PawnGenerator.
        if (faction.hidden) {
            return true;
        }

        if (isCombinedCosmereScenario) {
            return isCosmereFaction;
        }

        if (isScadrialScenario) {
            return isScadrialFaction;
        }

        return isRosharFaction;
    }

    /// <summary>
    ///     The three factions Page_CreateWorldParams prints a yellow warning about when they are
    ///     missing - broken Royalty quests, no mech clusters, no infestations. Sound advice in a
    ///     vanilla game and noise in a Cosmere one, where their absence is the premise.
    /// </summary>
    public static bool IsWarnedAboutWhenMissing(FactionDef faction) {
        string defName = faction.defName ?? string.Empty;
        return defName == "Empire" || defName == "Mechanoid" || defName == "Insect";
    }
}

[Patch(typeof(FactionGenerator))]
public static class FactionGeneratorConfigurableFactionsPatch {
    /// <summary>
    ///     Factions we cleared displayInFactionSelection on, so it can be put back. The field has
    ///     exactly one reader in the whole game - WorldFactionsUIUtility.DoWindowContents, which
    ///     uses it for the row loop and the Add menu - so writing it here reaches nothing else.
    /// </summary>
    private static readonly HashSet<FactionDef> concealed = [];

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

            // Dropping one of the warned-about three outright makes the worldgen page print a wall
            // of yellow about content this scenario never had. Leave it in the list with its row
            // hidden instead: vanilla sees it present and stays quiet, and
            // CreateFactionAndAddToManager still refuses to build it, so it never reaches the world.
            if (!allowed && FactionGeneratorPatch.IsWarnedAboutWhenMissing(faction)) {
                Conceal(faction);
                Logger.Verbose($"FactionGeneratorPatch: {faction.defName} -> concealed");
                yield return faction;
                continue;
            }

            Reveal(faction);
            Logger.Verbose($"FactionGeneratorPatch: {faction.defName} -> {(allowed ? "allowed" : "filtered")}");
            if (allowed) {
                yield return faction;
            }
        }
    }

    private static void Conceal(FactionDef faction) {
        if (!faction.displayInFactionSelection) return;
        faction.displayInFactionSelection = false;
        concealed.Add(faction);
    }

    private static void Reveal(FactionDef faction) {
        if (concealed.Remove(faction)) faction.displayInFactionSelection = true;
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
