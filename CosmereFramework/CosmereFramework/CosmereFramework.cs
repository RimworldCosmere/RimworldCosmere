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

    public class CosmereFramework : Mod {
        public CosmereFramework(ModContentPack content) : base(content) { }

        public CosmereSettings Settings {
            get => GetSettings<CosmereSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"Log Level - {Settings.logLevel.ToString()}");
            Settings.logLevel = (LogLevel)(int)listingStandard.Slider((float)Settings.logLevel, (float)LogLevel.None, (float)LogLevel.Verbose);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() {
            return "Cosmere";
        }
    }

    public class CosmereSettings : ModSettings {
        public LogLevel logLevel = LogLevel.Verbose;

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref logLevel, "logLevel", LogLevel.Verbose);
        }
    }
}