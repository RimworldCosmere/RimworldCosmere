using Cosmere.System.Roshar.Surgebinding.Ability.Abrasion;
using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(MapInterface), nameof(MapInterface.MapInterfaceUpdate))]
public static class SoulcastOverlayPatch {
    private static void Postfix() {
        if (Find.CurrentMap != null) {
            SoulcastOverlay.Draw();
            FrictionTrapOverlay.Draw();
        }
    }
}

[HarmonyPatch(typeof(DesignationManager), nameof(DesignationManager.RemoveDesignation))]
public static class DesignationRemovedPatch {
    private static void Postfix(Designation des) {
        if (des.def == Designator_Soulcast.DesignationDef) {
            SoulcastOverlay.Remove(des.target.Cell);
        }
    }
}