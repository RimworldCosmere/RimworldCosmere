using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public class Dazzle : SurgebindingAbility {
    private const int BaseRadius = 4;

    public Dazzle(Pawn pawn) : base(pawn) { }
    public Dazzle(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float radius => BaseRadius + gene.currentIdeal;

    private int durationTicks => (int)(GenTicks.TicksPerRealSecond * (8f + gene.currentIdeal * 4f));

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << gene.currentIdeal);
        if (!gene.CanLowerReserve(cost)) return false;

        gene.RemoveFromReserve(cost);

        float currentRadius = radius;
        HediffDef? hediffDef = def.hediff;
        if (hediffDef == null) return false;

        FleckMaker.Static(pawn.Position, pawn.Map, FleckDefOf.PsycastAreaEffect);

        foreach (Verse.Thing thing in GenRadial.RadialDistinctThingsAround(
                     pawn.Position,
                     pawn.Map,
                     currentRadius,
                     true
                 )) {
            if (thing is not Pawn targetPawn) continue;
            if (targetPawn == pawn) continue;
            if (targetPawn.Dead) continue;
            if (targetPawn.Faction == pawn.Faction) continue;

            HediffWithComps hediff = (HediffWithComps)HediffMaker.MakeHediff(hediffDef, targetPawn);
            HediffComp_Disappears? disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (disappears != null) {
                disappears.ticksToDisappear = durationTicks;
            }

            targetPawn.health.AddHediff(hediff);
        }

        return true;
    }
}