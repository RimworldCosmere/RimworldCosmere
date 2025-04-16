using System.Linq;
using System.Text;
using CosmereFramework.Utils;
using CosmereScadrial.Comps;
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

        public ToggleBurnMetalStatus Status => Parent.Severity switch {
            <= 0f => ToggleBurnMetalStatus.Off,
            <= 0.5f => ToggleBurnMetalStatus.Passive,
            <= 1f => ToggleBurnMetalStatus.Burning,
            _ => ToggleBurnMetalStatus.Flaring,
        };

        public BurningMetal Parent => parent as BurningMetal;

        public string Metal => Parent.def.GetModExtension<MetalLinked>().metal;

        public bool IsAtLeastPassive => Parent.Severity >= 0.5f;

        public bool IsFlared => Parent.Severity >= 2.0f;

        private float FlareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

        public Properties.BurnMetal Props => (Properties.BurnMetal)props;

        public override string CompLabelInBracketsExtra => $"{Status}";

        public void ToggleBurn() {
            ToggleBurn(Parent.Severity >= 1.0f);
        }

        public void ToggleBurn(bool nextBurning) {
            switch (nextBurning) {
                case true when Parent.Severity >= 1.0f:
                    return;
                case false when Mathf.Approximately(Parent.Severity, 0f):
                    break;
            }

            UpdateSeverity(nextBurning ? 1f : 0f);
        }

        public void UpdateSeverity(float severity) {
            if (Mathf.Approximately(severity, 1f) && !Mathf.Approximately(Parent.Severity, severity)) {
                Log.Info($"Pawn {Parent.pawn} has started burning {Metal}");
            }

            if (Mathf.Approximately(severity, 0f) && !Mathf.Approximately(Parent.Severity, severity)) {
                Log.Info($"Pawn {Parent.pawn} has stopped burning {Metal}");
                CompPostPostRemoved();
            }

            Log.Verbose($"Severity updated for {Pawn.NameFullColored}:{Metal} {Parent.Severity} -> {severity}");
            Parent.Severity = severity;
        }

        public override void CompPostTick(ref float severityAdjustment) {
            if (IsAtLeastPassive) {
                GetSleepSeverity();
                TryBurn();

                if (Parent.pawn.IsHashIntervalTick((int)TickUtility.Seconds(3))) {
                    TryEmitEffect();
                }

                if (IsFlared) {
                    if (flareStartTick >= 0) return;

                    flareStartTick = Find.TickManager.TicksGame;
                    var drag = Pawn.health.hediffSet.GetFirstHediffOfDef(Props.dragHediff);
                    if (drag == null) return;

                    var existingSeverity = drag.Severity;
                    Pawn.health.RemoveHediff(drag);
                    flareStartTick -= (int)(existingSeverity * 3000f);

                    // Don't want to apply the drag below, if we are still flared.
                    return;
                }
            }

            if (flareStartTick <= 0) return;

            // Flare ended, throw on a half-strength drag
            ApplyDrag(FlareDuration / 3000f / 2);
            flareStartTick = -1;
        }

        private void GetSleepSeverity() {
            var severity = Parent.Severity;
            var pawn = Parent.pawn;
            var sleeping = pawn.CurJob?.def == JobDefOf.LayDown && pawn.jobs.curDriver is JobDriver_LayDown driver && driver.asleep;

            // If the pawn isn't sleeping, check if we are at the Sleeping severity, and then update it
            if (!sleeping) {
                if (Mathf.Approximately(severity, 0.5f)) {
                    UpdateSeverity(1.0f);
                }

                return;
            }

            // If we are sleeping, and the severity is less than 1, we are at the Sleeping severity, and we are good.
            if (severity < 1.0f) return;

            // If the pawn is sleeping, and is not flaring, and is not at the Sleeping severity, reduce severity
            UpdateSeverity(0.5f);
        }

        public override void CompPostPostRemoved() {
            if (flareStartTick <= 0) return;

            ApplyDrag(FlareDuration / 3000f);
            flareStartTick = -1;
        }

        private void TryEmitEffect() {
            if (!Parent.pawn.Spawned || Parent.pawn.Map == null) {
                return;
            }

            var mote = MoteMaker.MakeAttachedOverlay(
                Parent.pawn,
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
            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.unitsPerBurn * Parent.Severity;
            Log.Verbose($"Pawn {Parent.pawn} is burning {Metal} Severity={Parent.Severity} BEUs={beuRequired} metalRequired={beuRequired / AllomancyUtility.BEU_PER_METAL_UNIT} DragSeverity={FlareDuration / 6000f}");

            if (AllomancyUtility.TryBurnMetalForInvestiture(Parent.pawn, Metal, beuRequired)) {
                tryingToRefuel = false;
                return;
            }

            if (tryingToRefuel || TryAutoRefuel()) {
                return;
            }

            ToggleBurn(false);
        }

        public override string CompDebugString() {
            var beuRequired = AllomancyUtility.BEU_PER_METAL_UNIT * Props.unitsPerBurn * Parent.Severity;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Drag Severity={FlareDuration / 3000f:0.000}");
            stringBuilder.AppendLine($"BEUs Required={beuRequired:0.000}");
            stringBuilder.AppendLine($"Metal Required={beuRequired / AllomancyUtility.BEU_PER_METAL_UNIT:0.000}");
            stringBuilder.AppendLine($"Hidden={!Parent.Visible}");

            return stringBuilder.ToString();
        }

        private bool TryAutoRefuel() {
            var vial = Parent.pawn.inventory?.innerContainer?
                .FirstOrDefault(t => t.def.defName == $"Cosmere_AllomanticVial_{Metal.CapitalizeFirst()}");

            if (vial == null || Parent.pawn.jobs == null) {
                Log.Warning($"{Parent.pawn.NameFullColored} cannot auto-ingest Cosmere_AllomanticVial_{Metal.CapitalizeFirst()}. Doesn't have one.");
                return false;
            }

            var job = JobMaker.MakeJob(CosmereJobDefOf.Cosmere_IngestAndReburn, vial);
            job.count = 1;

            tryingToRefuel = true;
            Parent.pawn.jobs.TryTakeOrderedJob(job);
            Log.Warning($"{Parent.pawn.NameShortColored} is auto-ingesting {vial.LabelShort} for {Metal}.");
            return true;
        }

        private void ApplyDrag(float severity) {
            if (Props.dragHediff == null || severity <= 1f) return;

            Log.Warning($"Applying {Props.dragHediff.defName} Drag to {Parent.pawn.NameFullColored} with Severity={severity}");
            var drag = Parent.pawn.health.GetOrAddHediff(Props.dragHediff);
            drag.Severity = severity;
        }
    }
}