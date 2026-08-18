using System.Reflection;
using Concord;
using Cosmere.Core.Def;
using Cosmere.Core.Settings;
using Cosmere.Core.Util;
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
        // a non-cosmere scenario keeps the full vanilla roster; this guard skips the world lookup below.
        if (!ScenarioDefUtility.IsCosmere) {
            return true;
        }

        // not seeded yet on the worldgen page's first list build; infer instead of allowing everything.
        CosmereWorldDef? world = WorldUtility.Primary ?? WorldUtility.InferFromScenario();
        if (world == null) {
            return true;
        }

        string defName = faction.defName ?? string.Empty;
        CoreModSettings settings = Mod.GetModSettings<CoreModSettings>();

        if (settings.disableEmpireInCosmereScenarios && defName == "Empire") {
            return false;
        }

        // checks defNames Mechanoid/Insect, not labels MechanoidHive/InsectGeneline; the old check was a no-op.
        if (settings.disableOdysseyFactionsInCosmereScenarios &&
            (defName == "Mechanoid" || defName == "Insect")) {
            return false;
        }

        Scenario scenario = Find.Scenario;
        if (scenario != null &&
            defName == "Cosmere_Scadrial_Faction_FinalEmpireNPC" &&
            GetPlayerFactionDefName(scenario) == "Cosmere_Scadrial_Faction_FinalEmpire") {
            return false;
        }

        // hidden factions skip the faction list, but vanilla assumes they exist; a null OfAncients NREs.
        if (faction.hidden) {
            return true;
        }

        CosmereWorldDef? home = WorldUtility.WorldForFaction(faction);

        // a cross-world save reaches every shardworld, so any Cosmere faction belongs there.
        return world.crossWorld ? home != null : home == world;
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

    /// <summary>
    ///     Creates a faction a story beat asked for, bypassing the world gate.
    /// </summary>
    /// <remarks>
    ///     The gate exists to keep another world's factions out of world generation. A scripted
    ///     beat is the opposite case: the story has already decided this faction arrives, so
    ///     turning it away makes the beat fail with nothing logged. Every one of our own callers
    ///     goes through here rather than FactionGenerator directly - a flag set around world
    ///     generation instead would leave the gate off if it ever failed to attach, which is the
    ///     more expensive way to be wrong.
    /// </remarks>
    public static void CreateScripted(FactionDef def) {
        bool previous = scripted;
        scripted = true;
        try {
            FactionGenerator.CreateFactionAndAddToManager(def);
        } finally {
            scripted = previous;
        }
    }

    [global::System.ThreadStatic]
    private static bool scripted;

    internal static bool IsScriptedCreation => scripted;
}

[Patch(typeof(FactionGenerator))]
public static class FactionGeneratorConfigurableFactionsPatch {
    /// <summary>
    ///     Factions we cleared displayInFactionSelection on, so it can be put back. The field has
    ///     exactly one reader in the whole game - WorldFactionsUIUtility.DoWindowContents, which
    ///     uses it for the row loop and the Add menu - so writing it here reaches nothing else.
    /// </summary>
    private static readonly HashSet<FactionDef> concealed = [];

    /// <summary>
    ///     ConfigurableFactions is a yield-return iterator. PatchBody.Declared (the default) attaches to
    ///     the method as written, which hands back the IEnumerable this replaces.
    /// </summary>
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

            // concealing (not dropping) avoids vanilla's wall of yellow warnings for expected factions.
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
        // a scripted story beat outranks the world gate; ordinary world generation does not bypass it.
        if (FactionGeneratorPatch.IsScriptedCreation) return Control.Continue;

        return FactionGeneratorPatch.IsFactionAllowedForScenario(facDef) ? Control.Continue : Control.Cancel;
    }
}
