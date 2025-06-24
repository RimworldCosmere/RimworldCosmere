using System.Collections.Generic;
using CosmereFramework.Extensions;
using CosmereScadrial.Abilities.Allomancy;
using Verse;
using Verse.AI;

namespace CosmereScadrial.JobDrivers;

public class FollowGoldHallucination : AllomanticJobDriver {
    protected virtual Pawn hallucination => TargetA.Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(pawn, job, errorOnFailed: errorOnFailed, maxPawns: int.MaxValue, stackCount: 1,
            ignoreOtherReservations: true);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        foreach (Toil baseToil in base.MakeNewToils()) {
            yield return baseToil;
        }

        this.FailOnDespawnedNullOrForbidden(TargetIndex.A)
            .FailOnDowned(TargetIndex.A);

        Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

        Toil toil = new Toil {
            defaultCompleteMode = ToilCompleteMode.Never,
            tickAction = () => {
                if (ShouldStopJob()) {
                    ability.UpdateStatus(BurningStatus.Off);
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                // Maintain proximity
                pawn.MaintainProximityTo(hallucination, job.followRadius, PathEndMode.InteractionCell);
            },
        };

        yield return toil;
    }

    protected override bool ShouldStopJob() {
        return base.ShouldStopJob() || !pawn.CanReach(TargetA, PathEndMode.ClosestTouch, Danger.None);
    }
}