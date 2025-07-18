using CosmereRoshar.Comps.WeaponsAndArmor;
using HarmonyLib;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
public static class ShardbladePatchDrop {
    private static void Postfix(Pawn pawn, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos) {
        if (eq != null && pawn != null) {
            if (eq.def == CosmereRosharDefs.WhtwlMeleeWeaponShardblade) {
                CompShardblade blade = eq.GetComp<CompShardblade>();
                if (blade != null) {
                    if (blade.IsBonded(pawn)) {
                        eq.DeSpawn();
                        Log.Message($"Despawning blade bonded with {pawn.NameShortColored}");
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment))]
public static class ShardbladePatchePickup {
    private static void Postfix(Pawn pawn, ThingWithComps newEq) {
        if (newEq != null) {
            if (newEq.def == CosmereRosharDefs.WhtwlMeleeWeaponShardblade) {
                CompShardblade blade = newEq.GetComp<CompShardblade>();
                if (blade != null) {
                    if (blade.IsBonded(null)) {
                        if (!StormlightUtilities.PawnHasAbility(pawn, CosmereRosharDefs.WhtwlSummonShardblade)) {
                            Log.Message($"[stormlight mod] {pawn.Name} picked up an unbounded shardblade!");
                            blade.BondWithPawn(pawn, true);
                        }
                    }
                }
            }
        }
    }
}