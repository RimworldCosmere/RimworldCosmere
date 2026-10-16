using Cosmere.System.Roshar.Surgebinding.Ability;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.JobDriver;

public class CastInstant : Verse.AI.JobDriver {
    private SurgebindingAbility Ability => (SurgebindingAbility)job.source!;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return true;
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        Verse.AI.Toil castToil = ToilMaker.MakeToil(nameof(CastInstant));
        castToil.initAction = () => { Ability.Activate(TargetA, TargetB); };
        castToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return castToil;
    }
}
