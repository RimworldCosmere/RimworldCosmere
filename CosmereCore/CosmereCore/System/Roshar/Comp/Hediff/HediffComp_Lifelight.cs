using Cosmere.Core.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Hediff;

public class HediffCompProperties_Lifelight : HediffCompProperties {
    public float investiturePerDay = 100f;

    public HediffCompProperties_Lifelight() {
        compClass = typeof(HediffComp_Lifelight);
    }
}

public class HediffComp_Lifelight : HediffComp {
    private new HediffCompProperties_Lifelight props => (HediffCompProperties_Lifelight)base.props;

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        if (!GenTicks.IsTickIntervalDelta(GenDate.TicksPerDay, delta)) return;

        Verse.Pawn pawn = parent.pawn;
        if (pawn?.needs?.food == null) return;

        float foodLevel = pawn.needs.food.CurLevel;
        if (foodLevel <= 0f) return;

        InvestitureHolder? holder = pawn.TryGetComp<InvestitureHolder>();
        if (holder == null) return;

        holder.currentInvestitureSelf += foodLevel * props.investiturePerDay;
    }
}
