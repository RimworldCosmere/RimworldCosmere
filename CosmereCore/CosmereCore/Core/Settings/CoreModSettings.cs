using System;
using Cosmere.Core.BetaHub;
using Cosmere.Core.Framework;
using Cosmere.Core.Quickstart;
using Cosmere.Core.Settings.Model;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Settings;

public class CoreModSettings : CosmereModSettings {
    public const float DefaultDockSectionMaxHeight = 600f;

    private const float MinDockSectionMaxHeight = 200f;
    private const float MaxDockSectionMaxHeight = 1200f;

    private readonly Dictionary<string, string> quickstarters = typeof(AbstractQuickstart).AllSubclassesNonAbstract()
        .ToDictionary(
            q => $"{q.Assembly.GetName().Name}: {q.Name}",
            q => q.AssemblyQualifiedName
        );

    public bool debugMode;

    // Faction filtering settings
    public bool disableEmpireInCosmereScenarios;
    public bool disableOdysseyFactionsInCosmereScenarios;
    public bool highContrast;
    public LogLevel logLevel = LogLevel.Verbose;
    public string? quickstartName;

    public bool radialAnchorMouse = true;
    public bool radialPausesGame;
    public bool reduceMotion;

    // Where the player dragged the investiture dock to, in screen pixels. Held unclamped, so it can
    // legitimately sit slightly outside the screen when dragged into a corner - which is why a
    // separate flag records whether it was ever set rather than reading a sentinel out of the value.
    public Vector2 dockPosition;
    public bool dockPositionSet;

    // The dock grows to whatever the selected pawn carries, and a Mistborn's sixteen
    // metals with a detail panel open runs past the bottom of most screens. Past this
    // the open section scrolls rather than the dock getting taller.
    public float dockSectionMaxHeight = DefaultDockSectionMaxHeight;

    public bool showDormantConnection;
    public string? testScenarioDefName;

    public bool showFeedbackButtons = true;
    public string? feedbackDiscordUsername;

    public override string Name => "Core";

    public override string DisplayLabel => (string)"CC_Settings_System_Core".Translate();

