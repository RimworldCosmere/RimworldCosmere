using System.Linq;
using CosmereFramework.Utils;
using CosmereScadrial.Registry;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Hediffs.Comps {
    public class AllomancyAura : HediffComp {
        private Mote mote;

        private Properties.AllomancyAura Props => (Properties.AllomancyAura)props;

        private float moteScale {
            get {
                var moteDef = DefDatabase<ThingDef>.GetNamed("Cosmere_Mote_Allomancy_Aura");
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
                TryAct(pawn);
            }
        }

        [CanBeNull]
        private Mote CreateMote() {
            if (mote?.Destroyed == false) {
                return mote;
            }

            mote = MoteMaker.MakeAttachedOverlay(
                parent.pawn,
                DefDatabase<ThingDef>.GetNamed("Cosmere_Mote_Allomancy_Aura"),
                Vector3.zero,
                moteScale
            );
            mote.instanceColor = MetalRegistry.Metals["brass"].Color;

            return mote;
        }

        private void TryAct(Pawn target) {
            if (target?.mindState == null || target.Dead) {
                return;
            }

            var hediff = target.health.GetOrAddHediff(Props.actHediff);
            hediff.Severity = parent.Severity;
            var offset = hediff.ageTicks / TickUtility.Minutes(5) * hediff.Severity; // Jumps for every hour

            // Reset the Disappears timer
            var disappearsComp = hediff.TryGetComp<DisappearsScaled>();
            disappearsComp?.CompPostMake();

            // Update the mood offset
            var thoughtComp = hediff.TryGetComp<HediffComp_ThoughtSetter>();
            var newOffset = Mathf.RoundToInt(Mathf.Clamp(offset, 2f, 10f));
            thoughtComp?.OverrideMoodOffset(newOffset);

            if (hediff.ageTicks >= Props.tickInterval) return;

            MoteMaker.ThrowText(target.DrawPos, target.Map, Props.actVerb, Color.cyan);
        }
    }
}