using System.Linq;
using System.Text;
using CosmereFramework.Utils;
using CosmereScadrial.DefModExtensions;
using CosmereScadrial.Registry;
using CosmereScadrial.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Hediffs.Comps {
    public class BurnMetal : HediffComp {
        private int flareStartTick = -1;
        private bool tryingToRefuel;

        public string Metal => parent.def.GetModExtension<MetalLinked>().metal;

        public bool IsFlared => parent.Severity >= 2.0f;

        private float FlareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

        public Properties.BurnMetal Props => (Properties.BurnMetal)props;

        public void Stop() {
            Log.Verbose($"Pawn {parent.pawn} has stopped burning {Metal}");

            // Remove the hediff
            parent.pawn.health.RemoveHediff(parent);
        }

        public void UpdateSeverity(float severity) {
            Log.Info($"Severity updated for {Pawn.NameFullColored}:{Metal} {parent.Severity} -> {severity}");
            parent.Severity = severity;

            PortraitsCache.SetDirty(parent.pawn);
            SelectionDrawer.Notify_Selected(parent.pawn);
        }

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            GetSleepSeverity();
            TryBurn();

            if (parent.pawn.IsHashIntervalTick((int)TickUtility.Seconds(3))) {
                TryEmitEffect();
            }

            if (IsFlared) {
                if (flareStartTick < 0) {
                    flareStartTick = Find.TickManager.TicksGame;
                }
            }
            else if (flareStartTick > 0) {
                // Flare ended, throw on a half-strength drag
                ApplyDrag(FlareDuration / 6000f / 2);
                flareStartTick = -1;
            }
        }

        private void GetSleepSeverity() {
            var pawn = parent.pawn;
            var sleeping = pawn.CurJob?.def == JobDefOf.LayDown && pawn.jobs.curDriver is JobDriver_LayDown driver && driver.asleep;

            // If the pawn isn't sleeping, check if we are at the Sleeping severity, and then update it
            if (!sleeping) {
                if (Mathf.Approximately(parent.Severity, 0.5f)) {
                    UpdateSeverity(1.0f);
                }

                return;
            }

            // If we are sleeping, and the severity is less than 1, we are at the Sleeping severity, and we are good.
            if (parent.Severity <= 1.0f) return;

            Log.Verbose($"GetSleepSeverity called for {parent.pawn}:{Metal} currentSeverity={parent.Severity}");

            // If the pawn is sleeping, and is not flaring, and is not at the Sleeping severity, reduce severity
            UpdateSeverity(0.5f);
        }

        public override void CompPostPostRemoved() {
            if (flareStartTick <= 0) return;

            ApplyDrag(FlareDuration / 6000f);
            flareStartTick = -1;
        }

        private void TryEmitEffect() {
            if (!parent.pawn.Spawned || parent.pawn.Map == null) {
                return;
            }

            var mote = MoteMaker.MakeAttachedOverlay(
                parent.pawn,
                DefDatabase<ThingDef>.GetNamed("Mote_ToxicDamage"),
                Vector3.zero
            );

            MetalRegistry.Metals.TryGetValue(Metal, out var metal);
            if (metal == null) {
                Log.Error($"Could not find metal with ID {Metal} ({string.Join(", ", MetalRegistry.Metals.Keys.ToArray())})");
                return;
            }

            mote.instanceColor = metal.Color;
            mote.exactRotation = Rand.Range(0f, 1f);
        }

        public void TryBurn() {
            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.unitsPerBurn * parent.Severity;
            // Log.Verbose($"Pawn {parent.pawn} is burning {Metal} Severity={parent.Severity} BEUs={beuRequired} metalRequired={beuRequired / AllomancyUtility.BEU_PER_METAL_UNIT} DragSeverity={flareDuration / 6000f}");

            if (AllomancyUtility.TryBurnMetalForInvestiture(parent.pawn, Metal, beuRequired)) {
                tryingToRefuel = false;
                return;
            }

            if (tryingToRefuel || TryAutoRefuel()) {
                return;
            }

            Stop();
        }

        public override string CompDebugString() {
            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.unitsPerBurn * parent.Severity;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Drag Severity={FlareDuration / 6000f}");
            stringBuilder.AppendLine($"BEUs Required={beuRequired}");
            stringBuilder.AppendLine($"Metal Required={beuRequired / AllomancyUtility.BEU_PER_METAL_UNIT}");

            return stringBuilder.ToString();
        }

        private bool TryAutoRefuel() {
            var vial = parent.pawn.inventory?.innerContainer?
                .FirstOrDefault(t => t.def.defName == $"Cosmere_AllomanticVial_{Metal.CapitalizeFirst()}");

            if (vial == null || parent.pawn.jobs == null) {
                Log.Warning($"{parent.pawn.NameFullColored} cannot auto-ingest Cosmere_AllomanticVial_{Metal.CapitalizeFirst()}. Doesn't have one.");
                return false;
            }

            var job = JobMaker.MakeJob(CosmereJobDefOf.Cosmere_IngestAndReburn, vial);
            job.count = 1;

            tryingToRefuel = true;
            parent.pawn.jobs.TryTakeOrderedJob(job);
            Log.Warning($"{parent.pawn.NameShortColored} is auto-ingesting {vial.LabelShort} for {Metal}.");
            return true;
        }

        private void ApplyDrag(float severity) {
            if (Props.dragHediff == null || severity <= 1f) return;

            Log.Warning($"Applying {Props.dragHediff.defName} Drag to {parent.pawn.NameFullColored} with Severity={severity}");
            var drag = parent.pawn.health.GetOrAddHediff(Props.dragHediff);
            drag.Severity = severity;
        }
    }
}