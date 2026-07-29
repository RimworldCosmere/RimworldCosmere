using System;
using Cosmere.Core.Settings;
using Cosmere.Core.UI;
using Cosmere.Core.Window;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Cosmere.Core;

public enum LogLevel {
    // ReSharper disable once UnusedMember.Global
    None = 0,
    Important,
    Error,
    Warning,
    Info,
    Verbose,
}

public class Mod : CosmereMod<CoreModSettings> {
    private static readonly Dictionary<Type, CosmereModSettings> settingsByType = [];
    private static List<CosmereModSettings>? settingsList;

    private SettingsWindow? settingsWindow;

    public Mod(ModContentPack content) : base(content) {
        GetSettings<CosmereSettings>();
    }

    public static bool debugMode => GetModSettings<CoreModSettings>().debugMode;

    public static LogLevel logLevel => GetModSettings<CoreModSettings>().logLevel;

    public static List<CosmereModSettings> cosmereSettings => settingsList ??= BuildSettingsList();

    public static T GetModSettings<T>()
        where T : CosmereModSettings, new() {
        if (settingsByType.TryGetValue(typeof(T), out CosmereModSettings? cached)) return (T)cached;
        EnsureSettingsBuilt();
        return (T)settingsByType[typeof(T)];
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        settingsWindow ??= new SettingsWindow(cosmereSettings);
        settingsWindow.Draw(inRect);
    }

    public bool ConsumeSettingsCloseRequest() {
        return settingsWindow?.ConsumeCloseRequest() ?? false;
    }

    public override string SettingsCategory() {
        return "Cosmere";
    }

    private static void EnsureSettingsBuilt() {
        if (settingsList == null) settingsList = BuildSettingsList();
    }

    private static List<CosmereModSettings> BuildSettingsList() {
        List<CosmereModSettings> result = [];
        IEnumerable<Type> types = typeof(CosmereModSettings).AllSubclassesNonAbstract();
        foreach (Type t in types) {
            try {
                CosmereModSettings instance = (CosmereModSettings)Activator.CreateInstance(t);
                if (!instance.Enabled) continue;
                settingsByType[t] = instance;
                result.Add(instance);
            } catch (Exception ex) {
                Logger.Error($"Failed to instantiate settings type {t.FullName}: {ex}");
            }
        }

        return result;
    }
}
