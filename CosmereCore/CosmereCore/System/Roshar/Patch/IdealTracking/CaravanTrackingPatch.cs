using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(
    typeof(CaravanExitMapUtility),
    nameof(CaravanExitMapUtility.ExitMapAndCreateCaravan),
    typeof(IEnumerable<Pawn>),
    typeof(Faction),
    typeof(PlanetTile),
    typeof(PlanetTile),
    typeof(PlanetTile),
    typeof(bool)
)]
public static class CaravanTrackingPatch {
    private static void Postfix(IEnumerable<Pawn> pawns) {
        foreach (Pawn pawn in pawns) {
            if (pawn.Dead || !pawn.RaceProps.Humanlike) continue;

            Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) continue;

            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_CaravanTrips, 1);
        }
    }
}
