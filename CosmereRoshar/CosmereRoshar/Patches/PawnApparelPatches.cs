using CosmereRoshar.Comp.Apparel;
using CosmereRoshar.Tab;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches;

//Give pawn sphere tab when pouch is equipped
[HarmonyPatch(typeof(Pawn_ApparelTracker))]
public static class PawnApparelPatches {
    [HarmonyPatch(nameof(Pawn_ApparelTracker.Wear))]
    [HarmonyPostfix]
    public static void PostfixWear(Pawn_ApparelTracker instance, Apparel? newApparel) {
        Pawn pawn = instance.pawn;
        if (pawn == null || newApparel == null) return;

        if (newApparel.TryGetComp<CompSpherePouch>() == null) return;
        if (pawn.def.inspectorTabsResolved.Exists(tab => tab is SpherePouch)) return;

        pawn.def.inspectorTabsResolved.Add(new SpherePouch());
    }
}