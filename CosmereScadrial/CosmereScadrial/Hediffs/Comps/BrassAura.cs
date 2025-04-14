using System.Linq;
using CosmereFramework.Utils;
using CosmereScadrial.Registry;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Hediffs.Comps {
    public class BrassAura : HediffComp {
        private Mote mote;

        private Properties.BrassAura Props {
            get => (Properties.BrassAura)props;
        }

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            if (!parent.pawn.IsHashIntervalTick(Props.tickInterval)) {
                return;
            }

            var radius = Props.radius * parent.Severity;
            var center = parent.pawn.Position;
            var map = parent.pawn.Map;

            if (map == null || !center.IsValid) {
                return;
            }

            var nearbyPawns = GenRadial.RadialCellsAround(center, radius, true)
                .Select(cell => cell.GetFirstPawn(map))
                .Where(p => p != null && p != parent.pawn);

            foreach (var pawn in nearbyPawns) {
                TrySoothe(pawn);
            }

            // every second
            if (parent.pawn.IsHashIntervalTick((int)TickUtility.Seconds(3))) {
                if (mote == null || mote.Destroyed) {
                    TryEmitEffect();
                }
                else {
                    mote.Maintain();
                    // mote.Scale = 1.0f + Mathf.Sin(Find.TickManager.TicksGame * 0.05f) * 0.2f;
                }
            }
        }

        private void TryEmitEffect() {
            if (!parent.pawn.Spawned || parent.pawn.Map == null) {
                return;
            }

            var moteDef = DefDatabase<ThingDef>.GetNamed("Mote_ActivatorProximityGlow");
            var desiredRadius = Props.radius * parent.Severity; // includes flare doubling if needed
            var drawSize = moteDef.graphicData.drawSize.magnitude; // assume square; use magnitude if not
            var scale = desiredRadius * 2f / drawSize; // times 2 since radius = diameter / 2

            mote = MoteMaker.MakeAttachedOverlay(
                parent.pawn,
                moteDef,
                Vector3.zero,
                scale
            );

            var baseColor = MetalRegistry.Metals["brass"].Color;
            // Scale alpha based on severity (clamp between 0.1 and 1.0 for visibility)
            baseColor.a = Mathf.Clamp01(parent.Severity);

            mote.instanceColor = baseColor;
            Log.Warning($"Emitting mote: {moteDef.defName} on Map: {parent.pawn.Map} at pawn: {parent.pawn.Position} with scale: {scale}");
        }

        private void TrySoothe(Pawn target) {
            if (target?.mindState == null || target.Dead) {
                return;
            }

            // Lower mental break threshold indirectly
            // You could apply a Hediff, Thought, or just reduce mood pressure

            // Optional: apply a custom soothing Hediff
            var soothed = target.health.GetOrAddHediff(HediffDef.Named("Cosmere_Hediff_Soothed"));
            soothed.Severity = parent.Severity;
            var offset = soothed.ageTicks / TickUtility.Minutes(5) * soothed.Severity; // Jumps for every hour

            // Reset the Disappears timer
            var disappearsComp = soothed.TryGetComp<DisappearsScaled>();
            disappearsComp?.CompPostMake();

            // Update the mood offset
            var thoughtComp = soothed.TryGetComp<HediffComp_ThoughtSetter>();
            var newOffset = Mathf.RoundToInt(Mathf.Clamp(offset, 2f, 10f));
            Log.Verbose($"Updating moodOffset to {newOffset} on {target.LabelShort} offset={offset} Severity={parent.Severity}");
            thoughtComp?.OverrideMoodOffset(newOffset);

            if (soothed.ageTicks >= Props.tickInterval) return;

            // Just been soothed
            Log.Verbose($"Soothing {target.LabelShort}");
            MoteMaker.ThrowText(target.DrawPos, target.Map, "Soothed", Color.cyan);
        }
    }
}