using Cosmere.Core.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Cohesion;

public class ShapeStone : SurgebindingAbility {
    public ShapeStone(Pawn pawn) : base(pawn) { }
    public ShapeStone(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
        Designator_ShapeStone designator = new Designator_ShapeStone(this);
        Find.DesignatorManager.Select(designator);

        if (status.isActive) UpdateStatus(Active.Off);
        return true;
    }

    public void ExecuteAt(IntVec3 cell, Map map) {
        float cost = def.beuPerTick / (1 << gene.currentIdeal);
        if (!gene.CanLowerReserve(cost)) return;

        Building? building = cell.GetFirstBuilding(map);

        if (building != null && IsMineable(building)) {
            gene.RemoveFromReserve(cost);
            building.Destroy(DestroyMode.KillFinalize);
            FleckMaker.Static(cell, map, FleckDefOf.PsycastAreaEffect);
        } else if (building == null && cell.Standable(map)) {
            gene.RemoveFromReserve(cost);
            ThingDef wallDef = RimWorld.ThingDefOf.Wall;
            ThingDef stuffDef = GetLocalStoneStuff(cell, map);
            Verse.Thing wall = ThingMaker.MakeThing(wallDef, stuffDef);
            GenSpawn.Spawn(wall, cell, map);
            FleckMaker.Static(cell, map, FleckDefOf.PsycastAreaEffect);
        }
    }

    private static ThingDef GetLocalStoneStuff(IntVec3 cell, Map map) {
        ThingDef? nearestStone = FindNearestNaturalRockStuff(cell, map);
        if (nearestStone != null) return nearestStone;

        int tile = map.Tile;
        foreach (ThingDef rockDef in Find.World.NaturalRockTypesIn(tile)) {
            if (rockDef.building?.mineableThing?.IsStuff == true) {
                return rockDef.building.mineableThing;
            }
        }

        return RimWorld.ThingDefOf.BlocksGranite;
    }

    private static ThingDef? FindNearestNaturalRockStuff(IntVec3 cell, Map map) {
        foreach (IntVec3 nearby in GenRadial.RadialCellsAround(cell, 15f, true)) {
            if (!nearby.InBounds(map)) continue;
            Building? building = nearby.GetFirstBuilding(map);
            if (building == null) continue;
            if (building.def.building?.isNaturalRock != true) continue;

            ThingDef? mineableThing = building.def.building?.mineableThing;
            if (mineableThing?.IsStuff == true) return mineableThing;

            ThingDef? blocksVersion = DefDatabase<ThingDef>.GetNamedSilentFail(
                "Blocks" + building.def.defName.Replace("Smoothed", "")
            );
            if (blocksVersion != null) return blocksVersion;
        }

        return null;
    }

    private static bool IsMineable(Building building) {
        return building.def.building?.isNaturalRock == true ||
               building.def.building?.mineableThing != null ||
               building.def == RimWorld.ThingDefOf.Wall;
    }
}