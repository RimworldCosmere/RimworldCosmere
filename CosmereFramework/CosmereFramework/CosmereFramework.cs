using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereFramework {
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
        public CosmereSettings settings => GetSettings<CosmereSettings>();

        public override void DoSettingsWindowContents(Rect inRect) {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"Log Level - {settings.logLevel.ToString()}");
            settings.logLevel = (LogLevel)(int)listingStandard.Slider((float)settings.logLevel, (float)LogLevel.None, (float)LogLevel.Verbose);
            listingStandard.CheckboxLabeled("Debug Mode", ref settings.debugMode, "Opens the logs.");
            DrawScenarioDropdown(listingStandard);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        private void DrawScenarioDropdown(Listing_Standard listingStandard) {
            // Fetch all `ScenarioDef` objects.
            var scenarios = DefDatabase<ScenarioDef>.AllDefsListForReading;

            // Create a list of labels (scenario names) and a lookup dictionary.
            var scenarioLabels = new List<string>();
            var scenarioLookup = new Dictionary<string, ScenarioDef>();
            foreach (var scenario in scenarios) {
                scenarioLabels.Add(scenario.label); // Display label for dropdown.
                scenarioLookup[scenario.label] = scenario; // Map label to ScenarioDef.
            }

            // Get the current selected label based on `quickstartScenario` from settings.
            var currentSelectionLabel = scenarioLookup.Values
                                            .FirstOrDefault(scenario => scenario.defName == settings.quickstartScenario)?.label
                                        ?? ScenarioDefOf.Crashlanded.defName; // Fallback if quickstartScenario doesn't point to any ScenarioDef.

            // Dropdown logic using Widgets.DropDown.
            if (!Widgets.ButtonText(listingStandard.GetRect(30f), currentSelectionLabel)) return;

            var options = scenarioLabels.Select(label => new FloatMenuOption(label, delegate {
                    // Update Settings.quickstartScenario when clicked.
                    settings.quickstartScenario = scenarioLookup[label].defName;
                }))
                .ToList();

            Find.WindowStack.Add(new FloatMenu(options));
        }

        public override string SettingsCategory() {
            return "Cosmere";
        }
    }

    public class CosmereSettings : ModSettings {
        public bool debugMode;
        public LogLevel logLevel = LogLevel.Verbose;
        public string quickstartScenario = "Cosmere_Scadrial_PreCatacendre";

        public override void ExposeData() {
            Scribe_Values.Look(ref logLevel, "logLevel", LogLevel.Verbose);
            Scribe_Values.Look(ref debugMode, "debugMode");
            Scribe_Values.Look(ref quickstartScenario, "quickstartScenario", "Crashlanded");
        }
    }
}