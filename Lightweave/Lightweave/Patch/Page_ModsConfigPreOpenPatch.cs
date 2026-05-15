using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Window), nameof(Window.PreOpen))]
public static class Page_ModsConfigPreOpenPatch {
    public static void Postfix(Window __instance) {
        if (__instance is not Page_ModsConfig page) {
            return;
        }
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return;
        }

        page.doCloseButton = false;
        page.doCloseX = false;
        page.absorbInputAroundWindow = true;
        page.forcePause = true;
        page.doWindowBackground = false;
        page.drawShadow = false;
        // Page overrides Margin to 25 and InnerWindowOnGUI passes
        // rect.ContractedBy(Margin) to DoWindowContents. To make the
        // vignette/scrim cover the full game window, inflate windowRect
        // by Margin*2 so the contraction lands back on the screen rect.
        const float margin = 25f;
        page.windowRect = new Rect(
            -margin,
            -margin,
            UnityEngine.Screen.width + margin * 2f,
            UnityEngine.Screen.height + margin * 2f
        );
    }
}
