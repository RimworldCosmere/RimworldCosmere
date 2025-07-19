using CosmereRoshar.Comp.Apparel;
using CosmereRoshar.ITabs;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches;

//Give pawn sphere tab when pouch is equipped
[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
public static class PawnApparelPatchAdd {
    public static void Postfix(Pawn_ApparelTracker instance, Apparel newApparel) {
        Pawn pawn = instance.pawn;
        if (pawn == null || newApparel == null) return;

        if (newApparel.TryGetComp<CompSpherePouch>() != null) {
            if (!pawn.def.inspectorTabsResolved.Exists(tab => tab is TabSpherePouch)) {
                pawn.def.inspectorTabsResolved.Add(new TabSpherePouch());
            }
        }
    }
}