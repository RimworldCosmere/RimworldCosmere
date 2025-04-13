using System.Linq;
using CosmereScadrial.Registry;
using CosmereScadrial.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Hediffs.Comps {
    public class BurnMetal : HediffComp {
        private const int TICKS_PER_BURN = 600;
        private int lastBurnTick = -1;
        private float? nextSeverity;
        private bool shouldStop;
        private bool tryingToRefuel;

        public Properties.BurnMetal Props {
            get => (Properties.BurnMetal)props;
        }

        public void Toggle() {
            shouldStop = !shouldStop;
        }

        public void UpdateSeverity(float severity) {
            nextSeverity = severity;
        }

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            if (parent.pawn.IsHashIntervalTick(200)) {
                // Initial burn or re-burn if it's been an hour
                if (lastBurnTick == -1 || Find.TickManager.TicksGame - lastBurnTick >= TICKS_PER_BURN) {
                    TryBurn();
                }
            }

            // every second
            if (parent.pawn.IsHashIntervalTick(60)) {
                TryEmitEffect();
            }
        }

        private void TryEmitEffect() {
            if (!parent.pawn.Spawned || parent.pawn.Map == null) {
                return;
            }

            var mote = MoteMaker.MakeAttachedOverlay(
                parent.pawn,
                DefDatabase<ThingDef>.GetNamed("Mote_ToxicDamage"),
                Vector3.zero
            ); // scale
            mote.exactRotation = Rand.Range(0f, 1f);

            MetalRegistry.Metals.TryGetValue(Props.metal, out var metal);
            if (metal == null) {
                Log.Error($"[CosmereScadrial] Could not find metal with ID {Props.metal} ({string.Join(", ", MetalRegistry.Metals.Keys.ToArray())})");
                return;
            }

            mote.instanceColor = Color.red;
            if (mote.def.mote.needsMaintenance) {
                mote.Maintain();
            }
        }

        public void TryBurn() {
            if (nextSeverity != null) {
                parent.Severity = nextSeverity.Value;
                nextSeverity = null;
            }

            if (shouldStop) {
                Log.Warning($"[CosmereScadrial] Pawn {parent.pawn} has stopped burning {Props.metal}");

                // Remove the hediff
                parent.pawn.health.RemoveHediff(parent);
                return;
            }

            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.unitsPerBurn * parent.Severity;
            Log.Warning($"[CosmereScadrial] Pawn {parent.pawn} is burning {Props.metal} Severity={parent.Severity} BEUs={beuRequired} metalRequired={beuRequired / AllomancyUtility.BEU_PER_METAL_UNIT}");

            if (AllomancyUtility.TryBurnMetalForInvestiture(parent.pawn, Props.metal, beuRequired)) {
                lastBurnTick = Find.TickManager.TicksGame;
                tryingToRefuel = false;
                return;
            }

            if (tryingToRefuel || TryAutoRefuel()) {
                return;
            }

            shouldStop = true;
        }

        private bool TryAutoRefuel() {
            var vial = parent.pawn.inventory?.innerContainer?
                .FirstOrDefault(t => t.def.defName == $"Cosmere_AllomanticVial_{Props.metal.CapitalizeFirst()}");

            if (vial == null || parent.pawn.jobs == null) {
                Log.Warning($"[CosmereScadrial] {parent.pawn.NameFullColored} cannot auto-ingest Cosmere_AllomanticVial_{Props.metal.CapitalizeFirst()}. Doesn't have one.");
                return false;
            }

            var job = JobMaker.MakeJob(CosmereJobDefOf.Cosmere_IngestAndReburn, vial);
            job.count = 1;

            tryingToRefuel = true;
            parent.pawn.jobs.TryTakeOrderedJob(job);
            Log.Error($"{parent.pawn.NameShortColored} is auto-ingesting {vial.LabelShort} for {Props.metal}.");
            return true;
        }
    }
}