using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Tension;

public class Stiffen : SurgebindingAbility {
    public Stiffen(Pawn pawn) : base(pawn) { }

    public Stiffen(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private int durationTicks => (int)(GenTicks.TicksPerRealSecond * (15f + Gene.CurrentIdeal * 5f));

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Pawn? targetPawn = target.Pawn;
        if (targetPawn == null || targetPawn.Dead) return false;

        Gene.RemoveFromReserve(cost);

        bool isFriendly = targetPawn.Faction == pawn.Faction;
        HediffDef? hediffDef = isFriendly ? def.hediffFriendly : def.hediffHostile;
        if (hediffDef == null) hediffDef = def.hediff;
        if (hediffDef == null) return false;

        HediffWithComps hediff = (HediffWithComps)HediffMaker.MakeHediff(hediffDef, targetPawn);
        HediffComp_Disappears? disappears = hediff.TryGetComp<HediffComp_Disappears>();
        if (disappears != null) {
            disappears.ticksToDisappear = durationTicks;
        }

        targetPawn.health.AddHediff(hediff);

        FleckMaker.Static(targetPawn.Position, targetPawn.Map, FleckDefOf.PsycastAreaEffect);

        return true;
    }
}
