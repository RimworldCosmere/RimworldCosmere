using HarmonyLib;
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
        public CosmereSettings cosmereSettings => GetSettings<CosmereSettings>();

        public override void DoSettingsWindowContents(Rect inRect) {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"Log Level - {cosmereSettings.logLevel.ToString()}");
            cosmereSettings.logLevel = (LogLevel)(int)listingStandard.Slider((float)cosmereSettings.logLevel, (float)LogLevel.None, (float)LogLevel.Verbose);
            listingStandard.CheckboxLabeled("Debug Mode", ref cosmereSettings.debugMode, "Opens the logs.");
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
        }
    }

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Log.Important($"Build Rev: {BuildInfo.REVISION} @ {BuildInfo.BUILD_TIME}. DebugMode={cosmereSettings.debugMode} LogLevel={cosmereSettings.logLevel}");
            new Harmony("cryptiklemur.cosmere.core").PatchAll();
            Log.Verbose("Harmony patches applied.");
        }

        private static CosmereSettings cosmereSettings => LoadedModManager.GetMod<CosmereFramework>().cosmereSettings;
    }
}