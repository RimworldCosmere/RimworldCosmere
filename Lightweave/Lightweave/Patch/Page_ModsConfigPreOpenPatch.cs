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
        float w = Mathf.Min(UnityEngine.Screen.width * 0.77f, 2200f);
        float h = Mathf.Min(UnityEngine.Screen.height * 0.81f, 1280f);
        page.windowRect = new Rect(
            (UnityEngine.Screen.width - w) / 2f,
            (UnityEngine.Screen.height - h) / 2f,
            w,
            h
        );
    }
}
