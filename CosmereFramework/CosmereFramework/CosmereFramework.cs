using System;
using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Quickstart;
using UnityEngine;
using Verse;

namespace CosmereFramework;

public enum LogLevel {
    // ReSharper disable once UnusedMember.Global
    None = 0,
    Important,
    Error,
    Warning,
    Info,
    Verbose,
}

public class CosmereFramework(ModContentPack content) : Mod(content) {
    public static CosmereFramework cosmereFrameworkMod => LoadedModManager.GetMod<CosmereFramework>();

    public static CosmereSettings cosmereSettings => cosmereFrameworkMod.settings;

    public CosmereSettings settings => GetSettings<CosmereSettings>();

    public override void DoSettingsWindowContents(Rect inRect) {
        const float categoryPadding = 10f;
        const float categoryInset = 30f;
        const float subListingSpacing = 6f;
        const float subListingLabelWidth = 100f;
        const float subListingRowHeight = 30f;
        const float listingColumnSpacing = 17f;
        float expectedHeight = categoryPadding * 2 + (subListingRowHeight + subListingSpacing) * 1;

        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.Label($"Log Level - {settings.logLevel.ToString()}");
        settings.logLevel = (LogLevel)(int)listingStandard.Slider((float)settings.logLevel, (float)LogLevel.None,
            (float)LogLevel.Verbose);
        listingStandard.CheckboxLabeled("Debug Mode", ref settings.debugMode, "Opens the logs.");

        MakeSubListing(listingStandard, 0, expectedHeight, categoryPadding, categoryInset, subListingSpacing,
            (sub, width) => {
                sub.ColumnWidth = subListingLabelWidth;

                Rect rect = sub.GetRect(subListingRowHeight);
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(rect, "Quickstarter:");
                sub.Gap(subListingSpacing);

                sub.NewColumn();
                sub.ColumnWidth = width - subListingLabelWidth - listingColumnSpacing;
                MakeSelectScenarioButton(sub, settings);
            });

        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
    }

    private void MakeSelectScenarioButton(Listing_Standard sub, CosmereSettings settings) {
        const float buttonHeight = 30f;
        Rect buttonRect = sub.GetRect(buttonHeight);
        string? selected = settings.quickstartName;
        string label = settings.quickstartName ?? "Select a Quickstarter";
        if (selected != null) {
            Type? type = Type.GetType(settings.quickstartName);
            if (type == null) {
                settings.quickstartName = null;
            } else {
                label = $"{type.Assembly.GetName().Name}: {type.Name}";
            }
        }

        if (Widgets.ButtonText(buttonRect, label)) {
            List<FloatMenuOption> options =
                new List<FloatMenuOption>([new FloatMenuOption("None", () => { settings.quickstartName = null; })]);

            options.AddRange(typeof(AbstractQuickstart).AllSubclassesNonAbstract().Select(s => {
                return new FloatMenuOption($"{s.Assembly.GetName().Name}: {s.Name}",
                    () => { settings.quickstartName = s.AssemblyQualifiedName; });
            }));
            Find.WindowStack.Add(new FloatMenu(options));
        }

        sub.Gap(sub.verticalSpacing);
    }

    private void MakeSubListing(
        Listing_Standard mainListing,
        float width,
        float allocatedHeight,
        float padding,
        float extraInset,
        float verticalSpacing,
        Action<Listing_Standard, float> drawContents
    ) {
        Rect subRect = mainListing.GetRect(allocatedHeight);
        width = width > 0 ? width : subRect.width - (padding + extraInset);
        subRect = new Rect(subRect.x + padding + extraInset, subRect.y + padding, width, subRect.height - padding * 2f);
        Listing_Standard sub = new Listing_Standard { verticalSpacing = verticalSpacing };
        sub.Begin(subRect);
        drawContents(sub, width);
        sub.End();
    }

    public override string SettingsCategory() {
        return "Cosmere";
    }
}

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Framework");
    }
}