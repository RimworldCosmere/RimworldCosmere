using System;
using System.Linq;
using CosmereFramework.Utils;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using HediffUtility = CosmereScadrial.Utils.HediffUtility;

namespace CosmereScadrial.Abilities.Hediffs.Comps {
    public class AllomancyAura : HediffComp {
        private Mote mote;

        private new Properties.AllomancyAura props => (Properties.AllomancyAura)base.props;

        private new AllomanticHediff parent => base.parent as AllomanticHediff;

        private bool isAtLeastPassive => parent.Severity >= 0.5f;

        private AbstractAllomanticAbility ability => parent.sourceAbilities.First(_ => true);

        private float moteScale {
            get {
                var moteDef = DefDatabase<ThingDef>.GetNamed("Cosmere_Mote_Allomancy_Aura");
                var desiredRadius = props.radius * base.parent.Severity; // includes flare doubling if needed
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
            if (!isAtLeastPassive) {
                return;
            }

            base.CompPostTick(ref severityAdjustment);
            var radius = props.radius * base.parent.Severity;
            var center = base.parent.pawn.Position;
            var map = base.parent.pawn.Map;

            if (map == null || !center.IsValid) {
                return;
            }

            CreateMote()?.Maintain();
            mote.Scale = moteScale;

            if (!base.parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) {
                return;
            }

            var nearbyPawns = GenRadial.RadialCellsAround(center, Math.Min(radius, GenRadial.MaxRadialPatternRadius), true)
                .Select(cell => cell.GetFirstPawn(map))
                .Where(p => p != null && p != base.parent.pawn);

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
                base.parent.pawn,
                DefDatabase<ThingDef>.GetNamed("Cosmere_Mote_Allomancy_Aura"),
                Vector3.zero,
                moteScale
            );
            mote.instanceColor = MetallicArtsMetalDefOf.Brass.color;

            return mote;
        }

        private void TryAct(Pawn target) {
            if (target?.mindState == null || target.Dead) {
                return;
            }

            var hediff = HediffUtility.GetOrAddHediff(Pawn, target, ability, props);

            // Reset the Disappears timer
            if (hediff.TryGetComp<DisappearsScaled>(out var disappearsComp)) {
                disappearsComp.CompPostMake();
            }

            // Update the mood offset
            if (hediff.TryGetComp<HediffComp_ThoughtSetter>(out var thoughtComp)) {
                var offset = hediff.ageTicks / TickUtility.Minutes(5) * hediff.Severity; // Jumps for every hour
                var newOffset = Mathf.RoundToInt(Mathf.Clamp(offset, 2f, 10f));
                thoughtComp.OverrideMoodOffset(newOffset);
            }

            if (hediff.ageTicks >= GenTicks.TicksPerRealSecond) return;

            MoteMaker.ThrowText(target.DrawPos, target.Map, props.verb, Color.cyan);
        }
    }
}