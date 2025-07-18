using System;
using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Extension;
using CosmereFramework.Listing;
using CosmereFramework.Quickstart;
using CosmereFramework.Util;
using UnityEngine;
using Verse;

namespace CosmereFramework.Settings;

public class FrameworkModSettings : CosmereModSettings {
    private readonly Dictionary<string, string> quickstarters = typeof(AbstractQuickstart).AllSubclassesNonAbstract()
        .ToDictionary(
            q => $"{q.Assembly.GetName().Name}: {q.Name}",
            q => q.AssemblyQualifiedName
        );

    public bool debugMode;
    public LogLevel logLevel = LogLevel.Verbose;
    public string? quickstartName;

    public override string Name => "Framework";

    public override void DoTabContents(Rect inRect, ListingForm listing) {
        listing.Fieldset(
            "CF_Settings_Category_Debug".Translate(),
            fieldset => {
                fieldset.Field(
                    "CF_Settings_LogLevel_Label".Translate(),
                    "CF_Settings_LogLevel_Tooltip".Translate(),
                    sub => UIUtil.IntEnumDropdown(sub, logLevel, v => logLevel = v, false)
                );

                if (!Prefs.DevMode) return;

                fieldset.Field(
                    "CF_Settings_DebugMode_Label".Translate(),
                    "CF_Settings_DebugMode_Tooltip".Translate(),
                    sub => sub.Checkbox(ref debugMode)
                );

                fieldset.Field(
                    "CF_Settings_Quickstarter_Label".Translate(),
                    "CF_Settings_Quickstarter_Tooltip".Translate(),
                    sub => UIUtil.Dropdown(
                        sub,
                        GetQuickstartScenarioLabel,
                        quickstartName,
                        "CF_Settings_Quickstarter_Placeholder".Translate(),
                        quickstarters,
                        val => quickstartName = val
                    ),
                    new FieldOptions { minimumColumnWidth = 400 }
                );

                fieldset.Field(
                    "",
                    sub => {
                        if (string.IsNullOrEmpty(quickstartName)) {
                            return;
                        }

                        Type? type = Type.GetType(quickstartName!);
                        if (type == null) {
                            Rect errorRect = sub.GetRect(FieldOptions.RowHeight);
                            Widgets.Label(errorRect, "CF_Settings_Quickstarter_FailedToFind".Translate());
                            return;
                        }

                        AbstractQuickstart? quickstart = (AbstractQuickstart)Activator.CreateInstance(type);
                        TaggedString description = quickstart.GetDescription();

                        Widgets.Label(sub.GetRect(Text.CalcHeight(description, sub.ColumnWidth)), description);
                    },
                    new FieldOptions { minimumColumnWidth = 400, height = inRect.height - listing.CurHeight }
                );
            },
            SubListingOptions.WithoutTopPadding()
        );
    }

    private string? GetQuickstartScenarioLabel(string? quickstarter) {
        if (quickstarter == null) return null;
        Type? type = Type.GetType(quickstarter);

        return type == null ? null : $"{type.Assembly.GetName().Name}: {type.Name}";
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref logLevel, "logLevel", LogLevel.Verbose);
        Scribe_Values.Look(ref debugMode, "debugMode");
        Scribe_Values.Look(ref quickstartName, "quickstartName");
    }
}