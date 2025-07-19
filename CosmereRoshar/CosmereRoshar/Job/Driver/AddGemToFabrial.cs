using System.Collections.Generic;
using CosmereRoshar.Job.Toil;
using Verse.AI;

namespace CosmereRoshar.Job.Driver;

public class AddGemToFabrial : JobDriver {
    private const TargetIndex FabrialIndex = TargetIndex.A;
    private const TargetIndex GemIndex = TargetIndex.B;
    public const int ReGemmingDuration = 240;


    protected Verse.Thing? fabrial => job.GetTarget(FabrialIndex).Thing;
    protected Verse.Thing? gemstone => job.GetTarget(GemIndex).Thing;


    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(fabrial, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(gemstone, job, 1, -1, null, errorOnFailed);
    }


    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(FabrialIndex);

        AddFailCondition(() => fabrial == null);
        AddFailCondition(() => gemstone == null);

        yield return Toils_General.DoAtomic(delegate { job.count = 1; });

        yield return Toils_Goto.GotoThing(GemIndex, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(GemIndex)
            .FailOnSomeonePhysicallyInteracting(GemIndex);
        yield return Toils_Haul.StartCarryThing(GemIndex, false, true).FailOnDestroyedNullOrForbidden(GemIndex);

        yield return Toils_Goto.GotoThing(FabrialIndex, PathEndMode.Touch);
        yield return Toils_General.Wait(ReGemmingDuration)
            .FailOnDestroyedNullOrForbidden(GemIndex)
            .FailOnDestroyedNullOrForbidden(FabrialIndex)
            .FailOnCannotTouch(FabrialIndex, PathEndMode.Touch)
            .WithProgressBarToilDelay(FabrialIndex);
        yield return RefuelFabrial.SwapInNewGemstone(GemIndex, FabrialIndex); //custom toil
    }
}