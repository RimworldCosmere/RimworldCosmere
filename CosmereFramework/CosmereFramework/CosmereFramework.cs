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
        public CosmereSettings Settings => GetSettings<CosmereSettings>();

        public override void DoSettingsWindowContents(Rect inRect) {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"Log Level - {Settings.logLevel.ToString()}");
            Settings.logLevel = (LogLevel)(int)listingStandard.Slider((float)Settings.logLevel, (float)LogLevel.None, (float)LogLevel.Verbose);
            listingStandard.CheckboxLabeled("Debug Mode", ref Settings.debugMode, "Opens the logs.");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() {
            return "Cosmere";
        }
    }

    public class CosmereSettings : ModSettings {
        public bool debugMode;
        public LogLevel logLevel = LogLevel.Verbose;

        public override void ExposeData() {
            Scribe_Values.Look(ref logLevel, "logLevel", LogLevel.Verbose);
            Scribe_Values.Look(ref debugMode, "debugMode");
            base.ExposeData();
        }
    }
}