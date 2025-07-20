using Cosmere.Roshar.Tab;
using HarmonyLib;
using RimWorld;
using Verse;
using Thing_SpherePouch = Cosmere.Roshar.Comp.Thing.SpherePouch;

namespace Cosmere.Roshar.Patches;

//Give pawn sphere tab when pouch is equipped
[HarmonyPatch(typeof(Pawn_ApparelTracker))]
public static class PawnApparelPatches {
    [HarmonyPatch(nameof(Pawn_ApparelTracker.Wear))]
    [HarmonyPostfix]
    public static void PostfixWear(Pawn_ApparelTracker __instance, Apparel? newApparel) {
        Pawn pawn = __instance.pawn;
        if (pawn == null || newApparel == null) return;

        if (newApparel.TryGetComp<Thing_SpherePouch>() == null) return;
        if (pawn.def.inspectorTabsResolved.Exists(tab => tab is SpherePouch)) return;

        pawn.def.inspectorTabsResolved.Add(new SpherePouch());
    }
}