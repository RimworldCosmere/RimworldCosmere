using HarmonyLib;
using RimWorld;
using Verse;
using Cosmere.System.Roshar.Surgebinding.Ability.Abrasion;
using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

namespace Cosmere.System.Roshar.Patch.Soulcasting;

[HarmonyPatch(typeof(MapInterface), nameof(MapInterface.MapInterfaceUpdate))]
public static class SoulcastOverlayPatch {
    private static void Postfix() {
        Map? map = Find.CurrentMap;
        if (map != null) {
            SoulcastOverlay.Draw(map);
            FrictionTrapOverlay.Draw();
        }
    }
}

[HarmonyPatch(typeof(DesignationManager), nameof(DesignationManager.RemoveDesignation))]
public static class DesignationRemovedPatch {
    private static void Postfix(DesignationManager __instance, Designation des) {
        if (des.def == Designator_Soulcast.DesignationDef) {
            SoulcastOverlay.Remove(des.target.Cell, __instance.map);
        }
    }
}
