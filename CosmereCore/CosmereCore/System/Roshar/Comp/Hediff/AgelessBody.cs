using Verse;

namespace Cosmere.System.Roshar.Comp.Hediff;

public class AgelessBodyProperties : HediffCompProperties {
    public AgelessBodyProperties() {
        compClass = typeof(AgelessBody);
    }
}

public class AgelessBody : HediffComp {
    private long frozenAgeTicks = -1;

    public override void CompPostMake() {
        base.CompPostMake();
        frozenAgeTicks = parent.pawn.ageTracker.AgeBiologicalTicks;
    }

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (frozenAgeTicks < 0) return;
        if (parent.pawn.ageTracker.AgeBiologicalTicks > frozenAgeTicks) {
            parent.pawn.ageTracker.AgeBiologicalTicks = frozenAgeTicks;
        }
    }

    public override void CompExposeData() {
        base.CompExposeData();
        Scribe_Values.Look(ref frozenAgeTicks, "frozenAgeTicks", -1L);
    }
}
