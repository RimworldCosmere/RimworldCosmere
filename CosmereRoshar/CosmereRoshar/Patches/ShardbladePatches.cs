using CosmereRoshar.Combat.Abilities.Implementations;
using HarmonyLib;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(Pawn_EquipmentTracker))]
public static class ShardbladePatches {
    [HarmonyPatch(nameof(Pawn_EquipmentTracker.TryDropEquipment))]
    [HarmonyPostfix]
    private static void PostfixTryDropEquipment(Pawn pawn, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos) {
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

    [HarmonyPatch(nameof(Pawn_EquipmentTracker.AddEquipment))]
    [HarmonyPostfix]
    private static void PostfixAddEquipment(Pawn pawn, ThingWithComps? newEq) {
        if (newEq == null) return;
        if (!newEq.def.Equals(CosmereRosharDefs.WhtwlMeleeWeaponShardblade)) return;
        if (!newEq.TryGetComp(out ShardBlade blade)) return;
        if (!blade.IsBonded(null)) return;
        if (StormlightUtilities.PawnHasAbility(pawn, CosmereRosharDefs.WhtwlSummonShardblade)) return;
        Log.Message($"[stormlight mod] {pawn.Name} picked up an unbounded shardblade!");
        blade.BondWithPawn(pawn, true);
    }
}