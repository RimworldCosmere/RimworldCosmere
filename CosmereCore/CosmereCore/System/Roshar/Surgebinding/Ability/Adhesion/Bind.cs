using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Adhesion;

public class Bind : SurgebindingAbility {
    public Bind(Pawn pawn) : base(pawn) { }

    public Bind(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private int DurationTicks => (int)(GenTicks.TicksPerRealSecond * (5f + Gene.CurrentIdeal * 2.5f));

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = Def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Pawn? targetPawn = target.Pawn;
        if (targetPawn == null || targetPawn.Dead) return false;

        Gene.RemoveFromReserve(cost);

        HediffDef? hediffDef = Def.hediff;
        if (hediffDef == null) return false;

        HediffWithComps hediff = (HediffWithComps)HediffMaker.MakeHediff(hediffDef, targetPawn);
        HediffComp_Disappears? disappears = hediff.TryGetComp<HediffComp_Disappears>();
        if (disappears != null) {
            disappears.ticksToDisappear = DurationTicks;
        }

        targetPawn.health.AddHediff(hediff);

        FleckMaker.Static(targetPawn.Position, targetPawn.Map, FleckDefOf.PsycastAreaEffect);

        return true;
    }
}
