using CosmereFramework.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps {
    public class AllomancyTarget : HediffComp {
        private new Properties.AllomancyTarget props => (Properties.AllomancyTarget)base.props;

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);
            var map = parent.pawn.Map;

            if (map == null) {
                return;
            }

            if (!parent.pawn.IsHashIntervalTick(props.tickInterval)) {
                return;
            }

            TryAct(parent.pawn);
        }

        private void TryAct(Pawn target) {
            if (target?.mindState == null || target.Dead) {
                return;
            }

            var hediff = target.health.GetOrAddHediff(props.actHediff);
            hediff.Severity = parent.Severity * props.severityMultiplier;
            var offset = hediff.ageTicks / TickUtility.Minutes(5) * hediff.Severity; // Jumps for every hour

            // Reset the Disappears timer
            var disappearsComp = hediff.TryGetComp<DisappearsScaled>();
            disappearsComp?.CompPostMake();

            // Update the mood offset
            var thoughtComp = hediff.TryGetComp<HediffComp_ThoughtSetter>();
            var newOffset = Mathf.RoundToInt(Mathf.Clamp(offset, 2f, 10f));
            thoughtComp?.OverrideMoodOffset(newOffset);

            if (hediff.ageTicks >= props.tickInterval) return;

            MoteMaker.ThrowText(target.DrawPos, target.Map, props.actVerb, Color.cyan);
        }
    }
}