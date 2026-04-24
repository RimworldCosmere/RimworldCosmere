using System;
using Cosmere.Core.UI.Radial;
using HarmonyLib;
using RimWorld;

namespace Cosmere.Core.UI.Harmony;

[HarmonyPatch(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI))]
public static class RadialHotkeyPatch {
    [HarmonyPostfix]
    public static void Postfix() {
        try {
            RadialController.OnHotkeyPoll();
        } catch (Exception ex) {
            Logger.Error($"radial hotkey poll failed: {ex}");
        }
    }
}