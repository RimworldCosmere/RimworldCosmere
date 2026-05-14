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
        loadDialog.windowRect = new Rect(0f, 0f, UnityEngine.Screen.width, UnityEngine.Screen.height);
    }
}
