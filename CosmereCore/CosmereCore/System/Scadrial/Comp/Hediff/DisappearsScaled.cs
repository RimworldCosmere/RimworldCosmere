using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Hediff;

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

    private DisappearsScaledProperties Props => (DisappearsScaledProperties)props;

    public override string CompLabelInBracketsExtra => $"{ticksLeft.ToStringTicksToPeriod()} left";

    public override void CompPostMake() {
        base.CompPostMake();
        initialSeverity = parent.Severity;
        totalTicks = ticksLeft = (int)(Props.baseTicks * initialSeverity);
        startTick = CurrentTick;
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        base.CompPostTickInterval(ref severityAdjustment, delta);

        int currentTick = CurrentTick;
        if (currentTick - startTick < GenTicks.SecondsToTicks(3)) return;

        ticksLeft -= delta;
        if (ticksLeft <= 0) {
            parent.pawn.health.RemoveHediff(parent);
            return;
        }

        if (!Props.scaleSeverity) return;

        float targetSeverity = Props.minSeverity;
        float decayAmount = (initialSeverity - targetSeverity) / totalTicks;
        severityAdjustment -= decayAmount;
    }
}
