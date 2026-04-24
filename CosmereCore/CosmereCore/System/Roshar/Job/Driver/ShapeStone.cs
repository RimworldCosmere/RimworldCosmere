using Cosmere.System.Roshar.Surgebinding.Ability.Cohesion;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Job.Driver;

public class ShapeStone : JobDriver {
    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        AddFinishAction(_ => {
                pawn.Map?.designationManager.TryRemoveDesignation(TargetA.Cell, Designator_ShapeStone.DesignationDef);
            }
        );

        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);

        Verse.AI.Toil castToil = ToilMaker.MakeToil(nameof(ShapeStone));
        castToil.initAction = () => {
            IntVec3 cell = TargetA.Cell;
            if (pawn.Map.designationManager.DesignationAt(cell, Designator_ShapeStone.DesignationDef) == null) {
                return;
            }

            Surgebinding.Ability.Cohesion.ShapeStone? shapeStone =
                job.ability as Surgebinding.Ability.Cohesion.ShapeStone;
            shapeStone?.ExecuteAt(cell, pawn.Map);

            pawn.Map.designationManager.TryRemoveDesignation(cell, Designator_ShapeStone.DesignationDef);
        };
        castToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return castToil;
    }
}