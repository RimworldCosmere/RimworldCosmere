using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Cohesion;

public class Designator_ShapeStone : Designator {
    private static DesignationDef? cachedDesignationDef;
    private readonly ShapeStone ability;

    public Designator_ShapeStone(ShapeStone ability) {
        this.ability = ability;

        defaultLabel = "Shape Stone";
        defaultDesc = "Select cells to raise or destroy stone walls.";
        useMouseIcon = true;
        soundSucceeded = SoundDefOf.Designate_Mine;
    }

    internal static DesignationDef DesignationDef =>
        cachedDesignationDef ??= DefDatabase<DesignationDef>.GetNamed("Cosmere_Roshar_Designation_ShapeStone");

    public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Areas;
    public override bool DragDrawMeasurements => true;

    public override AcceptanceReport CanDesignateCell(IntVec3 cell) {
        if (!cell.InBounds(Map)) return false;

        Building? building = cell.GetFirstBuilding(Map);
        if (building != null) return IsMineable(building);

        return cell.Standable(Map);
    }

    public override void DesignateSingleCell(IntVec3 c) {
        QueueJobAt(c);
    }

    public override void DesignateThing(Verse.Thing t) {
        QueueJobAt(t.Position);
    }

    public override void RenderHighlight(List<IntVec3> dragCells) {
        DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
    }

    public override void SelectedUpdate() {
        GenUI.RenderMouseoverBracket();
    }

    private void QueueJobAt(IntVec3 cell) {
        JobDef jobDef = JobDefOf.Cosmere_Roshar_Job_ShapeStone;
        Verse.AI.Job job = JobMaker.MakeJob(jobDef, new LocalTargetInfo(cell));
        job.ability = ability;
        job.playerForced = true;

        if (Map.designationManager.DesignationAt(cell, DesignationDef) == null) {
            Map.designationManager.AddDesignation(new Designation(cell, DesignationDef));
        }

        ability.pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, true);
    }

    private static bool IsMineable(Building building) {
        return building.def.building?.isNaturalRock == true ||
               building.def.building?.mineableThing != null ||
               building.def == RimWorld.ThingDefOf.Wall;
    }
}