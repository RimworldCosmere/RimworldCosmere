using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar;

//Give pawn sphere tab when pouch is equipped
[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
public static class PawnApparelPatch_Add {
    public static void Postfix(Pawn_ApparelTracker __instance, Apparel newApparel) {
        Pawn pawn = __instance.pawn;
        if (pawn == null || newApparel == null) return;

        if (newApparel.TryGetComp<CompSpherePouch>() != null) {
            if (!pawn.def.inspectorTabsResolved.Exists(tab => tab is ITab_SpherePouch)) {
                pawn.def.inspectorTabsResolved.Add(new ITab_SpherePouch());
            }
        }
    }
}