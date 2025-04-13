using System;
using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Hediffs.Comps;
using RimWorld;
using Verse;
using Verse.AI;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Jobs {
    public class IngestAndReburn : JobDriver_Ingest {
        protected override IEnumerable<Toil> MakeNewToils() {
            foreach (var toil in base.MakeNewToils()) {
                yield return toil;
            }

            // Inject a final "Reburn" step
            yield return new Toil {
                initAction = () => {
                    var metal = ExtractMetalNameFromThing(TargetThingA.def.defName);

                    var burnComp = pawn.health.hediffSet.hediffs
                        .OfType<HediffWithComps>()
                        .SelectMany(h => h.comps.OfType<BurnMetal>())
                        .FirstOrDefault(c => c.Props.metal == metal);

                    if (burnComp == null) return;
                    Log.Info($"{pawn.LabelShort} auto-burning {metal} after ingestion.");
                    burnComp.TryBurn();
                },
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
        }

        private string ExtractMetalNameFromThing(string defName) {
            return defName.StartsWith("Cosmere_Vial_", StringComparison.OrdinalIgnoreCase) ? defName.Substring("Cosmere_Vial_".Length) : defName;
        }
    }
}