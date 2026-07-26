using Cosmere.System.Scadrial.Hemalurgy;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class WaitInBed : Verse.AI.JobDriver {
    private const int MaxWaitTicks = 10000;

    private Building_Bed bed => (Building_Bed)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);

        Toil gotoToil = ToilMaker.MakeToil("GotoSurgeryBed");
        gotoToil.initAction = () => {
            IntVec3 sleepSpot = RestUtility.GetBedSleepingSlotPosFor(pawn, bed);
            pawn.pather.StartPath(sleepSpot, PathEndMode.OnCell);
        };
        gotoToil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        yield return gotoToil;

        Toil waitToil = ToilMaker.MakeToil("WaitForSurgeon");
        waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
        waitToil.defaultDuration = MaxWaitTicks;
        waitToil.initAction = () => { pawn.jobs.posture = PawnPosture.LayingInBed; };
        yield return waitToil;
    }
}