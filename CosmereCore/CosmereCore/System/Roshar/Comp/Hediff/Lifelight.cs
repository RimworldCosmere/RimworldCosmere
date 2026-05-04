using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Hediff;

public class LifelightProperties : HediffCompProperties {
    public float investiturePerDay = 100f;

    public LifelightProperties() {
        compClass = typeof(Lifelight);
    }
}

public class Lifelight : HediffComp {
    private new LifelightProperties props => (LifelightProperties)base.props;

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (!GenTicks.IsTickIntervalDelta(GenDate.TicksPerDay, delta)) return;

        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Cultivation)) return;

        Pawn pawn = parent.pawn;
        if (pawn?.needs?.food == null) return;

        float foodLevel = pawn.needs.food.CurLevel;
        if (foodLevel <= 0f) return;

        InvestitureHolder? holder = pawn.TryGetComp<InvestitureHolder>();
        if (holder == null) return;

        holder.currentInvestitureSelf += foodLevel * props.investiturePerDay;
    }
}
