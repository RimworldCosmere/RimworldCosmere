using CosmereFramework.Settings;
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
        Listing_Standard listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.Label($"Log Level - {settings.logLevel.ToString()}");
        settings.logLevel = (LogLevel)(int)listingStandard.Slider((float)settings.logLevel, (float)LogLevel.None,
            (float)LogLevel.Verbose);
        listingStandard.CheckboxLabeled("Debug Mode", ref settings.debugMode, "Opens the logs.");
        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
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