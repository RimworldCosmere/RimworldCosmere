using System;
using System.Reflection;
using Cosmere.Lightweave.MainMenu;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.MainMenuOnGUI))]
public static class MainMenuRedesignPatch {
    private static readonly Guid RootId = Guid.NewGuid();

    private static readonly FieldInfo? AnyMapFilesField = AccessTools.Field(typeof(MainMenuDrawer), "anyMapFiles");

    public static bool Prefix() {
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return true;
        }

        if (Current.ProgramState != ProgramState.Entry) {
            return true;
        }

        try {
            Rect screen = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
            bool anyMapFiles = AnyMapFilesField?.GetValue(null) is bool b && b;
            LightweaveRoot.Render(screen, RootId, () => MainMenuRoot.Build(anyMapFiles));
        }
        catch (Exception ex) {
            LightweaveLog.Error("Main menu redesign failed: " + ex);
            return true;
        }

        return false;
    }
}
