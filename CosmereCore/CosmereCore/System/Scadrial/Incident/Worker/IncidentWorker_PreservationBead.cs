using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Incident.Worker;

/// <summary>
///     Preservation congeals a bead of lerasium out of its own mists and leaves it where the
///     colony will find it. Vanishingly rare, and gated on Preservation still holding its
///     Shard - after the Catacendre there is nobody left to leave it.
/// </summary>
public class IncidentWorker_PreservationBead : IncidentWorker {
    protected override bool CanFireNowSub(IncidentParms parms) {
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_PreservationBead)) return false;
        return base.CanFireNowSub(parms);
    }

    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map? map = parms.target as Map ?? Find.AnyPlayerHomeMap;
        if (map == null) return false;

        Verse.Thing bead = ThingMaker.MakeThing(Core.ThingDefOf.Lerasium);
        bead.stackCount = 1;

        IntVec3 cell = DropCellFinder.RandomDropSpot(map);
        GenPlace.TryPlaceThing(bead, cell, map, ThingPlaceMode.Near);

        SendStandardLetter(
            def.letterLabel,
            def.letterText,
            def.letterDef ?? LetterDefOf.PositiveEvent,
            parms,
            new TargetInfo(cell, map)
        );

        return true;
    }
}
