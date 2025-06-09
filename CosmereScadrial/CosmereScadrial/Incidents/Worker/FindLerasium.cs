using System.Linq;
using RimWorld;
using Verse;
using ThingDefOf = CosmereMetals.ThingDefOf;

namespace CosmereScadrial.Incidents.Worker {
    public class FindLerasium : IncidentWorker {
        protected override bool CanFireNowSub(IncidentParms parms) {
            return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null;
        }

        protected override bool TryExecuteWorker(IncidentParms parms) {
            var map = (Map)parms.target;
            var chosenPawn = map.mapPawns.FreeColonists.Where(p => !p.Dead && p.Faction == Faction.OfPlayer)
                .RandomElementWithFallback();
            if (chosenPawn == null) return false;

            var bead = ThingMaker.MakeThing(ThingDefOf.Lerasium);
            GenPlace.TryPlaceThing(bead, chosenPawn.Position, map, ThingPlaceMode.Near);

            Find.LetterStack.ReceiveLetter(
                "A bead of Lerasium has appeared!",
                $"{chosenPawn.NameShortColored} feels a strange pull... A small bead of shining metal has appeared nearby.",
                LetterDefOf.PositiveEvent,
                new TargetInfo(chosenPawn.Position, map)
            );

            return true;
        }
    }
}