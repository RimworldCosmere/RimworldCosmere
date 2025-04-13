using System.Linq;
using CosmereScadrial.Registry;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Hediffs.Comps {
    public class BurnMetal : HediffComp {
        private const int TICKS_PER_HOUR = 60000;
        private int lastBurnTick = -1;
        private bool shouldStop;

        public Properties.BurnMetal Props {
            get => (Properties.BurnMetal)props;
        }

        public void Toggle() {
            shouldStop = !shouldStop;
        }

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            if (parent.pawn.IsHashIntervalTick(500)) {
                // Initial burn or re-burn if it's been an hour
                if (lastBurnTick == -1 || Find.TickManager.TicksGame - lastBurnTick >= TICKS_PER_HOUR) {
                    TryBurn();
                }
            }

            // every half second
            if (parent.pawn.IsHashIntervalTick(30)) {
                TryEmitEffect();
            }
        }

        private void TryEmitEffect() {
            if (!parent.pawn.Spawned || parent.pawn.Map == null) {
                return;
            }

            var mote = MoteMaker.MakeStaticMote(
                parent.pawn.DrawPos,
                parent.pawn.Map,
                ThingDefOf.Mote_PsyfocusPulse, // ← swap this for your own graphic later
                1.5f); // scale

            MetalRegistry.Metals.TryGetValue(Props.metal, out var metal);

            mote.instanceColor = metal.Color;
        }

        public void TryBurn() {
            Log.Warning($"[CosmereScadrial] Pawn {parent.pawn} is burning {Props.metal}");
            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.metalUnitsPerHour;

            if (shouldStop) {
                // Remove the hediff
                parent.pawn.health.RemoveHediff(parent);
                return;
            }

            if (AllomancyUtility.TryBurnMetalForInvestiture(parent.pawn, Props.metal, beuRequired)) {
                lastBurnTick = Find.TickManager.TicksGame;
                parent.Severity = 1f;
                return;
            }

            if (TryAutoRefuel(Props.metal, Props.metalUnitsPerHour)) {
                return;
            }

            shouldStop = true;
        }

        private bool TryAutoRefuel(string metal, float amount) {
            var vial = parent.pawn.inventory?.innerContainer?
                .FirstOrDefault(t => t.def.defName == $"Cosmere_Vial_{metal}");

            if (vial == null || parent.pawn.jobs == null) {
                return false;
            }

            var job = JobMaker.MakeJob(CosmereJobDefOf.Cosmere_IngestAndReburn, vial);
            job.count = 1;

            parent.pawn.jobs.TryTakeOrderedJob(job);
            Log.Message($"{parent.pawn.NameShortColored} is auto-ingesting {vial.LabelShort} for {metal}.");
            return true;
        }
    }
}