using Cosmere.Core.UI.Dock;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI))]
public static class UIRootPlayOnGUIPatch {
    [HarmonyPostfix]
    public static void Postfix() {
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;
        InvestitureDockController.UpdateVisibility();
    }
}
