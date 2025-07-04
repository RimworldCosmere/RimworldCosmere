using System;
using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Settings;
using CosmereFramework.Window;
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

public class CosmereFramework : Mod {
    internal static List<CosmereModSettings> allModSettings = typeof(CosmereModSettings).AllSubclassesNonAbstract()
        .Select(Activator.CreateInstance)
        .Cast<CosmereModSettings>()
        .ToList();

    private SettingsWindow? settingsWindow;

    public CosmereFramework(ModContentPack content) : base(content) {
        GetSettings<CosmereSettings>();
    }

    public static bool debugMode => GetModSettings<FrameworkModSettings>().debugMode;
    public static LogLevel logLevel => GetModSettings<FrameworkModSettings>().logLevel;

    public static List<CosmereModSettings> cosmereSettings => allModSettings;

    public static T GetModSettings<T>() where T : CosmereModSettings, new() {
        return (cosmereSettings.First(x => typeof(T).IsInstanceOfType(x)) as T)!;
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        settingsWindow ??= new SettingsWindow(allModSettings);
        settingsWindow.DoWindowContents(inRect);
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