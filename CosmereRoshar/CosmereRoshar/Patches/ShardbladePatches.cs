using CosmereRoshar.Combat.Abilities.Implementations;
using HarmonyLib;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
public static class ShardbladePatchDrop {
    private static void Postfix(Pawn pawn, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos) {
        if (eq != null && pawn != null) {
            if (eq.def == CosmereRosharDefs.WhtwlMeleeWeaponShardblade) {
                ShardBlade blade = eq.GetComp<ShardBlade>();
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

// @TODO Move to CosmereRoshar/Patches/PawnEquipmentTrackerPatches.cs
[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment))]
public static class ShardbladePatchePickup {
    private static void Postfix(Pawn pawn, ThingWithComps? newEq) {
        if (newEq == null) return;
        if (!newEq.def.Equals(CosmereRosharDefs.WhtwlMeleeWeaponShardblade)) return;
        if (!newEq.TryGetComp(out ShardBlade blade)) return;
        if (!blade.IsBonded(null)) return;
        if (StormlightUtilities.PawnHasAbility(pawn, CosmereRosharDefs.WhtwlSummonShardblade)) return;
        Log.Message($"[stormlight mod] {pawn.Name} picked up an unbounded shardblade!");
        blade.BondWithPawn(pawn, true);
    }
}