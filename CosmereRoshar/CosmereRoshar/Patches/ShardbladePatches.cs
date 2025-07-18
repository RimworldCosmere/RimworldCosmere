using HarmonyLib;
using Verse;

namespace CosmereRoshar;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
public static class ShardbladePatchDrop {
    private static void Postfix(Pawn ___pawn, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos) {
        if (eq != null && ___pawn != null) {
            if (eq.def == CosmereRosharDefs.whtwl_MeleeWeapon_Shardblade) {
                CompShardblade blade = eq.GetComp<CompShardblade>();
                if (blade != null) {
                    if (blade.isBonded(___pawn)) {
                        eq.DeSpawn();
                        Log.Message($"Despawning blade bonded with {___pawn.NameShortColored}");
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment))]
public static class ShardbladePatchePickup {
    private static void Postfix(Pawn ___pawn, ThingWithComps newEq) {
        if (newEq != null) {
            if (newEq.def == CosmereRosharDefs.whtwl_MeleeWeapon_Shardblade) {
                CompShardblade blade = newEq.GetComp<CompShardblade>();
                if (blade != null) {
                    if (blade.isBonded(null)) {
                        if (!StormlightUtilities.PawnHasAbility(___pawn, CosmereRosharDefs.whtwl_SummonShardblade)) {
                            Log.Message($"[stormlight mod] {___pawn.Name} picked up an unbounded shardblade!");
                            blade.bondWithPawn(___pawn, true);
                        }
                    }
                }
            }
        }
    }
}