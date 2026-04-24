using Cosmere.System.Roshar.Utility;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Job.Driver;

public class WaitInHighstormShelter : JobDriver {
    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        Verse.AI.Toil wait = ToilMaker.MakeToil("WaitInShelter");
        wait.initAction = () => { pawn.pather.StopDead(); };
        wait.tickAction = () => {
            if (!StormShelterManager.IsInsideShelter(pawn.Position)) {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        wait.defaultCompleteMode = ToilCompleteMode.Never;
        yield return wait;
    }
}