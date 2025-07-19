using CosmereRoshar.Combat.Abilities.Implementations;
using HarmonyLib;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(Pawn_EquipmentTracker))]
public static class ShardbladePatches {
    [HarmonyPatch(nameof(Pawn_EquipmentTracker.TryDropEquipment))]
    [HarmonyPostfix]
    private static void PostfixTryDropEquipment(
        Pawn_EquipmentTracker __instance,
        ThingWithComps? eq,
        ThingWithComps resultingEq,
        IntVec3 pos
    ) {
        Pawn? pawn = __instance.pawn;
        if (eq == null || pawn == null) return;
        if (!eq.TryGetComp(out ShardBlade blade)) return;
        if (!blade.IsBonded(pawn)) return;

        eq.DeSpawn();
        Log.Message($"Despawning blade bonded with {pawn.NameShortColored}");
    }

    [HarmonyPatch(nameof(Pawn_EquipmentTracker.AddEquipment))]
    [HarmonyPostfix]
    private static void PostfixAddEquipment(Pawn_EquipmentTracker __instance, ThingWithComps? newEq) {
        Pawn? pawn = __instance.pawn;
        if (newEq == null) return;
        if (!newEq.TryGetComp(out ShardBlade blade)) return;
        if (!blade.IsBonded(null)) return;
        if (StormlightUtilities.PawnHasAbility(pawn, CosmereRosharDefs.Cosmere_Roshar_SummonShardblade)) return;
        Log.Message($"[stormlight mod] {pawn.Name} picked up an unbounded shardblade!");
        blade.BondWithPawn(pawn, true);
    }
}