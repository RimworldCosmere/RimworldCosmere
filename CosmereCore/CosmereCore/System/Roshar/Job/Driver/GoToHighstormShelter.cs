using Cosmere.System.Roshar.Utility;
using Verse.AI;

namespace Cosmere.System.Roshar.Job.Driver;

public class GoToHighstormShelter : JobDriver {
    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        this.FailOn(() => !StormShelterManager.IsInsideShelter(job.targetA.Cell));
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
    }
}