using Cosmere.System.Roshar.Comp.Map;
using HarmonyLib;
using RimWorld;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(MainTabWindow_Architect), nameof(MainTabWindow_Architect.WindowUpdate))]
public static class StormlightOverlayPatch {
    private static string? stormlightCategoryDefName;

    [HarmonyPostfix]
    public static void Postfix(MainTabWindow_Architect __instance) {
        stormlightCategoryDefName ??= "Cosmere_Roshar_DesignationStormlight";

        ArchitectCategoryTab? openTab = __instance.selectedDesPanel;
        if (openTab != null && openTab.def.defName == stormlightCategoryDefName) {
            StormlightOverlayDrawHandler.DrawThisFrame();
        }
    }
}