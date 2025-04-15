using System.Linq;
using CosmereFramework.Utils;
using CosmereScadrial.Registry;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Hediffs.Comps {
    public class BrassAura : HediffComp {
        private Mote mote;

        private Properties.BrassAura Props => (Properties.BrassAura)props;

        private float moteScale {
            get {
                var moteDef = DefDatabase<ThingDef>.GetNamed("Cosmere_Mote_SootheAura");
                var desiredRadius = Props.radius * parent.Severity; // includes flare doubling if needed
                var drawSize = moteDef.graphicData.drawSize.magnitude; // assume square; use magnitude if not
                var scale = desiredRadius * 2f / drawSize; // times 2 since radius = diameter / 2

                return scale;
            }
        }

        public override void CompPostMake() {
            base.CompPostMake();
            CreateMote();
        }

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);
            var radius = Props.radius * parent.Severity;
            var center = parent.pawn.Position;
            var map = parent.pawn.Map;

            if (map == null || !center.IsValid) {
                return;
            }

            CreateMote()?.Maintain();
            mote.Scale = moteScale;

            if (!parent.pawn.IsHashIntervalTick(Props.tickInterval)) {
                return;
            }

            var nearbyPawns = GenRadial.RadialCellsAround(center, radius, true)
                .Select(cell => cell.GetFirstPawn(map))
                .Where(p => p != null && p != parent.pawn);

            foreach (var pawn in nearbyPawns) {
                TrySoothe(pawn);
            }
        }

        [CanBeNull]
        private Mote CreateMote() {
            if (mote?.Destroyed == false) {
                return mote;
            }

            mote = MoteMaker.MakeAttachedOverlay(
                parent.pawn,
                DefDatabase<ThingDef>.GetNamed("Cosmere_Mote_SootheAura"),
                Vector3.zero,
                moteScale
            );
            mote.instanceColor = MetalRegistry.Metals["brass"].Color;

            Log.Message($"Mote successfully created: {mote}, map={parent.pawn.Map} color={mote.instanceColor}, scale={moteScale}, radius={Props.radius * parent.Severity}");
            return mote;
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
            thoughtComp?.OverrideMoodOffset(newOffset);

            if (soothed.ageTicks >= Props.tickInterval) return;

            // Just been soothed
            Log.Info($"Soothing {target.LabelShort}");
            MoteMaker.ThrowText(target.DrawPos, target.Map, "Soothed", Color.cyan);
        }
    }
}