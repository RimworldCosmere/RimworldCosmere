using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.JobDriver;

public class Soulcast : Verse.AI.JobDriver {
    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        AddFinishAction(_ => {
            if (pawn.Map != null) SoulcastOverlay.Remove(TargetA.Cell, pawn.Map);
            pawn.Map?.designationManager.TryRemoveDesignation(TargetA.Cell, Designator_Soulcast.DesignationDef);
        }
        );

        if (TargetA.HasThing) {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        }
        else {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
        }

        Verse.AI.Toil castToil = ToilMaker.MakeToil(nameof(Soulcast));
        castToil.initAction = () => {
            IntVec3 cell = TargetA.Cell;
            if (pawn.Map.designationManager.DesignationAt(cell, Designator_Soulcast.DesignationDef) == null) {
                return;
            }

            Surgebinding.Ability.Transformation.Soulcast? soulcast =
                job.ability as Surgebinding.Ability.Transformation.Soulcast;
            if (soulcast == null) return;

            if (SoulcastOverlay.TryGetData(cell, pawn.Map, out SoulcastMode mode, out ThingDef? mat, out TerrainDef? ter)) {
                soulcast.storedMode = mode;
                soulcast.storedMaterial = mat;
                soulcast.storedTerrain = ter;
                soulcast.ExecuteStoredAction(TargetA);
            }

            pawn.Map.designationManager.TryRemoveDesignation(cell, Designator_Soulcast.DesignationDef);
        };
        castToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return castToil;
    }

    public override void Notify_PatherFailed() {
        EndJobWith(JobCondition.Incompletable);
    }
}