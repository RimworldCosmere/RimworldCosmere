using Cosmere.Lightweave.Settings;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Patch;

[HarmonyPatch(typeof(Window), nameof(Window.PreOpen))]
public static class Dialog_SaveFileListLoadCtorPatch {
    public static void Postfix(Window __instance) {
        if (__instance is not Dialog_SaveFileList_Load loadDialog) {
            return;
        }
        LightweaveSettings? settings = LightweaveMod.Settings;
        if (settings == null || !settings.RedesignMainMenu) {
            return;
        }

        loadDialog.doCloseButton = false;
        loadDialog.doCloseX = false;
        loadDialog.absorbInputAroundWindow = true;
        loadDialog.forcePause = true;
        loadDialog.doWindowBackground = false;
        loadDialog.drawShadow = false;
        float w = Mathf.Min(UnityEngine.Screen.width * 0.77f, 2200f);
        float h = Mathf.Min(UnityEngine.Screen.height * 0.81f, 1280f);
        loadDialog.windowRect = new Rect(
            (UnityEngine.Screen.width - w) / 2f,
            (UnityEngine.Screen.height - h) / 2f,
            w,
            h
        );
    }
}
