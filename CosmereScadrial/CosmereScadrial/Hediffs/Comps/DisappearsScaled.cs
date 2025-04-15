using Verse;

namespace CosmereScadrial.Hediffs.Comps {
    public class DisappearsScaled : HediffComp {
        private int ticksLeft = -1;

        public Properties.DisappearsScaled Props => (Properties.DisappearsScaled)props;

        public override string CompLabelInBracketsExtra => $"{ticksLeft / 250f:0.0}h left";

        public override void CompPostMake() {
            base.CompPostMake();
            var severity = parent.Severity;
            ticksLeft = (int)(Props.baseTicks * severity); // Scale duration
        }

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            ticksLeft--;
            if (ticksLeft <= 0) {
                parent.pawn.health.RemoveHediff(parent);
            }
        }
    }
}