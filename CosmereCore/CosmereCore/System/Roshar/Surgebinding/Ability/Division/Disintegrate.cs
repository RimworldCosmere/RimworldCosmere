using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Division;

public class Disintegrate : SurgebindingAbility {
    public Disintegrate(Pawn pawn) : base(pawn) { }
    public Disintegrate(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        Verse.Thing? targetThing = target.Thing;
        if (targetThing == null || targetThing.Destroyed) return false;

        if (!CanDisintegrate(targetThing)) return false;

        Gene.RemoveFromReserve(cost);

        IntVec3 pos = targetThing.Position;
        Map map = targetThing.Map;

        targetThing.Destroy();

        FleckMaker.Static(pos, map, FleckDefOf.PsycastAreaEffect);

        return true;
    }

    private bool CanDisintegrate(Verse.Thing thing) {
        if (thing is not Building) return false;

        if (thing.def.building?.isNaturalRock == true) return true;

        if (Gene.CurrentIdeal >= 4) return true;

        int actualMaxHp = thing.MaxHitPoints;
        return Gene.CurrentIdeal switch {
            3 => actualMaxHp <= 600,
            2 => actualMaxHp <= 400,
            _ => actualMaxHp <= 250,
        };
    }
}