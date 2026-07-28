using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch(typeof(CaravanExitMapUtility))]
public static class CaravanTrackingPatch {
    [Inject(
        At.Return,
        nameof(CaravanExitMapUtility.ExitMapAndCreateCaravan),
        parameterTypes: [
            typeof(IEnumerable<Pawn>),
            typeof(Faction),
            typeof(PlanetTile),
            typeof(PlanetTile),
            typeof(PlanetTile),
            typeof(bool),
        ]
    )]
    private static void AfterExitMapAndCreateCaravan(IEnumerable<Pawn> pawns) {
        foreach (Pawn pawn in pawns) {
            if (pawn.Dead || !pawn.RaceProps.Humanlike) continue;

            Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) continue;

            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_CaravanTrips, 1);
        }
    }
}
