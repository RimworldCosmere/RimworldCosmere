using Cosmere.System.Roshar.Comp.Map;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.JobDriver;

public class WaitInHighstormShelter : Verse.AI.JobDriver {
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