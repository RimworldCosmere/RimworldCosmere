using System.Linq;
using RimWorld;
using Verse;

namespace CosmereScadrial.IncidentWorker;

public class FindLerasium : RimWorld.IncidentWorker {
    protected override bool CanFireNowSub(IncidentParms parms) {
        return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms) {
        Map? map = (Map)parms.target;
        Pawn? chosenPawn = map.mapPawns.FreeColonists.Where(p => !p.Dead && p.Faction == Faction.OfPlayer)
            .RandomElementWithFallback();
        if (chosenPawn == null) return false;

        Verse.Thing? bead = ThingMaker.MakeThing(CosmereResources.ThingDefOf.Lerasium);
        GenPlace.TryPlaceThing(bead, chosenPawn.Position, map, ThingPlaceMode.Near);

        Find.LetterStack.ReceiveLetter(
            "CS_FindLerasium_Title".Translate(),
            "CS_FindLerasium_Message".Translate(chosenPawn.NameShortColored).Resolve(),
            LetterDefOf.PositiveEvent,
            new TargetInfo(chosenPawn.Position, map)
        );

        return true;
    }
}