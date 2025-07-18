using System;
using System.Collections.Generic;
using System.Linq;
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

    public override void DoTabContents(Rect inRect, Listing_Standard mainListing) {
        using (new TextBlock(GameFont.Medium)) mainListing.Label("CF_Settings_Category_Debug".Translate());
        using (new TextBlock(GameFont.Small)) {
            mainListing.GapLine();
            mainListing.Gap();
        }

        float expectedHeight = CategoryPadding * 2 + (SubListingRowHeight + SubListingSpacing) * 1;
        MakeSubListing(
            mainListing,
            expectedHeight,
            (sub, width) => {
                sub.ColumnWidth = SubListingLabelWidth;
                Rect rect = sub.GetRect(SubListingRowHeight);
                Widgets.Label(rect, "CF_Settings_LogLevel".Translate());

                sub.NewColumn();
                sub.ColumnWidth = Mathf.Min(100, width - SubListingLabelWidth - ListingColumnSpacing);
                UIUtil.IntEnumDropdown(sub, logLevel, v => logLevel = v, false);
            }
        );

        if (!Prefs.DevMode) return;
        {
            MakeSubListing(
                mainListing,
                expectedHeight,
                (sub, width) => {
                    sub.ColumnWidth = SubListingLabelWidth;
                    Rect rect = sub.GetRect(SubListingRowHeight);
                    Widgets.Label(rect, "CF_Settings_DebugMode".Translate());

                    sub.NewColumn();
                    sub.ColumnWidth = Mathf.Min(100, width - SubListingLabelWidth - ListingColumnSpacing);
                    sub.CheckboxLabeled("", ref debugMode, "CF_Settings_DebugMode_Tooltip".Translate());
                }
            );

            MakeSubListing(
                mainListing,
                expectedHeight,
                (sub, width) => {
                    sub.ColumnWidth = SubListingLabelWidth;
                    Rect rect = sub.GetRect(SubListingRowHeight);
                    Widgets.Label(rect, "CF_Settings_Quickstarter".Translate());
                    sub.Gap(SubListingSpacing);

                    sub.NewColumn();
                    sub.ColumnWidth = Mathf.Min(400, width - SubListingLabelWidth - ListingColumnSpacing);
                    UIUtil.Dropdown(
                        sub,
                        GetQuickstartScenarioLabel,
                        quickstartName,
                        "CF_Settings_Quickstarter_Placeholder".Translate(),
                        quickstarters,
                        val => quickstartName = val
                    );
                }
            );

            MakeSubListing(
                mainListing,
                inRect.height - mainListing.CurHeight,
                (sub, width) => {
                    sub.ColumnWidth = SubListingLabelWidth;
                    Rect rect = sub.GetRect(SubListingRowHeight);
                    Widgets.Label(rect, "");
                    sub.Gap(SubListingSpacing);

                    sub.NewColumn();
                    sub.ColumnWidth = 400;
                    if (string.IsNullOrEmpty(quickstartName)) {
                        return;
                    }

                    Type? type = Type.GetType(quickstartName!);
                    if (type == null) {
                        Rect errorRect = sub.GetRect(SubListingRowHeight);
                        Widgets.Label(errorRect, "CF_Settings_Quickstarter_FailedToFind".Translate());
                        return;
                    }

                    AbstractQuickstart? quickstart = (AbstractQuickstart)Activator.CreateInstance(type);
                    TaggedString description = quickstart.GetDescription();

                    Widgets.Label(sub.GetRect(Text.CalcHeight(description, sub.ColumnWidth)), description);
                }
            );
        }
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