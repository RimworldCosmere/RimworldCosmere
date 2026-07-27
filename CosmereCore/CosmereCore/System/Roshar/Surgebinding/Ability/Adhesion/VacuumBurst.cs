using Cosmere.Core.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Adhesion;

public class VacuumBurst : SurgebindingAbility {
    private const int BaseRadius = 3;
    private const float BaseDamage = 5f;

    public VacuumBurst(Pawn pawn) : base(pawn) { }

    public VacuumBurst(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float radius => BaseRadius + Gene.CurrentIdeal;

    private float damage => BaseDamage + Gene.CurrentIdeal * 3f;

    private int stunTicks => (int)(GenTicks.TicksPerRealSecond * (1f + Gene.CurrentIdeal * 0.5f));

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Gene.RemoveFromReserve(cost);

        float currentRadius = radius;
        float currentDamage = damage;

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

            DamageInfo dinfo = new DamageInfo(
                DamageDefOf.Crush,
                currentDamage,
                instigator: pawn
            );
            targetPawn.TakeDamage(dinfo);

            targetPawn.stances?.stunner?.StunFor(stunTicks, pawn);
        }

        return true;
    }
}
