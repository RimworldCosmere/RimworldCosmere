using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Cohesion;

public class MoldArmor : SurgebindingAbility {
    public MoldArmor(Pawn pawn) : base(pawn) { }

    public MoldArmor(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private int durationTicks => (int)(GenTicks.TicksPerRealSecond * (30f + Gene.CurrentIdeal * 15f));

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Gene.RemoveFromReserve(cost);

        HediffDef? hediffDef = def.hediff;
        if (hediffDef == null) return false;

        HediffWithComps hediff = (HediffWithComps)HediffMaker.MakeHediff(hediffDef, pawn);
        HediffComp_Disappears? disappears = hediff.TryGetComp<HediffComp_Disappears>();
        if (disappears != null) {
            disappears.ticksToDisappear = durationTicks;
        }

        pawn.health.AddHediff(hediff);

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect);

        return true;
    }
}
