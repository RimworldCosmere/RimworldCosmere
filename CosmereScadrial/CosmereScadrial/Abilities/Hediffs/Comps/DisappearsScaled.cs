using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps {
    public class DisappearsScaledProperties : HediffCompProperties {
        public int baseTicks = GenDate.TicksPerHour / 2;
        public float minSeverity = 0f;
        public bool scaleSeverity = true;

        public DisappearsScaledProperties() {
            compClass = typeof(DisappearsScaled);
        }
    }

    public class DisappearsScaled : HediffComp {
        private float initialSeverity;
        private int ticksLeft = -1;
        private int totalTicks = -1;

        private new DisappearsScaledProperties props => (DisappearsScaledProperties)base.props;
        
        private new AllomanticHediff parent => (AllomanticHediff)base.parent;

        public override string CompLabelInBracketsExtra => $"{ticksLeft.ToStringTicksToPeriod()} left";

        public override void CompPostMake() {
            base.CompPostMake();
            initialSeverity = parent.Severity;
            totalTicks = ticksLeft = (int)(props.baseTicks * initialSeverity); // Scale duration
        }

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            if (parent.sourceAbilities.Count > 0) return;            
            
            ticksLeft--;
            if (ticksLeft <= 0) {
                parent.pawn.health.RemoveHediff(parent);
                return;
            }

            if (!props.scaleSeverity) return;

            // Linear severity reduction based on initial severity and total duration
            var targetSeverity = props.minSeverity;
            var decayAmount = (initialSeverity - targetSeverity) / totalTicks;
            severityAdjustment -= decayAmount;
        }
    }
}