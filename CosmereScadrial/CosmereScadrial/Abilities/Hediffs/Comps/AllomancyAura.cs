using System;
using System.Linq;
using CosmereFramework.Utils;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Abilities.Hediffs.Comps {
    public class AllomancyAura : HediffComp {
        private Mote mote;

        private new Properties.AllomancyAura props => (Properties.AllomancyAura)base.props;

        public new AllomanticHediff parent => base.parent as AllomanticHediff;

        public bool isAtLeastPassive => parent.Severity >= 0.5f;

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

            var nearbyPawns = GenRadial.RadialCellsAround(center, radius, true)
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

            var hediff = GetOrAddHediff(target);
            var offset = hediff.ageTicks / TickUtility.Minutes(5) * hediff.Severity; // Jumps for every hour

            // Reset the Disappears timer
            var disappearsComp = hediff.TryGetComp<DisappearsScaled>();
            disappearsComp?.CompPostMake();

            // Update the mood offset
            var thoughtComp = hediff.TryGetComp<HediffComp_ThoughtSetter>();
            var newOffset = Mathf.RoundToInt(Mathf.Clamp(offset, 2f, 10f));
            thoughtComp?.OverrideMoodOffset(newOffset);

            if (hediff.ageTicks >= GenTicks.TicksPerRealSecond) return;

            MoteMaker.ThrowText(target.DrawPos, target.Map, props.actVerb, Color.cyan);
        }

        public virtual AllomanticHediff GetOrAddHediff(Pawn targetPawn) {
            if (targetPawn.health.hediffSet.TryGetHediff(props.actHediff, out var ret)) return ret as AllomanticHediff;

            var hediff = (AllomanticHediff)Activator.CreateInstance(props.actHediff.hediffClass);
            hediff.def = props.actHediff;
            hediff.pawn = targetPawn;
            hediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
            hediff.sourceAbility = parent.sourceAbility;
            hediff.PostMake();

            Log.Warning($"{parent.pawn.NameFullColored} applying {props.actHediff.defName} hediff to {targetPawn.NameFullColored} with Severity={hediff.Severity}");
            targetPawn.health.AddHediff(hediff);

            return hediff;
        }
    }
}