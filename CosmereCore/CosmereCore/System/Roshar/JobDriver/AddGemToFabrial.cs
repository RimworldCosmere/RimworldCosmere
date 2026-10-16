using Cosmere.System.Roshar.Toil;
using Verse.AI;

namespace Cosmere.System.Roshar.JobDriver;

public class AddGemToFabrial : Verse.AI.JobDriver {
    private const TargetIndex FabrialIndex = TargetIndex.A;
    private const TargetIndex GemIndex = TargetIndex.B;
    public const int ReGemmingDuration = 240;

    protected Verse.Thing? Fabrial => job.GetTarget(FabrialIndex).Thing;

    protected Verse.Thing? Gemstone => job.GetTarget(GemIndex).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(Fabrial, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(Gemstone, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(FabrialIndex);

        AddFailCondition(() => Fabrial == null);
        AddFailCondition(() => Gemstone == null);

        yield return Toils_General.DoAtomic(() => { job.count = 1; });

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
        yield return RefuelFabrial.SwapInNewGemstone(GemIndex, FabrialIndex); // custom toil
    }
}
