using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.UI.Harmony;

[HarmonyLib.HarmonyPatch(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI))]
public static class RadialHotkeyPatch {
    [HarmonyLib.HarmonyPostfix]
    public static void Postfix() {
        Cosmere.Core.UI.Radial.RadialController.OnHotkeyPoll();
    }
}
