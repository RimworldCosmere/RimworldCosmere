using Cosmere.System.Scadrial.Allomancy.Ability;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class FollowGoldHallucination : AllomanticJobDriver {
    protected virtual Pawn Hallucination => TargetA.Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(
            pawn,
            job,
            errorOnFailed: errorOnFailed,
            maxPawns: int.MaxValue,
            stackCount: 1,
            ignoreOtherReservations: true
        );
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        foreach (Toil baseToil in base.MakeNewToils()) {
            yield return baseToil;
        }

        this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
            .FailOnDowned(TargetIndex.A);

        Toil toil = new Toil {
            defaultCompleteMode = ToilCompleteMode.Never,
            tickAction = () => {
                if (ShouldStopJob()) {
                    Ability.UpdateStatus(BurningStatus.Off);
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                // Maintain proximity
                pawn.MaintainProximityTo(Hallucination, job.followRadius, PathEndMode.InteractionCell);
            },
        };

        yield return toil;
    }

    protected override bool ShouldStopJob() {
        return base.ShouldStopJob() || !pawn.CanReach(TargetA, PathEndMode.ClosestTouch, Danger.None);
    }
}
