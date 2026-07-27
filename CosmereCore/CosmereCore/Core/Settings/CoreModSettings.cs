using System;
using Cosmere.Core.Listing;
using Cosmere.Core.Quickstart;
using Cosmere.Core.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Settings;

public class CoreModSettings : CosmereModSettings {
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

    public bool showDormantConnection;
    public string? testScenarioDefName;

    public override string Name => "Core";

    public override void DoTabContents(Form listing) {
        listing.Fieldset(
            "CC_Settings_Category_Connection".Translate(),
            fieldset => {
                fieldset.Field(
                    "CC_Settings_Connection_ShowDormantConnection_Label".Translate(),
                    "CC_Settings_Connection_ShowDormantConnection_Tooltip".Translate(),
                    sub => sub.Checkbox(ref showDormantConnection)
                );
            },
            SubListingOptions.WithoutTopPadding()
        );

        listing.Fieldset(
            "CC_Settings_Category_FactionFiltering".Translate(),
            fieldset => {
                fieldset.Field(
                    "CC_Settings_DisableEmpire_Label".Translate(),
                    "CC_Settings_DisableEmpire_Tooltip".Translate(),
                    sub => sub.Checkbox(ref disableEmpireInCosmereScenarios)
                );
                fieldset.Field(
                    "CC_Settings_DisableOdyssey_Label".Translate(),
                    "CC_Settings_DisableOdyssey_Tooltip".Translate(),
                    sub => sub.Checkbox(ref disableOdysseyFactionsInCosmereScenarios)
                );
            },
            SubListingOptions.WithoutTopPadding()
        );

        listing.Fieldset(
            "CC_Settings_Category_Radial".Translate(),
            fieldset => {
                fieldset.Field(
                    "CC_Settings_RadialAnchorMouse_Label".Translate(),
                    "CC_Settings_RadialAnchorMouse_Tooltip".Translate(),
                    sub => sub.Checkbox(ref radialAnchorMouse)
                );
                fieldset.Field(
                    "CC_Settings_RadialPause_Label".Translate(),
                    "CC_Settings_RadialPause_Tooltip".Translate(),
                    sub => sub.Checkbox(ref radialPausesGame)
                );
            },
            SubListingOptions.WithoutTopPadding()
        );

        listing.Fieldset(
            "CC_Settings_Category_Debug".Translate(),
            fieldset => {
                fieldset.Field(
                    "CC_Settings_LogLevel_Label".Translate(),
                    "CC_Settings_LogLevel_Tooltip".Translate(),
                    sub => UIHelpers.IntEnumDropdown(sub, logLevel, v => logLevel = v, false)
                );

                if (!Prefs.DevMode) return;

                fieldset.Field(
                    "CC_Settings_DebugMode_Label".Translate(),
                    "CC_Settings_DebugMode_Tooltip".Translate(),
                    sub => sub.Checkbox(ref debugMode)
                );

                fieldset.Field(
                    "CC_Settings_Quickstarter_Label".Translate(),
                    "CC_Settings_Quickstarter_Tooltip".Translate(),
                    sub => UIHelpers.Dropdown(
                        sub,
                        GetQuickstartScenarioLabel,
                        quickstartName,
                        "CC_Settings_Quickstarter_Placeholder".Translate(),
                        quickstarters,
                        val => quickstartName = val
                    ),
                    new FieldOptions { minimumColumnWidth = 400 }
                );

                if (IsScenarioTestQuickstartSelected()) {
                    fieldset.Field(
                        "CC_Settings_TestScenario_Label".Translate(),
                        "CC_Settings_TestScenario_Tooltip".Translate(),
                        sub => UIHelpers.Dropdown(
                            sub,
                            GetTestScenarioLabel,
                            testScenarioDefName,
                            "CC_Settings_TestScenario_Placeholder".Translate(),
                            GetScenarioDefItems(),
                            val => testScenarioDefName = val
                        ),
                        new FieldOptions { minimumColumnWidth = 400 }
                    );
                }

                if (!string.IsNullOrEmpty(quickstartName)) {
                    TaggedString? description = GetDescription();
                    if (description == null) {
                        fieldset.Label("CC_Settings_Quickstarter_FailedToFind".Translate());
                    } else {
                        using (new TextBlock(TextAnchor.UpperLeft)) {
                            fieldset.Label(description.Value);
                        }
                    }
                }
            },
            SubListingOptions.WithoutTopPadding().WithTextBlock(new TextBlock(TextAnchor.MiddleLeft))
        );
    }

    private TaggedString? GetDescription() {
        if (string.IsNullOrEmpty(quickstartName)) return null;
        Type? type = Type.GetType(quickstartName!);
        if (type == null) {
            return null;
        }

        AbstractQuickstart? quickstart = (AbstractQuickstart)Activator.CreateInstance(type);
        return quickstart.GetDescription().Resolve();
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
    }
}
