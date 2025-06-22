using RimWorld;
using Verse;

namespace CosmereScadrial.Comps.Hediffs;

public class DisappearsScaledProperties : HediffCompProperties {
    public int baseTicks = GenDate.TicksPerHour;
    public float minSeverity = 0f;
    public bool scaleSeverity = true;

    public DisappearsScaledProperties() {
        compClass = typeof(DisappearsScaled);
    }
}

public class DisappearsScaled : HediffComp {
    private float initialSeverity;
    private int startTick = -1;
    private int ticksLeft = -1;
    private int totalTicks = -1;
    private static int CurrentTick => Find.TickManager.TicksGame;

    private new DisappearsScaledProperties props => (DisappearsScaledProperties)base.props;

    public override string CompLabelInBracketsExtra => $"{ticksLeft.ToStringTicksToPeriod()} left";

    public override void CompPostMake() {
        base.CompPostMake();
        initialSeverity = parent.Severity;
        totalTicks = ticksLeft = (int)(props.baseTicks * initialSeverity); // Scale duration

        // Set the start tick to the current tick
        startTick = CurrentTick;
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        base.CompPostTickInterval(ref severityAdjustment, delta);

        // If the startTick is less than 5 seconds ago, don't do anything
        if (CurrentTick - startTick < GenTicks.SecondsToTicks(3)) return;

        ticksLeft -= delta;
        if (ticksLeft <= 0) {
            parent.pawn.health.RemoveHediff(parent);
            return;
        }

        if (!props.scaleSeverity) return;

        // Linear severity reduction based on initial severity and total duration
        float targetSeverity = props.minSeverity;
        float decayAmount = (initialSeverity - targetSeverity) / totalTicks;
        severityAdjustment -= decayAmount;
    }
}