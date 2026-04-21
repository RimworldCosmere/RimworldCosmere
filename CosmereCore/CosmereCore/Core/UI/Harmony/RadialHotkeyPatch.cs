using Cosmere.Core;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.UI.Harmony;

[HarmonyPatch(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI))]
public static class RadialHotkeyPatch {
    [HarmonyPostfix]
    public static void Postfix() {
        try {
            Cosmere.Core.UI.Radial.RadialController.OnHotkeyPoll();
        } catch (global::System.Exception ex) {
            Logger.Error($"radial hotkey poll failed: {ex}");
        }
    }
}
