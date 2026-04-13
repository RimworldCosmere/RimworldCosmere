using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

public class Designator_Soulcast : Designator {
    private static DesignationDef? cachedDesignationDef;
    private readonly Soulcast ability;
    private readonly SoulcastMode mode;
    private readonly ThingDef? material;
    private readonly TerrainDef? terrain;

    public Designator_Soulcast(Soulcast ability, SoulcastMode mode, ThingDef? material, TerrainDef? terrain) {
        this.ability = ability;
        this.mode = mode;
        this.material = material;
        this.terrain = terrain;

        defaultLabel = "Soulcast";
        defaultDesc = "Select targets to soulcast.";
        useMouseIcon = true;
        soundSucceeded = SoundDefOf.Designate_Mine;
    }

    internal static DesignationDef DesignationDef =>
        cachedDesignationDef ??= DefDatabase<DesignationDef>.GetNamed("Cosmere_Roshar_Designation_Soulcast");

    public override DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Areas;
    public override bool DragDrawMeasurements => true;

    public override AcceptanceReport CanDesignateCell(IntVec3 cell) {
        if (!cell.InBounds(Map)) return false;

        if (mode is SoulcastMode.Wall or SoulcastMode.Terraform or SoulcastMode.Geyser) {
            if (mode == SoulcastMode.Wall && (!cell.Standable(Map) || cell.GetFirstBuilding(Map) != null)) {
                return false;
            }
            return true;
        }

        Verse.Thing? target = FindValidThingAt(cell);
        return target != null;
    }

    public override AcceptanceReport CanDesignateThing(Verse.Thing t) {
        return IsValidTarget(t);
    }

    public override void DesignateSingleCell(IntVec3 c) {
        if (mode is SoulcastMode.Wall or SoulcastMode.Terraform or SoulcastMode.Geyser) {
            QueueJobAt(new LocalTargetInfo(c));
            return;
        }

        Verse.Thing? target = FindValidThingAt(c);
        if (target != null) {
            QueueJobAt(new LocalTargetInfo(target));
        }
    }

    public override void DesignateThing(Verse.Thing t) {
        QueueJobAt(new LocalTargetInfo(t));
    }

    public override void RenderHighlight(List<IntVec3> dragCells) {
        DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
    }

    public override void SelectedUpdate() {
        GenUI.RenderMouseoverBracket();
    }

    protected override void FinalizeDesignationSucceeded() {
        base.FinalizeDesignationSucceeded();

        foreach (Designation designation in Map.designationManager.SpawnedDesignationsOfDef(DesignationDef)) {
            IntVec3 cell = designation.target.Cell;
            if (!ability.pawn.CanReach(new LocalTargetInfo(cell), PathEndMode.Touch, Danger.Deadly)) continue;
            if (!SoulcastOverlay.TryGetData(cell, out _, out _, out _)) continue;

            JobDef soulcastJobDef = JobDefOf.Cosmere_Roshar_Job_Soulcast;
            Verse.AI.Job job = JobMaker.MakeJob(soulcastJobDef, cell);
            job.ability = ability;
            job.playerForced = true;
            ability.pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            return;
        }
    }

    private void QueueJobAt(LocalTargetInfo target) {
        IntVec3 cell = target.Cell;
        SoulcastOverlay.Add(cell, material, terrain, mode);

        if (Map.designationManager.DesignationAt(cell, DesignationDef) == null) {
            Map.designationManager.AddDesignation(new Designation(cell, DesignationDef));
        }
    }

    private Verse.Thing? FindValidThingAt(IntVec3 cell) {
        List<Verse.Thing> things = cell.GetThingList(Map);
        for (int i = 0; i < things.Count; i++) {
            if (IsValidTarget(things[i])) return things[i];
        }
        return null;
    }

    private bool IsValidTarget(Verse.Thing thing) {
        if (thing.Destroyed) return false;

        return mode switch {
            SoulcastMode.ConvertDrop => thing.def.category == ThingCategory.Item
                                       || thing.def.plant != null
                                       || thing.def.mineable,
            SoulcastMode.ChangeStuff => (thing.def.MadeFromStuff && thing.Stuff != null)
                                        || thing.def.mineable,
            SoulcastMode.Sculpture => thing is Verse.Pawn or Corpse,
            SoulcastMode.Destroy => true,
            _ => false,
        };
    }
}
