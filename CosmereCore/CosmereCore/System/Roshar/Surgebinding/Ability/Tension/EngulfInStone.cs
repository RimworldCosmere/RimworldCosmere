using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Tension;

public class EngulfInStone : SurgebindingAbility {
    public EngulfInStone(Pawn pawn) : base(pawn) { }
    public EngulfInStone(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        float cost = def.beuPerTick / (1 << gene.currentIdeal);
        if (!gene.CanLowerReserve(cost)) return false;

        IntVec3 cell = target.Cell;
        Map map = pawn.Map;

        Verse.Thing? existingBuilding = cell.GetFirstBuilding(map);
        if (existingBuilding != null) {
            if (existingBuilding.def == RimWorld.ThingDefOf.Wall) return false;

            ThingDef? stuff = existingBuilding.Stuff;
            existingBuilding.Destroy();

            ThingDef wallStuff = stuff ?? RimWorld.ThingDefOf.BlocksGranite;
            Verse.Thing wall = ThingMaker.MakeThing(RimWorld.ThingDefOf.Wall, wallStuff);
            GenSpawn.Spawn(wall, cell, map);
        } else if (cell.Standable(map)) {
            Verse.Thing wall = ThingMaker.MakeThing(
                RimWorld.ThingDefOf.Wall,
                RimWorld.ThingDefOf.BlocksGranite
            );
            GenSpawn.Spawn(wall, cell, map);
        } else {
            return false;
        }

        gene.RemoveFromReserve(cost);
        FleckMaker.Static(cell, map, FleckDefOf.PsycastAreaEffect);

        return true;
    }
}