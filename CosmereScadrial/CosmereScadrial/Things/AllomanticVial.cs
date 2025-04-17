using System.Collections.Generic;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Things {
    public class AllomanticVial : ThingWithComps {
        public override bool IngestibleNow {
            get {
                if (!base.IngestibleNow) {
                    return false;
                }

                var comp = GetComp<Comps.Things.AllomanticVial>();
                var pawn = Find.Selector?.SingleSelectedThing as Pawn; // not reliable for ingestion, fallback in ingest jobs

                // Safeguard — if being evaluated during actual ingestion
                if (Spawned && Position.GetFirstPawn(Map) is { } standingPawn) {
                    pawn = standingPawn;
                }

                if (pawn == null || comp?.props.metals == null) {
                    return false;
                }

                if (comp.props.metals.Count > 1) {
                    return AllomancyUtility.IsMistborn(pawn) && comp.props.metals.Any(metal => AllomancyUtility.GetReservePercent(pawn, metal) < 0.8f);
                }

                var metal = comp.props.metals[0];
                if (!AllomancyUtility.CanUseMetal(pawn, metal)) {
                    return false;
                }

                return AllomancyUtility.GetReservePercent(pawn, metal) < 0.8f;
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn) {
            if (!pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Cosmere_Metalborn"))) {
                yield return new FloatMenuOption("Cannot ingest: Not Metalborn", null);
                yield break;
            }

            var comp = GetComp<Comps.Things.AllomanticVial>();
            if (comp?.props.metals == null) {
                yield return null;
                yield break;
            }

            if (comp.props.metals.Count > 1) {
                if (AllomancyUtility.IsMistborn(pawn) && comp.props.metals.Any(metal => AllomancyUtility.GetReservePercent(pawn, metal) < 0.8f)) {
                    foreach (var option in base.GetFloatMenuOptions(pawn)) {
                        yield return option;
                    }

                    yield break;
                }
            }

            var metal = comp.props.metals[0];
            if (!AllomancyUtility.CanUseMetal(pawn, metal)) {
                yield return new FloatMenuOption("Cannot ingest: wrong metal", null);
                yield break;
            }

            if (AllomancyUtility.GetReservePercent(pawn, metal) >= 0.8f) {
                yield return new FloatMenuOption("Cannot ingest: reserve too full", null);
                yield break;
            }

            // Default ingest option
            foreach (var option in base.GetFloatMenuOptions(pawn)) {
                yield return option;
            }
        }
    }
}