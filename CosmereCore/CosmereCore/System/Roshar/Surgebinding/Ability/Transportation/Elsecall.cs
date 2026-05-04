using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transportation;

public class Elsecall : SurgebindingAbility {
    public Elsecall(Pawn pawn) : base(pawn) { }
    public Elsecall(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << Gene.CurrentIdeal);
        if (!Gene.CanLowerReserve(cost)) return false;

        IntVec3 cell = target.Cell;
        Map map = pawn.Map;

        if (!cell.Standable(map)) return false;

        Gene.RemoveFromReserve(cost);

        IntVec3 origin = pawn.Position;

        pawn.jobs.StopAll();
        pawn.DeSpawn();

        FleckMaker.Static(origin, map, FleckDefOf.PsycastAreaEffect);

        GenSpawn.Spawn(pawn, cell, map);
        pawn.Notify_Teleported(true, false);

        FleckMaker.Static(cell, map, FleckDefOf.PsycastAreaEffect);

        Find.Selector.Select(pawn, false);
        CameraJumper.TryJump(pawn);

        return true;
    }
}