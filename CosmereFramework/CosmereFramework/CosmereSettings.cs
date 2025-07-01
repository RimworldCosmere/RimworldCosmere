using Verse;

namespace CosmereFramework;

public class CosmereSettings : ModSettings {
    public bool debugMode;
    public LogLevel logLevel = LogLevel.Verbose;
    public string? quickstartName;

    public override void ExposeData() {
        Scribe_Values.Look(ref logLevel, "logLevel", LogLevel.Verbose);
        Scribe_Values.Look(ref debugMode, "debugMode");
        Scribe_Values.Look(ref quickstartName, "quickstartName");
    }
}