using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using static CosmereFramework.CosmereFramework;
using HediffUtility = CosmereScadrial.Utils.HediffUtility;

namespace CosmereScadrial.Abilities.Allomancy {
    public class TimeAbility : AbilitySelfTarget {
        private const int BASE_RADIUS = 3;
        private readonly List<Pawn> pawnsInBubble = [];
        private Mote bubble;
        private Mote bubbleWithDistortion;

        public TimeAbility() {
            status = BurningStatus.Off;
        }

        public TimeAbility(Pawn pawn) : base(pawn) {
            status = BurningStatus.Off;
        }

        public TimeAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) {
            status = BurningStatus.Off;
        }

        public TimeAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
            status = BurningStatus.Off;
        }

        public TimeAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
            status = BurningStatus.Off;
        }

        private ThingDef moteDef => metal.Equals(MetallicArtsMetalDefOf.Cadmium)
            ? ThingDefOf.Cosmere_Scadrial_TimeBubble_Cadmium
            : ThingDefOf.Cosmere_Scadrial_TimeBubble_Bendalloy;

        private ThingDef warpMoteDef => ThingDefOf.Cosmere_Scadrial_TimeBubble_Warp;

        private float moteScale {
            get {
                var desiredRadius = BASE_RADIUS * GetStrength(); // includes flare doubling if needed
                var drawSize = moteDef.graphicData.drawSize.magnitude; // assume square; use magnitude if not
                var scale = desiredRadius * 2f * drawSize; // times 2 since radius = diameter / 2

                return scale + GetStrength() * drawSize;
            }
        }

        private HediffDef hediffToApply => metal.defName switch {
            "Cadmium" => HediffDefOf.Cosmere_Hediff_TimeBubble_Cadmium,
            "Bendalloy" => HediffDefOf.Cosmere_Hediff_TimeBubble_Bendalloy,
            _ => null,
        };

        protected override void OnEnable() {
            base.OnEnable();

            bubble = MoteMaker.MakeStaticMote(pawn.Position, pawn.MapHeld, moteDef, moteScale);
            bubbleWithDistortion = MoteMaker.MakeStaticMote(pawn.Position, pawn.MapHeld, warpMoteDef, moteScale * 0.8f);

            bubble.Maintain();
            bubbleWithDistortion.Maintain();
        }

        protected override void OnDisable() {
            base.OnDisable();
            if (!bubble.Destroyed) bubble.Destroy();
            if (!bubbleWithDistortion.Destroyed) bubbleWithDistortion.Destroy();
        }

        public override void AbilityTick() {
            base.AbilityTick();

            bubble?.Maintain();
            bubbleWithDistortion?.Maintain();
            if (!atLeastPassive || bubble == null || bubbleWithDistortion == null) return;

            bubble.Scale = moteScale;
            bubbleWithDistortion.Scale = moteScale * 0.8f;


            var radius = BASE_RADIUS * GetStrength();

            if (CosmereSettings.debugMode) {
                GenDraw.DrawCircleOutline(bubble.DrawPos, radius, SimpleColor.Blue);
            }

            foreach (var pawnInBubble in pawnsInBubble.Where(otherPawn =>
                         !otherPawn.Position.InHorDistOf(bubble.Position, radius))) {
                HediffUtility.RemoveHediff(pawnInBubble, this, hediffToApply);
            }

            if (!pawn.Position.InHorDistOf(bubble.Position, radius)) {
                UpdateStatus(BurningStatus.Off);
                return;
            }

            if (!pawn.IsHashIntervalTick(30)) return;

            foreach (var targetPawn in GenRadial.RadialDistinctThingsAround(bubble.Position, bubble.Map, radius, true)
                         .OfType<Pawn>()) {
                HediffUtility.GetOrAddHediff(targetPawn, this, hediffToApply);
                pawnsInBubble.AddDistinct(targetPawn);
            }
        }
    }
}