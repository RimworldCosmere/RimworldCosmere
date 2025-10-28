using System;
using Cosmere.Settings;
using Cosmere.UI;
using Cosmere.Window;
using UnityEngine;
using Verse;

namespace Cosmere;

public enum LogLevel {
    // ReSharper disable once UnusedMember.Global
    None = 0,
    Important,
    Error,
    Warning,
    Info,
    Verbose,
}

public class Mod : CosmereMod<FoundationModSettings> {
    private SettingsWindow? settingsWindow;

    public Mod(ModContentPack content) : base(content) {
        GetSettings<CosmereSettings>();
    }

    public static bool debugMode => GetModSettings<FoundationModSettings>().debugMode;

    public static LogLevel logLevel => GetModSettings<FoundationModSettings>().logLevel;

    public static List<CosmereModSettings> cosmereSettings { get; } = typeof(CosmereModSettings)
        .AllSubclassesNonAbstract()
        .Select(Activator.CreateInstance)
        .Cast<CosmereModSettings>()
        .Where(s => s.Enabled)
        .ToList();

    public static T GetModSettings<T>() where T : CosmereModSettings, new() {
        return (cosmereSettings.First(x => typeof(T).IsInstanceOfType(x)) as T)!;
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        settingsWindow ??= new SettingsWindow(cosmereSettings);
        settingsWindow.DoWindowContents(inRect.ContractedBy(new Padding(32, 0, 0, 0)));
    }

    public override string SettingsCategory() {
        return "Cosmere";
    }
}