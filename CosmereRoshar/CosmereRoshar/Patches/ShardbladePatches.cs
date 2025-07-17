using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace CosmereRoshar {
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
    public static class ShardbladePatchDrop {
        static void Postfix(Pawn ___pawn, ThingWithComps eq, ThingWithComps resultingEq, IntVec3 pos) {
            if (eq != null && ___pawn != null) {
                if (eq.def == CosmereRosharDefs.whtwl_MeleeWeapon_Shardblade) {
                    CompShardblade blade = eq.GetComp<CompShardblade>();
                    if (blade != null) {
                        if (blade.isBonded(___pawn)) { eq.DeSpawn(); Log.Message($"Despawning blade bonded with {___pawn.NameShortColored}"); }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment))]
    public static class ShardbladePatchePickup {
        static void Postfix(Pawn ___pawn, ThingWithComps newEq) {
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

    [HarmonyPatch(typeof(Mineable), "TrySpawnYield")]
    [HarmonyPatch(new Type[] { typeof(Map), typeof(bool), typeof(Pawn) })]
    public static class PatchSpawnYieldAfterMining {
        static void Postfix(Map map, bool moteOnWaste, Pawn pawn) {

            if (map != null) {
                ThingDef sphereThing = StormlightUtilities.RollForRandomGemSpawn();
                if (sphereThing != null) {
                    Thing sphere = ThingMaker.MakeThing(sphereThing);
                    IntVec3 dropPosition = pawn.Position;
                    GenPlace.TryPlaceThing(sphere, dropPosition, map, ThingPlaceMode.Direct);
                }
            }

        }

    }

}