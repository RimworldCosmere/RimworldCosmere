using System.Collections.Generic;
using CosmereRoshar.Job.Toil;
using Verse.AI;

namespace CosmereRoshar.Job.Driver;

public class RemoveGemFromFabrial : JobDriver {
    private const TargetIndex FabrialIndex = TargetIndex.A;
    public const int JobDuration = 75;


    protected Verse.Thing? fabrial => job.GetTarget(FabrialIndex).Thing;


    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(fabrial, job, 1, -1, null, errorOnFailed);
    }


    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(FabrialIndex);

        AddFailCondition(() => fabrial == null);

        yield return Toils_General.DoAtomic(delegate { job.count = 1; });

        yield return Toils_Goto.GotoThing(FabrialIndex, PathEndMode.Touch);
        yield return Toils_General.Wait(JobDuration)
            .FailOnDestroyedNullOrForbidden(FabrialIndex)
            .FailOnCannotTouch(FabrialIndex, PathEndMode.Touch)
            .WithProgressBarToilDelay(FabrialIndex);
        yield return RefuelFabrial.RemoveGemstone(FabrialIndex); //custom toil
    }
}