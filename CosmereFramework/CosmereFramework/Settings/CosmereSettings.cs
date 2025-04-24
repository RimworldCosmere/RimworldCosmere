using Verse;

namespace CosmereFramework.Settings {
    public class CosmereSettings : ModSettings {
        public bool debugMode;
        public LogLevel logLevel = LogLevel.Verbose;

        public override void ExposeData() {
            Scribe_Values.Look(ref logLevel, "logLevel", LogLevel.Verbose);
            Scribe_Values.Look(ref debugMode, "debugMode");
        }
    }
}