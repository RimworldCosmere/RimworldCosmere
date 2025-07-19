using HarmonyLib;
using RimWorld;
using Verse;
using SpherePouch = CosmereRoshar.Comp.Thing.SpherePouch;

namespace CosmereRoshar.Patches;

//Give pawn sphere tab when pouch is equipped
[HarmonyPatch(typeof(Pawn_ApparelTracker))]
public static class PawnApparelPatches {
    [HarmonyPatch(nameof(Pawn_ApparelTracker.Wear))]
    [HarmonyPostfix]
    public static void PostfixWear(Pawn_ApparelTracker instance, Apparel? newApparel) {
        Pawn pawn = instance.pawn;
        if (pawn == null || newApparel == null) return;

        if (newApparel.TryGetComp<SpherePouch>() == null) return;
        if (pawn.def.inspectorTabsResolved.Exists(tab => tab is Tab.SpherePouch)) return;

        pawn.def.inspectorTabsResolved.Add(new Tab.SpherePouch());
    }
}