    public override IReadOnlyList<SettingSection> BuildSections() {
        IReadOnlyList<SettingSection> sections = [
            new SettingSection(
                "connection",
                "CC_Settings_Category_Connection",
                [
                    new SettingDescriptor(
                        "show-dormant-connection",
                        "CC_Settings_Connection_ShowDormantConnection_Label",
                        "CC_Settings_Connection_ShowDormantConnection_Description",
                        new CheckboxControl(
                            () => showDormantConnection,
                            updated => showDormantConnection = updated,
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "faction-filtering",
                "CC_Settings_Category_FactionFiltering",
                [
                    new SettingDescriptor(
                        "disable-empire",
                        "CC_Settings_DisableEmpire_Label",
                        "CC_Settings_DisableEmpire_Description",
                        new CheckboxControl(
                            () => disableEmpireInCosmereScenarios,
                            updated => disableEmpireInCosmereScenarios = updated,
                            false
                        )
                    ),
                    new SettingDescriptor(
                        "disable-odyssey-factions",
                        "CC_Settings_DisableOdyssey_Label",
                        "CC_Settings_DisableOdyssey_Description",
                        new CheckboxControl(
                            () => disableOdysseyFactionsInCosmereScenarios,
                            updated => disableOdysseyFactionsInCosmereScenarios = updated,
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "ability-radial",
                "CC_Settings_Category_Radial",
                [
                    new SettingDescriptor(
                        "anchor-mouse",
                        "CC_Settings_RadialAnchorMouse_Label",
                        "CC_Settings_RadialAnchorMouse_Description",
                        new CheckboxControl(
                            () => radialAnchorMouse,
                            updated => radialAnchorMouse = updated,
                            true
                        )
                    ),
                    new SettingDescriptor(
                        "pause-game",
                        "CC_Settings_RadialPause_Label",
                        "CC_Settings_RadialPause_Description",
                        new CheckboxControl(
                            () => radialPausesGame,
                            updated => radialPausesGame = updated,
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "interface",
                "CC_Settings_Category_Interface",
                [
                    new SettingDescriptor(
                        "reset-dock-position",
                        "CC_Settings_ResetDockPosition_Label",
                        "CC_Settings_ResetDockPosition_Description",
                        new ButtonControl(
                            "CC_Settings_ResetDockPosition_Button",
                            () => {
                                dockPositionSet = false;
                                dockPosition = Vector2.zero;
                            },
                            () => dockPositionSet ? null : "CC_Settings_ResetDockPosition_Default"
                        )
                    ),
                    new SettingDescriptor(
                        "dock-section-max-height",
                        "CC_Settings_DockSectionMaxHeight_Label",
                        "CC_Settings_DockSectionMaxHeight_Description",
                        new SliderControl(
                            () => dockSectionMaxHeight,
                            updated => dockSectionMaxHeight = Mathf.Round(updated / 10f) * 10f,
                            DefaultDockSectionMaxHeight,
                            MinDockSectionMaxHeight,
                            MaxDockSectionMaxHeight,
                            10f,
                            value => value.ToString("0")
                        )
                    ),
                    new SettingDescriptor(
                        "show-feedback-buttons",
                        "CC_Settings_ShowFeedbackButtons_Label",
                        "CC_Settings_ShowFeedbackButtons_Description",
                        new CheckboxControl(
                            () => showFeedbackButtons,
                            updated => showFeedbackButtons = updated,
                            true
                        ),
                        () => BetaHubGate.IsBetaRevision(BuildInfo.Revision)
                    ),
                ]
            ),
            new SettingSection(
                "accessibility",
                "CC_Settings_Category_Accessibility",
                [
                    new SettingDescriptor(
                        "reduce-motion",
                        "CC_Settings_ReduceMotion_Label",
                        "CC_Settings_ReduceMotion_Description",
                        new CheckboxControl(
                            () => reduceMotion,
                            updated => reduceMotion = updated,
                            false
                        )
                    ),
                    new SettingDescriptor(
                        "high-contrast",
                        "CC_Settings_HighContrast_Label",
                        "CC_Settings_HighContrast_Description",
                        new CheckboxControl(
                            () => highContrast,
                            updated => highContrast = updated,
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "debug",
                "CC_Settings_Category_Debug",
                [
                    new SettingDescriptor(
                        "log-level",
                        "CC_Settings_LogLevel_Label",
                        "CC_Settings_LogLevel_Description",
                        new ChoiceControl(
                            () => logLevel.ToString(),
                            updated => logLevel = Enum.Parse<LogLevel>(updated!),
                            nameof(LogLevel.Verbose),
                            () => [
                                new Choice(nameof(LogLevel.None), "CC_Settings_LogLevel_None"),
                                new Choice(nameof(LogLevel.Important), "CC_Settings_LogLevel_Important"),
                                new Choice(nameof(LogLevel.Error), "CC_Settings_LogLevel_Error"),
                                new Choice(nameof(LogLevel.Warning), "CC_Settings_LogLevel_Warning"),
                                new Choice(nameof(LogLevel.Info), "CC_Settings_LogLevel_Info"),
                                new Choice(nameof(LogLevel.Verbose), "CC_Settings_LogLevel_Verbose"),
                            ],
                            false
                        )
                    ),
                    new SettingDescriptor(
                        "debug-mode",
                        "CC_Settings_DebugMode_Label",
                        "CC_Settings_DebugMode_Description",
                        new CheckboxControl(
                            () => debugMode,
                            updated => debugMode = updated,
                            false
                        ),
                        () => Prefs.DevMode
                    ),
                    new SettingDescriptor(
                        "quickstarter",
                        "CC_Settings_Quickstarter_Label",
                        "CC_Settings_Quickstarter_Description",
                        new ChoiceControl(
                            () => quickstartName,
                            updated => quickstartName = updated,
                            null,
                            GetQuickstarterChoices,
                            true
                        ),
                        () => Prefs.DevMode
                    ),
                    new SettingDescriptor(
                        "test-scenario",
                        "CC_Settings_TestScenario_Label",
                        "CC_Settings_TestScenario_Description",
                        new ChoiceControl(
                            () => testScenarioDefName,
                            updated => testScenarioDefName = updated,
                            null,
                            GetScenarioChoices,
                            true
                        ),
                        () => Prefs.DevMode && IsScenarioTestQuickstartSelected()
                    ),
                ]
            ),
        ];

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate(Name, sections);
        foreach (string error in errors) {
            Logger.Error($"Settings descriptor validation failed: {error}");
        }

        return sections;
    }

    private IReadOnlyList<Choice> GetQuickstarterChoices() {
        List<Choice> choices = [];
        foreach (KeyValuePair<string, string> quickstarter in quickstarters) {
            string? label = GetQuickstartScenarioLabel(quickstarter.Value);
            choices.Add(Choice.Literal(quickstarter.Value, label ?? quickstarter.Key));
        }

        return choices;
    }

    private string? GetQuickstartScenarioLabel(string? quickstarter) {
        if (quickstarter == null) return null;
        Type? type = Type.GetType(quickstarter);

        return type == null ? null : $"{type.Assembly.GetName().Name}: {type.Name}";
    }

    private bool IsScenarioTestQuickstartSelected() {
        if (string.IsNullOrEmpty(quickstartName)) return false;
        Type? type = Type.GetType(quickstartName!);
        return type == typeof(ScenarioTestQuickstart);
    }

    private string? GetTestScenarioLabel(string? defName) {
        if (string.IsNullOrEmpty(defName)) return null;
        ScenarioDef? def = DefDatabase<ScenarioDef>.GetNamedSilentFail(defName);
        return def == null ? defName : def.LabelCap.ToString();
    }

    private IReadOnlyList<Choice> GetScenarioChoices() {
        Dictionary<string, string> scenarioDefs = GetScenarioDefItems();
        List<Choice> choices = [];
        foreach (KeyValuePair<string, string> scenarioDef in scenarioDefs) {
            string? label = GetTestScenarioLabel(scenarioDef.Value);
            choices.Add(Choice.Literal(scenarioDef.Value, label ?? scenarioDef.Key));
        }

        return choices;
    }

    private static Dictionary<string, string> GetScenarioDefItems() {
        Dictionary<string, string> items = new Dictionary<string, string>();
        List<ScenarioDef> defs = DefDatabase<ScenarioDef>.AllDefsListForReading;
        for (int i = 0; i < defs.Count; i++) {
            ScenarioDef def = defs[i];
            string label = $"{def.LabelCap} ({def.defName})";
            items[label] = def.defName;
        }

        return items;
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref showDormantConnection, "showDormantConnection");
        Scribe_Values.Look(ref logLevel, "logLevel", LogLevel.Verbose);
        Scribe_Values.Look(ref debugMode, "debugMode");
        Scribe_Values.Look(ref quickstartName, "quickstartName");
        Scribe_Values.Look(ref testScenarioDefName, "testScenarioDefName");
        Scribe_Values.Look(ref disableEmpireInCosmereScenarios, "disableEmpireInCosmereScenarios");
        Scribe_Values.Look(ref disableOdysseyFactionsInCosmereScenarios, "disableOdysseyFactionsInCosmereScenarios");
        Scribe_Values.Look(ref reduceMotion, "reduceMotion");
        Scribe_Values.Look(ref highContrast, "highContrast");
        Scribe_Values.Look(ref radialAnchorMouse, "radialAnchorMouse", true);
        Scribe_Values.Look(ref radialPausesGame, "radialPausesGame");
        Scribe_Values.Look(ref dockPosition, "dockPosition");
        Scribe_Values.Look(ref dockPositionSet, "dockPositionSet");
        Scribe_Values.Look(ref dockSectionMaxHeight, "dockSectionMaxHeight", DefaultDockSectionMaxHeight);
        Scribe_Values.Look(ref showFeedbackButtons, "showFeedbackButtons", true);
        Scribe_Values.Look(ref feedbackDiscordUsername, "feedbackDiscordUsername");
    }
}
