using System;
using Cosmere.Core.Listing;
using Cosmere.Core.Quickstart;
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
    public LogLevel logLevel = LogLevel.Verbose;
    public string? quickstartName;

    public bool showDormantConnection;

    // Faction filtering settings
    public bool disableEmpireInCosmereScenarios;
    public bool disableOdysseyFactionsInCosmereScenarios;

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
            "CC_Settings_Category_Debug".Translate(),
            fieldset => {
                fieldset.Field(
                    "CC_Settings_LogLevel_Label".Translate(),
                    "CC_Settings_LogLevel_Tooltip".Translate(),
                    sub => Util.UI.IntEnumDropdown(sub, logLevel, v => logLevel = v, false)
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
                    sub => Util.UI.Dropdown(
                        sub,
                        GetQuickstartScenarioLabel,
                        quickstartName,
                        "CC_Settings_Quickstarter_Placeholder".Translate(),
                        quickstarters,
                        val => quickstartName = val
                    ),
                    new FieldOptions { minimumColumnWidth = 400 }
                );

                TaggedString? description = GetDescription();
                float descriptionHeight = Text.CalcHeight(description ?? "", listing.ColumnWidth);

                fieldset.Field(
                    sub => {
                        if (string.IsNullOrEmpty(quickstartName)) return;
                        if (description == null) {
                            sub.Label("CC_Settings_Quickstarter_FailedToFind".Translate());
                            return;
                        }

                        using (new TextBlock(TextAnchor.UpperLeft)) sub.Label(description.Value);
                    },
                    new FieldOptions { minimumColumnWidth = 400, height = descriptionHeight }
                );
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

    public override void ExposeData() {
        Scribe_Values.Look(ref showDormantConnection, "showDormantConnection");
        Scribe_Values.Look(ref logLevel, "logLevel", LogLevel.Verbose);
        Scribe_Values.Look(ref debugMode, "debugMode");
        Scribe_Values.Look(ref quickstartName, "quickstartName");
        Scribe_Values.Look(ref disableEmpireInCosmereScenarios, "disableEmpireInCosmereScenarios", false);
        Scribe_Values.Look(ref disableOdysseyFactionsInCosmereScenarios, "disableOdysseyFactionsInCosmereScenarios", false);
    }
}