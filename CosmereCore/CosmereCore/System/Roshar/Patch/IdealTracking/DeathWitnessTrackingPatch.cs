using Concord;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

/// <summary>
///     Concord allows one whole-method Around per target, so PawnKillPatch owns the single
///     Pawn.Kill injection and calls into both concerns; this type holds the witness half.
/// </summary>
public static class DeathWitnessTracking {
    public static Map? CaptureMapHeld(Pawn pawn) {
        return pawn.MapHeld;
    }

    public static void NotifyWitnesses(Pawn victim, Map? mapHeld) {
        if (!victim.RaceProps.Humanlike) return;
        if (!victim.IsColonist) return;
        if (mapHeld == null) return;

        List<Pawn> colonists = mapHeld.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (colonist == victim) continue;
            if (colonist.Dead) continue;

            Surgebinder? surgebinder = colonist.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) continue;

            surgebinder.OnWitnessedDeath(victim);
        }
    }
}
