using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Gravitation;

public class ReverseLashing : SurgebindingAbility {
    private const float BaseDamage = 15f;

    public ReverseLashing(Pawn pawn) : base(pawn) { }

    public ReverseLashing(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    private float damage => BaseDamage + Gene.CurrentIdeal * 10f;

    private int stunTicks => (int)(GenTicks.TicksPerRealSecond * (1.5f + Gene.CurrentIdeal * 0.5f));

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Pawn? targetPawn = target.Pawn;
        if (targetPawn == null || targetPawn.Dead) return false;

        Gene.RemoveFromReserve(cost);

        Map map = targetPawn.Map;
        IntVec3 landingCell = targetPawn.Position;

        FleckMaker.Static(landingCell, map, FleckDefOf.PsycastAreaEffect);

        DamageInfo dinfo = new DamageInfo(
            DamageDefOf.Crush,
            damage,
            instigator: pawn
        );
        targetPawn.TakeDamage(dinfo);

        if (!targetPawn.Dead && targetPawn.Spawned) {
            targetPawn.stances?.stunner?.StunFor(stunTicks, pawn);

            RimWorld.PawnFlyer flyer = RimWorld.PawnFlyer.MakeFlyer(
                RimWorld.ThingDefOf.PawnFlyer_Stun,
                targetPawn,
                landingCell,
                null,
                null
            );

            if (flyer != null) {
                GenSpawn.Spawn(flyer, landingCell, map);
            }
        }

        return true;
    }
}
