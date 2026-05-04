using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Ability;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.JobDriver;

public class MaintainSurgebindingTarget : Verse.AI.JobDriver {
    private SurgebindingAbility ability => (SurgebindingAbility)job.source!;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(
            TargetA,
            job,
            errorOnFailed: errorOnFailed,
            maxPawns: int.MaxValue,
            stackCount: 1,
            ignoreOtherReservations: true
        );
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        AddFinishAction(condition => {
            if (condition == JobCondition.Ongoing) return;
            if (ability.status.IsActive) {
                ability.UpdateStatus(Active.Off);
            }
        }
        );

        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

        Verse.AI.Toil maintainToil = ToilMaker.MakeToil(nameof(MaintainSurgebindingTarget));
        maintainToil.defaultCompleteMode = ToilCompleteMode.Never;
        maintainToil.tickAction = () => {
            if (pawn.Downed || pawn.Dead || !ability.status.IsActive) {
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            Verse.Thing? targetThing = TargetA.Thing;
            if (targetThing == null || targetThing.Destroyed) {
                ability.UpdateStatus(Active.Off);
                EndJobWith(JobCondition.Incompletable);
                return;
            }

            pawn.MaintainProximityTo(
                TargetA,
                job.followRadius,
                PathEndMode.Touch
            );
        };

        yield return maintainToil;
    }
}