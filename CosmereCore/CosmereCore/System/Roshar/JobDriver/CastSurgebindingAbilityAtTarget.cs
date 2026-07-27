using Cosmere.System.Roshar.Surgebinding.Ability;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.JobDriver;

public class CastSurgebindingAbilityAtTarget : Verse.AI.JobDriver {
    private SurgebindingAbility ability => (SurgebindingAbility)job.source!;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        if (TargetA.HasThing) {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        } else {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
        }

        Verse.AI.Toil castToil = ToilMaker.MakeToil(nameof(CastSurgebindingAbilityAtTarget));
        castToil.initAction = () => { ability.Activate(TargetA, TargetB); };
        castToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return castToil;
    }
}
