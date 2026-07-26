using Cosmere.System.Roshar.Comp.Map;
using Verse.AI;

namespace Cosmere.System.Roshar.JobDriver;

public class GoToHighstormShelter : Verse.AI.JobDriver {
    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        this.FailOn(() => !StormShelterManager.IsInsideShelter(job.targetA.Cell));
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
    }
}