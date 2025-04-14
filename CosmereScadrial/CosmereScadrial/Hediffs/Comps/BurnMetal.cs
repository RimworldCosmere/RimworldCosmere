using System.Linq;
using System.Text;
using CosmereFramework.Utils;
using CosmereScadrial.Comps;
using CosmereScadrial.Registry;
using CosmereScadrial.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Hediffs.Comps {
    public class BurnMetal : HediffComp {
        private int flareStartTick = -1;
        private int lastBurnTick = -1;
        private float? nextSeverity;
        private bool shouldStop;
        private bool tryingToRefuel;

        public bool IsFlared {
            get => parent.Severity >= 2.0f;
        }

        private float flareDuration {
            get => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;
        }

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

            if (parent.pawn.IsHashIntervalTick(Props.tickInterval)) {
                // Initial burn or re-burn if it's been an hour
                if (lastBurnTick == -1 || Find.TickManager.TicksGame - lastBurnTick >= Props.burnInterval) {
                    TryBurn();
                }
            }

            // every second
            if (parent.pawn.IsHashIntervalTick((int)TickUtility.Seconds(3))) {
                TryEmitEffect();
            }

            if (IsFlared) {
                if (flareStartTick < 0) {
                    flareStartTick = Find.TickManager.TicksGame;
                }
            }
            else if (flareStartTick > 0 && !shouldStop) {
                // Flare ended, throw on a half-strength drag
                ApplyDrag(flareDuration / 6000f / 2);
                flareStartTick = -1;
            }
        }

        public override void CompPostPostRemoved() {
            if (flareStartTick <= 0) return;

            ApplyDrag(flareDuration / 6000f);
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

            MetalRegistry.Metals.TryGetValue(Props.metal, out var metal);
            if (metal == null) {
                Log.Error($"Could not find metal with ID {Props.metal} ({string.Join(", ", MetalRegistry.Metals.Keys.ToArray())})");
                return;
            }

            mote.instanceColor = metal.Color;
            mote.exactRotation = Rand.Range(0f, 1f);
        }

        public void TryBurn() {
            if (nextSeverity != null) {
                parent.Severity = nextSeverity.Value;
                nextSeverity = null;
            }

            if (shouldStop) {
                Log.Verbose($"Pawn {parent.pawn} has stopped burning {Props.metal}");

                // Remove the hediff
                parent.pawn.health.RemoveHediff(parent);

                // Toggle Ability off
                var ability = parent.pawn.abilities.abilities.FirstOrDefault(x =>
                    x.def.defName.StartsWith($"Cosmere_Ability_Burn{Props.metal.CapitalizeFirst()}") &&
                    (x.CompOfType<CompToggleBurnMetal>()?.burning ?? false)
                );
                if (ability != null) {
                    ability.CompOfType<CompToggleBurnMetal>().burning = false;
                }

                return;
            }

            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.unitsPerBurn * parent.Severity;
            Log.Verbose(
                $"Pawn {parent.pawn} is burning {Props.metal} Severity={parent.Severity} BEUs={beuRequired} metalRequired={beuRequired / AllomancyUtility.BEU_PER_METAL_UNIT} DragSeverity={flareDuration / 6000f}");

            if (AllomancyUtility.TryBurnMetalForInvestiture(parent.pawn, Props.metal, beuRequired)) {
                lastBurnTick = Find.TickManager.TicksGame;
                tryingToRefuel = false;
                return;
            }

            if (tryingToRefuel || TryAutoRefuel()) {
                return;
            }

            shouldStop = true;
            parent.Severity = 1f;
        }

        public override string CompDebugString() {
            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.unitsPerBurn * parent.Severity;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Drag Severity={flareDuration / 6000f}");
            stringBuilder.AppendLine($"BEUs Required={beuRequired}");
            stringBuilder.AppendLine($"Metal Required={beuRequired / AllomancyUtility.BEU_PER_METAL_UNIT}");

            return stringBuilder.ToString();
        }

        private bool TryAutoRefuel() {
            var vial = parent.pawn.inventory?.innerContainer?
                .FirstOrDefault(t => t.def.defName == $"Cosmere_AllomanticVial_{Props.metal.CapitalizeFirst()}");

            if (vial == null || parent.pawn.jobs == null) {
                Log.Warning($"{parent.pawn.NameFullColored} cannot auto-ingest Cosmere_AllomanticVial_{Props.metal.CapitalizeFirst()}. Doesn't have one.");
                return false;
            }

            var job = JobMaker.MakeJob(CosmereJobDefOf.Cosmere_IngestAndReburn, vial);
            job.count = 1;

            tryingToRefuel = true;
            parent.pawn.jobs.TryTakeOrderedJob(job);
            Log.Warning($"{parent.pawn.NameShortColored} is auto-ingesting {vial.LabelShort} for {Props.metal}.");
            return true;
        }

        private void ApplyDrag(float severity) {
            if (severity <= 0f) return;

            var burnoutDef = DefDatabase<HediffDef>.GetNamedSilentFail($"Cosmere_Hediff_{Props.metal.CapitalizeFirst()}Drag");
            if (burnoutDef == null) return;

            Log.Warning($"Applying Drag to {parent.pawn.NameFullColored} with Severity={severity}");
            var drag = parent.pawn.health.GetOrAddHediff(burnoutDef);
            drag.Severity = severity;
        }
    }
}