using System;
using Cosmere.Lightweave.ModsConfig;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Page_ModsConfig), nameof(Page_ModsConfig.DoWindowContents))]
public static class Page_ModsConfigRedesignPatch {
    private static readonly Guid RootId = Guid.NewGuid();

    public static bool Prefix(Page_ModsConfig __instance, Rect rect) {
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return true;
        }

        try {
            LightweaveRoot.Render(rect, RootId, () => ModsConfigRoot.Build(
                __instance,
                () => __instance.Close()
            ));
        }
        catch (Exception ex) {
            LightweaveLog.Error("ModsConfig redesign failed: " + ex);
            return true;
        }
        return false;
    }
}
