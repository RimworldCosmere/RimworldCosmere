using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Window), nameof(Window.PreOpen))]
public static class Dialog_OptionsPreOpenPatch {
    public static void Postfix(Window __instance) {
        if (__instance is not Dialog_Options options) {
            return;
        }
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return;
        }

        options.doCloseButton = false;
        options.doCloseX = false;
        options.absorbInputAroundWindow = true;
        options.forcePause = true;
        options.doWindowBackground = false;
        options.drawShadow = false;
        float w = Mathf.Min(UnityEngine.Screen.width * 0.77f, 2200f);
        float h = Mathf.Min(UnityEngine.Screen.height * 0.81f, 1280f);
        options.windowRect = new Rect(
            (UnityEngine.Screen.width - w) / 2f,
            (UnityEngine.Screen.height - h) / 2f,
            w,
            h
        );
    }
}
