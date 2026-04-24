using Cosmere.Core.UI.Dock;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.UI.Harmony;

[HarmonyPatch(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI))]
public static class UIRoot_Play_UIRootOnGUI_Patch {
    [HarmonyPostfix]
    public static void Postfix() {
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;
        InvestitureDockController.UpdateVisibility();
    }
}