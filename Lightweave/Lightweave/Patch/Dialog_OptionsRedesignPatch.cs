using System;
using Cosmere.Lightweave.Options;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents))]
public static class Dialog_OptionsRedesignPatch {
    private static readonly Guid RootId = Guid.NewGuid();

    public static bool Prefix(Dialog_Options __instance, Rect inRect) {
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return true;
        }

        try {
            LightweaveRoot.Render(inRect, RootId, () => OptionsRoot.Build(
                __instance,
                () => __instance.Close()
            ));
        }
        catch (Exception ex) {
            LightweaveLog.Error("Options redesign failed: " + ex);
            return true;
        }
        return false;
    }
}
