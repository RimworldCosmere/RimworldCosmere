using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps {
    public class DisappearsScaled : HediffComp {
        private int ticksLeft = -1;

        public new Properties.DisappearsScaled props => (Properties.DisappearsScaled)base.props;

        public override string CompLabelInBracketsExtra => $"{ticksLeft.ToStringTicksToPeriod()} left";

        public override void CompPostMake() {
            base.CompPostMake();
            var severity = parent.Severity;
            ticksLeft = (int)(props.baseTicks * severity); // Scale duration
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