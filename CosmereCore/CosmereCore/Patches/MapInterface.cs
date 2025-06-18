using CosmereCore.Utils;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace CosmereCore.Patches {
    [HarmonyPatch]
    public static class MapInterface {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RimWorld.MapInterface), nameof(RimWorld.MapInterface.MapInterfaceOnGUI_BeforeMainTabs))]
        public static void PostfixMapInterfaceOnGUI_BeforeMainTabs() {
            if (Find.CurrentMap == null || !WorldRendererUtility.DrawingMap) return;
            MapCosmereGizmoUtility.MapUIOnGUI();
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RimWorld.MapInterface), nameof(RimWorld.MapInterface.MapInterfaceUpdate))]
        public static void PostfixMapInterfaceUpdate() {
            if (Find.CurrentMap == null || !WorldRendererUtility.DrawingMap) return;
            MapCosmereGizmoUtility.MapUIUpdate();
        }
    }
}